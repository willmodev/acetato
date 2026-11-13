using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Overlay;
using Acetato.Domain;
using Acetato.Presentation.Settings;
using Acetato.Presentation.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Barra con el overlay ya visible en modo dibujo, como al arrancar la app. Usa
/// el controlador, el estado de dibujo y el estado plegado reales, con
/// superficies falsas. Debe llamarse dentro de <see cref="WpfTestHost.Run{T}"/>.
/// </summary>
internal static class ToolbarHost
{
    public static T With<T>(Func<ToolbarContext, T> work)
    {
        using var coordinator = TestCoordinators.Started();
        using var shortcuts = new ToolbarShortcuts(coordinator);
        var settings = new DrawingSettings();
        var surface = Substitute.For<IOverlaySurface>();
        var controller = new OverlayController(
            surface,
            Substitute.For<IToolbarSurface>(),
            Substitute.For<IToolbarWindowStyler>());
        controller.Toggle(); // oculta → visible y en modo dibujo
        surface.IsVisible.Returns(true);
        var dragHandler = Substitute.For<IWindowDragHandler>();
        var canvas = Substitute.For<IOverlayCanvas>();
        canvas.Monitor.Returns(new MonitorInfo(0, 0, 1920, 1080, IsPrimary: true));
        var canvases = Substitute.For<IOverlayCanvasProvider>();
        canvases.Canvases.Returns([canvas]);
        var cursor = Substitute.For<ICursorPositionProvider>();
        cursor.GetCursorPosition().Returns((10, 10));
        using var whiteboard = new WhiteboardController(canvases, cursor, settings, controller);
        var broadcaster = new OverlayBroadcaster(
            canvases,
            cursor,
            settings,
            new ScreenClipServices(Substitute.For<IScreenClipLauncher>(), NullLogger<OverlayBroadcaster>.Instance, whiteboard),
            controller);
        var layout = new ToolbarLayout();
        var launcher = new SettingsWindowLauncher(
            controller,
            new SettingsViewModelFactory(
                coordinator, dragHandler, settings, Substitute.For<IDrawingPreferencesStore>(), TestAbout.Services()));
        var chrome = new ToolbarChrome(dragHandler, launcher, shortcuts, layout, whiteboard);
        using var viewModel = new ToolbarViewModel(broadcaster, settings, controller, chrome);
        return work(new ToolbarContext(settings, controller, layout, viewModel, whiteboard, canvas));
    }
}
