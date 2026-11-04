using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace Acetato.Infrastructure.Diagnostics;

/// <summary>
/// Arma el registro de Acetato (HU-32) con Serilog detrás de <see cref="ILoggerFactory"/>:
/// un archivo de texto por día (acetato-AAAAMMDD.log), los últimos 7, con tope de 10 MB
/// por archivo (si se llena, se abre otro ese mismo día). Cada línea lleva fecha y hora,
/// nivel, de dónde viene y el mensaje; los errores, además, el detalle técnico completo.
/// Desechar la fábrica vacía el registro y cierra el archivo.
/// </summary>
public static class SerilogSetup
{
    /// <summary>Patrón de los nombres de archivo, para leerlos al armar el reporte.</summary>
    public const string FilePattern = "acetato-*.log";

    /// <summary>Archivos que se conservan; los más viejos se borran solos.</summary>
    public const int RetainedFiles = 7;

    /// <summary>Tope por archivo: 10 MB.</summary>
    public const long FileSizeLimitBytes = 10L * 1024 * 1024;

    // Serilog intercala la fecha (AAAAMMDD) antes de la extensión: acetato-20261103.log.
    private const string FileTemplate = "acetato-.log";

    private const string OutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

    /// <summary>Registro en <paramref name="directory"/> con nivel mínimo Information.</summary>
    public static ILoggerFactory Create(string directory)
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(directory, FileTemplate),
                outputTemplate: OutputTemplate,
                formatProvider: CultureInfo.InvariantCulture,
                fileSizeLimitBytes: FileSizeLimitBytes,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: RetainedFiles,
                encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            .CreateLogger();
        return new SerilogLoggerFactory(logger, dispose: true);
    }
}
