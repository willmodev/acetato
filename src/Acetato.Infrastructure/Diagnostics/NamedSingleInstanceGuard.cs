using Acetato.Application.Abstractions;

namespace Acetato.Infrastructure.Diagnostics;

/// <summary>
/// Una sola copia por sesión de Windows (HU-31) con un mutex y una señal con nombre en el
/// espacio <c>Local\</c>, que es el de la sesión: dos usuarios con sesión abierta en el
/// mismo equipo tienen cada uno su copia. La señal es un timbre que solo la primera copia
/// escucha; la espera corre en el pool de hilos, no en el de la interfaz. .NET
/// administrado, sin P/Invoke.
/// </summary>
public sealed class NamedSingleInstanceGuard : ISingleInstanceGuard
{
    private readonly string _mutexName;
    private readonly string _signalName;
    private Mutex? _mutex;
    private EventWaitHandle? _signal;
    private RegisteredWaitHandle? _wait;

    /// <param name="name">Nombre de la app; los tests pasan uno único.</param>
    public NamedSingleInstanceGuard(string name)
    {
        _mutexName = $@"Local\{name}.Instancia";
        _signalName = $@"Local\{name}.Activar";
    }

    public event EventHandler? ActivationRequested;

    public bool TryClaim()
    {
        if (_mutex is not null)
        {
            return true;
        }

        // El mutex vive mientras su manejador siga abierto: basta con crearlo, no hace
        // falta tomarlo (y así no depende del hilo que lo creó).
        var mutex = new Mutex(initiallyOwned: false, _mutexName, out bool createdNew);
        if (!createdNew)
        {
            mutex.Dispose();
            return false;
        }

        _mutex = mutex;
        _signal = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, _signalName);
        _wait = ThreadPool.RegisterWaitForSingleObject(
            _signal, OnSignaled, state: null, Timeout.Infinite, executeOnlyOnce: false);
        return true;
    }

    public void NotifyOwner()
    {
        // Abre la señal de la dueña (o la crea si aún no existe) y la toca.
        using var signal = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, _signalName);
        _ = signal.Set();
    }

    public void Dispose()
    {
        _ = _wait?.Unregister(null);
        _signal?.Dispose();
        _mutex?.Dispose();
        _wait = null;
        _signal = null;
        _mutex = null;
    }

    private void OnSignaled(object? state, bool timedOut) => ActivationRequested?.Invoke(this, EventArgs.Empty);
}
