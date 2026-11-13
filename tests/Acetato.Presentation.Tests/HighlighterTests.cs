using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Acetato.Application.Drawing;
using Acetato.Domain;
using Acetato.Presentation.Behaviors;
using Acetato.Presentation.Overlay;
using Acetato.Presentation.Resources;
using Acetato.Presentation.ViewModels;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Resaltador (HU-34): trazo translúcido de tres veces el grosor, con la tinta sólida,
/// sin degradado y con el círculo de tinta al 50 %.
/// </summary>
public sealed class HighlighterTests
{
    [Fact]
    public void With_the_highlighter_the_next_stroke_is_translucent_and_three_times_thicker()
    {
        var attributes = WpfTestHost.Run(() =>
        {
            var settings = new DrawingSettings();
            using var overlay = new OverlayViewModel(settings);
            settings.SelectThickness(0);
            settings.SelectTool(ToolKind.Highlighter);
            return (overlay.ActiveAttributes.IsHighlighter, overlay.ActiveAttributes.Width, overlay.ActiveAttributes.Height);
        });

        attributes.IsHighlighter.Should().BeTrue();
        attributes.Width.Should().Be(9d);
        attributes.Height.Should().Be(9d);
    }

    [Fact]
    public void Going_back_to_the_pencil_restores_the_normal_stroke()
    {
        var attributes = WpfTestHost.Run(() =>
        {
            var settings = new DrawingSettings();
            using var overlay = new OverlayViewModel(settings);
            settings.SelectThickness(4);
            settings.SelectTool(ToolKind.Highlighter);
            settings.SelectTool(ToolKind.Pencil);
            return (overlay.ActiveAttributes.IsHighlighter, overlay.ActiveAttributes.Width);
        });

        attributes.IsHighlighter.Should().BeFalse();
        attributes.Width.Should().Be(18d);
    }

    [Fact]
    public void The_stroke_keeps_the_solid_ink_and_never_a_gradient()
    {
        var stroke = WpfTestHost.Run(() =>
        {
            var settings = new DrawingSettings();
            using var overlay = new OverlayViewModel(settings);
            settings.SelectColor(TintaColor.Blue);
            settings.SelectGradient();
            settings.SelectTool(ToolKind.Highlighter);
            return Build(overlay);
        });

        stroke.DrawingAttributes.IsHighlighter.Should().BeTrue();
        stroke.Gradient.Should().BeNull();
        stroke.DrawingAttributes.Color.Should().Be(Color.FromRgb(0x0A, 0x84, 0xFF));
    }

    [Fact]
    public void Painted_over_white_the_stroke_lets_half_of_the_background_through()
    {
        var pixel = WpfTestHost.Run(() =>
        {
            var settings = new DrawingSettings();
            using var overlay = new OverlayViewModel(settings);
            settings.SelectColor(TintaColor.Red);
            settings.SelectTool(ToolKind.Highlighter);
            return CenterPixelOverWhite(Build(overlay));
        });

        // Rojo (255, 59, 48) al 50 % sobre blanco: el verde y el azul quedan a mitad de camino.
        pixel[2].Should().BeGreaterThan(240); // rojo
        pixel[1].Should().BeInRange(130, 180); // verde
        pixel[0].Should().BeInRange(125, 175); // azul
    }

    [Fact]
    public void In_the_canvas_a_highlighter_drawn_later_stays_under_a_pencil_stroke()
    {
        var pixels = WpfTestHost.Run(() =>
        {
            var canvas = new InkCanvas { Width = 200, Height = 80, Background = Brushes.White };
            canvas.Strokes.Add(Line(ToolKind.Pencil, TintaColor.Black, new Point(100, 5), new Point(100, 75), width: 6d));
            canvas.Strokes.Add(Line(ToolKind.Highlighter, TintaColor.Red, new Point(20, 40), new Point(180, 40), width: 20d));
            canvas.Measure(new Size(200, 80));
            canvas.Arrange(new Rect(0, 0, 200, 80));
            canvas.UpdateLayout();
            var bitmap = new RenderTargetBitmap(200, 80, 96d, 96d, PixelFormats.Pbgra32);
            bitmap.Render(canvas);
            return (Cross: Read(bitmap, 100, 40), Side: Read(bitmap, 50, 40));
        });

        // En el cruce manda el lápiz (negro); a un lado, el resaltador deja ver el fondo.
        pixels.Cross[2].Should().BeLessThan(40);
        pixels.Side[2].Should().BeGreaterThan(240);
        pixels.Side[1].Should().BeInRange(130, 180);
    }

    [Fact]
    public void The_cursor_shows_the_widened_translucent_ink()
    {
        var tip = WpfTestHost.Run(() =>
        {
            var settings = new DrawingSettings();
            using var overlay = new OverlayViewModel(settings);
            settings.SelectThickness(2);
            settings.SelectTool(ToolKind.Highlighter);
            return overlay.CursorTip;
        });

        tip.Highlighter.Should().BeTrue();
        tip.Thickness.Should().Be(24d);
        tip.Gradient.Should().BeNull();
    }

    [Fact]
    public void The_cursor_circle_is_filled_at_half_opacity()
    {
        var color = WpfTestHost.Run(() =>
            ((SolidColorBrush)InkCursorFactory.FillOf(new InkCursorTip(Color.FromRgb(0xFF, 0x3B, 0x30), null, 9d, Highlighter: true))).Color);

        color.A.Should().Be(0x80);
        color.R.Should().Be(0xFF);
    }

    [Fact]
    public void The_highlighter_gets_the_ink_circle_instead_of_the_cross()
    {
        var cursor = WpfTestHost.Run(() =>
            ToolCursorBehavior.CursorFor(ToolKind.Highlighter, new InkCursorTip(Color.FromRgb(0xFF, 0x3B, 0x30), null, 9d, Highlighter: true), 1d));

        cursor.Should().NotBeNull().And.NotBe(Cursors.Cross);
    }

    private static StyledStroke Line(ToolKind tool, TintaColor ink, Point from, Point to, double width)
    {
        var attributes = new DrawingAttributes
        {
            Color = TintaColorMap.Resolve(ink),
            Width = width,
            Height = width,
            IsHighlighter = tool == ToolKind.Highlighter,
        };
        var points = new StylusPointCollection { new StylusPoint(from.X, from.Y), new StylusPoint(to.X, to.Y) };
        return new StyledStroke(points, attributes, head: null);
    }

    private static byte[] Read(BitmapSource bitmap, int x, int y)
    {
        var pixel = new byte[4];
        bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);
        return pixel;
    }

    private static StyledStroke Build(OverlayViewModel overlay)
    {
        var session = new FreehandSession(ToolKind.Highlighter, overlay.ActiveAttributes, overlay.ActiveGradient);
        _ = session.Add(new StrokePoint(20, 40));
        _ = session.Add(new StrokePoint(180, 40));
        return (StyledStroke)session.BuildFinal()!;
    }

    // Pinta el trazo con Stroke.Draw (el camino de la capa en vivo) sobre un fondo blanco.
    private static byte[] CenterPixelOverWhite(Stroke stroke)
    {
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            context.DrawRectangle(Brushes.White, null, new Rect(0, 0, 200, 80));
            stroke.Draw(context);
        }

        var bitmap = new RenderTargetBitmap(200, 80, 96d, 96d, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var pixel = new byte[4];
        bitmap.CopyPixels(new Int32Rect(100, 40, 1, 1), pixel, 4, 0);
        return pixel;
    }
}
