using Acetato.Domain;
using Acetato.Presentation.Settings;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

public sealed class KeyChordTextTests
{
    [Theory]
    [InlineData(HotkeyModifiers.Ctrl | HotkeyModifiers.Shift, HotkeyKey.Z, "Ctrl+Mayús+Z")]
    [InlineData(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, HotkeyKey.D, "Ctrl+Alt+D")]
    [InlineData(HotkeyModifiers.Ctrl, HotkeyKey.Back, "Ctrl+Retroceso")]
    [InlineData(HotkeyModifiers.Ctrl, HotkeyKey.Space, "Ctrl+Espacio")]
    [InlineData(HotkeyModifiers.Ctrl, HotkeyKey.D1, "Ctrl+1")]
    [InlineData(HotkeyModifiers.Win, HotkeyKey.NumPad7, "Win+Num 7")]
    [InlineData(HotkeyModifiers.None, HotkeyKey.F8, "F8")]
    [InlineData(HotkeyModifiers.Alt, HotkeyKey.Delete, "Alt+Supr")]
    [InlineData(HotkeyModifiers.Alt, HotkeyKey.Insert, "Alt+Insert")]
    [InlineData(HotkeyModifiers.Alt, HotkeyKey.Home, "Alt+Inicio")]
    [InlineData(HotkeyModifiers.Alt, HotkeyKey.End, "Alt+Fin")]
    [InlineData(HotkeyModifiers.Alt, HotkeyKey.PageUp, "Alt+RePág")]
    [InlineData(HotkeyModifiers.Alt, HotkeyKey.PageDown, "Alt+AvPág")]
    [InlineData(HotkeyModifiers.Alt, HotkeyKey.Left, "Alt+←")]
    [InlineData(HotkeyModifiers.Alt, HotkeyKey.Up, "Alt+↑")]
    [InlineData(HotkeyModifiers.Alt, HotkeyKey.Right, "Alt+→")]
    [InlineData(HotkeyModifiers.Alt, HotkeyKey.Down, "Alt+↓")]
    public void Formats_in_spanish(HotkeyModifiers modifiers, HotkeyKey key, string expected)
    {
        KeyChordText.Format(new KeyChord(modifiers, key)).Should().Be(expected);
    }

    [Fact]
    public void Modifiers_follow_the_order_Ctrl_Alt_Shift_Win()
    {
        var chord = new KeyChord(HotkeyModifiers.Win | HotkeyModifiers.Shift | HotkeyModifiers.Alt | HotkeyModifiers.Ctrl, HotkeyKey.A);

        KeyChordText.Format(chord).Should().Be("Ctrl+Alt+Mayús+Win+A");
    }

    [Fact]
    public void Every_key_has_a_non_empty_name()
    {
        Enum.GetValues<HotkeyKey>().Should().AllSatisfy(key => KeyChordText.KeyName(key).Should().NotBeNullOrWhiteSpace());
    }
}
