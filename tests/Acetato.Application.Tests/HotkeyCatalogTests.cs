using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Application.Tests;

public sealed class HotkeyCatalogTests
{
    [Fact]
    public void Every_action_has_exactly_one_definition()
    {
        HotkeyCatalog.All.Select(definition => definition.Action)
            .Should().BeEquivalentTo(Enum.GetValues<HotkeyAction>())
            .And.OnlyHaveUniqueItems();
    }

    [Fact]
    public void No_default_chord_is_repeated()
    {
        HotkeyCatalog.All.Select(definition => definition.Default).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Every_default_passes_the_chord_rules()
    {
        HotkeyCatalog.All.Should().AllSatisfy(definition =>
            KeyChordRules.Validate(definition.Default).Should().BeNull());
    }

    [Fact]
    public void Show_and_drawing_mode_are_always_active_and_the_rest_only_while_drawing()
    {
        HotkeyCatalog.All.Where(definition => definition.Scope == HotkeyScope.Always)
            .Select(definition => definition.Action)
            .Should().Equal(HotkeyAction.Toggle, HotkeyAction.DrawingModeToggle);
    }

    [Fact]
    public void Defaults_are_the_short_tables_of_HU_21_and_HU_22()
    {
        HotkeyCatalog.All.Select(definition => (definition.Action, definition.Default.ToString()))
            .Should().Equal(
                (HotkeyAction.Toggle, "Ctrl+Alt+D"),
                (HotkeyAction.DrawingModeToggle, "Ctrl+Alt+E"),
                (HotkeyAction.ColorRed, "Ctrl+D1"),
                (HotkeyAction.ColorBlue, "Ctrl+D2"),
                (HotkeyAction.ColorYellow, "Ctrl+D3"),
                (HotkeyAction.ColorGreen, "Ctrl+D4"),
                (HotkeyAction.ColorWhite, "Ctrl+D5"),
                (HotkeyAction.ColorBlack, "Ctrl+D6"),
                (HotkeyAction.ColorGradient, "Ctrl+D7"),
                (HotkeyAction.Undo, "Ctrl+Z"),
                (HotkeyAction.Clear, "Ctrl+Back"),
                (HotkeyAction.CycleTool, "Ctrl+Space"),
                (HotkeyAction.SelectPencilTool, "Ctrl+P"),
                (HotkeyAction.SelectLaserTool, "Ctrl+L"),
                (HotkeyAction.SelectEraserTool, "Ctrl+B"),
                (HotkeyAction.SelectLineTool, "Ctrl+I"),
                (HotkeyAction.SelectArrowTool, "Ctrl+F"),
                (HotkeyAction.SelectRectangleTool, "Ctrl+R"),
                (HotkeyAction.SelectTextTool, "Ctrl+T"),
                (HotkeyAction.ThicknessUp, "Ctrl+PageUp"),
                (HotkeyAction.ThicknessDown, "Ctrl+PageDown"),
                (HotkeyAction.Capture, "Ctrl+S"));
    }

    [Theory]
    [InlineData(HotkeyAction.SelectPencilTool)]
    [InlineData(HotkeyAction.SelectEraserTool)]
    [InlineData(HotkeyAction.SelectLineTool)]
    [InlineData(HotkeyAction.SelectArrowTool)]
    [InlineData(HotkeyAction.SelectRectangleTool)]
    [InlineData(HotkeyAction.ThicknessUp)]
    [InlineData(HotkeyAction.ThicknessDown)]
    public void The_actions_of_HU_22_are_only_active_while_drawing(HotkeyAction action)
    {
        HotkeyCatalog.Of(action).Scope.Should().Be(HotkeyScope.DrawingMode);
    }

    [Fact]
    public void Of_returns_the_definition_of_the_action()
    {
        HotkeyCatalog.Of(HotkeyAction.Undo).Action.Should().Be(HotkeyAction.Undo);
    }
}
