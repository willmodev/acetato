using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class DipConverterTests
{
    [Theory]
    [InlineData(1920, 1.0, 1920)]
    [InlineData(1920, 1.25, 1536)]
    [InlineData(1920, 1.5, 1280)]
    [InlineData(1920, 2.0, 960)]
    public void ToDip_divides_physical_pixels_by_the_dpi_scale(double physical, double scale, double expected)
    {
        DipConverter.ToDip(physical, scale).Should().BeApproximately(expected, 1e-9);
    }

    [Fact]
    public void ToDip_treats_a_non_positive_scale_as_one()
    {
        DipConverter.ToDip(800, 0).Should().Be(800);
    }
}
