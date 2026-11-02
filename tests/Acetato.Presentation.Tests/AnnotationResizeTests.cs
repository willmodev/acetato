using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using Acetato.Domain;
using Acetato.Presentation.Overlay;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Redimensionar lo seleccionado (HU-28): los trazos van al recuadro nuevo sin cambiar
/// su grosor, la flecha rehace su punta, la letra del texto crece sin deformarse y un
/// solo Deshacer lo devuelve a como estaba.
/// </summary>
public sealed class AnnotationResizeTests
{
    private static readonly SelectionBox Start = new(0, 0, 100, 20);

    [Fact]
    public void Strokes_go_to_the_new_box_without_changing_their_thickness()
    {
        var stroke = Line();
        var resize = new AnnotationResize([stroke], [], Start);

        resize.ResizeTo(new SelectionBox(10, 10, 200, 40));

        stroke.StylusPoints[0].ToPoint().Should().Be(new Point(10, 10));
        stroke.StylusPoints[1].ToPoint().Should().Be(new Point(210, 50));
        stroke.DrawingAttributes.Width.Should().Be(4);
        stroke.DrawingAttributes.Height.Should().Be(4);
    }

    [Fact]
    public void A_free_stretch_moves_only_its_axis()
    {
        var stroke = Line();
        var resize = new AnnotationResize([stroke], [], Start);

        resize.ResizeTo(new SelectionBox(0, 0, 300, 20));

        stroke.StylusPoints[1].ToPoint().Should().Be(new Point(300, 20));
    }

    [Fact]
    public void A_stretched_arrow_rebuilds_its_head_with_the_same_size()
    {
        var arrow = Arrow();
        var resize = new AnnotationResize([arrow], [], Start);

        resize.ResizeTo(new SelectionBox(0, 0, 300, 20));

        ArrowHeadBuilder.TryBuild([new StrokePoint(0, 10), new StrokePoint(300, 10)], 4, out var expected).Should().BeTrue();
        arrow.Head.Should().Be(expected);
    }

    [Fact]
    public void A_moved_arrow_still_takes_its_head_along_unchanged()
    {
        var arrow = Arrow();
        var original = arrow.Head!.Value;
        var resize = new AnnotationResize([arrow], [], Start);

        resize.ResizeTo(new SelectionBox(50, 0, 100, 20));

        arrow.Head!.Value.Tip.Should().Be(new StrokePoint(original.Tip.X + 50, original.Tip.Y));
    }

    [Fact]
    public void Text_grows_its_letter_evenly_and_moves_its_corner()
    {
        var (left, top, size) = WpfTestHost.Run(() =>
        {
            var text = Text(left: 50, top: 10, size: 20);
            new AnnotationResize([], [text], Start).ResizeTo(new SelectionBox(0, 0, 200, 40));
            return (InkCanvas.GetLeft(text), InkCanvas.GetTop(text), text.FontSize);
        });

        (left, top).Should().Be((100d, 20d));
        size.Should().BeApproximately(40, 1e-9);
    }

    [Fact]
    public void In_a_free_stretch_the_letter_grows_by_the_area()
    {
        double size = WpfTestHost.Run(() =>
        {
            var text = Text(left: 0, top: 0, size: 20);
            new AnnotationResize([], [text], Start).ResizeTo(new SelectionBox(0, 0, 200, 20));
            return text.FontSize;
        });

        size.Should().BeApproximately(20 * Math.Sqrt(2), 1e-9);
    }

    [Fact]
    public void One_undo_brings_back_size_and_place()
    {
        var (point, left, size) = WpfTestHost.Run(() =>
        {
            var stroke = Line();
            var text = Text(left: 50, top: 10, size: 20);
            var resize = new AnnotationResize([stroke], [text], Start);
            resize.ResizeTo(new SelectionBox(0, 0, 150, 30));
            resize.ResizeTo(new SelectionBox(-20, -5, 400, 80));

            resize.Finish()!.Undo();

            return (stroke.StylusPoints[1].ToPoint(), InkCanvas.GetLeft(text), text.FontSize);
        });

        point.X.Should().BeApproximately(100, 1e-9);
        point.Y.Should().BeApproximately(20, 1e-9);
        left.Should().Be(50);
        size.Should().Be(20);
    }

    [Fact]
    public void A_click_on_a_handle_without_dragging_leaves_no_entry()
    {
        var resize = new AnnotationResize([Line()], [], Start);

        resize.ResizeTo(Start);

        resize.Finish().Should().BeNull();
    }

    [Fact]
    public void Undo_through_the_history_restores_the_stroke()
    {
        var strokes = new StrokeCollection();
        using var history = new AnnotationHistory(strokes);
        var stroke = Line();
        strokes.Add(stroke);
        var resize = new AnnotationResize([stroke], [], Start);
        resize.ResizeTo(new SelectionBox(0, 0, 200, 40));
        history.Record(resize.Finish()!);

        history.UndoLast();

        stroke.StylusPoints[1].ToPoint().Should().Be(new Point(100, 20));
        strokes.Should().Equal(stroke);
    }

    private static Stroke Line() =>
        new(
            new StylusPointCollection([new StylusPoint(0, 0), new StylusPoint(100, 20)]),
            new DrawingAttributes { Width = 4, Height = 4 });

    private static StyledStroke Arrow()
    {
        var path = new[] { new StrokePoint(0, 10), new StrokePoint(100, 10) };
        ArrowHeadBuilder.TryBuild(path, 4, out var head);
        return new StyledStroke(
            new StylusPointCollection(path.Select(point => new StylusPoint(point.X, point.Y))),
            new DrawingAttributes { Width = 4, Height = 4 },
            head);
    }

    private static TextBlock Text(double left, double top, double size)
    {
        var text = new TextBlock { Text = "Hola", FontSize = size };
        InkCanvas.SetLeft(text, left);
        InkCanvas.SetTop(text, top);
        return text;
    }
}
