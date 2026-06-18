using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class LaserFadeTests
{
    [Fact]
    public void OpacityForAge_is_fully_opaque_when_just_created()
    {
        LaserFade.OpacityForAge(ageMs: 0d, lifetimeMs: 500d).Should().Be(1d);
    }

    [Fact]
    public void OpacityForAge_is_fully_transparent_at_the_end_of_life()
    {
        LaserFade.OpacityForAge(ageMs: 500d, lifetimeMs: 500d).Should().Be(0d);
    }

    [Fact]
    public void OpacityForAge_is_half_way_at_half_life()
    {
        LaserFade.OpacityForAge(ageMs: 250d, lifetimeMs: 500d).Should().BeApproximately(0.5d, 1e-9);
    }

    [Fact]
    public void OpacityForAge_clamps_below_zero_for_ages_past_the_lifetime()
    {
        LaserFade.OpacityForAge(ageMs: 900d, lifetimeMs: 500d).Should().Be(0d);
    }

    [Fact]
    public void OpacityForAge_clamps_above_one_for_negative_ages()
    {
        LaserFade.OpacityForAge(ageMs: -100d, lifetimeMs: 500d).Should().Be(1d);
    }

    [Fact]
    public void OpacityForAge_collapses_to_zero_when_lifetime_is_not_positive()
    {
        LaserFade.OpacityForAge(ageMs: 0d, lifetimeMs: 0d).Should().Be(0d);
        LaserFade.OpacityForAge(ageMs: 0d, lifetimeMs: -10d).Should().Be(0d);
    }

    [Theory]
    [InlineData(0d, 125d)]
    [InlineData(125d, 250d)]
    [InlineData(250d, 375d)]
    [InlineData(375d, 500d)]
    public void OpacityForAge_decreases_monotonically_with_age(double youngerMs, double olderMs)
    {
        var younger = LaserFade.OpacityForAge(youngerMs, lifetimeMs: 500d);
        var older = LaserFade.OpacityForAge(olderMs, lifetimeMs: 500d);

        younger.Should().BeGreaterThan(older);
    }
}
