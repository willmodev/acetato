using System.Windows.Threading;
using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Overlay;
using Acetato.Presentation.Overlay;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Enrutado del atajo de recorte (HU-23): el router marshala la acción al hilo
/// de UI y el broadcaster abre la herramienta del sistema.
/// </summary>
public sealed class HotkeyActionRouterTests
{
    [Fact]
    public void Capture_launches_the_screen_clip()
    {
        var screenClip = Substitute.For<IScreenClipLauncher>();

        WpfTestHost.Run(() => Route(HotkeyAction.Capture, screenClip));

        screenClip.Received(1).Launch();
    }

    // Enruta la acción y vacía la cola del dispatcher para que corra el manejador.
    private static bool Route(HotkeyAction action, IScreenClipLauncher screenClip)
    {
        var broadcaster = new OverlayBroadcaster(
            Substitute.For<IOverlayCanvasProvider>(),
            Substitute.For<ICursorPositionProvider>(),
            Substitute.For<IDrawingSettings>(),
            screenClip);
        var dispatcher = Dispatcher.CurrentDispatcher;
        var router = new HotkeyActionRouter(Substitute.For<IOverlayController>(), broadcaster, dispatcher);

        router.Handle(null, new HotkeyActionEventArgs(action));
        dispatcher.Invoke(static () => { }, DispatcherPriority.ContextIdle);
        return true;
    }
}
