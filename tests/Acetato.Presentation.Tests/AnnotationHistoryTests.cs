using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using Acetato.Application.Drawing;
using Acetato.Domain;
using Acetato.Presentation.Overlay;
using Acetato.Presentation.ViewModels;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Historial por acciones de una pane (SPEC 11): Deshacer revierte lo último, sea un
/// trazo dibujado, un texto o lo quitado con el Borrador, y la previsualización de las
/// formas no deja rastro.
/// </summary>
public sealed class AnnotationHistoryTests
{
    [Fact]
    public void Undo_removes_the_last_drawn_stroke()
    {
        var strokes = new StrokeCollection();
        using var history = new AnnotationHistory(strokes);
        var first = AddStroke(strokes);
        AddStroke(strokes);

        history.UndoLast();

        strokes.Should().Equal(first);
    }

    [Fact]
    public void A_stroke_erased_with_the_eraser_comes_back_to_its_place()
    {
        var strokes = new StrokeCollection();
        using var history = new AnnotationHistory(strokes);
        var (a, b, c) = (AddStroke(strokes), AddStroke(strokes), AddStroke(strokes));

        history.RecordsErasures = true;
        strokes.Remove(b);
        history.UndoLast();

        strokes.Should().Equal(a, b, c);
    }

    [Fact]
    public void Several_strokes_erased_at_once_come_back_with_one_undo()
    {
        var strokes = new StrokeCollection();
        using var history = new AnnotationHistory(strokes);
        var (a, b, c, d) = (AddStroke(strokes), AddStroke(strokes), AddStroke(strokes), AddStroke(strokes));

        history.RecordsErasures = true;
        strokes.Remove(new StrokeCollection([b, d]));
        history.UndoLast();

        strokes.Should().Equal(a, b, c, d);
    }

    [Fact]
    public void Without_the_eraser_a_removed_stroke_is_forgotten_silently()
    {
        var strokes = new StrokeCollection();
        using var history = new AnnotationHistory(strokes);

        // Previsualización de una forma: se pone, se quita y queda la definitiva.
        var preview = AddStroke(strokes);
        strokes.Remove(preview);
        AddStroke(strokes);
        history.UndoLast();
        history.UndoLast();

        strokes.Should().BeEmpty();
    }

    [Fact]
    public void Drawing_and_erasing_are_undone_in_reverse_order()
    {
        var strokes = new StrokeCollection();
        using var history = new AnnotationHistory(strokes);
        var a = AddStroke(strokes);
        history.RecordsErasures = true;
        strokes.Remove(a);
        history.RecordsErasures = false;
        AddStroke(strokes);

        history.UndoLast();
        strokes.Should().BeEmpty();

        history.UndoLast();
        strokes.Should().Equal(a);

        history.UndoLast();
        strokes.Should().BeEmpty();
    }

    [Fact]
    public void Clear_empties_the_history()
    {
        var strokes = new StrokeCollection();
        using var history = new AnnotationHistory(strokes);
        AddStroke(strokes);
        history.RecordsErasures = true;

        history.Clear();
        history.UndoLast();

        strokes.Should().BeEmpty();
    }

    [Fact]
    public void Undo_without_actions_does_not_fail()
    {
        using var history = new AnnotationHistory([]);

        var undo = history.UndoLast;

        undo.Should().NotThrow();
    }

    [Fact]
    public void Texts_and_strokes_are_undone_in_order()
    {
        var steps = WpfTestHost.Run(() =>
        {
            var canvas = new InkCanvas();
            using var history = new AnnotationHistory(canvas.Strokes);
            AddStroke(canvas.Strokes);
            var text = new TextBlock { Text = "Hola" };
            _ = canvas.Children.Add(text);
            history.RecordText(canvas, text);

            history.UndoLast();
            var afterFirst = (canvas.Children.Count, canvas.Strokes.Count);
            history.UndoLast();
            return (afterFirst, (canvas.Children.Count, canvas.Strokes.Count));
        });

        steps.afterFirst.Should().Be((0, 1));
        steps.Item2.Should().Be((0, 0));
    }

    [Fact]
    public void With_the_eraser_tool_the_overlay_undoes_an_erasure()
    {
        var (before, after) = WpfTestHost.Run(() =>
        {
            var settings = new DrawingSettings();
            using var overlay = new OverlayViewModel(settings);
            var a = AddStroke(overlay.Strokes);
            AddStroke(overlay.Strokes);
            settings.SelectTool(ToolKind.Eraser);

            overlay.Strokes.Remove(a);
            var erased = overlay.Strokes.ToList();
            overlay.UndoCommand.Execute(null);
            return (erased, overlay.Strokes.ToList());
        });

        before.Should().HaveCount(1);
        after.Should().HaveCount(2);
    }

    private static Stroke AddStroke(StrokeCollection strokes)
    {
        var stroke = new Stroke(new StylusPointCollection([new StylusPoint(0, 0), new StylusPoint(10, 10)]));
        strokes.Add(stroke);
        return stroke;
    }
}
