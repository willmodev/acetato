using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class KeyChordRulesTests
{
    [Theory]
    [InlineData(HotkeyModifiers.Ctrl, HotkeyKey.Z)]
    [InlineData(HotkeyModifiers.Alt, HotkeyKey.Z)]
    [InlineData(HotkeyModifiers.Win, HotkeyKey.Z)]
    [InlineData(HotkeyModifiers.Ctrl | HotkeyModifiers.Shift, HotkeyKey.A)]
    [InlineData(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, HotkeyKey.Space)]
    [InlineData(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, HotkeyKey.F4)]
    [InlineData(HotkeyModifiers.None, HotkeyKey.F1)]
    [InlineData(HotkeyModifiers.None, HotkeyKey.F8)]
    [InlineData(HotkeyModifiers.None, HotkeyKey.F12)]
    public void Valid_chords_pass(HotkeyModifiers modifiers, HotkeyKey key)
    {
        KeyChordRules.Validate(new KeyChord(modifiers, key)).Should().BeNull();
    }

    [Theory]
    [InlineData(HotkeyKey.Z)]
    [InlineData(HotkeyKey.D1)]
    [InlineData(HotkeyKey.Space)]
    [InlineData(HotkeyKey.NumPad0)]
    public void A_key_without_modifier_fails_unless_it_is_a_function_key(HotkeyKey key)
    {
        KeyChordRules.Validate(new KeyChord(HotkeyModifiers.None, key)).Should().Be(KeyChordError.MissingModifier);
    }

    [Theory]
    [InlineData(HotkeyKey.A)]
    [InlineData(HotkeyKey.F8)]
    public void Shift_alone_is_not_enough(HotkeyKey key)
    {
        KeyChordRules.Validate(new KeyChord(HotkeyModifiers.Shift, key)).Should().Be(KeyChordError.ShiftOnly);
    }

    [Theory]
    [InlineData(HotkeyKey.F4)]
    [InlineData(HotkeyKey.Space)]
    public void Windows_reserved_chords_fail(HotkeyKey key)
    {
        KeyChordRules.Validate(new KeyChord(HotkeyModifiers.Alt, key)).Should().Be(KeyChordError.SystemShortcut);
    }
}
