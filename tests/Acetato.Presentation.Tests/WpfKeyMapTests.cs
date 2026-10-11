using System.Windows.Input;
using Acetato.Domain;
using Acetato.Presentation.Settings;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

public sealed class WpfKeyMapTests
{
    [Theory]
    [InlineData(Key.A, HotkeyKey.A)]
    [InlineData(Key.Z, HotkeyKey.Z)]
    [InlineData(Key.D0, HotkeyKey.D0)]
    [InlineData(Key.D9, HotkeyKey.D9)]
    [InlineData(Key.F1, HotkeyKey.F1)]
    [InlineData(Key.F12, HotkeyKey.F12)]
    [InlineData(Key.NumPad0, HotkeyKey.NumPad0)]
    [InlineData(Key.NumPad9, HotkeyKey.NumPad9)]
    [InlineData(Key.Space, HotkeyKey.Space)]
    [InlineData(Key.Back, HotkeyKey.Back)]
    [InlineData(Key.Delete, HotkeyKey.Delete)]
    [InlineData(Key.PageUp, HotkeyKey.PageUp)]
    [InlineData(Key.PageDown, HotkeyKey.PageDown)]
    [InlineData(Key.Down, HotkeyKey.Down)]
    public void Supported_keys_map_to_their_domain_key(Key key, HotkeyKey expected)
    {
        WpfKeyMap.ToHotkeyKey(key).Should().Be(expected);
    }

    [Fact]
    public void Every_domain_key_has_a_wpf_key()
    {
        var mapped = Enum.GetValues<Key>()
            .Select(WpfKeyMap.ToHotkeyKey)
            .OfType<HotkeyKey>()
            .Distinct();

        mapped.Should().BeEquivalentTo(Enum.GetValues<HotkeyKey>());
    }

    [Theory]
    [InlineData(Key.OemPlus)]
    [InlineData(Key.OemMinus)]
    [InlineData(Key.Oem3)]
    [InlineData(Key.OemOpenBrackets)]
    [InlineData(Key.Enter)]
    [InlineData(Key.Tab)]
    [InlineData(Key.F13)]
    [InlineData(Key.Add)]
    public void Symbol_and_other_keys_are_not_supported(Key key)
    {
        WpfKeyMap.ToHotkeyKey(key).Should().BeNull();
    }

    [Fact]
    public void Modifiers_map_one_to_one()
    {
        WpfKeyMap.ToModifiers(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift | ModifierKeys.Windows)
            .Should().Be(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt | HotkeyModifiers.Shift | HotkeyModifiers.Win);
        WpfKeyMap.ToModifiers(ModifierKeys.None).Should().Be(HotkeyModifiers.None);
    }

    [Theory]
    [InlineData(Key.LeftCtrl, true)]
    [InlineData(Key.RightAlt, true)]
    [InlineData(Key.LeftShift, true)]
    [InlineData(Key.LWin, true)]
    [InlineData(Key.Z, false)]
    public void Lone_modifier_keys_are_detected(Key key, bool expected)
    {
        WpfKeyMap.IsModifier(key).Should().Be(expected);
    }
}
