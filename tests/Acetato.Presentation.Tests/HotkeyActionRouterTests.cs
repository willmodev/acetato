using System.Windows.Threading;
using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Overlay;
using Acetato.Presentation.Overlay;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Enrutado de atajos: el router marshala la acción al hilo de UI. El recorte
/// abre la herramienta del sistema (HU-23) y Esc sale del modo dibujo (HU-24).
/// </summary>
public sealed class HotkeyActionRouterTests
{
    [Fact]
    public void Capture_launches_the_screen_clip()
    {
        var screenClip = Substitute.For<IScreenClipLauncher>();

        WpfTestHost.Run(() => Route(HotkeyAction.Capture, screenClip, Substitute.For<IOverlayController>()));

        screenClip.Received(1).Launch();
    }

    [Fact]
    public void Esc_leaves_drawing_mode()
    {
        var controller = Substitute.For<IOverlayController>();

        WpfTestHost.Run(() => Route(HotkeyAction.EnterPointerMode, Substitute.For<IScreenClipLauncher>(), controller));

        controller.Received(1).SetDrawingMode(false);
    }

    // Enruta la acción y vacía la cola del dispatcher para que corra el manejador.
    private static bool Route(HotkeyAction action, IScreenClipLauncher screenClip, IOverlayController controller)
    {
        var broadcaster = new OverlayBroadcaster(
            Substitute.For<IOverlayCanvasProvider>(),
            Substitute.For<ICursorPositionProvider>(),
            Substitute.For<IDrawingSettings>(),
            screenClip);
        var dispatcher = Dispatcher.CurrentDispatcher;
        var router = new HotkeyActionRouter(controller, broadcaster, dispatcher);

        router.Handle(null, new HotkeyActionEventArgs(action));
        dispatcher.Invoke(static () => { }, DispatcherPriority.ContextIdle);
        return true;
    }
}
