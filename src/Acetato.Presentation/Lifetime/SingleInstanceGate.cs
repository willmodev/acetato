using System.Windows.Input;
using System.Windows.Threading;
using Acetato.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Acetato.Presentation.Lifetime;

/// <summary>
/// Una sola copia por sesión de Windows (HU-31). Si ya había otra copia abierta, le avisa,
/// lo anota y la nueva se cierra sin mostrar nada. La copia que ya estaba abierta hace lo
/// mismo que «Activar dibujo» de la bandeja; el aviso llega en otro hilo y se pasa al de la
/// interfaz. Parte del composition root, aparte de <see cref="App"/>.
/// </summary>
internal sealed partial class SingleInstanceGate : IDisposable
{
    private readonly ISingleInstanceGuard _guard;
    private readonly ILogger _logger;
    private EventHandler? _activation;

    public SingleInstanceGate(ISingleInstanceGuard guard, ILogger<SingleInstanceGate> logger)
    {
        _guard = guard;
        _logger = logger;
    }

    /// <summary>
    /// <c>true</c> si esta es la primera copia. Si no, avisa a la que ya estaba y devuelve
    /// <c>false</c>: esta copia debe cerrarse.
    /// </summary>
    public bool TryEnter()
    {
        if (_guard.TryClaim())
        {
            return true;
        }

        LogAlreadyRunning();
        _guard.NotifyOwner();
        return false;
    }

    /// <summary>Cuando otra copia avise, ejecuta <paramref name="activate"/> en el hilo de la interfaz.</summary>
    public void OnActivationRequested(Dispatcher dispatcher, ICommand activate)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(activate);
        _activation = (_, _) => _ = dispatcher.BeginInvoke(() => activate.Execute(null));
        _guard.ActivationRequested += _activation;
    }

    public void Dispose()
    {
        _guard.ActivationRequested -= _activation;
        _guard.Dispose();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Ya había otra copia abierta; esta se cierra")]
    private partial void LogAlreadyRunning();
}
