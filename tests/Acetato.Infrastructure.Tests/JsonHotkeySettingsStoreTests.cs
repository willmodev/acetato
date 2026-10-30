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
    public void A_file_from_HU_21_keeps_its_changes_and_gets_the_new_defaults_of_HU_22()
    {
        WriteSettings("""
            {
              "version": 1,
              "hotkeys": {
                "Undo": "Alt+Z",
                "Capture": null
              }
            }
            """);

        var map = CreateStore().Load().Map;

        map.ChordOf(HotkeyAction.Undo).Should().Be(AltZ);
        map.ChordOf(HotkeyAction.Capture).Should().BeNull();
        map.ChordOf(HotkeyAction.SelectRectangleTool).Should().Be(new KeyChord(HotkeyModifiers.Ctrl, HotkeyKey.R));
        map.ChordOf(HotkeyAction.ThicknessUp).Should().Be(new KeyChord(HotkeyModifiers.Ctrl, HotkeyKey.PageUp));
    }

    [Fact]
    public void A_file_without_fold_bar_gets_ctrl_alt_h()
    {
        WriteSettings("""
            {
              "version": 1,
              "hotkeys": {
                "Undo": "Alt+Z"
              }
            }
            """);

        var map = CreateStore().Load().Map;

        map.ChordOf(HotkeyAction.ToggleToolbarCollapsed)
            .Should().Be(new KeyChord(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, HotkeyKey.H));
    }

    [Fact]
    public void A_file_that_already_used_ctrl_alt_h_keeps_it_and_fold_bar_stays_without_chord()
    {
        WriteSettings("""
            {
              "version": 1,
              "hotkeys": {
                "ColorRed": "Ctrl+Alt+H"
              }
            }
            """);

        var map = CreateStore().Load().Map;

        map.ChordOf(HotkeyAction.ColorRed).Should().Be(new KeyChord(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, HotkeyKey.H));
        map.ChordOf(HotkeyAction.ToggleToolbarCollapsed).Should().BeNull();
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
