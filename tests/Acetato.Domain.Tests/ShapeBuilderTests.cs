using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class ShapeBuilderTests
{
    [Fact]
    public void Line_is_a_two_point_segment()
    {
        var start = new StrokePoint(1, 2);
        var end = new StrokePoint(10, 20);

        var points = ShapeBuilder.Build(ToolKind.Line, start, end);

        points.Should().Equal(start, end);
    }

    [Fact]
    public void Rectangle_is_closed_with_five_points()
    {
        var points = ShapeBuilder.Build(ToolKind.Rectangle, new StrokePoint(0, 0), new StrokePoint(10, 4));

        points.Should().HaveCount(5);
        points[0].Should().Be(points[4]); // cerrado
    }

    [Fact]
    public void Rectangle_is_normalized_regardless_of_drag_direction()
    {
        // Arrastre de abajo-derecha hacia arriba-izquierda.
        var points = ShapeBuilder.Build(ToolKind.Rectangle, new StrokePoint(10, 4), new StrokePoint(0, 0));

        points.Should().Contain(new StrokePoint(0, 0));   // esquina superior-izquierda
        points.Should().Contain(new StrokePoint(10, 4));  // esquina inferior-derecha
    }

    [Fact]
    public void Arrow_keeps_endpoints_and_has_five_points()
    {
        var start = new StrokePoint(0, 0);
        var end = new StrokePoint(100, 0);

        var points = ShapeBuilder.Build(ToolKind.Arrow, start, end);

        points.Should().HaveCount(5);
        points[0].Should().Be(start);
        points[1].Should().Be(end);
        points[3].Should().Be(end); // retraza la punta
    }

    [Fact]
    public void Arrow_with_zero_length_falls_back_to_a_segment()
    {
        var p = new StrokePoint(5, 5);

        var points = ShapeBuilder.Build(ToolKind.Arrow, p, p);

        points.Should().Equal(p, p);
    }
}
