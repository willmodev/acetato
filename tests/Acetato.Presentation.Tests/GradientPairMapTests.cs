using System.Windows.Media;
using Acetato.Domain;
using Acetato.Presentation.Resources;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Colores de un par de degradado (HU-27): cada tinta va hacia su vecina fija y un par
/// neón resuelve lo mismo que antes.
/// </summary>
public sealed class GradientPairMapTests
{
    [Theory]
    [InlineData(TintaColor.Red, "#FFFF3B30", "#FFFF2D92")]
    [InlineData(TintaColor.Blue, "#FF0A84FF", "#FF5E5CE6")]
    [InlineData(TintaColor.Yellow, "#FFFFD60A", "#FFFF9F0A")]
    [InlineData(TintaColor.Green, "#FF30D158", "#FFA3E635")]
    [InlineData(TintaColor.White, "#FFFFFFFF", "#FFAEAEB2")]
    [InlineData(TintaColor.Black, "#FF0A0A0A", "#FF636366")]
    public void Each_ink_goes_from_itself_to_its_neighbor(TintaColor ink, string start, string end)
    {
        var (actualStart, actualEnd) = WpfTestHost.Run(() => GradientPairMap.Resolve(GradientPair.FromInk(ink)));

        actualStart.ToString(System.Globalization.CultureInfo.InvariantCulture).Should().Be(start);
        actualEnd.ToString(System.Globalization.CultureInfo.InvariantCulture).Should().Be(end);
    }

    [Fact]
    public void A_neon_pair_resolves_the_same_colors_as_before()
    {
        var (pair, neon) = WpfTestHost.Run(() => (
            GradientPairMap.Resolve(GradientPair.FromNeon(TintaGradiente.VioletPink)),
            TintaGradienteMap.Resolve(TintaGradiente.VioletPink)));

        pair.Should().Be(neon);
    }

    [Fact]
    public void The_start_of_an_ink_pair_is_the_flat_ink()
    {
        var (start, flat) = WpfTestHost.Run(() => (
            GradientPairMap.Resolve(GradientPair.FromInk(TintaColor.Green)).Start,
            TintaColorMap.Resolve(TintaColor.Green)));

        start.Should().Be(flat);
        start.Should().NotBe(Colors.Transparent);
    }
}
