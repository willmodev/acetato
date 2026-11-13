using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Overlay;
using Acetato.Domain;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Application.Tests;

/// <summary>
/// La pizarra (HU-35): apagada → blanca → negra → apagada, prendida en el monitor del
/// cursor, tinta contraria y apagado al salir del modo dibujo o al ocultar la capa.
/// </summary>
public sealed class WhiteboardControllerTests : IDisposable
{
    private readonly IOverlayCanvas _left = Canvas(new MonitorInfo(0, 0, 1920, 1080, IsPrimary: true));
    private readonly IOverlayCanvas _right = Canvas(new MonitorInfo(1920, 0, 1280, 1024, IsPrimary: false));
    private readonly ICursorPositionProvider _cursor = Substitute.For<ICursorPositionProvider>();
    private readonly IOverlayController _overlay = Substitute.For<IOverlayController>();
    private readonly DrawingSettings _settings = new();
    private readonly WhiteboardController _whiteboard;
    private int _changes;

    public WhiteboardControllerTests()
    {
        var canvases = Substitute.For<IOverlayCanvasProvider>();
        canvases.Canvases.Returns([_left, _right]);
        _cursor.GetCursorPosition().Returns((100, 100));
        _overlay.IsVisible.Returns(true);
        _overlay.IsDrawingMode.Returns(true);
        _whiteboard = new WhiteboardController(canvases, _cursor, _settings, _overlay);
        _whiteboard.Changed += (_, _) => _changes++;
    }

    private static IOverlayCanvas Canvas(MonitorInfo monitor)
    {
        var canvas = Substitute.For<IOverlayCanvas>();
        canvas.Monitor.Returns(monitor);
        return canvas;
    }

    private void RaiseOverlayChanged() => _overlay.StateChanged += Raise.Event();

    public void Dispose() => _whiteboard.Dispose();

    [Fact]
    public void It_starts_off()
    {
        _whiteboard.Mode.Should().Be(WhiteboardMode.Off);
    }

    [Fact]
    public void The_first_cycle_turns_on_the_white_board_on_the_monitor_under_the_cursor()
    {
        _cursor.GetCursorPosition().Returns((2000, 300));

        _whiteboard.Cycle();

        _whiteboard.Mode.Should().Be(WhiteboardMode.White);
        _right.Received(1).ShowWhiteboard(WhiteboardMode.White);
        _left.DidNotReceiveWithAnyArgs().ShowWhiteboard(default);
    }

    [Fact]
    public void The_next_cycles_stay_on_the_same_monitor_even_if_the_cursor_leaves()
    {
        _cursor.GetCursorPosition().Returns((100, 100));
        _whiteboard.Cycle();
        _cursor.GetCursorPosition().Returns((2000, 300));

        _whiteboard.Cycle();

        _whiteboard.Mode.Should().Be(WhiteboardMode.Black);
        _left.Received(1).ShowWhiteboard(WhiteboardMode.Black);
        _right.DidNotReceiveWithAnyArgs().ShowWhiteboard(default);
    }

    [Fact]
    public void The_third_cycle_turns_it_off_and_the_next_one_looks_at_the_cursor_again()
    {
        _whiteboard.Cycle();
        _whiteboard.Cycle();

        _whiteboard.Cycle();

        _whiteboard.Mode.Should().Be(WhiteboardMode.Off);
        _left.Received(1).ShowWhiteboard(WhiteboardMode.Off);

        _cursor.GetCursorPosition().Returns((2000, 300));
        _whiteboard.Cycle();
        _right.Received(1).ShowWhiteboard(WhiteboardMode.White);
    }

    [Fact]
    public void Every_change_raises_the_event()
    {
        _whiteboard.Cycle();
        _whiteboard.Cycle();
        _whiteboard.Cycle();

        _changes.Should().Be(3);
    }

    [Fact]
    public void White_ink_becomes_black_on_the_white_board()
    {
        _settings.SelectColor(TintaColor.White);

        _whiteboard.Cycle();

        _settings.Color.Should().Be(TintaColor.Black);
    }

    [Fact]
    public void Black_ink_becomes_white_on_the_black_board()
    {
        _settings.SelectColor(TintaColor.Black);
        _whiteboard.Cycle(); // blanca: la negra se ve, no cambia
        _settings.Color.Should().Be(TintaColor.Black);

        _whiteboard.Cycle(); // negra: ahora se confunde con el fondo

        _settings.Color.Should().Be(TintaColor.White);
    }

    [Fact]
    public void A_colored_ink_is_never_changed()
    {
        _settings.SelectColor(TintaColor.Red);

        _whiteboard.Cycle();
        _whiteboard.Cycle();

        _settings.Color.Should().Be(TintaColor.Red);
    }

    [Fact]
    public void Gradient_mode_is_left_alone()
    {
        _settings.SelectColor(TintaColor.White);
        _settings.SelectGradient();

        _whiteboard.Cycle();

        _settings.IsGradient.Should().BeTrue();
        _settings.Color.Should().Be(TintaColor.White);
    }

    [Fact]
    public void Turning_it_off_does_not_restore_the_ink()
    {
        _settings.SelectColor(TintaColor.White);
        _whiteboard.Cycle(); // negra por contraste
        _whiteboard.Cycle();
        _whiteboard.Cycle();

        _settings.Color.Should().Be(TintaColor.White); // blanca y negra se alternaron; al apagar queda como esté
    }

    [Fact]
    public void Leaving_drawing_mode_turns_it_off()
    {
        _whiteboard.Cycle();
        _overlay.IsDrawingMode.Returns(false);

        RaiseOverlayChanged();

        _whiteboard.Mode.Should().Be(WhiteboardMode.Off);
        _left.Received(1).ShowWhiteboard(WhiteboardMode.Off);
    }

    [Fact]
    public void Hiding_the_layer_turns_it_off()
    {
        _whiteboard.Cycle();
        _overlay.IsVisible.Returns(false);

        RaiseOverlayChanged();

        _whiteboard.Mode.Should().Be(WhiteboardMode.Off);
    }

    [Fact]
    public void A_state_change_that_keeps_drawing_leaves_it_on()
    {
        _whiteboard.Cycle();

        RaiseOverlayChanged();

        _whiteboard.Mode.Should().Be(WhiteboardMode.White);
    }

    [Fact]
    public void The_screen_clip_keeps_it_on_for_one_pointer_mode_only()
    {
        _whiteboard.Cycle();
        _whiteboard.KeepThroughNextPointerMode();
        _overlay.IsDrawingMode.Returns(false);

        RaiseOverlayChanged();
        _whiteboard.Mode.Should().Be(WhiteboardMode.White);

        RaiseOverlayChanged(); // otra salida del dibujo, sin el recorte
        _whiteboard.Mode.Should().Be(WhiteboardMode.Off);
    }

    [Fact]
    public void Going_back_to_drawing_forgets_the_pending_clip()
    {
        _whiteboard.Cycle();
        _whiteboard.KeepThroughNextPointerMode();
        _overlay.IsDrawingMode.Returns(true);
        RaiseOverlayChanged();

        _overlay.IsDrawingMode.Returns(false);
        RaiseOverlayChanged();

        _whiteboard.Mode.Should().Be(WhiteboardMode.Off);
    }

    [Fact]
    public void Without_canvases_the_cycle_does_nothing()
    {
        var canvases = Substitute.For<IOverlayCanvasProvider>();
        canvases.Canvases.Returns([]);
        using var empty = new WhiteboardController(canvases, _cursor, _settings, _overlay);

        empty.Cycle();

        empty.Mode.Should().Be(WhiteboardMode.Off);
    }
}
