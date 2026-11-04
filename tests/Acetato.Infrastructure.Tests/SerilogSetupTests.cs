using System.Globalization;
using System.IO;
using Acetato.Infrastructure.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Acetato.Infrastructure.Tests;

/// <summary>Registro en archivo (HU-32), cada test en su propia carpeta temporal.</summary>
public sealed class SerilogSetupTests : IDisposable
{
    private static readonly Action<ILogger, string, Exception?> Write =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1), "{Text}");

    private static readonly Action<ILogger, Exception?> Fail =
        LoggerMessage.Define(LogLevel.Error, new EventId(2), "Error en la interfaz");

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "Acetato.Tests", Guid.NewGuid().ToString("N"));

    private string TodayPath =>
        Path.Combine(_directory, $"acetato-{DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.log");

    [Fact]
    public void Writes_one_file_named_by_day_with_time_level_source_and_message()
    {
        using (var loggers = SerilogSetup.Create(_directory))
        {
            Write(loggers.CreateLogger("Acetato.Prueba"), "Inicio", null);
        }

        File.Exists(TodayPath).Should().BeTrue();
        File.ReadAllText(TodayPath).Should().MatchRegex(
            @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2} \[INF\] Acetato\.Prueba: Inicio\r?\n$");
    }

    [Fact]
    public void Errors_carry_the_full_technical_detail()
    {
        using (var loggers = SerilogSetup.Create(_directory))
        {
            Fail(loggers.CreateLogger("Acetato.Prueba"), new InvalidOperationException("detalle del fallo"));
        }

        File.ReadAllText(TodayPath).Should()
            .Contain("[ERR] Acetato.Prueba: Error en la interfaz")
            .And.Contain("System.InvalidOperationException: detalle del fallo");
    }

    [Fact]
    public void Keeps_only_the_last_seven_files()
    {
        Directory.CreateDirectory(_directory);
        for (int day = 1; day <= 9; day++)
        {
            File.WriteAllText(Path.Combine(_directory, $"acetato-202610{day:00}.log"), "viejo");
        }

        using (var loggers = SerilogSetup.Create(_directory))
        {
            Write(loggers.CreateLogger("Acetato.Prueba"), "Inicio", null);
        }

        var files = Directory.GetFiles(_directory, SerilogSetup.FilePattern).Select(Path.GetFileName).ToList();
        files.Should().HaveCount(7).And.Contain(Path.GetFileName(TodayPath));
        files.Should().NotContain(["acetato-20261001.log", "acetato-20261002.log", "acetato-20261003.log"]);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
