using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Hotkeys;

/// <summary>
/// Atajos globales (HU-01/HU-02) con RegisterHotKey: Ctrl+Alt+D activa/oculta el
/// overlay y Ctrl+Alt+E alterna el modo dibujo/normal. Se registran con hWnd = 0,
/// por lo que WM_HOTKEY llega como mensaje de hilo; un bucle GetMessage en un
/// hilo dedicado lo recoge. Así funciona sin importar la app en primer plano.
/// El interop vive en Infrastructure.
/// </summary>
public sealed class Win32GlobalHotkeyService : IGlobalHotkeyService, IDisposable
{
    private const int ToggleHotkeyId = 1;
    private const int DrawingModeHotkeyId = 2;
    private const uint Modifiers = NativeMethods.ModControl | NativeMethods.ModAlt | NativeMethods.ModNoRepeat;
    private const uint VkD = 0x44; // tecla 'D'
    private const uint VkE = 0x45; // tecla 'E'

    private readonly Lock _gate = new();
    private Thread? _pump;
    private uint _pumpThreadId;
    private bool _disposed;

    public event EventHandler? ToggleRequested;

    public event EventHandler? DrawingModeToggleRequested;

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
        var registered = RegisterHotkeys();
        ready.Set();

        if (!registered)
        {
            UnregisterHotkeys();
            return;
        }

        try
        {
            PumpMessages();
        }
        finally
        {
            UnregisterHotkeys();
        }
    }

    private static bool RegisterHotkeys()
    {
        var toggle = NativeMethods.RegisterHotKey(nint.Zero, ToggleHotkeyId, Modifiers, VkD);
        var mode = NativeMethods.RegisterHotKey(nint.Zero, DrawingModeHotkeyId, Modifiers, VkE);
        return toggle && mode;
    }

    private static void UnregisterHotkeys()
    {
        NativeMethods.UnregisterHotKey(nint.Zero, ToggleHotkeyId);
        NativeMethods.UnregisterHotKey(nint.Zero, DrawingModeHotkeyId);
    }

    private void PumpMessages()
    {
        while (NativeMethods.GetMessage(out var message, nint.Zero, NativeMethods.WmHotkey, NativeMethods.WmHotkey) > 0)
        {
            RaiseForHotkey(message.WParam);
        }
    }

    private void RaiseForHotkey(nint hotkeyId)
    {
        if (hotkeyId == ToggleHotkeyId)
        {
            ToggleRequested?.Invoke(this, EventArgs.Empty);
        }
        else if (hotkeyId == DrawingModeHotkeyId)
        {
            DrawingModeToggleRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
