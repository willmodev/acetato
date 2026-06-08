using Acetato.Application.Abstractions;
using Acetato.Application.Capture;
using Acetato.Application.Drawing;
using Acetato.Application.Overlay;
using Acetato.Domain;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Acetato.Application.Tests;

public sealed class OverlayBroadcasterTests
{
    private readonly ICursorPositionProvider _cursor = Substitute.For<ICursorPositionProvider>();
    private readonly IDrawingSettings _settings = Substitute.For<IDrawingSettings>();
    private readonly ICaptureService _capture = Substitute.For<ICaptureService>();
    private readonly IOverlayCanvasProvider _provider = Substitute.For<IOverlayCanvasProvider>();

    private OverlayBroadcaster CreateBroadcaster() => new(_provider, _cursor, _settings, _capture);

    private static IOverlayCanvas Canvas(MonitorInfo monitor)
    {
        var canvas = Substitute.For<IOverlayCanvas>();
        canvas.Monitor.Returns(monitor);
        return canvas;
    }

    [Fact]
    public void Clear_clears_every_canvas()
    {
        var first = Canvas(new MonitorInfo(0, 0, 1920, 1080, true));
        var second = Canvas(new MonitorInfo(1920, 0, 1280, 1024, false));
        _provider.Canvases.Returns([first, second]);

        CreateBroadcaster().Clear();

        first.Received(1).Clear();
        second.Received(1).Clear();
    }

    [Fact]
    public void Undo_targets_only_the_canvas_under_the_cursor()
    {
        var primary = Canvas(new MonitorInfo(0, 0, 1920, 1080, true));
        var secondary = Canvas(new MonitorInfo(1920, 0, 1280, 1024, false));
        _provider.Canvases.Returns([primary, secondary]);
        _cursor.GetCursorPosition().Returns((2000, 200)); // sobre el secundario

        CreateBroadcaster().Undo();

        secondary.Received(1).Undo();
        primary.DidNotReceive().Undo();
    }

    [Fact]
    public void Undo_does_nothing_when_there_are_no_canvases()
    {
        _provider.Canvases.Returns([]);

        CreateBroadcaster().Undo();

        _cursor.DidNotReceive().GetCursorPosition();
    }

    [Fact]
    public void SelectColor_updates_the_shared_settings()
    {
        CreateBroadcaster().SelectColor(TintaColor.Blue);

        _settings.Received(1).SelectColor(TintaColor.Blue);
    }

    [Fact]
    public void CycleTool_advances_the_shared_settings()
    {
        CreateBroadcaster().CycleTool();

        _settings.Received(1).CycleTool();
    }

    [Fact]
    public async Task CaptureAsync_invokes_the_capture_service()
    {
        _capture.CaptureAsync().Returns(new CaptureResult("ruta.png"));

        await CreateBroadcaster().CaptureAsync();

        _ = _capture.Received(1).CaptureAsync();
    }

    [Fact]
    public async Task CaptureAsync_swallows_io_failures()
    {
        _capture.CaptureAsync().ThrowsAsync(new IOException("disco lleno"));

        // No debe propagar: el caso de uso traga la falla de E/S.
        await CreateBroadcaster().CaptureAsync();

        _ = _capture.Received(1).CaptureAsync();
    }
}
