using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class HighlighterWidthTests
{
    [Theory]
    [InlineData(3d, 9d)]
    [InlineData(5d, 15d)]
    [InlineData(8d, 24d)]
    [InlineData(12d, 36d)]
    [InlineData(18d, 54d)]
    public void From_is_three_times_the_chosen_thickness(double thickness, double expected)
    {
        HighlighterWidth.From(thickness).Should().Be(expected);
    }

    [Fact]
    public void Every_step_of_the_scale_gets_a_wider_highlighter()
    {
        foreach (double step in ThicknessScale.Steps)
        {
            HighlighterWidth.From(step).Should().BeGreaterThan(step);
        }
    }
}
