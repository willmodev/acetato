using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class JitterFilterTests
{
    private const double Smoothing = 0.5d;
    private const double MinDistance = 1d;

    [Fact]
    public void TryAdd_lets_the_first_point_through_unchanged()
    {
        var filter = new JitterFilter(Smoothing, MinDistance);

        filter.TryAdd(new StrokePoint(10, 20), out var filtered).Should().BeTrue();

        filtered.Should().Be(new StrokePoint(10, 20));
    }

    [Fact]
    public void TryAdd_discards_points_closer_than_the_minimum_distance()
    {
        var filter = new JitterFilter(Smoothing, MinDistance);
        filter.TryAdd(new StrokePoint(0, 0), out _);

        filter.TryAdd(new StrokePoint(1, 0), out _).Should().BeFalse(); // filtrado a 0,5 DIP
        filter.TryAdd(new StrokePoint(10, 0), out _).Should().BeTrue();
    }

    [Fact]
    public void TryAdd_attenuates_a_zigzag()
    {
        var filter = new JitterFilter(Smoothing, MinDistance);
        const double amplitude = 6d;
        double maxDeviation = 0d;

        for (int i = 0; i < 40; i++)
        {
            double y = i % 2 == 0 ? amplitude : -amplitude;
            if (filter.TryAdd(new StrokePoint(i * 4d, y), out var filtered) && i > 5)
            {
                maxDeviation = Math.Max(maxDeviation, Math.Abs(filtered.Y));
            }
        }

        maxDeviation.Should().BeLessThan(amplitude);
    }

    [Fact]
    public void TryAdd_keeps_a_straight_line_straight()
    {
        var filter = new JitterFilter(Smoothing, MinDistance);

        for (int i = 0; i < 30; i++)
        {
            filter.TryAdd(new StrokePoint(i * 5d, i * 2.5d), out var filtered);
            filtered.Y.Should().BeApproximately(filtered.X / 2d, 1e-9);
        }
    }

    [Fact]
    public void Reset_starts_a_new_stroke()
    {
        var filter = new JitterFilter(Smoothing, MinDistance);
        filter.TryAdd(new StrokePoint(0, 0), out _);
        filter.Reset();

        filter.TryAdd(new StrokePoint(100, 100), out var filtered).Should().BeTrue();

        filtered.Should().Be(new StrokePoint(100, 100));
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(1.5d)]
    public void Constructor_rejects_a_smoothing_outside_0_and_1(double smoothing)
    {
        var act = () => new JitterFilter(smoothing, MinDistance);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
