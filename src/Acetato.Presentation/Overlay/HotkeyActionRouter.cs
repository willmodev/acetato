using System.Windows.Threading;
using Acetato.Application.Abstractions;
using Acetato.Application.Overlay;
using Acetato.Domain;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Enruta cada <see cref="HotkeyAction"/> a su manejador y lo marshala al hilo
/// de UI (los eventos del servicio de atajos llegan en su hilo de bombeo). El
/// mapeo es un diccionario para escalar a N atajos sin un switch que dispararía
/// la complejidad permitida. Las acciones de dibujo van por el
/// <see cref="OverlayBroadcaster"/>, que las reparte entre las panes (HU-09).
/// </summary>
internal sealed class HotkeyActionRouter
{
    private readonly Dispatcher _dispatcher;
    private readonly Dictionary<HotkeyAction, Action> _handlers;

    public HotkeyActionRouter(IOverlayController controller, OverlayBroadcaster broadcaster, Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        _handlers = BuildHandlers(controller, broadcaster);
    }

    /// <summary>Manejador del evento <c>ActionRequested</c> del servicio de atajos.</summary>
    public void Handle(object? sender, HotkeyActionEventArgs e)
    {
        if (_handlers.TryGetValue(e.Action, out var handler))
        {
            _ = _dispatcher.BeginInvoke(handler);
        }
    }

    private static Dictionary<HotkeyAction, Action> BuildHandlers(IOverlayController controller, OverlayBroadcaster broadcaster) => new()
    {
        [HotkeyAction.Toggle] = controller.Toggle,
        [HotkeyAction.DrawingModeToggle] = () => controller.SetDrawingMode(!controller.IsDrawingMode),
        [HotkeyAction.Clear] = broadcaster.Clear,
        [HotkeyAction.ColorRed] = () => broadcaster.SelectColor(TintaColor.Red),
        [HotkeyAction.ColorBlue] = () => broadcaster.SelectColor(TintaColor.Blue),
        [HotkeyAction.ColorYellow] = () => broadcaster.SelectColor(TintaColor.Yellow),
        [HotkeyAction.ColorGreen] = () => broadcaster.SelectColor(TintaColor.Green),
        [HotkeyAction.ColorWhite] = () => broadcaster.SelectColor(TintaColor.White),
        [HotkeyAction.ColorBlack] = () => broadcaster.SelectColor(TintaColor.Black),
        [HotkeyAction.Undo] = broadcaster.Undo,
        [HotkeyAction.CycleTool] = broadcaster.CycleTool,
        [HotkeyAction.Capture] = () => _ = broadcaster.CaptureAsync(),
        [HotkeyAction.SelectTextTool] = () => broadcaster.SelectTool(ToolKind.Text),
        [HotkeyAction.SelectLaserTool] = () => broadcaster.SelectTool(ToolKind.Laser),
    };
}
