using System.Windows.Media;
using Acetato.Domain;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Botón Pizarra de la barra (HU-35): recorre la pizarra, el icono enseña su hoja y el botón
/// nunca usa el acento. Desde el modo puntero vuelve al dibujo, como las herramientas.
/// </summary>
public sealed class ToolbarWhiteboardTests
{
    [Fact]
    public void The_button_starts_with_an_empty_icon_and_the_shortcut_in_its_hint()
    {
        var (fill, hint) = WpfTestHost.Run(() => ToolbarHost.With(toolbar =>
            (toolbar.ViewModel.WhiteboardFill, toolbar.ViewModel.WhiteboardHint.ToString())));

        fill.Should().Be(Brushes.Transparent);
        hint.Should().Be("Pizarra  Ctrl+W");
    }

    [Fact]
    public void Each_click_cycles_the_whiteboard_and_the_icon_shows_the_sheet()
    {
        var steps = WpfTestHost.Run(() => ToolbarHost.With(toolbar =>
        {
            var white = Step(toolbar);
            var black = Step(toolbar);
            var off = Step(toolbar);
            return (white, black, off);
        }));

        steps.white.Mode.Should().Be(WhiteboardMode.White);
        steps.white.Color.Should().Be(Color.FromRgb(0xF4, 0xF1, 0xEA));
        steps.black.Mode.Should().Be(WhiteboardMode.Black);
        steps.black.Color.Should().Be(Color.FromRgb(0x1B, 0x1D, 0x1F));
        steps.off.Mode.Should().Be(WhiteboardMode.Off);
        steps.off.Fill.Should().Be(Brushes.Transparent);
    }

    [Fact]
    public void The_whiteboard_goes_on_in_the_canvas_under_the_cursor()
    {
        var received = WpfTestHost.Run(() => ToolbarHost.With(toolbar =>
        {
            toolbar.ViewModel.ToggleWhiteboardCommand.Execute(null);
            toolbar.Canvas.Received(1).ShowWhiteboard(WhiteboardMode.White);
            return true;
        }));

        received.Should().BeTrue();
    }

    [Fact]
    public void From_pointer_mode_the_button_returns_to_drawing_and_turns_the_whiteboard_on()
    {
        var (drawing, mode) = WpfTestHost.Run(() => ToolbarHost.With(toolbar =>
        {
            toolbar.Controller.SetDrawingMode(false);

            toolbar.ViewModel.ToggleWhiteboardCommand.Execute(null);

            return (toolbar.Controller.IsDrawingMode, toolbar.Whiteboard.Mode);
        }));

        drawing.Should().BeTrue();
        mode.Should().Be(WhiteboardMode.White);
    }

    [Fact]
    public void From_pointer_mode_with_the_whiteboard_kept_by_a_clip_it_only_returns_to_drawing()
    {
        var (drawing, mode) = WpfTestHost.Run(() => ToolbarHost.With(toolbar =>
        {
            toolbar.ViewModel.ToggleWhiteboardCommand.Execute(null); // blanca
            toolbar.Whiteboard.KeepThroughNextPointerMode();
            toolbar.Controller.SetDrawingMode(false); // el recorte pasa a modo puntero

            toolbar.ViewModel.ToggleWhiteboardCommand.Execute(null);

            return (toolbar.Controller.IsDrawingMode, toolbar.Whiteboard.Mode);
        }));

        drawing.Should().BeTrue();
        mode.Should().Be(WhiteboardMode.White); // no avanzó a negra
    }

    [Fact]
    public void Leaving_drawing_mode_turns_the_icon_back_to_empty()
    {
        var fill = WpfTestHost.Run(() => ToolbarHost.With(toolbar =>
        {
            toolbar.ViewModel.ToggleWhiteboardCommand.Execute(null);

            toolbar.Controller.SetDrawingMode(false);

            return toolbar.ViewModel.WhiteboardFill;
        }));

        fill.Should().Be(Brushes.Transparent);
    }

    private static (WhiteboardMode Mode, Brush Fill, Color Color) Step(ToolbarContext toolbar)
    {
        toolbar.ViewModel.ToggleWhiteboardCommand.Execute(null);
        var fill = toolbar.ViewModel.WhiteboardFill;
        return (toolbar.Whiteboard.Mode, fill, fill is SolidColorBrush solid ? solid.Color : default);
    }
}
