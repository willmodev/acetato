using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class FontScaleTests
{
    [Theory]
    [InlineData(0, 16d)]
    [InlineData(1, 22d)]
    [InlineData(2, 30d)]
    [InlineData(3, 42d)]
    [InlineData(4, 60d)]
    public void FromThicknessIndex_maps_each_index_to_its_size(int index, double expected)
    {
        FontScale.FromThicknessIndex(index).Should().Be(expected);
    }

    [Fact]
    public void FromThicknessIndex_clamps_an_out_of_range_index()
    {
        FontScale.FromThicknessIndex(-5).Should().Be(16d);
        FontScale.FromThicknessIndex(99).Should().Be(60d);
    }

    [Fact]
    public void Sizes_grow_monotonically_with_thickness()
    {
        FontScale.Sizes.Should().BeInAscendingOrder()
            .And.HaveCount(ThicknessScale.Steps.Count);
    }
}
