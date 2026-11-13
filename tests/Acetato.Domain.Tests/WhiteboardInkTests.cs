using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class WhiteboardInkTests
{
    [Fact]
    public void White_ink_on_the_white_board_becomes_black()
    {
        WhiteboardInk.Contrast(TintaColor.White, WhiteboardMode.White).Should().Be(TintaColor.Black);
    }

    [Fact]
    public void Black_ink_on_the_black_board_becomes_white()
    {
        WhiteboardInk.Contrast(TintaColor.Black, WhiteboardMode.Black).Should().Be(TintaColor.White);
    }

    [Theory]
    [InlineData(TintaColor.Black, WhiteboardMode.White)]
    [InlineData(TintaColor.White, WhiteboardMode.Black)]
    public void An_ink_opposite_to_the_background_is_kept(TintaColor ink, WhiteboardMode mode)
    {
        WhiteboardInk.Contrast(ink, mode).Should().BeNull();
    }

    [Theory]
    [InlineData(TintaColor.Red)]
    [InlineData(TintaColor.Blue)]
    [InlineData(TintaColor.Yellow)]
    [InlineData(TintaColor.Green)]
    public void Colored_inks_are_never_changed(TintaColor ink)
    {
        WhiteboardInk.Contrast(ink, WhiteboardMode.White).Should().BeNull();
        WhiteboardInk.Contrast(ink, WhiteboardMode.Black).Should().BeNull();
    }

    [Theory]
    [InlineData(TintaColor.White)]
    [InlineData(TintaColor.Black)]
    public void With_the_board_off_nothing_changes(TintaColor ink)
    {
        WhiteboardInk.Contrast(ink, WhiteboardMode.Off).Should().BeNull();
    }
}
