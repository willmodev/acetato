using System.IO;
using System.IO.Compression;
using Acetato.Application.Abstractions;
using Acetato.Application.Diagnostics;
using Acetato.Domain;
using Acetato.Infrastructure.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Acetato.Infrastructure.Tests;

/// <summary>
/// «Reportar problema» (HU-33), con el registro, los ajustes y el escritorio en carpetas
/// temporales. El Explorador no se abre: el revelador es un doble.
/// </summary>
public sealed class ZipProblemReportWriterTests : IDisposable
{
    private static readonly Action<ILogger, string, Exception?> Write =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1), "{Text}");

    private readonly string _root = Path.Combine(Path.GetTempPath(), "Acetato.Tests", Guid.NewGuid().ToString("N"));
    private readonly IAppInfo _appInfo = Substitute.For<IAppInfo>();
    private readonly IMonitorService _monitors = Substitute.For<IMonitorService>();
    private readonly IFileRevealer _revealer = Substitute.For<IFileRevealer>();
    private readonly RecordingLogger<ZipProblemReportWriter> _log = new();

    public ZipProblemReportWriterTests()
    {
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(SettingsDirectory);
        Directory.CreateDirectory(DesktopDirectory);
        _appInfo.Version.Returns(AppVersion.Parse("0.9.0+abc123"));
        _monitors.GetMonitors().Returns([new MonitorInfo(0, 0, 1920, 1080, IsPrimary: true)]);
    }

    private string LogsDirectory => Path.Combine(_root, "logs");

    private string SettingsDirectory => Path.Combine(_root, "settings");

    private string DesktopDirectory => Path.Combine(_root, "desktop");

    // Así se ven los nombres reales del equipo dentro de un registro.
    private static string PersonalLine =>
        $@"C:\Users\{Environment.UserName}\AppData\Local\Acetato en {Environment.MachineName}";

    [Fact]
    public async Task Creates_the_zip_on_the_desktop_with_logs_settings_and_system()
    {
        await File.WriteAllTextAsync(Path.Combine(LogsDirectory, "acetato-20261102.log"), "ayer");
        await File.WriteAllTextAsync(Path.Combine(LogsDirectory, "acetato-20261103.log"), "hoy");
        await File.WriteAllTextAsync(Path.Combine(SettingsDirectory, "settings.json"), """{ "version": 1 }""");

        var result = await CreateWriter(DesktopDirectory).CreateAsync(CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        Path.GetFileName(result.FilePath).Should().MatchRegex(@"^Acetato-reporte-\d{8}-\d{4}\.zip$");
        Path.GetDirectoryName(result.FilePath).Should().Be(DesktopDirectory);
        var entries = ReadZip(result.FilePath!);
        entries.Keys.Should().BeEquivalentTo(
            "logs/acetato-20261102.log", "logs/acetato-20261103.log", "settings.json", "sistema.txt");
        entries["logs/acetato-20261103.log"].Should().Contain("hoy");
        entries["sistema.txt"].Should().ContainAll(
            "Acetato: 0.9.0+abc123", "Windows:", ".NET:", "Idioma:", "Monitores: 1", "1920 × 1080 px", "escala", "principal");
        _revealer.Received(1).Reveal(result.FilePath!);
    }

    [Fact]
    public async Task Removes_the_user_and_machine_names_from_every_file()
    {
        await File.WriteAllTextAsync(Path.Combine(LogsDirectory, "acetato-20261103.log"), PersonalLine);
        await File.WriteAllTextAsync(Path.Combine(SettingsDirectory, "settings.json"), PersonalLine);

        var result = await CreateWriter(DesktopDirectory).CreateAsync(CancellationToken.None);

        foreach (string content in ReadZip(result.FilePath!).Values)
        {
            WithoutPlaceholders(content).Should()
                .NotContainEquivalentOf(Environment.UserName)
                .And.NotContainEquivalentOf(Environment.MachineName);
        }

        ReadZip(result.FilePath!)["settings.json"].Should().Contain(@"C:\Users\<usuario>\AppData");
    }

    [Fact]
    public async Task Reads_the_log_that_Serilog_has_open()
    {
        using var loggers = SerilogSetup.Create(LogsDirectory);
        Write(loggers.CreateLogger("Acetato.Prueba"), "registro abierto", null);

        var result = await CreateWriter(DesktopDirectory).CreateAsync(CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        ReadZip(result.FilePath!).Should().Contain(entry =>
            entry.Key.StartsWith("logs/acetato-", StringComparison.Ordinal) && entry.Value.Contains("registro abierto", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Without_logs_or_settings_the_zip_still_has_the_system()
    {
        Directory.Delete(LogsDirectory);

        var result = await CreateWriter(DesktopDirectory).CreateAsync(CancellationToken.None);

        ReadZip(result.FilePath!).Keys.Should().Equal("sistema.txt");
    }

    [Fact]
    public async Task A_desktop_that_cannot_be_written_fails_and_logs_without_throwing()
    {
        var result = await CreateWriter(Path.Combine(_root, "no-existe")).CreateAsync(CancellationToken.None);

        result.Should().Be(ProblemReportResult.Failed);
        _log.Entries.Should().ContainSingle().Which.Level.Should().Be(LogLevel.Error);
        _revealer.DidNotReceiveWithAnyArgs().Reveal(default!);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static string WithoutPlaceholders(string content) =>
        content.Replace(ReportRedactor.UserPlaceholder, string.Empty, StringComparison.Ordinal)
            .Replace(ReportRedactor.MachinePlaceholder, string.Empty, StringComparison.Ordinal);

    private static Dictionary<string, string> ReadZip(string path)
    {
        using var zip = ZipFile.OpenRead(path);
        return zip.Entries.ToDictionary(
            entry => entry.FullName,
            entry =>
            {
                using var reader = new StreamReader(entry.Open());
                return reader.ReadToEnd();
            },
            StringComparer.Ordinal);
    }

    private ZipProblemReportWriter CreateWriter(string desktop) =>
        new(new ProblemReportSources(LogsDirectory, SettingsDirectory, desktop), _appInfo, _monitors, _revealer, _log);
}
