using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class ThicknessScaleTests
{
    [Fact]
    public void Steps_match_the_design_system_scale()
    {
        ThicknessScale.Steps.Should().Equal(3d, 5d, 8d, 12d, 18d);
    }

    [Fact]
    public void Next_advances_one_step()
    {
        ThicknessScale.Next(0).Should().Be(1);
    }

    [Fact]
    public void Next_from_last_index_stays_at_max()
    {
        ThicknessScale.Next(ThicknessScale.MaxIndex).Should().Be(ThicknessScale.MaxIndex);
    }

    [Fact]
    public void Previous_from_first_index_stays_at_min()
    {
        ThicknessScale.Previous(ThicknessScale.MinIndex).Should().Be(ThicknessScale.MinIndex);
    }

    [Fact]
    public void At_clamps_an_out_of_range_index()
    {
        ThicknessScale.At(-5).Should().Be(3d);
        ThicknessScale.At(99).Should().Be(18d);
    }
}
