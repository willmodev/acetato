using System.Windows.Ink;
using Acetato.Application.Drawing;
using Acetato.Domain;
using Acetato.Presentation.Behaviors;
using Acetato.Presentation.Overlay;
using Acetato.Presentation.ViewModels;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Trazos con Tintas degradadas (HU-27): un trazo libre se fija con el par de su tinta,
/// el mismo en cada trazo, y conserva su par aunque luego se apague el interruptor.
/// </summary>
public sealed class InkPairStrokeTests
{
    [Fact]
    public void A_pencil_stroke_with_ink_pairs_keeps_the_pair_of_its_ink()
    {
        var gradients = WpfTestHost.Run(() =>
        {
            var settings = new DrawingSettings();
            using var overlay = new OverlayViewModel(settings);
            settings.SelectColor(TintaColor.Red);
            settings.SetInkPairs(true);
            var first = Draw(overlay);
            overlay.GradientUsedCommand.Execute(null);
            var second = Draw(overlay);
            settings.SetInkPairs(false);
            return (First: first.Gradient, Second: second.Gradient, Flat: DrawAny(overlay) is StyledStroke);
        });

        gradients.First.Should().Be(GradientPair.FromInk(TintaColor.Red));
        gradients.Second.Should().Be(gradients.First);
        gradients.Flat.Should().BeFalse();
    }

    private static StyledStroke Draw(OverlayViewModel overlay) => (StyledStroke)DrawAny(overlay);

    private static Stroke DrawAny(OverlayViewModel overlay)
    {
        var session = new FreehandSession(ToolKind.Pencil, overlay.ActiveAttributes, overlay.ActiveGradient);
        _ = session.Add(new StrokePoint(10, 10));
        _ = session.Add(new StrokePoint(80, 40));
        return session.BuildFinal()!;
    }
}
