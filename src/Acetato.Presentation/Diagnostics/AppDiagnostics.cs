using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Acetato.Presentation.Diagnostics;

/// <summary>
/// Parte del composition root (HU-32): crea el registro antes que el resto, lo
/// registra en el contenedor y anota el inicio y la salida normal. Vive aparte de
/// <see cref="App"/> para no pasar de 400 líneas allí. Desecharlo vacía el registro.
/// </summary>
internal sealed partial class AppDiagnostics : IDisposable
{
    private readonly ILoggerFactory _loggers;
    private readonly ILogger _logger;

    private AppDiagnostics(ILoggerFactory loggers)
    {
        _loggers = loggers;
        _logger = loggers.CreateLogger<App>();
    }

    /// <summary>Abre el registro en %LocalAppData%\Acetato\logs.</summary>
    public static AppDiagnostics Start() => new(SerilogSetup.Create(LogDirectory.Default));

    /// <summary>Registra la fábrica de registros y la versión de la app en el contenedor.</summary>
    public void Register(IServiceCollection services)
    {
        // La fábrica va antes de AddLogging, que solo agrega la suya si no hay otra.
        services.AddSingleton(_loggers);
        services.AddLogging();
        services.AddSingleton<IAppInfo>(new AssemblyAppInfo());
    }

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

    public void Dispose() => _loggers.Dispose();

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Inicio: Acetato {Version} · {Windows} · {Runtime} · idioma {Language} · {Monitors} monitores")]
    private partial void LogStarted(string version, string windows, string runtime, string language, int monitors);

    [LoggerMessage(Level = LogLevel.Information, Message = "Salida normal")]
    private partial void LogExited();
}
