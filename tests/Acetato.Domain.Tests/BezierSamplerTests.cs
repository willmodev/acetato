using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class BezierSamplerTests
{
    private const double Tolerance = 1e-9;

    private static readonly CubicBezier Arc = new(new(0, 0), new(30, 60), new(90, 60), new(120, 0));

    [Fact]
    public void At_returns_the_end_points_at_0_and_1()
    {
        Arc.At(0d).Should().Be(Arc.P0);
        Arc.At(1d).Should().Be(Arc.P3);
    }

    [Fact]
    public void At_of_a_straight_curve_stays_on_the_line()
    {
        var line = new CubicBezier(new(0, 0), new(10, 10), new(20, 20), new(30, 30));

        var middle = line.At(0.37d);

        middle.Y.Should().BeApproximately(middle.X, Tolerance);
    }

    [Fact]
    public void Sample_starts_and_ends_on_the_chain_end_points()
    {
        var next = new CubicBezier(Arc.P3, new(150, -60), new(180, -60), new(210, 0));

        var points = BezierSampler.Sample([Arc, next], spacing: 2d);

        points[0].Should().Be(Arc.P0);
        points[^1].Should().Be(next.P3);
    }

    [Fact]
    public void Sample_does_not_repeat_the_point_shared_by_two_curves()
    {
        var next = new CubicBezier(Arc.P3, new(150, -60), new(180, -60), new(210, 0));

        var points = BezierSampler.Sample([Arc, next], spacing: 2d);

        points.Count(p => p == Arc.P3).Should().Be(1);
    }

    [Theory]
    [InlineData(2d)]
    [InlineData(5d)]
    public void Sample_keeps_the_points_about_the_requested_spacing_apart(double spacing)
    {
        var points = BezierSampler.Sample([Arc], spacing);

        for (int i = 1; i < points.Count; i++)
        {
            points[i - 1].DistanceTo(points[i]).Should().BeLessThanOrEqualTo(spacing * 1.5d);
        }
    }

    [Fact]
    public void Sample_of_no_curves_is_empty()
    {
        BezierSampler.Sample([], spacing: 2d).Should().BeEmpty();
    }
}
