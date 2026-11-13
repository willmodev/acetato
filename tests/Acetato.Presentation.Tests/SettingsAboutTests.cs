using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Hotkeys;
using Acetato.Presentation.Settings;
using Acetato.Presentation.ViewModels;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>Bloque «Acerca de» de Ajustes (HU-33): versión, copiarla y reportar un problema.</summary>
public sealed class SettingsAboutTests : IDisposable
{
    private readonly IClipboardWriter _clipboard = Substitute.For<IClipboardWriter>();
    private readonly IProblemReportWriter _reports = Substitute.For<IProblemReportWriter>();
    private readonly HotkeyCoordinator _coordinator = TestCoordinators.Started();
    private readonly SettingsViewModel _settings;

    public SettingsAboutTests() =>
        _settings = new SettingsViewModel(
            _coordinator,
            Substitute.For<IWindowDragHandler>(),
            new DrawingSettings(),
            Substitute.For<IDrawingPreferencesStore>(),
            TestAbout.Services(_clipboard, _reports));

    [Fact]
    public void Shows_the_screen_version_without_the_commit()
    {
        _settings.VersionText.Should().Be("Acetato 0.9.0");
    }

    [Fact]
    public void Copy_version_leaves_the_screen_version_on_the_clipboard()
    {
        _settings.CopyVersionCommand.Execute(null);

        _clipboard.Received(1).SetText("0.9.0");
    }

    [Fact]
    public void The_hint_explains_the_report_before_using_it()
    {
        _settings.ReportStatus.Should().Be("Crea un .zip en el escritorio para enviarlo a soporte");
    }

    [Fact]
    public async Task Report_is_disabled_while_it_runs_and_then_says_it_was_saved()
    {
        var pending = new TaskCompletionSource<ProblemReportResult>();
        _reports.CreateAsync(Arg.Any<CancellationToken>()).Returns(pending.Task);

        var running = _settings.ReportProblemCommand.ExecuteAsync(null);

        _settings.ReportProblemCommand.CanExecute(null).Should().BeFalse();
        pending.SetResult(new ProblemReportResult(Succeeded: true, @"C:\Escritorio\Acetato-reporte-20261103-1530.zip"));
        await running.ConfigureAwait(true);

        _settings.ReportProblemCommand.CanExecute(null).Should().BeTrue();
        _settings.ReportStatus.Should().Be("Reporte guardado en el escritorio");
    }

    [Fact]
    public async Task A_failed_report_says_so()
    {
        _reports.CreateAsync(Arg.Any<CancellationToken>()).Returns(ProblemReportResult.Failed);

        await _settings.ReportProblemCommand.ExecuteAsync(null);

        _settings.ReportStatus.Should().Be("No se pudo crear el reporte");
    }

    public void Dispose()
    {
        _settings.Dispose();
        _coordinator.Dispose();
    }
}
