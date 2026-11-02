using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using Acetato.Application.Abstractions;
using Acetato.Domain;
using Acetato.Presentation.Behaviors;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Qué hace Mover con un clic (SPEC 11): la mano agarra lo de debajo y deja pasar el
/// lazo; con otra herramienta, Ctrl agarra sin seleccionar y nunca deja dibujar; sin
/// Ctrl, el clic llega a la herramienta como siempre.
/// </summary>
public sealed class MoveWithCtrlTests
{
    private static readonly Point OnStroke = new(100, 50);
    private static readonly Point Empty = new(100, 250);

    [Fact]
    public void With_ctrl_a_drawing_tool_drags_the_stroke_under_the_cursor_without_selecting_it()
    {
        var (decision, stroke) = Decide(ToolKind.Pencil, ctrl: true, OnStroke);

        decision.Handles.Should().BeTrue();
        decision.Targets.Strokes.Should().Equal(stroke);
        decision.KeepSelected.Should().BeFalse();
    }

    [Theory]
    [InlineData(ToolKind.Pencil)]
    [InlineData(ToolKind.Laser)]
    [InlineData(ToolKind.Eraser)]
    [InlineData(ToolKind.Arrow)]
    [InlineData(ToolKind.Ellipse)]
    [InlineData(ToolKind.Text)]
    public void With_ctrl_a_click_on_empty_space_draws_nothing(ToolKind tool)
    {
        var (decision, _) = Decide(tool, ctrl: true, Empty);

        decision.Handles.Should().BeTrue();
        decision.Targets.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Without_ctrl_the_click_reaches_the_tool()
    {
        var (decision, _) = Decide(ToolKind.Pencil, ctrl: false, OnStroke);

        decision.Should().Be(GrabDecision.Pass);
    }

    [Fact]
    public void The_hand_drags_the_stroke_under_the_cursor_and_keeps_it_selected()
    {
        var (decision, stroke) = Decide(ToolKind.Move, ctrl: false, OnStroke);

        decision.Handles.Should().BeTrue();
        decision.Targets.Strokes.Should().Equal(stroke);
        decision.KeepSelected.Should().BeTrue();
    }

    [Fact]
    public void The_hand_lets_the_lasso_through_on_empty_space()
    {
        var (decision, _) = Decide(ToolKind.Move, ctrl: true, Empty);

        decision.Should().Be(GrabDecision.Pass);
    }

    [Fact]
    public void A_text_under_the_cursor_is_grabbed_before_the_strokes_beneath_it()
    {
        var grabbed = WpfTestHost.Run(() =>
        {
            var canvas = CanvasWithStroke(ToolKind.Pencil, ctrl: true, out _);
            var text = new TextBlock { Text = "Hola", FontSize = 24 };
            InkCanvas.SetLeft(text, 80);
            InkCanvas.SetTop(text, 40);
            _ = canvas.Children.Add(text);
            canvas.Measure(new Size(400, 400));
            canvas.Arrange(new Rect(0, 0, 400, 400));

            var decision = MoveToolBehavior.Decide(canvas, new Point(90, 50));
            return (decision.Targets.Elements.SingleOrDefault() == text, decision.Targets.Strokes.Count);
        });

        grabbed.Should().Be((true, 0));
    }

    private static (GrabDecision Decision, Stroke Stroke) Decide(ToolKind tool, bool ctrl, Point point) =>
        WpfTestHost.Run(() =>
        {
            var canvas = CanvasWithStroke(tool, ctrl, out var stroke);
            return (MoveToolBehavior.Decide(canvas, point), stroke);
        });

    // Lienzo con una línea horizontal en y = 50, la herramienta y el estado de Ctrl dados.
    private static InkCanvas CanvasWithStroke(ToolKind tool, bool ctrl, out Stroke stroke)
    {
        var canvas = new InkCanvas();
        stroke = new Stroke(new StylusPointCollection([new StylusPoint(0, 50), new StylusPoint(200, 50)]));
        canvas.Strokes.Add(stroke);
        MoveToolBehavior.SetKeyState(canvas, new FixedKeyState(ctrl));
        MoveToolBehavior.SetTool(canvas, tool);
        return canvas;
    }

    private sealed class FixedKeyState : IModifierKeyState
    {
        public FixedKeyState(bool ctrl) => IsCtrlDown = ctrl;

        public bool IsCtrlDown { get; }
    }
}
