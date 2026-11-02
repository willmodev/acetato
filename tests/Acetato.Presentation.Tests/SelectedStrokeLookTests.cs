using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Acetato.Domain;
using Acetato.Presentation.Overlay;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Un trazo seleccionado con Mover se ve con su color (SPEC 11). Sin cuidarlo, el lienzo de
/// Windows pinta «hueco» (blanco por dentro) cada trazo seleccionado de tinta plana.
/// </summary>
public sealed class SelectedStrokeLookTests
{
    [Fact]
    public void A_selected_flat_stroke_keeps_its_color()
    {
        var center = WpfTestHost.Run(() => CenterPixelOfSelected(gradient: null));

        center.Should().Be(Colors.Red);
    }

    [Fact]
    public void A_selected_gradient_stroke_is_not_painted_white()
    {
        var center = WpfTestHost.Run(() => CenterPixelOfSelected(GradientPair.FromInk(TintaColor.Red)));

        center.Should().NotBe(Colors.White);
    }

    // Lienzo con un trazo grueso horizontal, seleccionado; color del píxel en su centro.
    private static Color CenterPixelOfSelected(GradientPair? gradient)
    {
        var canvas = new InkCanvas { Background = Brushes.Transparent, EditingMode = InkCanvasEditingMode.Select };
        var stroke = new StyledStroke(
            new StylusPointCollection([new StylusPoint(10, 50), new StylusPoint(90, 50)]),
            new DrawingAttributes { Color = Colors.Red, Width = 18, Height = 18, FitToCurve = false },
            head: null,
            gradient);
        canvas.Strokes.Add(stroke);
        canvas.Select(canvas.Strokes);
        canvas.Measure(new Size(100, 100));
        canvas.Arrange(new Rect(0, 0, 100, 100));
        canvas.UpdateLayout();

        var bitmap = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(canvas);
        var pixel = new byte[4];
        bitmap.CopyPixels(new Int32Rect(50, 50, 1, 1), pixel, 4, 0);
        return Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]);
    }
}
