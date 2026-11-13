using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Text;
using Acetato.Application.Abstractions;
using Acetato.Application.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Acetato.Infrastructure.Diagnostics;

/// <summary>
/// «Reportar problema» (HU-33): arma en el escritorio Acetato-reporte-AAAAMMDD-HHMM.zip con
/// <c>logs/</c> (los registros que haya), <c>settings.json</c> y <c>sistema.txt</c>. Antes
/// de guardar cambia en el contenido entero el nombre de usuario de Windows y el del equipo
/// por <c>&lt;usuario&gt;</c> y <c>&lt;equipo&gt;</c>. Lee el registro del día aunque Serilog
/// lo tenga abierto. Al terminar abre el Explorador con el archivo seleccionado. Nunca
/// lanza: si falla, lo anota y devuelve <see cref="ProblemReportResult.Failed"/>.
/// </summary>
public sealed partial class ZipProblemReportWriter : IProblemReportWriter
{
    private const string LogsFolder = "logs/";
    private const string SettingsFileName = "settings.json";
    private const string SystemFileName = "sistema.txt";

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    // Lo que puede salir mal al leer los archivos o escribir el .zip (carpeta inexistente,
    // sin permiso, ruta inválida, disco lleno).
    private static readonly Type[] FileProblems =
    [
        typeof(IOException),
        typeof(UnauthorizedAccessException),
        typeof(SecurityException),
        typeof(ArgumentException),
        typeof(NotSupportedException),
        typeof(InvalidDataException),
    ];

    private readonly ProblemReportSources _sources;
    private readonly IAppInfo _appInfo;
    private readonly IMonitorService _monitors;
    private readonly IFileRevealer _revealer;
    private readonly ILogger _logger;

    public ZipProblemReportWriter(
        ProblemReportSources sources,
        IAppInfo appInfo,
        IMonitorService monitors,
        IFileRevealer revealer,
        ILogger<ZipProblemReportWriter> logger)
    {
        _sources = sources;
        _appInfo = appInfo;
        _monitors = monitors;
        _revealer = revealer;
        _logger = logger;
    }

    public Task<ProblemReportResult> CreateAsync(CancellationToken cancellationToken) =>
        Task.Run(Create, cancellationToken);

    private ProblemReportResult Create()
    {
        string path = Path.Combine(_sources.DesktopDirectory, FileName(TimeProvider.System.GetLocalNow()));
        try
        {
            WriteZip(path);
        }
        catch (Exception ex) when (IsFileProblem(ex))
        {
            LogFailed(ex);
            TryDelete(path);
            return ProblemReportResult.Failed;
        }

        Reveal(path);
        return new ProblemReportResult(Succeeded: true, path);
    }

    private static string FileName(DateTimeOffset now) =>
        $"Acetato-reporte-{now.ToString("yyyyMMdd-HHmm", CultureInfo.InvariantCulture)}.zip";

    private void WriteZip(string path)
    {
        string system = SystemInfoText.Build(_appInfo, _monitors);
        using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var zip = new ZipArchive(file, ZipArchiveMode.Create);
        foreach (string log in LogFiles())
        {
            AddRedactedFile(zip, LogsFolder + Path.GetFileName(log), log);
        }

        string settings = Path.Combine(_sources.SettingsDirectory, SettingsFileName);
        if (File.Exists(settings))
        {
            AddRedactedFile(zip, SettingsFileName, settings);
        }

        using var reader = new StringReader(system);
        AddRedacted(zip, SystemFileName, reader);
    }

    private List<string> LogFiles() =>
        Directory.Exists(_sources.LogsDirectory)
            ? [.. Directory.EnumerateFiles(_sources.LogsDirectory, SerilogSetup.FilePattern).Order(StringComparer.Ordinal)]
            : [];

    // Lectura compartida: Serilog puede tener abierto el registro del día.
    private static void AddRedactedFile(ZipArchive zip, string entryName, string sourcePath)
    {
        using var source = new FileStream(
            sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(source, Encoding.UTF8);
        AddRedacted(zip, entryName, reader);
    }

    // Línea por línea: un registro de 10 MB no se carga entero en memoria.
    private static void AddRedacted(ZipArchive zip, string entryName, TextReader reader)
    {
        using var writer = new StreamWriter(zip.CreateEntry(entryName).Open(), Utf8NoBom);
        while (reader.ReadLine() is { } line)
        {
            writer.WriteLine(ReportRedactor.Redact(line, Environment.UserName, Environment.MachineName));
        }
    }

    // Si el Explorador no abre, el reporte ya está en el escritorio: solo se anota.
    private void Reveal(string path)
    {
        try
        {
            _revealer.Reveal(path);
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            LogRevealFailed(ex);
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex) when (IsFileProblem(ex))
        {
            // Queda un .zip a medias; el siguiente intento lo reemplaza.
        }
    }

    private static bool IsFileProblem(Exception ex) => FileProblems.Any(type => type.IsInstanceOfType(ex));

    [LoggerMessage(Level = LogLevel.Error, Message = "No se pudo crear el reporte para soporte")]
    private partial void LogFailed(Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "El reporte se creó, pero no se pudo abrir el Explorador")]
    private partial void LogRevealFailed(Exception exception);
}
