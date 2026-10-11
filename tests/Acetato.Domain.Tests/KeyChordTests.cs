using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class KeyChordTests
{
    public static TheoryData<HotkeyModifiers> ModifierSets { get; } = new()
    {
        HotkeyModifiers.None,
        HotkeyModifiers.Ctrl,
        HotkeyModifiers.Ctrl | HotkeyModifiers.Alt,
        HotkeyModifiers.Ctrl | HotkeyModifiers.Shift,
        HotkeyModifiers.Ctrl | HotkeyModifiers.Alt | HotkeyModifiers.Shift | HotkeyModifiers.Win,
    };

    [Theory]
    [MemberData(nameof(ModifierSets))]
    public void ToString_and_TryParse_are_inverse_for_every_key(HotkeyModifiers modifiers)
    {
        foreach (var key in Enum.GetValues<HotkeyKey>())
        {
            var chord = new KeyChord(modifiers, key);

            KeyChord.TryParse(chord.ToString(), out var parsed).Should().BeTrue();
            parsed.Should().Be(chord);
        }
    }

    [Fact]
    public void ToString_writes_modifiers_in_canonical_order()
    {
        var chord = new KeyChord(HotkeyModifiers.Win | HotkeyModifiers.Shift | HotkeyModifiers.Ctrl, HotkeyKey.Z);

        chord.ToString().Should().Be("Ctrl+Shift+Win+Z");
    }

    [Fact]
    public void ToString_of_a_lone_key_has_no_separator()
    {
        new KeyChord(HotkeyModifiers.None, HotkeyKey.F8).ToString().Should().Be("F8");
    }

    [Fact]
    public void TryParse_ignores_case_and_spaces()
    {
        KeyChord.TryParse(" ctrl + alt + back ", out var chord).Should().BeTrue();

        chord.Should().Be(new KeyChord(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, HotkeyKey.Back));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ctrl+")]
    [InlineData("Ctrl")]
    [InlineData("Ctrl+Foo")]
    [InlineData("Ctrl+5")]
    [InlineData("Ctrl+Oem1")]
    [InlineData("Ctrl+Ctrl+Z")]
    [InlineData("Ctrl+Alt+ctrl+Z")]
    [InlineData("Z+Ctrl")]
    [InlineData("Hyper+Z")]
    [InlineData("Ctrl++Z")]
    public void TryParse_rejects_invalid_text(string? text)
    {
        KeyChord.TryParse(text, out var chord).Should().BeFalse();
        chord.Should().Be(default(KeyChord));
    }
}
