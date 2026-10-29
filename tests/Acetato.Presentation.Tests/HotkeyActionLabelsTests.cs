using Acetato.Application.Abstractions;
using Acetato.Presentation.Settings;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

public sealed class HotkeyActionLabelsTests
{
    [Fact]
    public void Every_action_has_a_non_empty_label()
    {
        Enum.GetValues<HotkeyAction>().Should().AllSatisfy(action =>
            HotkeyActionLabels.Of(action).Should().NotBeNullOrWhiteSpace());
    }

    [Theory]
    [InlineData(HotkeyAction.SelectPencilTool, "Lápiz")]
    [InlineData(HotkeyAction.SelectEraserTool, "Borrador")]
    [InlineData(HotkeyAction.SelectLineTool, "Línea")]
    [InlineData(HotkeyAction.SelectArrowTool, "Flecha")]
    [InlineData(HotkeyAction.SelectRectangleTool, "Rectángulo")]
    [InlineData(HotkeyAction.ThicknessUp, "Más grueso")]
    [InlineData(HotkeyAction.ThicknessDown, "Más fino")]
    public void The_actions_of_HU_22_have_their_spanish_label(HotkeyAction action, string expected)
    {
        HotkeyActionLabels.Of(action).Should().Be(expected);
    }

    [Fact]
    public void Capture_is_labelled_as_screen_clip()
    {
        HotkeyActionLabels.Of(HotkeyAction.Capture).Should().Be("Recortar");
    }
}
