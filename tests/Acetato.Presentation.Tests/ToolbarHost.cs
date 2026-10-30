using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Overlay;
using Acetato.Presentation.Settings;
using Acetato.Presentation.ViewModels;
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
        var broadcaster = new OverlayBroadcaster(
            Substitute.For<IOverlayCanvasProvider>(),
            Substitute.For<ICursorPositionProvider>(),
            settings,
            Substitute.For<IScreenClipLauncher>(),
            controller);
        var layout = new ToolbarLayout();
        var launcher = new SettingsWindowLauncher(controller, coordinator, dragHandler);
        var chrome = new ToolbarChrome(dragHandler, launcher, shortcuts, layout);
        using var viewModel = new ToolbarViewModel(broadcaster, settings, controller, chrome);
        return work(new ToolbarContext(settings, controller, layout, viewModel));
    }
}
