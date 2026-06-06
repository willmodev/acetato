using System.Windows.Threading;
using Acetato.Application.Abstractions;
using Acetato.Domain;
using Acetato.Presentation.ViewModels;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Enruta cada <see cref="HotkeyAction"/> a su manejador y lo marshala al hilo
/// de UI (los eventos del servicio de atajos llegan en su hilo de bombeo). El
/// mapeo es un diccionario para escalar a N atajos sin un switch que dispararía
/// la complejidad permitida.
/// </summary>
internal sealed class HotkeyActionRouter
{
    private readonly Dispatcher _dispatcher;
    private readonly Dictionary<HotkeyAction, Action> _handlers;

    public HotkeyActionRouter(IOverlayController controller, OverlayViewModel viewModel, Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        _handlers = BuildHandlers(controller, viewModel);
    }

    /// <summary>Manejador del evento <c>ActionRequested</c> del servicio de atajos.</summary>
    public void Handle(object? sender, HotkeyActionEventArgs e)
    {
        if (_handlers.TryGetValue(e.Action, out var handler))
        {
            _ = _dispatcher.BeginInvoke(handler);
        }
    }

    private static Dictionary<HotkeyAction, Action> BuildHandlers(IOverlayController controller, OverlayViewModel vm) => new()
    {
        [HotkeyAction.Toggle] = controller.Toggle,
        [HotkeyAction.DrawingModeToggle] = () => controller.SetDrawingMode(!controller.IsDrawingMode),
        [HotkeyAction.Clear] = () => vm.ClearCommand.Execute(null),
        [HotkeyAction.ColorRed] = () => vm.SetColorCommand.Execute(TintaColor.Red),
        [HotkeyAction.ColorBlue] = () => vm.SetColorCommand.Execute(TintaColor.Blue),
        [HotkeyAction.ColorYellow] = () => vm.SetColorCommand.Execute(TintaColor.Yellow),
        [HotkeyAction.ColorGreen] = () => vm.SetColorCommand.Execute(TintaColor.Green),
        [HotkeyAction.ColorWhite] = () => vm.SetColorCommand.Execute(TintaColor.White),
        [HotkeyAction.ColorBlack] = () => vm.SetColorCommand.Execute(TintaColor.Black),
        [HotkeyAction.Undo] = () => vm.UndoCommand.Execute(null),
        [HotkeyAction.CycleTool] = () => vm.CycleToolCommand.Execute(null),
    };
}
