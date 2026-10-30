using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Application.Tests;

public sealed class HotkeyMapTests
{
    private static readonly KeyChord AltZ = new(HotkeyModifiers.Alt, HotkeyKey.Z);

    private static KeyChord DefaultOf(HotkeyAction action) => HotkeyCatalog.Of(action).Default;

    [Fact]
    public void Defaults_uses_the_catalog_chords_and_has_no_overrides()
    {
        var map = HotkeyMap.Defaults;

        map.Overrides.Should().BeEmpty();
        map.ChordOf(HotkeyAction.Undo).Should().Be(DefaultOf(HotkeyAction.Undo));
        map.IsDefault(HotkeyAction.Undo).Should().BeTrue();
    }

    [Fact]
    public void With_assigns_the_chord_without_changing_the_original()
    {
        var map = HotkeyMap.Defaults.With(HotkeyAction.Undo, AltZ);

        map.ChordOf(HotkeyAction.Undo).Should().Be(AltZ);
        map.IsDefault(HotkeyAction.Undo).Should().BeFalse();
        HotkeyMap.Defaults.ChordOf(HotkeyAction.Undo).Should().Be(DefaultOf(HotkeyAction.Undo));
    }

    [Fact]
    public void A_default_already_given_by_the_user_to_another_action_yields_to_it()
    {
        var ctrlAltH = DefaultOf(HotkeyAction.ToggleToolbarCollapsed);

        var map = HotkeyMap.Defaults.With(HotkeyAction.ColorRed, ctrlAltH);

        map.ChordOf(HotkeyAction.ColorRed).Should().Be(ctrlAltH);
        map.ChordOf(HotkeyAction.ToggleToolbarCollapsed).Should().BeNull();
        map.OwnerOf(ctrlAltH).Should().Be(HotkeyAction.ColorRed);
    }

    [Fact]
    public void With_null_leaves_the_action_without_chord()
    {
        var map = HotkeyMap.Defaults.With(HotkeyAction.Capture, null);

        map.ChordOf(HotkeyAction.Capture).Should().BeNull();
        map.IsDefault(HotkeyAction.Capture).Should().BeFalse();
        map.Overrides.Should().ContainKey(HotkeyAction.Capture).WhoseValue.Should().BeNull();
    }

    [Fact]
    public void With_the_default_chord_removes_the_override()
    {
        var map = HotkeyMap.Defaults
            .With(HotkeyAction.Undo, AltZ)
            .With(HotkeyAction.Undo, DefaultOf(HotkeyAction.Undo));

        map.IsDefault(HotkeyAction.Undo).Should().BeTrue();
        map.Overrides.Should().BeEmpty();
    }

    [Fact]
    public void Reset_returns_the_action_to_its_default()
    {
        var map = HotkeyMap.Defaults.With(HotkeyAction.Undo, null).Reset(HotkeyAction.Undo);

        map.ChordOf(HotkeyAction.Undo).Should().Be(DefaultOf(HotkeyAction.Undo));
        map.IsDefault(HotkeyAction.Undo).Should().BeTrue();
    }

    [Fact]
    public void OwnerOf_finds_the_action_using_the_chord()
    {
        var map = HotkeyMap.Defaults.With(HotkeyAction.Undo, AltZ);

        map.OwnerOf(AltZ).Should().Be(HotkeyAction.Undo);
        map.OwnerOf(DefaultOf(HotkeyAction.Toggle)).Should().Be(HotkeyAction.Toggle);
    }

    [Fact]
    public void OwnerOf_returns_null_for_a_free_chord_or_a_removed_one()
    {
        var undoDefault = DefaultOf(HotkeyAction.Undo);
        var map = HotkeyMap.Defaults.With(HotkeyAction.Undo, null);

        map.OwnerOf(new KeyChord(HotkeyModifiers.Win, HotkeyKey.F9)).Should().BeNull();
        map.OwnerOf(undoDefault).Should().BeNull();
    }

    [Fact]
    public void Overrides_contains_only_the_differences()
    {
        var map = HotkeyMap.Defaults
            .With(HotkeyAction.Undo, AltZ)
            .With(HotkeyAction.Capture, null)
            .With(HotkeyAction.Clear, DefaultOf(HotkeyAction.Clear));

        map.Overrides.Should().BeEquivalentTo(new Dictionary<HotkeyAction, KeyChord?>
        {
            [HotkeyAction.Undo] = AltZ,
            [HotkeyAction.Capture] = null,
        });
    }
}
