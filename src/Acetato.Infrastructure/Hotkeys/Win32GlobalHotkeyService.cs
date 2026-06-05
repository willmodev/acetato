using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Hotkeys;

/// <summary>
/// Atajos globales (HU-01/02/04…) con RegisterHotKey. Se registran con
/// hWnd = 0, por lo que WM_HOTKEY llega como mensaje de hilo; un bucle
/// GetMessage en un hilo dedicado lo recoge. Así funciona sin importar la app
/// en primer plano. Las combinaciones viven en <see cref="HotkeyBindingTable"/>;
/// aquí solo el ciclo de vida y el bombeo. El interop vive en Infrastructure.
/// </summary>
public sealed class Win32GlobalHotkeyService : IGlobalHotkeyService, IDisposable
{
    private readonly Lock _gate = new();
    private Thread? _pump;
    private uint _pumpThreadId;
    private bool _disposed;

    public event EventHandler<HotkeyActionEventArgs>? ActionRequested;

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
        RegisterHotkeys();
        ready.Set();

        try
        {
            PumpMessages();
        }
        finally
        {
            UnregisterHotkeys();
        }
    }

    private static void RegisterHotkeys()
    {
        // Registro independiente y best-effort: si un atajo falla (p. ej. ya lo
        // tomó otra app), los demás deben seguir funcionando.
        foreach (var binding in HotkeyBindingTable.All)
        {
            _ = NativeMethods.RegisterHotKey(nint.Zero, binding.Id, binding.Modifiers, binding.VirtualKey);
        }
    }

    private static void UnregisterHotkeys()
    {
        foreach (var binding in HotkeyBindingTable.All)
        {
            NativeMethods.UnregisterHotKey(nint.Zero, binding.Id);
        }
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
        var binding = HotkeyBindingTable.FindById((int)hotkeyId);
        if (binding is not null)
        {
            ActionRequested?.Invoke(this, new HotkeyActionEventArgs(binding.Action));
        }
    }
}
