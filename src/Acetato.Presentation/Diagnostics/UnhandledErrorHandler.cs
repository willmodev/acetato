using System.Windows.Threading;
using Acetato.Application.Abstractions;
using Acetato.Application.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Acetato.Presentation.Diagnostics;

/// <summary>
/// Recibe los errores inesperados (HU-32) y aplica <see cref="ErrorPolicy"/>. En la
/// interfaz (barra, lienzo, Ajustes) Acetato sigue abierto: la primera vez avisa, después
/// solo anota, y una racha se trata como fatal. En una tarea de fondo solo anota una
/// advertencia. Fuera de la interfaz o al arrancar, anota y cierra. Nunca anota lo
/// dibujado ni el texto de las anotaciones: solo el error y su detalle técnico.
/// </summary>
internal sealed partial class UnhandledErrorHandler : IDisposable
{
    private readonly ILogger _logger;
    private readonly ErrorPolicy _policy;
    private readonly IErrorNotifier _notifier;
    private readonly IFatalExit _fatal;
    private System.Windows.Application? _app;
    private int _closing;

    public UnhandledErrorHandler(
        ILogger<UnhandledErrorHandler> logger, ErrorPolicy policy, IErrorNotifier notifier, IFatalExit fatal)
    {
        _logger = logger;
        _policy = policy;
        _notifier = notifier;
        _fatal = fatal;
    }

    /// <summary>Se suscribe a los errores de la interfaz, de fondo y del proceso.</summary>
    public void Attach(System.Windows.Application app)
    {
        _app = app;
        app.DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    /// <summary>
    /// Error en la interfaz. Devuelve si queda atendido, para que Acetato siga abierto. Si
    /// la racha lo vuelve fatal, el proceso termina aquí.
    /// </summary>
    public bool HandleInterfaceError(Exception exception)
    {
        if (Volatile.Read(ref _closing) != 0)
        {
            return true; // Ya se está cerrando: el mensaje está en pantalla.
        }

        var decision = _policy.Register();
        if (decision == ErrorDecision.Close)
        {
            LogBurst(exception);
            Close(exception);
            return true;
        }

        LogInterfaceError(exception);
        if (decision == ErrorDecision.Notify)
        {
            _notifier.NotifyRecovered();
        }

        return true;
    }

    /// <summary>Error en una tarea de fondo: solo se anota.</summary>
    public void HandleBackgroundError(Exception exception) => LogBackgroundError(exception);

    /// <summary>Error fuera de la interfaz, sin recuperación posible: se anota y se cierra.</summary>
    public void HandleFatalError(Exception exception)
    {
        LogFatalError(exception);
        Close(exception);
    }

    /// <summary>Error al arrancar, antes de que exista la barra: se anota y se cierra.</summary>
    public void HandleStartupError(Exception exception)
    {
        LogStartupError(exception);
        Close(exception);
    }

    public void Dispose()
    {
        if (_app is not null)
        {
            _app.DispatcherUnhandledException -= OnDispatcherUnhandledException;
        }

        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
    }

    // Un solo cierre: si otro error llega mientras el mensaje está abierto, se ignora.
    private void Close(Exception exception)
    {
        if (Interlocked.Exchange(ref _closing, 1) == 0)
        {
            _fatal.Exit(exception);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) =>
        e.Handled = HandleInterfaceError(e.Exception);

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        HandleBackgroundError(e.Exception);
        e.SetObserved();
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) =>
        HandleFatalError(e.ExceptionObject as Exception ?? new InvalidOperationException("Error desconocido fuera de la interfaz"));

    [LoggerMessage(Level = LogLevel.Error, Message = "Error en la interfaz: Acetato sigue abierto")]
    private partial void LogInterfaceError(Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Error en una tarea de fondo")]
    private partial void LogBackgroundError(Exception exception);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Racha de errores en la interfaz: Acetato se cierra")]
    private partial void LogBurst(Exception exception);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Error fatal fuera de la interfaz: Acetato se cierra")]
    private partial void LogFatalError(Exception exception);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Error al arrancar: Acetato se cierra")]
    private partial void LogStartupError(Exception exception);
}
