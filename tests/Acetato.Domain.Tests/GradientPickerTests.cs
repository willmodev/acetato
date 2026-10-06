using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class GradientPickerTests
{
    private static readonly int[] OneOfEachIndex = [0, 1, 2, 3, 4];

    [Theory]
    [InlineData(TintaGradiente.BlueCyan)]
    [InlineData(TintaGradiente.YellowLime)]
    [InlineData(TintaGradiente.PinkRed)]
    [InlineData(TintaGradiente.VioletPink)]
    [InlineData(TintaGradiente.GreenCyan)]
    [InlineData(TintaGradiente.OrangePink)]
    public void Next_never_returns_the_current_pair(TintaGradiente current)
    {
        var random = new SequenceRandomSource(OneOfEachIndex);

        foreach (var _ in OneOfEachIndex)
        {
            GradientPicker.Next(current, random).Should().NotBe(current);
        }
    }

    [Fact]
    public void Next_reaches_all_six_pairs_in_100_draws()
    {
        var random = new SequenceRandomSource(OneOfEachIndex);
        var seen = new HashSet<TintaGradiente>();
        var current = TintaGradiente.BlueCyan;
        seen.Add(current);

        for (var i = 0; i < 100; i++)
        {
            current = GradientPicker.Next(current, random);
            seen.Add(current);
        }

        seen.Should().BeEquivalentTo(Enum.GetValues<TintaGradiente>());
    }

    [Fact]
    public void Next_never_repeats_with_the_production_source()
    {
        var current = TintaGradiente.BlueCyan;

        for (var i = 0; i < 500; i++)
        {
            var next = GradientPicker.Next(current, SecureRandomSource.Instance);
            next.Should().NotBe(current);
            Enum.IsDefined(next).Should().BeTrue();
            current = next;
        }
    }

    [Fact]
    public void Next_throws_when_the_random_source_is_null()
    {
        var act = () => GradientPicker.Next(TintaGradiente.BlueCyan, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
