using Acetato.Application.Abstractions;
using Acetato.Application.Diagnostics;
using Acetato.Infrastructure.Diagnostics;
using Acetato.Presentation.Lifetime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Acetato.Presentation.Diagnostics;

/// <summary>
/// Parte del composition root (HU-32): abre el registro antes que el resto, engancha el
/// manejador de errores, lo registra en el contenedor y anota el inicio y la salida
/// normal. Vive aparte de <see cref="App"/> para no pasar de 400 líneas allí. Desecharlo
/// suelta el manejador y vacía el registro.
/// </summary>
internal sealed partial class AppDiagnostics : IDisposable
{
    private readonly ILoggerFactory _loggers;
    private readonly ILogger _logger;
    private readonly UnhandledErrorHandler _errors;

    private AppDiagnostics(ILoggerFactory loggers, UnhandledErrorHandler errors)
    {
        _loggers = loggers;
        _logger = loggers.CreateLogger<App>();
        _errors = errors;
    }

    /// <summary>
    /// Abre el registro en %LocalAppData%\Acetato\logs y engancha el manejador de errores
    /// a <paramref name="app"/>. Desde aquí, un error inesperado queda anotado.
    /// </summary>
    public static AppDiagnostics Start(System.Windows.Application app)
    {
        var loggers = SerilogSetup.Create(LogDirectory.Default);
        var errors = new UnhandledErrorHandler(
            loggers.CreateLogger<UnhandledErrorHandler>(),
            new ErrorPolicy(TimeProvider.System),
            new ToastErrorNotifier(loggers.CreateLogger<ToastErrorNotifier>()),
            new FatalExit(loggers));
        errors.Attach(app);
        return new AppDiagnostics(loggers, errors);
    }

    /// <summary>Registra la fábrica de registros y la versión de la app en el contenedor.</summary>
    public void Register(IServiceCollection services)
    {
        // La fábrica va antes de AddLogging, que solo agrega la suya si no hay otra.
        services.AddSingleton(_loggers);
        services.AddLogging();
        services.AddSingleton<IAppInfo>(new AssemblyAppInfo());
    }

    /// <summary>Guardia de una sola copia por sesión de Windows (HU-31), con su registro.</summary>
    public SingleInstanceGate CreateSingleInstanceGate(string name) =>
        new(new NamedSingleInstanceGuard(name), _loggers.CreateLogger<SingleInstanceGate>());

    /// <summary>Error al arrancar: se anota y Acetato se cierra con el mensaje.</summary>
    public void FailStartup(Exception exception) => _errors.HandleStartupError(exception);

    /// <summary>Línea «Inicio» con la versión completa, Windows, .NET, idioma y monitores.</summary>
    public void LogStartup(IServiceProvider services)
    {
        if (!_logger.IsEnabled(LogLevel.Information))
        {
            return;
        }

        string version = services.GetRequiredService<IAppInfo>().Version.Full;
        string windows = SystemDescription.WindowsVersion();
        int monitors = services.GetRequiredService<IMonitorService>().GetMonitors().Count;
        LogStarted(version, windows, SystemDescription.RuntimeVersion, SystemDescription.Language, monitors);
    }

    /// <summary>Línea «Salida normal», al cerrar desde el menú o el atajo.</summary>
    public void LogNormalExit() => LogExited();

    public void Dispose()
    {
        _errors.Dispose();
        _loggers.Dispose();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Inicio: Acetato {Version} · {Windows} · {Runtime} · idioma {Language} · {Monitors} monitores")]
    private partial void LogStarted(string version, string windows, string runtime, string language, int monitors);

    [LoggerMessage(Level = LogLevel.Information, Message = "Salida normal")]
    private partial void LogExited();
}
