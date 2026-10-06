using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class ShapeBuilderTests
{
    private const double Thickness = 3d;
    private const double Tolerance = 1e-6;

    [Fact]
    public void Line_is_a_two_point_segment()
    {
        var start = new StrokePoint(1, 2);
        var end = new StrokePoint(10, 20);

        var points = ShapeBuilder.Build(ToolKind.Line, start, end, Thickness);

        points.Should().Equal(start, end);
    }

    [Fact]
    public void Line_ignores_the_thickness()
    {
        var start = new StrokePoint(1, 2);
        var end = new StrokePoint(10, 20);

        var points = ShapeBuilder.Build(ToolKind.Line, start, end, 18d);

        points.Should().Equal(start, end);
    }

    [Fact]
    public void Rectangle_is_closed()
    {
        var points = ShapeBuilder.Build(ToolKind.Rectangle, new StrokePoint(0, 0), new StrokePoint(200, 100), Thickness);

        points[0].Should().Be(points[^1]);
    }

    [Fact]
    public void Rectangle_approximates_each_corner_with_six_segments()
    {
        var points = ShapeBuilder.Build(ToolKind.Rectangle, new StrokePoint(0, 0), new StrokePoint(200, 100), Thickness);

        // 4 esquinas x 7 puntos (6 segmentos) + el punto de cierre.
        points.Should().HaveCount((4 * 7) + 1);
    }

    [Fact]
    public void Rectangle_has_no_sharp_corners()
    {
        var points = ShapeBuilder.Build(ToolKind.Rectangle, new StrokePoint(0, 0), new StrokePoint(200, 100), Thickness);

        points.Should().NotContain(new StrokePoint(0, 0));
        points.Should().NotContain(new StrokePoint(200, 0));
        points.Should().NotContain(new StrokePoint(200, 100));
        points.Should().NotContain(new StrokePoint(0, 100));
    }

    [Fact]
    public void Rectangle_corner_points_lie_on_a_circle_of_the_radius()
    {
        var points = ShapeBuilder.Build(ToolKind.Rectangle, new StrokePoint(0, 0), new StrokePoint(200, 100), Thickness);

        double radius = RadiusOf(points, right: 200d);
        var center = new StrokePoint(200 - radius, radius); // esquina superior derecha
        for (int i = 0; i <= 6; i++)
        {
            double dx = points[i].X - center.X;
            double dy = points[i].Y - center.Y;
            Math.Sqrt((dx * dx) + (dy * dy)).Should().BeApproximately(radius, Tolerance);
        }
    }

    [Fact]
    public void Rectangle_is_normalized_regardless_of_drag_direction()
    {
        // Arrastre de abajo-derecha hacia arriba-izquierda.
        var points = ShapeBuilder.Build(ToolKind.Rectangle, new StrokePoint(200, 100), new StrokePoint(0, 0), Thickness);

        points.Min(p => p.X).Should().BeApproximately(0d, Tolerance);
        points.Max(p => p.X).Should().BeApproximately(200d, Tolerance);
        points.Min(p => p.Y).Should().BeApproximately(0d, Tolerance);
        points.Max(p => p.Y).Should().BeApproximately(100d, Tolerance);
    }

    [Fact]
    public void Rectangle_radius_grows_with_the_thickness()
    {
        var thin = ShapeBuilder.Build(ToolKind.Rectangle, new StrokePoint(0, 0), new StrokePoint(200, 100), 3d);
        var thick = ShapeBuilder.Build(ToolKind.Rectangle, new StrokePoint(0, 0), new StrokePoint(200, 100), 18d);

        RadiusOf(thick, right: 200d).Should().BeGreaterThan(RadiusOf(thin, right: 200d));
    }

    [Theory]
    [InlineData(3d)]
    [InlineData(18d)]
    public void Rectangle_radius_never_exceeds_half_of_the_shorter_side(double thickness)
    {
        // Lado menor 6: el radio no puede pasar de 3.
        var points = ShapeBuilder.Build(ToolKind.Rectangle, new StrokePoint(0, 0), new StrokePoint(10, 6), thickness);

        RadiusOf(points, right: 10d).Should().BeLessThanOrEqualTo(3d + Tolerance);
        points.Min(p => p.X).Should().BeGreaterThanOrEqualTo(0d - Tolerance);
        points.Max(p => p.X).Should().BeLessThanOrEqualTo(10d + Tolerance);
        points.Min(p => p.Y).Should().BeGreaterThanOrEqualTo(0d - Tolerance);
        points.Max(p => p.Y).Should().BeLessThanOrEqualTo(6d + Tolerance);
    }

    [Fact]
    public void Rectangle_without_height_stays_a_closed_flat_shape()
    {
        var points = ShapeBuilder.Build(ToolKind.Rectangle, new StrokePoint(0, 5), new StrokePoint(100, 5), Thickness);

        points.Should().HaveCount(5);
        points[0].Should().Be(points[^1]);
        points.Should().OnlyContain(p => !double.IsNaN(p.X) && !double.IsNaN(p.Y));
    }

    [Fact]
    public void Arrow_is_no_longer_a_shape_and_falls_back_to_a_segment()
    {
        // La flecha pasó a ser un trazo libre (FreehandStrokeBehavior + ArrowHeadBuilder).
        var start = new StrokePoint(0, 0);
        var end = new StrokePoint(100, 0);

        var points = ShapeBuilder.Build(ToolKind.Arrow, start, end, Thickness);

        points.Should().Equal(start, end);
    }

    // El primer punto del rectángulo es donde termina el borde superior (derecha - radio).
    private static double RadiusOf(IReadOnlyList<StrokePoint> points, double right) => right - points[0].X;
}
