using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

/// <summary>Tamaño del círculo del cursor de tinta (HU-26).</summary>
public sealed class CursorTipSizeTests
{
    [Fact]
    public void Thicknesses_below_the_minimum_use_the_visible_minimum()
    {
        CursorTipSize.Diameter(3d).Should().Be(6d);
        CursorTipSize.Diameter(5d).Should().Be(6d);
    }

    [Theory]
    [InlineData(8d)]
    [InlineData(12d)]
    [InlineData(18d)]
    public void Other_thicknesses_match_the_stroke(double thickness)
    {
        CursorTipSize.Diameter(thickness).Should().Be(thickness);
    }

    [Theory]
    [InlineData(8d, 1d, 8)]
    [InlineData(8d, 1.5d, 12)]
    [InlineData(12d, 1.25d, 15)]
    [InlineData(18d, 2.5d, 45)]
    [InlineData(3d, 1.5d, 9)]
    public void Pixel_diameter_follows_the_monitor_scale(double thickness, double scale, int expected)
    {
        CursorTipSize.PixelDiameter(thickness, scale).Should().Be(expected);
    }
}
