using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Hotkeys;

/// <summary>
/// Atajo global de activación (HU-01) con RegisterHotKey. El atajo se registra
/// con hWnd = 0, por lo que WM_HOTKEY llega como mensaje de hilo; un bucle
/// GetMessage en un hilo dedicado lo recoge. Así funciona sin importar la app
/// en primer plano. El interop vive en Infrastructure.
/// </summary>
public sealed class Win32GlobalHotkeyService : IGlobalHotkeyService, IDisposable
{
    private const int ToggleHotkeyId = 1;
    private const uint VkD = 0x44; // tecla 'D'

    private readonly Lock _gate = new();
    private Thread? _pump;
    private uint _pumpThreadId;
    private bool _disposed;

    public event EventHandler? ToggleRequested;

    public void Register()
    {
        lock (_gate)
        {
            if (_pump is not null)
            {
                return;
            }

            using var ready = new ManualResetEventSlim(false);
            _pump = StartPumpThread(ready);
            ready.Wait();
        }
    }

    public void Unregister()
    {
        lock (_gate)
        {
            if (_pump is null)
            {
                return;
            }

            NativeMethods.PostThreadMessage(_pumpThreadId, NativeMethods.WmQuit, nint.Zero, nint.Zero);
            _pump.Join();
            _pump = null;
            _pumpThreadId = 0;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Unregister();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private Thread StartPumpThread(ManualResetEventSlim ready)
    {
        var thread = new Thread(() => RunPump(ready))
        {
            IsBackground = true,
            Name = "Acetato.Hotkeys",
        };
        thread.Start();
        return thread;
    }

    private void RunPump(ManualResetEventSlim ready)
    {
        _pumpThreadId = NativeMethods.GetCurrentThreadId();
        var registered = NativeMethods.RegisterHotKey(
            nint.Zero,
            ToggleHotkeyId,
            NativeMethods.ModControl | NativeMethods.ModAlt | NativeMethods.ModNoRepeat,
            VkD);
        ready.Set();

        if (!registered)
        {
            return;
        }

        try
        {
            PumpMessages();
        }
        finally
        {
            NativeMethods.UnregisterHotKey(nint.Zero, ToggleHotkeyId);
        }
    }

    private void PumpMessages()
    {
        while (NativeMethods.GetMessage(out var message, nint.Zero, NativeMethods.WmHotkey, NativeMethods.WmHotkey) > 0)
        {
            if (message.WParam == ToggleHotkeyId)
            {
                ToggleRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
