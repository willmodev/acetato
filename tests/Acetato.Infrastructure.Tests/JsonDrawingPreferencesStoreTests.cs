using System.IO;
using System.Text.Json;
using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using Acetato.Infrastructure.Settings;
using FluentAssertions;
using Xunit;

namespace Acetato.Infrastructure.Tests;

/// <summary>
/// Persistencia de las opciones de dibujo (HU-27) en el mismo settings.json que los
/// atajos, cada test en su propia carpeta temporal.
/// </summary>
public sealed class JsonDrawingPreferencesStoreTests : IDisposable
{
    private static readonly KeyChord AltZ = new(HotkeyModifiers.Alt, HotkeyKey.Z);

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "Acetato.Tests", Guid.NewGuid().ToString("N"));

    private string SettingsPath => Path.Combine(_directory, "settings.json");

    private JsonDrawingPreferencesStore CreateStore() => new(_directory);

    private JsonHotkeySettingsStore CreateHotkeyStore() => new(_directory);

    private void WriteSettings(string json)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(SettingsPath, json);
    }

    [Fact]
    public void Without_file_ink_pairs_are_off()
    {
        CreateStore().Load().Should().Be(DrawingPreferences.Defaults);
        DrawingPreferences.Defaults.InkPairs.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Round_trip_keeps_ink_pairs(bool inkPairs)
    {
        CreateStore().Save(new DrawingPreferences(inkPairs));

        CreateStore().Load().InkPairs.Should().Be(inkPairs);
    }

    [Fact]
    public void Writes_the_drawing_section_with_the_version()
    {
        CreateStore().Save(new DrawingPreferences(InkPairs: true));

        using var document = JsonDocument.Parse(File.ReadAllText(SettingsPath));
        var root = document.RootElement;
        root.GetProperty("version").GetInt32().Should().Be(1);
        root.GetProperty("drawing").GetProperty("inkPairs").GetBoolean().Should().BeTrue();
    }

    [Theory]
    [InlineData("{ \"version\": 1, \"hotkeys\": { \"Undo\": \"Alt+Z\" } }")]
    [InlineData("{ \"version\": 1, \"drawing\": { } }")]
    [InlineData("{ \"version\": 1, \"drawing\": { \"inkPairs\": \"yes\" } }")]
    [InlineData("{ \"version\": 1, \"drawing\": { \"inkPairs\": 1 } }")]
    [InlineData("{ \"version\": 1, \"drawing\": true }")]
    public void A_missing_or_invalid_value_starts_off(string json)
    {
        WriteSettings(json);

        CreateStore().Load().InkPairs.Should().BeFalse();
    }

    [Fact]
    public void Round_trip_keeps_free_resize_next_to_ink_pairs()
    {
        CreateStore().Save(new DrawingPreferences(InkPairs: true, FreeResize: true));

        CreateStore().Load().Should().Be(new DrawingPreferences(InkPairs: true, FreeResize: true));
    }

    [Fact]
    public void A_file_from_before_free_resize_keeps_ink_pairs_and_starts_even()
    {
        WriteSettings("{ \"version\": 1, \"drawing\": { \"inkPairs\": true } }");

        CreateStore().Load().Should().Be(new DrawingPreferences(InkPairs: true, FreeResize: false));
    }

    [Fact]
    public void An_unreadable_file_starts_off_and_is_left_for_the_hotkey_store()
    {
        WriteSettings("{ not json");

        CreateStore().Load().InkPairs.Should().BeFalse();

        File.Exists(SettingsPath).Should().BeTrue();
    }

    [Fact]
    public void Saving_the_hotkeys_keeps_the_drawing_section()
    {
        CreateStore().Save(new DrawingPreferences(InkPairs: true));

        CreateHotkeyStore().Save(HotkeyMap.Defaults.With(HotkeyAction.Undo, AltZ));

        CreateStore().Load().InkPairs.Should().BeTrue();
        CreateHotkeyStore().Load().Map.ChordOf(HotkeyAction.Undo).Should().Be(AltZ);
    }

    [Fact]
    public void Saving_the_drawing_section_keeps_the_hotkeys()
    {
        CreateHotkeyStore().Save(HotkeyMap.Defaults.With(HotkeyAction.Undo, AltZ));

        CreateStore().Save(new DrawingPreferences(InkPairs: true));

        CreateHotkeyStore().Load().Map.ChordOf(HotkeyAction.Undo).Should().Be(AltZ);
        CreateStore().Load().InkPairs.Should().BeTrue();
    }

    [Fact]
    public void A_file_with_only_hotkeys_keeps_working()
    {
        WriteSettings("""
            {
              "version": 1,
              "hotkeys": {
                "Undo": "Alt+Z"
              }
            }
            """);

        CreateStore().Load().InkPairs.Should().BeFalse();
        CreateHotkeyStore().Load().Map.ChordOf(HotkeyAction.Undo).Should().Be(AltZ);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
