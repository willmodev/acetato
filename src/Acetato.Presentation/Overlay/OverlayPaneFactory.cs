using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Domain;
using Acetato.Presentation.ViewModels;
using Acetato.Presentation.Views;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Fábrica de panes del overlay (HU-09): crea una ventana con su ViewModel por
/// monitor. Cada pane tiene sus propias hojas (trazos/undo), pero comparten el mismo
/// <see cref="IDrawingSettings"/>, así color/grosor/herramienta se mantienen
/// sincronizados entre pantallas. El posicionamiento físico lo aporta el placer.
/// </summary>
public sealed class OverlayPaneFactory : IOverlayPaneFactory
{
    private readonly IDrawingSettings _settings;
    private readonly IOverlayWindowStyler _styler;
    private readonly IOverlayWindowPlacer _placer;
    private readonly IModifierKeyState _keyState;

    public OverlayPaneFactory(
        IDrawingSettings settings,
        IOverlayWindowStyler styler,
        IOverlayWindowPlacer placer,
        IModifierKeyState keyState)
    {
        _settings = settings;
        _styler = styler;
        _placer = placer;
        _keyState = keyState;
    }

    public OverlayPane Create(MonitorInfo monitor)
    {
        // Dos hojas por monitor (HU-35): la de la pantalla y la de la pizarra, cada una con su historial.
        var sheets = new OverlaySheets(new OverlayViewModel(_settings, _keyState), new OverlayViewModel(_settings, _keyState));
        var window = new OverlayWindow { DataContext = sheets };
        return new OverlayPane(window, sheets, _styler, _placer, monitor);
    }
}
