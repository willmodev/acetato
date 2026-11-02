using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

/// <summary>
/// Nuevo recuadro al jalar un tirador de la selección (HU-28), en modo parejo y libre.
/// </summary>
public sealed class SelectionResizeTests
{
    private static readonly SelectionBox Box = new(100, 100, 200, 100);

    [Fact]
    public void Even_corner_keeps_the_proportion_and_the_opposite_corner()
    {
        var result = SelectionResize.Resize(Box, ResizeHandle.BottomRight, 200, 10, free: false);

        result.Should().Be(new SelectionBox(100, 100, 400, 200));
    }

    [Fact]
    public void Even_top_left_corner_anchors_the_bottom_right()
    {
        var result = SelectionResize.Resize(Box, ResizeHandle.TopLeft, -200, -100, free: false);

        result.Should().Be(new SelectionBox(-100, 0, 400, 200));
    }

    [Fact]
    public void Even_side_grows_both_axes_around_the_opposite_side_center()
    {
        var result = SelectionResize.Resize(Box, ResizeHandle.Right, 200, 0, free: false);

        result.Should().Be(new SelectionBox(100, 50, 400, 200));
    }

    [Fact]
    public void Free_side_stretches_only_its_axis()
    {
        var result = SelectionResize.Resize(Box, ResizeHandle.Right, 200, 50, free: true);

        result.Should().Be(new SelectionBox(100, 100, 400, 100));
    }

    [Fact]
    public void Free_corner_stretches_each_axis_on_its_own()
    {
        var result = SelectionResize.Resize(Box, ResizeHandle.TopLeft, -50, 20, free: true);

        result.Should().Be(new SelectionBox(50, 120, 250, 80));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Crossing_the_opposite_side_does_not_flip(bool free)
    {
        var result = SelectionResize.Resize(Box, ResizeHandle.BottomRight, -500, -500, free);

        result.Width.Should().BeGreaterThan(0);
        result.Height.Should().BeGreaterThan(0);
        result.X.Should().Be(100);
        result.Y.Should().Be(100);
    }

    [Fact]
    public void Free_side_does_not_go_below_the_minimum()
    {
        var result = SelectionResize.Resize(Box, ResizeHandle.Left, 500, 0, free: true);

        result.Width.Should().Be(SelectionResize.MinSide);
        result.X.Should().Be(300 - SelectionResize.MinSide);
    }

    [Fact]
    public void Even_long_side_does_not_go_below_the_minimum()
    {
        var result = SelectionResize.Resize(Box, ResizeHandle.BottomRight, -1000, -1000, free: false);

        result.Width.Should().BeApproximately(SelectionResize.MinSide, 1e-9);
        result.Height.Should().BeApproximately(SelectionResize.MinSide / 2, 1e-9);
    }

    [Fact]
    public void A_side_already_shorter_than_the_minimum_does_not_shrink_more()
    {
        var line = new SelectionBox(0, 0, 300, 3);

        var result = SelectionResize.Resize(line, ResizeHandle.Bottom, 0, -10, free: true);

        result.Height.Should().Be(3);
    }

    [Fact]
    public void A_short_side_can_still_grow()
    {
        var line = new SelectionBox(0, 0, 300, 3);

        var result = SelectionResize.Resize(line, ResizeHandle.Bottom, 0, 50, free: true);

        result.Height.Should().Be(53);
    }
}
