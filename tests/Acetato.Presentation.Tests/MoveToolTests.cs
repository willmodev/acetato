using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using Acetato.Application.Drawing;
using Acetato.Domain;
using Acetato.Presentation.Behaviors;
using Acetato.Presentation.ViewModels;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Herramienta Mover (SPEC 11): pone el lienzo en modo selección, muestra la mano y
/// suelta la selección al cambiar de herramienta.
/// </summary>
public sealed class MoveToolTests
{
    [Fact]
    public void The_move_tool_puts_the_canvas_in_selection_mode()
    {
        var mode = WpfTestHost.Run(() =>
        {
            var settings = new DrawingSettings();
            using var overlay = new OverlayViewModel(settings);
            settings.SelectTool(ToolKind.Move);
            return overlay.EditingMode;
        });

        mode.Should().Be(InkCanvasEditingMode.Select);
    }

    [Fact]
    public void The_move_tool_uses_the_open_hand_cursor()
    {
        var (cursor, hand) = WpfTestHost.Run(() =>
            (ToolCursorBehavior.CursorFor(ToolKind.Move, tip: null, 1d), HandCursorFactory.Open(1d)));

        cursor.Should().BeSameAs(hand);
    }

    [Fact]
    public void The_closed_hand_is_a_different_cursor()
    {
        var (open, grab) = WpfTestHost.Run(() => (HandCursorFactory.Open(1.5d), HandCursorFactory.Grab(1.5d)));

        grab.Should().NotBeSameAs(open);
    }

    [Fact]
    public void Changing_tool_drops_the_selection()
    {
        var (selectedBefore, selectedAfter) = WpfTestHost.Run(() =>
        {
            var canvas = new InkCanvas { EditingMode = InkCanvasEditingMode.Select };
            canvas.Strokes.Add(new Stroke(new StylusPointCollection([new StylusPoint(0, 0), new StylusPoint(10, 10)])));
            MoveToolBehavior.SetTool(canvas, ToolKind.Pencil);
            MoveToolBehavior.SetTool(canvas, ToolKind.Move);
            canvas.Select(canvas.Strokes);
            int before = canvas.GetSelectedStrokes().Count;

            MoveToolBehavior.SetTool(canvas, ToolKind.Pencil);
            return (before, canvas.GetSelectedStrokes().Count);
        });

        selectedBefore.Should().Be(1);
        selectedAfter.Should().Be(0);
    }
}
