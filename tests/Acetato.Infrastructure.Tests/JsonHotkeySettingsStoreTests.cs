using System.IO;
using System.Text.Json;
using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using Acetato.Infrastructure.Settings;
using FluentAssertions;
using Xunit;

namespace Acetato.Infrastructure.Tests;

/// <summary>Persistencia de atajos (HU-21), cada test en su propia carpeta temporal.</summary>
public sealed class JsonHotkeySettingsStoreTests : IDisposable
{
    private static readonly KeyChord AltZ = new(HotkeyModifiers.Alt, HotkeyKey.Z);

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "Acetato.Tests", Guid.NewGuid().ToString("N"));

    private string SettingsPath => Path.Combine(_directory, "settings.json");

    private string CorruptPath => Path.Combine(_directory, "settings.corrupt.json");

    private JsonHotkeySettingsStore CreateStore() => new(_directory);

    private void WriteSettings(string json)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(SettingsPath, json);
    }

    [Fact]
    public void Round_trip_keeps_changes_and_removed_chords()
    {
        var map = HotkeyMap.Defaults.With(HotkeyAction.Undo, AltZ).With(HotkeyAction.Capture, null);

        CreateStore().Save(map);
        var load = CreateStore().Load();

        load.WasCorrupt.Should().BeFalse();
        load.Map.Overrides.Should().BeEquivalentTo(map.Overrides);
    }

    [Fact]
    public void Without_file_loads_the_defaults()
    {
        var load = CreateStore().Load();

        load.WasCorrupt.Should().BeFalse();
        load.Map.Overrides.Should().BeEmpty();
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("[1, 2]")]
    [InlineData("{ \"version\": 1, \"hotkeys\": [] }")]
    public void Unreadable_json_loads_defaults_and_renames_the_file(string json)
    {
        WriteSettings(json);

        var load = CreateStore().Load();

        load.WasCorrupt.Should().BeTrue();
        load.Map.Overrides.Should().BeEmpty();
        File.Exists(SettingsPath).Should().BeFalse();
        File.ReadAllText(CorruptPath).Should().Be(json);
    }

    [Fact]
    public void Unknown_actions_and_invalid_chords_are_ignored()
    {
        WriteSettings("""
            {
              "version": 1,
              "hotkeys": {
                "Undo": "Alt+Z",
                "Teleport": "Ctrl+Q",
                "3": "Ctrl+Q",
                "Clear": "Ctrl+Foo",
                "Capture": "Z",
                "CycleTool": 42,
                "SelectTextTool": null
              }
            }
            """);

        var load = CreateStore().Load();

        load.WasCorrupt.Should().BeFalse();
        load.Map.Overrides.Should().BeEquivalentTo(new Dictionary<HotkeyAction, KeyChord?>
        {
            [HotkeyAction.Undo] = AltZ,
            [HotkeyAction.SelectTextTool] = null,
        });
    }

    [Fact]
    public void Only_the_differences_are_written()
    {
        var map = HotkeyMap.Defaults.With(HotkeyAction.Undo, AltZ).With(HotkeyAction.Capture, null);

        CreateStore().Save(map);

        using var document = JsonDocument.Parse(File.ReadAllText(SettingsPath));
        var root = document.RootElement;
        root.GetProperty("version").GetInt32().Should().Be(1);
        var hotkeys = root.GetProperty("hotkeys");
        hotkeys.EnumerateObject().Select(entry => entry.Name).Should().Equal("Undo", "Capture");
        hotkeys.GetProperty("Undo").GetString().Should().Be("Alt+Z");
        hotkeys.GetProperty("Capture").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public void Saving_the_defaults_writes_an_empty_hotkeys_section()
    {
        CreateStore().Save(HotkeyMap.Defaults.With(HotkeyAction.Undo, AltZ));

        CreateStore().Save(HotkeyMap.Defaults);

        CreateStore().Load().Map.Overrides.Should().BeEmpty();
        Directory.GetFiles(_directory).Should().ContainSingle();
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
