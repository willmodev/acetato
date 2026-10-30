using System.ComponentModel;
using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Overlay;
using Acetato.Domain;
using NSubstitute;
using Xunit;

namespace Acetato.Application.Tests;

public sealed class OverlayBroadcasterTests
{
    private readonly ICursorPositionProvider _cursor = Substitute.For<ICursorPositionProvider>();
    private readonly IDrawingSettings _settings = Substitute.For<IDrawingSettings>();
    private readonly IScreenClipLauncher _screenClip = Substitute.For<IScreenClipLauncher>();
    private readonly IOverlayCanvasProvider _provider = Substitute.For<IOverlayCanvasProvider>();
    private readonly IOverlayController _overlay = Substitute.For<IOverlayController>();

    private OverlayBroadcaster CreateBroadcaster() => new(_provider, _cursor, _settings, _screenClip, _overlay);

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
    public void TryCommitText_commits_on_every_canvas_and_reports_if_any_was_editing()
    {
        var idle = Canvas(new MonitorInfo(0, 0, 1920, 1080, true));
        var editing = Canvas(new MonitorInfo(1920, 0, 1280, 1024, false));
        editing.TryCommitText().Returns(true);
        _provider.Canvases.Returns([idle, editing]);

        var committed = CreateBroadcaster().TryCommitText();

        Assert.True(committed);
        idle.Received(1).TryCommitText();
        editing.Received(1).TryCommitText();
    }

    [Fact]
    public void TryCommitText_is_false_when_no_text_is_being_edited()
    {
        var idle = Canvas(new MonitorInfo(0, 0, 1920, 1080, true));
        _provider.Canvases.Returns([idle]);

        Assert.False(CreateBroadcaster().TryCommitText());
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
    public void SelectGradient_updates_the_shared_settings()
    {
        CreateBroadcaster().SelectGradient();

        _settings.Received(1).SelectGradient();
    }

    [Fact]
    public void CycleTool_advances_the_shared_settings()
    {
        CreateBroadcaster().CycleTool();

        _settings.Received(1).CycleTool();
    }

    [Fact]
    public void SelectTool_updates_the_shared_settings()
    {
        CreateBroadcaster().SelectTool(ToolKind.Text);

        _settings.Received(1).SelectTool(ToolKind.Text);
    }

    [Fact]
    public void IncreaseThickness_moves_the_shared_settings_one_step_up()
    {
        CreateBroadcaster().IncreaseThickness();

        _settings.Received(1).IncreaseThickness();
    }

    [Fact]
    public void DecreaseThickness_moves_the_shared_settings_one_step_down()
    {
        CreateBroadcaster().DecreaseThickness();

        _settings.Received(1).DecreaseThickness();
    }

    [Fact]
    public void LaunchScreenClip_opens_the_system_screen_clip()
    {
        CreateBroadcaster().LaunchScreenClip();

        _screenClip.Received(1).Launch();
    }

    [Fact]
    public void LaunchScreenClip_leaves_drawing_mode_before_opening()
    {
        _overlay.IsDrawingMode.Returns(true);

        CreateBroadcaster().LaunchScreenClip();

        Received.InOrder(() =>
        {
            _overlay.SetDrawingMode(false);
            _screenClip.Launch();
        });
    }

    [Fact]
    public void LaunchScreenClip_in_pointer_mode_does_not_touch_the_mode()
    {
        _overlay.IsDrawingMode.Returns(false);

        CreateBroadcaster().LaunchScreenClip();

        _overlay.DidNotReceive().SetDrawingMode(Arg.Any<bool>());
        _screenClip.Received(1).Launch();
    }

    [Fact]
    public void LaunchScreenClip_swallows_launch_failures()
    {
        _screenClip.When(launcher => launcher.Launch()).Do(_ => throw new Win32Exception("protocolo no registrado"));

        // No debe propagar: si Windows no abre el recorte, el atajo no hace nada.
        CreateBroadcaster().LaunchScreenClip();

        _screenClip.Received(1).Launch();
    }
}
