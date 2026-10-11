using System.Collections.Concurrent;
using Acetato.Application.Abstractions;
using Acetato.Domain;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Hotkeys;

/// <summary>
/// Atajos globales (HU-01/02/04…/21) con RegisterHotKey. Se registran con
/// hWnd = 0, por lo que WM_HOTKEY llega como mensaje de hilo; un bucle
/// GetMessage en un hilo dedicado lo recoge. Así funciona sin importar la app
/// en primer plano. Como el registro queda atado al hilo que lo hace,
/// <see cref="Apply"/> e <see cref="IsAvailable"/> encolan la petición al hilo
/// de atajos (aviso WM_APP) y esperan su resultado con tope. El registro en sí
/// vive en <see cref="HotkeyRegistry"/>; aquí el ciclo de vida y el bombeo.
/// </summary>
public sealed class Win32GlobalHotkeyService : IGlobalHotkeyService, IDisposable
{
    // Tope de espera al cerrar (HU-20). El hilo es de fondo: si el tope vence, el
    // proceso termina igual en vez de quedar vivo bloqueando el build.
    private static readonly TimeSpan StopTimeout = TimeSpan.FromSeconds(2);

    // Tope por petición (HU-21): si el hilo no responde, la UI no se congela.
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(1);

    private readonly Lock _gate = new();
    private readonly ConcurrentQueue<Action> _requests = new();
    private readonly HotkeyRegistry _registry = new();
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

            // Si el aviso no se pudo encolar, no tiene sentido esperar: el hilo de
            // fondo muere con el proceso.
            if (NativeMethods.PostThreadMessage(_pumpThreadId, NativeMethods.WmQuit, nint.Zero, nint.Zero))
            {
                _ = _pump.Join(StopTimeout);
            }

            // Peticiones sin atender: no deben ejecutarse en un hilo futuro.
            _requests.Clear();
            _pump = null;
            _pumpThreadId = 0;
        }
    }

    public IReadOnlyList<HotkeyAction> Apply(IReadOnlyList<HotkeyAssignment> assignments)
    {
        // Si el hilo no responde, ninguna se pudo registrar.
        var failed = assignments.Select(assignment => assignment.Action).ToList();
        return RunOnPump(() => _registry.Apply(assignments), failed);
    }

    public bool IsAvailable(KeyChord chord) => RunOnPump(() => HotkeyRegistry.IsAvailable(chord), fallback: false);

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

    // Encola el trabajo al hilo de atajos y espera su resultado; devuelve
    // fallback si el hilo no corre o no responde a tiempo.
    private T RunOnPump<T>(Func<T> work, T fallback)
    {
        uint threadId;
        lock (_gate)
        {
            if (_pump is null)
            {
                return fallback;
            }

            if (Thread.CurrentThread == _pump)
            {
                return work();
            }

            threadId = _pumpThreadId;
        }

        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        _requests.Enqueue(() => completion.TrySetResult(work()));
        if (!NativeMethods.PostThreadMessage(threadId, NativeMethods.WmApp, nint.Zero, nint.Zero))
        {
            return fallback;
        }

        return completion.Task.Wait(RequestTimeout) ? completion.Task.Result : fallback;
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
        // Crea la cola de mensajes del hilo antes de avisar: sin ella,
        // PostThreadMessage fallaría hasta el primer GetMessage.
        _ = NativeMethods.PeekMessage(out _, nint.Zero, 0, 0, NativeMethods.PmNoRemove);
        ready.Set();

        try
        {
            PumpMessages();
        }
        finally
        {
            _registry.UnregisterAll();
        }
    }

    // Sin filtro de mensajes (0, 0): un WM_QUIT ENVIADO con PostThreadMessage es un
    // mensaje más de la cola y un filtro solo-WM_HOTKEY lo descartaría, dejando el
    // bucle (y el cierre de la app) colgado para siempre (HU-20). GetMessage devuelve
    // 0 con WM_QUIT y el bucle termina; el resto de mensajes se ignora.
    private void PumpMessages()
    {
        while (NativeMethods.GetMessage(out var message, nint.Zero, 0, 0) > 0)
        {
            if (message.Message == NativeMethods.WmHotkey)
            {
                RaiseForHotkey(message.WParam);
            }
            else if (message.Message == NativeMethods.WmApp)
            {
                DrainRequests();
            }
        }
    }

    private void DrainRequests()
    {
        while (_requests.TryDequeue(out var request))
        {
            request();
        }
    }

    private void RaiseForHotkey(nint hotkeyId)
    {
        if (_registry.Find((int)hotkeyId) is { } action)
        {
            ActionRequested?.Invoke(this, new HotkeyActionEventArgs(action));
        }
    }
}
