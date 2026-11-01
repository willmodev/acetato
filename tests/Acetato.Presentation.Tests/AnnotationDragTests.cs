using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Acetato.Domain;
using Acetato.Presentation.Overlay;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Mover lo dibujado (SPEC 11): una flecha conserva su punta y su degradado al moverse,
/// y el arrastre entrega una entrada que Deshacer revierte.
/// </summary>
public sealed class AnnotationDragTests
{
    private static readonly ArrowHead Head = new(new StrokePoint(90, 0), new StrokePoint(100, 10), new StrokePoint(90, 20));

    [Fact]
    public void A_moved_arrow_takes_its_head_along()
    {
        var arrow = Arrow(gradient: null);

        AnnotationOffset.Apply([arrow], [], new Vector(30, 40));

        arrow.Head.Should().Be(new ArrowHead(new StrokePoint(120, 40), new StrokePoint(130, 50), new StrokePoint(120, 60)));
        arrow.StylusPoints[0].ToPoint().Should().Be(new Point(30, 50));
    }

    [Fact]
    public void A_copy_of_a_moved_arrow_keeps_the_moved_head()
    {
        var arrow = Arrow(gradient: null);
        AnnotationOffset.Apply([arrow], [], new Vector(5, 5));

        var copy = (StyledStroke)arrow.Clone();

        copy.Head.Should().Be(arrow.Head);
    }

    [Fact]
    public void The_gradient_of_a_moved_stroke_follows_it()
    {
        var (before, after) = WpfTestHost.Run(() =>
        {
            var arrow = Arrow(GradientPair.FromInk(TintaColor.Red));
            var start = ((LinearGradientBrush)arrow.GradientBrush()!).StartPoint;
            AnnotationOffset.Apply([arrow], [], new Vector(30, 40));
            return (start, ((LinearGradientBrush)arrow.GradientBrush()!).StartPoint);
        });

        after.Should().Be(new Point(before.X + 30, before.Y + 40));
    }

    [Fact]
    public void A_drag_moves_strokes_and_texts_and_its_entry_brings_them_back()
    {
        var (moved, restored) = WpfTestHost.Run(() =>
        {
            var stroke = Line();
            var text = new TextBlock();
            InkCanvas.SetLeft(text, 10);
            InkCanvas.SetTop(text, 20);

            var drag = new AnnotationDrag([stroke], [text]);
            drag.MoveBy(new Vector(5, 0));
            drag.MoveBy(new Vector(10, 10));
            var movedState = Snapshot(stroke, text);
            drag.Finish()!.Undo();
            return (movedState, Snapshot(stroke, text));
        });

        moved.Should().Be((new Point(15, 10), new Point(25, 30)));
        restored.Should().Be((new Point(0, 0), new Point(10, 20)));
    }

    [Fact]
    public void A_click_without_dragging_leaves_no_entry()
    {
        var drag = new AnnotationDrag([Line()], []);

        drag.MoveBy(default);

        drag.Finish().Should().BeNull();
    }

    [Fact]
    public void Undo_brings_back_a_recorded_move()
    {
        var strokes = new StrokeCollection();
        using var history = new AnnotationHistory(strokes);
        var stroke = Line();
        strokes.Add(stroke);
        var drag = new AnnotationDrag([stroke], []);
        drag.MoveBy(new Vector(50, 50));
        history.Record(drag.Finish()!);

        history.UndoLast();

        stroke.StylusPoints[0].ToPoint().Should().Be(new Point(0, 0));
        strokes.Should().Equal(stroke);
    }

    private static Stroke Line() =>
        new(new StylusPointCollection([new StylusPoint(0, 0), new StylusPoint(10, 10)]));

    private static StyledStroke Arrow(GradientPair? gradient) =>
        new(
            new StylusPointCollection([new StylusPoint(0, 10), new StylusPoint(100, 10)]),
            new DrawingAttributes { Width = 4, Height = 4 },
            Head,
            gradient);

    private static (Point Stroke, Point Text) Snapshot(Stroke stroke, UIElement text) =>
        (stroke.StylusPoints[0].ToPoint(), new Point(InkCanvas.GetLeft(text), InkCanvas.GetTop(text)));
}
