using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class GradientPairTests
{
    [Fact]
    public void A_neon_pair_carries_only_its_neon_colors()
    {
        var pair = GradientPair.FromNeon(TintaGradiente.PinkRed);

        pair.Neon.Should().Be(TintaGradiente.PinkRed);
        pair.Ink.Should().BeNull();
    }

    [Fact]
    public void An_ink_pair_carries_only_its_ink()
    {
        var pair = GradientPair.FromInk(TintaColor.Yellow);

        pair.Ink.Should().Be(TintaColor.Yellow);
        pair.Neon.Should().BeNull();
    }

    [Fact]
    public void Equal_pairs_are_the_same_value()
    {
        GradientPair.FromInk(TintaColor.Red).Should().Be(GradientPair.FromInk(TintaColor.Red));
        GradientPair.FromNeon(TintaGradiente.BlueCyan).Should().Be(GradientPair.FromNeon(TintaGradiente.BlueCyan));
    }

    [Fact]
    public void Different_pairs_are_different_values()
    {
        GradientPair.FromInk(TintaColor.Red).Should().NotBe(GradientPair.FromInk(TintaColor.Blue));
        GradientPair.FromNeon(TintaGradiente.BlueCyan).Should().NotBe(GradientPair.FromInk(TintaColor.Blue));
    }
}
