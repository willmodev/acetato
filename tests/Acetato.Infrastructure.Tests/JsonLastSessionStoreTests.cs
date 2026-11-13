using System.IO;
using Acetato.Application.Abstractions;
using Acetato.Domain;
using Acetato.Infrastructure.Settings;
using FluentAssertions;
using Xunit;

namespace Acetato.Infrastructure.Tests;

/// <summary>
/// Persistencia de la última sesión (HU-29) en el mismo settings.json que los atajos y
/// las opciones de dibujo, cada test en su propia carpeta temporal.
/// </summary>
public sealed class JsonLastSessionStoreTests : IDisposable
{
    private static readonly LastSession Full = new(
        TintaColor.Blue,
        Gradient: true,
        ThicknessIndex: 3,
        ToolKind.Arrow,
        ToolbarCollapsed: true,
        new ToolbarPlacement(new MonitorInfo(-1920, 0, 1920, 1080, IsPrimary: false), -1200, 64));

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "Acetato.Tests", Guid.NewGuid().ToString("N"));

    private string SettingsPath => Path.Combine(_directory, "settings.json");

    private JsonLastSessionStore CreateStore() => new(_directory);

    private void WriteSettings(string json)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(SettingsPath, json);
    }

    [Fact]
    public void Without_file_it_starts_as_always()
    {
        CreateStore().Load().Should().Be(LastSession.Defaults);
    }

    [Fact]
    public void Round_trip_keeps_the_whole_session()
    {
        CreateStore().Save(Full);

        CreateStore().Load().Should().Be(Full);
    }

    [Fact]
    public void The_highlighter_round_trips_as_the_last_tool()
    {
        var session = Full with { Tool = ToolKind.Highlighter };

        CreateStore().Save(session);

        CreateStore().Load().Tool.Should().Be(ToolKind.Highlighter);
    }

    [Fact]
    public void A_bar_never_moved_round_trips_without_placement()
    {
        var session = Full with { ToolbarPlacement = null };

        CreateStore().Save(session);

        CreateStore().Load().Should().Be(session);
    }

    [Fact]
    public void Each_bad_field_falls_back_on_its_own()
    {
        WriteSettings("""
            { "version": 1, "session": {
                "ink": "Purple", "gradient": "yes", "thickness": 9, "tool": "Arrow",
                "toolbar": { "collapsed": true, "placement": { "x": 10, "y": 20 } } } }
            """);

        CreateStore().Load().Should().Be(new LastSession(
            TintaColor.Red, Gradient: false, ThicknessScale.DefaultIndex, ToolKind.Arrow, ToolbarCollapsed: true, null));
    }

    [Theory]
    [InlineData("\"1\"")]
    [InlineData("1")]
    [InlineData("\"arrow\"")]
    public void A_tool_that_is_not_an_exact_name_falls_back_to_the_pencil(string tool)
    {
        WriteSettings($$"""{ "version": 1, "session": { "tool": {{tool}} } }""");

        CreateStore().Load().Tool.Should().Be(ToolKind.Pencil);
    }

    [Fact]
    public void A_monitor_without_size_drops_the_placement()
    {
        WriteSettings("""
            { "version": 1, "session": { "toolbar": { "placement": {
                "monitor": { "x": 0, "y": 0, "width": 0, "height": 1080 }, "x": 10, "y": 20 } } } }
            """);

        CreateStore().Load().ToolbarPlacement.Should().BeNull();
    }

    [Fact]
    public void Saving_the_session_keeps_the_drawing_section()
    {
        new JsonDrawingPreferencesStore(_directory).Save(new DrawingPreferences(InkPairs: true, FreeResize: true));

        CreateStore().Save(Full);

        new JsonDrawingPreferencesStore(_directory).Load().FreeResize.Should().BeTrue();
        CreateStore().Load().Should().Be(Full);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
