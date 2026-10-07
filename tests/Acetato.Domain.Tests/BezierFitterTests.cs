using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class BezierFitterTests
{
    private const double Tolerance = 6d;

    [Fact]
    public void Fit_keeps_the_first_and_last_points()
    {
        var path = WavyPath();

        var curves = BezierFitter.Fit(path, Tolerance);

        curves[0].P0.Should().Be(path[0]);
        curves[^1].P3.Should().Be(path[^1]);
    }

    [Fact]
    public void Fit_keeps_every_point_within_the_tolerance()
    {
        var path = WavyPath();

        var curve = BezierSampler.Sample(BezierFitter.Fit(path, Tolerance), spacing: 0.5d);

        foreach (var point in path)
        {
            DistanceToPolyline(point, curve).Should().BeLessThanOrEqualTo(Tolerance + 0.5d);
        }
    }

    [Fact]
    public void Fit_turns_a_noisy_straight_line_into_one_almost_straight_curve()
    {
        var path = new List<StrokePoint>();
        for (int i = 0; i <= 100; i++)
        {
            path.Add(new StrokePoint(i * 3d, i % 2 == 0 ? 1d : -1d)); // temblor de ±1 DIP
        }

        var curves = BezierFitter.Fit(path, Tolerance);

        curves.Should().ContainSingle();
        BezierSampler.Sample(curves, spacing: 2d).Should().OnlyContain(p => Math.Abs(p.Y) <= 2d);
    }

    [Fact]
    public void Fit_keeps_the_corner_of_an_L()
    {
        var path = new List<StrokePoint>();
        for (int i = 0; i <= 50; i++)
        {
            path.Add(new StrokePoint(0d, i * 3d)); // baja
        }

        for (int i = 1; i <= 50; i++)
        {
            path.Add(new StrokePoint(i * 3d, 150d)); // y gira a la derecha
        }

        var curves = BezierFitter.Fit(path, Tolerance);

        curves.Count.Should().BeGreaterThanOrEqualTo(2);
        var corner = new StrokePoint(0d, 150d);
        DistanceToPolyline(corner, BezierSampler.Sample(curves, spacing: 0.5d)).Should().BeLessThanOrEqualTo(Tolerance);
    }

    [Fact]
    public void Fit_of_an_empty_or_single_point_path_is_empty()
    {
        BezierFitter.Fit([], Tolerance).Should().BeEmpty();
        BezierFitter.Fit([new StrokePoint(5, 5)], Tolerance).Should().BeEmpty();
        BezierFitter.Fit([new StrokePoint(5, 5), new StrokePoint(5, 5)], Tolerance).Should().BeEmpty();
    }

    [Fact]
    public void Fit_of_two_points_is_one_straight_curve()
    {
        var curves = BezierFitter.Fit([new StrokePoint(0, 0), new StrokePoint(30, 0)], Tolerance);

        curves.Should().ContainSingle();
        BezierSampler.Sample(curves, spacing: 2d).Should().OnlyContain(p => Math.Abs(p.Y) < 1e-9);
    }

    [Fact]
    public void Smooth_returns_short_paths_unchanged()
    {
        IReadOnlyList<StrokePoint> path = [new StrokePoint(0, 0), new StrokePoint(10, 10)];

        ArrowPathSmoother.Smooth(path, thickness: 4d).Should().BeSameAs(path);
    }

    [Fact]
    public void Smooth_tolerance_grows_with_the_thickness()
    {
        ArrowPathSmoother.ToleranceFor(12d).Should().BeGreaterThan(ArrowPathSmoother.ToleranceFor(2d));
    }

    // Arco con temblor: media onda de 200 DIP de ancho y ±2 DIP de ruido alterno.
    private static List<StrokePoint> WavyPath()
    {
        var path = new List<StrokePoint>();
        for (int i = 0; i <= 100; i++)
        {
            double x = i * 2d;
            double noise = i % 2 == 0 ? 2d : -2d;
            path.Add(new StrokePoint(x, (60d * Math.Sin(Math.PI * x / 200d)) + noise));
        }

        return path;
    }

    private static double DistanceToPolyline(StrokePoint point, IReadOnlyList<StrokePoint> polyline) =>
        polyline.Min(p => p.DistanceTo(point));
}
