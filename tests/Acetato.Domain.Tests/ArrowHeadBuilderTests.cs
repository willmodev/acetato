using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class ArrowHeadBuilderTests
{
    private const double Thickness = 4d;
    private const double Tolerance = 1e-6;

    [Theory]
    [InlineData(0d)]
    [InlineData(45d)]
    [InlineData(90d)]
    [InlineData(180d)]
    [InlineData(270d)]
    public void TryBuild_points_in_the_direction_of_a_straight_path(double degrees)
    {
        double radians = degrees * Math.PI / 180d;
        var end = new StrokePoint(50 + (100 * Math.Cos(radians)), 50 + (100 * Math.Sin(radians)));
        var path = new List<StrokePoint> { new(50, 50), end };

        ArrowHeadBuilder.TryBuild(path, Thickness, out var head).Should().BeTrue();

        var (ux, uy) = DirectionOf(head);
        ux.Should().BeApproximately(Math.Cos(radians), Tolerance);
        uy.Should().BeApproximately(Math.Sin(radians), Tolerance);
    }

    [Fact]
    public void TryBuild_puts_the_tip_on_the_last_point()
    {
        var path = new List<StrokePoint> { new(0, 0), new(60, 20), new(120, 25) };

        ArrowHeadBuilder.TryBuild(path, Thickness, out var head).Should().BeTrue();

        head.Tip.Should().Be(new StrokePoint(120, 25));
    }

    [Fact]
    public void TryBuild_places_the_wings_symmetrically_behind_the_tip()
    {
        var path = new List<StrokePoint> { new(0, 0), new(100, 0) };

        ArrowHeadBuilder.TryBuild(path, Thickness, out var head).Should().BeTrue();

        head.Left.X.Should().BeApproximately(head.Right.X, Tolerance);
        head.Left.X.Should().BeLessThan(head.Tip.X);
        head.Left.Y.Should().BeApproximately(-head.Right.Y, Tolerance);
    }

    [Fact]
    public void TryBuild_puts_the_left_wing_on_the_left_of_the_travel_as_seen_on_screen()
    {
        var path = new List<StrokePoint> { new(0, 0), new(100, 0) };

        ArrowHeadBuilder.TryBuild(path, Thickness, out var head).Should().BeTrue();

        // Eje Y hacia abajo: avanzando a la derecha, la izquierda queda arriba.
        head.Left.Y.Should().BeLessThan(head.Tip.Y);
        head.Right.Y.Should().BeGreaterThan(head.Tip.Y);
    }

    [Fact]
    public void TryBuild_points_by_the_final_stretch_of_a_curved_path()
    {
        // Larga carrera horizontal que termina girando hacia abajo.
        var path = new List<StrokePoint> { new(0, 0), new(200, 0), new(200, 100) };

        ArrowHeadBuilder.TryBuild(path, Thickness, out var head).Should().BeTrue();

        var (ux, uy) = DirectionOf(head);
        ux.Should().BeApproximately(0d, Tolerance);
        uy.Should().BeApproximately(1d, Tolerance);
    }

    [Fact]
    public void TryBuild_ignores_a_slight_tremor_at_the_very_end()
    {
        var path = new List<StrokePoint> { new(0, 0), new(100, 0), new(102, -2) };

        ArrowHeadBuilder.TryBuild(path, Thickness, out var head).Should().BeTrue();

        var (ux, _) = DirectionOf(head);
        ux.Should().BeGreaterThan(0.99d);
    }

    [Fact]
    public void TryBuild_interpolates_when_the_last_points_are_far_apart()
    {
        // Un solo segmento largo (ratón rápido): la dirección sigue siendo la del trazo.
        var path = new List<StrokePoint> { new(0, 0), new(300, 300) };

        ArrowHeadBuilder.TryBuild(path, Thickness, out var head).Should().BeTrue();

        var (ux, uy) = DirectionOf(head);
        ux.Should().BeApproximately(Math.Sqrt(0.5d), Tolerance);
        uy.Should().BeApproximately(Math.Sqrt(0.5d), Tolerance);
    }

    [Fact]
    public void TryBuild_falls_back_to_the_last_segment_when_the_final_stretch_closes_on_itself()
    {
        // Dos vueltas a un cuadrado de 6 DIP: el tramo final (24 DIP) cierra sobre sí mismo.
        var path = new List<StrokePoint> { new(0, 0), new(6, 0), new(6, 6), new(0, 6), new(0, 0), new(6, 0), new(6, 6), new(0, 6), new(0, 0) };

        ArrowHeadBuilder.TryBuild(path, Thickness, out var head).Should().BeTrue();

        Reach(head).Should().BeApproximately(16d, Tolerance);
        double.IsNaN(head.Left.X).Should().BeFalse();
        double.IsNaN(head.Right.Y).Should().BeFalse();
    }

    [Fact]
    public void TryBuild_discards_a_path_shorter_than_the_head()
    {
        var path = new List<StrokePoint> { new(0, 0), new(10, 0) };

        ArrowHeadBuilder.TryBuild(path, Thickness, out _).Should().BeFalse();
    }

    [Fact]
    public void TryBuild_discards_a_single_click()
    {
        ArrowHeadBuilder.TryBuild([], Thickness, out _).Should().BeFalse();
        ArrowHeadBuilder.TryBuild([new StrokePoint(5, 5)], Thickness, out _).Should().BeFalse();
        ArrowHeadBuilder.TryBuild([new StrokePoint(5, 5), new StrokePoint(5, 5)], Thickness, out _).Should().BeFalse();
    }

    [Fact]
    public void TryBuild_makes_the_head_bigger_with_a_thicker_stroke()
    {
        var path = new List<StrokePoint> { new(0, 0), new(500, 0) };

        ArrowHeadBuilder.TryBuild(path, 4d, out var thin).Should().BeTrue();
        ArrowHeadBuilder.TryBuild(path, 20d, out var thick).Should().BeTrue();

        Reach(thick).Should().BeGreaterThan(Reach(thin));
        thick.Right.Y.Should().BeGreaterThan(thin.Right.Y);
    }

    [Fact]
    public void TryBuild_raises_the_discard_threshold_with_the_thickness()
    {
        var path = new List<StrokePoint> { new(0, 0), new(30, 0) };

        ArrowHeadBuilder.TryBuild(path, 4d, out _).Should().BeTrue();
        ArrowHeadBuilder.TryBuild(path, 20d, out _).Should().BeFalse();
    }

    [Fact]
    public void TryBuild_throws_when_the_path_is_null()
    {
        var act = () => ArrowHeadBuilder.TryBuild(null!, Thickness, out _);

        act.Should().Throw<ArgumentNullException>();
    }

    // Punto medio de las alas = base de la punta.
    private static StrokePoint Mid(ArrowHead head) =>
        new((head.Left.X + head.Right.X) / 2d, (head.Left.Y + head.Right.Y) / 2d);

    // Cuánto se extiende la punta hacia atrás desde el vértice.
    private static double Reach(ArrowHead head)
    {
        var mid = Mid(head);
        double dx = head.Tip.X - mid.X;
        double dy = head.Tip.Y - mid.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    // Vector unitario base → vértice: hacia donde apunta la flecha.
    private static (double X, double Y) DirectionOf(ArrowHead head)
    {
        var mid = Mid(head);
        double reach = Reach(head);
        return ((head.Tip.X - mid.X) / reach, (head.Tip.Y - mid.Y) / reach);
    }
}
