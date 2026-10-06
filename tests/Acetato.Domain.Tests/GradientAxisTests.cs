using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class GradientAxisTests
{
    private const double Thickness = 6d;

    [Fact]
    public void For_an_open_path_runs_from_the_first_to_the_last_point()
    {
        var path = new List<StrokePoint> { new(10, 20), new(80, 5), new(200, 90) };

        var axis = GradientAxis.For(path, Thickness);

        axis.Start.Should().Be(new StrokePoint(10, 20));
        axis.End.Should().Be(new StrokePoint(200, 90));
    }

    [Fact]
    public void For_a_closed_rectangle_runs_along_the_diagonal_of_its_box()
    {
        var path = new List<StrokePoint> { new(40, 30), new(240, 30), new(240, 130), new(40, 130), new(40, 30) };

        var axis = GradientAxis.For(path, Thickness);

        axis.Start.Should().Be(new StrokePoint(40, 30));
        axis.End.Should().Be(new StrokePoint(240, 130));
    }

    [Fact]
    public void For_a_closed_path_starting_mid_edge_still_uses_the_box_corners()
    {
        // Rectángulo redondeado: arranca donde acaba el borde superior, no en una esquina.
        var path = new List<StrokePoint> { new(232, 30), new(240, 38), new(240, 122), new(48, 130), new(40, 122), new(40, 38), new(48, 30), new(232, 30) };

        var axis = GradientAxis.For(path, Thickness);

        axis.Start.Should().Be(new StrokePoint(40, 30));
        axis.End.Should().Be(new StrokePoint(240, 130));
    }

    [Fact]
    public void For_treats_start_and_end_closer_than_twice_the_thickness_as_closed()
    {
        // Distancia inicio-fin 11 < 12 (= 2 x 6): figura cerrada.
        var path = new List<StrokePoint> { new(0, 0), new(50, 10), new(11, 0) };

        var axis = GradientAxis.For(path, Thickness);

        axis.Start.Should().Be(new StrokePoint(0, 0));
        axis.End.Should().Be(new StrokePoint(50, 10));
    }

    [Fact]
    public void For_treats_start_and_end_at_twice_the_thickness_as_open()
    {
        // Distancia inicio-fin 12 = 2 x 6: ya es un trazo abierto.
        var path = new List<StrokePoint> { new(0, 0), new(50, 10), new(12, 0) };

        var axis = GradientAxis.For(path, Thickness);

        axis.Start.Should().Be(new StrokePoint(0, 0));
        axis.End.Should().Be(new StrokePoint(12, 0));
    }

    [Fact]
    public void For_a_single_point_collapses_to_that_point()
    {
        var axis = GradientAxis.For([new StrokePoint(7, 9)], Thickness);

        axis.Start.Should().Be(new StrokePoint(7, 9));
        axis.End.Should().Be(new StrokePoint(7, 9));
    }

    [Fact]
    public void For_an_empty_path_returns_the_default_axis()
    {
        GradientAxis.For([], Thickness).Should().Be(default(GradientAxis));
    }

    [Fact]
    public void For_throws_when_the_path_is_null()
    {
        var act = () => GradientAxis.For(null!, Thickness);

        act.Should().Throw<ArgumentNullException>();
    }
}
