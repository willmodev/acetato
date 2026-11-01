using System.Globalization;
using System.Windows.Media;
using Acetato.Domain;
using Acetato.Presentation.Resources;
using Acetato.Presentation.ViewModels;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Tintas degradadas en la barra (HU-27): las muestras del popover y el botón Color
/// enseñan el par de cada tinta en diagonal; apagado, la tinta plana; en modo neón, el
/// botón sigue con el degradado de muestra.
/// </summary>
public sealed class ToolbarInkPairsTests
{
    [Fact]
    public void With_ink_pairs_on_each_ink_swatch_shows_its_pair_at_135_degrees()
    {
        var fills = WpfTestHost.Run(() => ToolbarHost.With(context =>
        {
            context.Settings.SetInkPairs(true);
            return context.ViewModel.Swatches
                .Where(swatch => swatch.Color is not null)
                .Select(swatch => (swatch.Color!.Value, Describe(swatch.Fill)))
                .ToList();
        }));

        fills.Should().HaveCount(6);
        foreach (var (color, fill) in fills)
        {
            var (start, end) = WpfTestHost.Run(() => GradientPairMap.Resolve(GradientPair.FromInk(color)));
            fill.Should().Be(PairFill(start, end));
        }
    }

    [Fact]
    public void With_ink_pairs_off_the_swatches_are_flat()
    {
        var fills = WpfTestHost.Run(() => ToolbarHost.With(context =>
        {
            context.Settings.SetInkPairs(true);
            context.Settings.SetInkPairs(false);
            return context.ViewModel.Swatches
                .Where(swatch => swatch.Color is not null)
                .Select(swatch => (swatch.Color!.Value, Describe(swatch.Fill)))
                .ToList();
        }));

        foreach (var (color, fill) in fills)
        {
            var flat = WpfTestHost.Run(() => TintaColorMap.Resolve(color));
            fill.Should().Be(flat.ToString(CultureInfo.InvariantCulture));
        }
    }

    [Fact]
    public void The_color_button_shows_the_pair_of_the_active_ink()
    {
        var (fill, start, end) = WpfTestHost.Run(() => ToolbarHost.With(context =>
        {
            context.Settings.SetInkPairs(true);
            context.Settings.SelectColor(TintaColor.Green);
            var (pairStart, pairEnd) = GradientPairMap.Resolve(GradientPair.FromInk(TintaColor.Green));
            return (Describe(context.ViewModel.ActiveInkBrush), pairStart, pairEnd);
        }));

        fill.Should().Be(PairFill(start, end));
    }

    [Fact]
    public void In_neon_mode_the_color_button_keeps_the_gradient_sample()
    {
        var isSample = WpfTestHost.Run(() => ToolbarHost.With(context =>
        {
            context.Settings.SetInkPairs(true);
            context.Settings.SelectGradient();
            return ReferenceEquals(context.ViewModel.ActiveInkBrush, InkSwatchFactory.GradientSample);
        }));

        isSample.Should().BeTrue();
    }

    [Fact]
    public void The_neon_swatch_keeps_its_sample_with_ink_pairs_on()
    {
        var isSample = WpfTestHost.Run(() => ToolbarHost.With(context =>
        {
            context.Settings.SetInkPairs(true);
            var neon = context.ViewModel.Swatches.Single(swatch => swatch.IsGradient);
            return ReferenceEquals(neon.Fill, InkSwatchFactory.GradientSample);
        }));

        isSample.Should().BeTrue();
    }

    private static string PairFill(Color start, Color end) =>
        string.Create(CultureInfo.InvariantCulture, $"{start}>{end}@0,0-1,1");

    // Texto comparable del pincel: color plano, o inicio>fin@ejes de un degradado lineal.
    private static string Describe(Brush? brush) => brush switch
    {
        SolidColorBrush solid => solid.Color.ToString(CultureInfo.InvariantCulture),
        LinearGradientBrush linear => string.Create(
            CultureInfo.InvariantCulture,
            $"{linear.GradientStops[0].Color}>{linear.GradientStops[^1].Color}@{linear.StartPoint.X},{linear.StartPoint.Y}-{linear.EndPoint.X},{linear.EndPoint.Y}"),
        _ => "otro",
    };
}
