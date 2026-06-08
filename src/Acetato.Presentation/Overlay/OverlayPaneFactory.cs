using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Domain;
using Acetato.Presentation.ViewModels;
using Acetato.Presentation.Views;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Fábrica de panes del overlay (HU-09): crea una ventana con su ViewModel por
/// monitor. Cada pane tiene sus propios trazos/undo, pero comparten el mismo
/// <see cref="IDrawingSettings"/>, así color/grosor/herramienta se mantienen
/// sincronizados entre pantallas. El posicionamiento físico lo aporta el placer.
/// </summary>
public sealed class OverlayPaneFactory : IOverlayPaneFactory
{
    private readonly IDrawingSettings _settings;
    private readonly IOverlayWindowStyler _styler;
    private readonly IOverlayWindowPlacer _placer;

    public OverlayPaneFactory(
        IDrawingSettings settings,
        IOverlayWindowStyler styler,
        IOverlayWindowPlacer placer)
    {
        _settings = settings;
        _styler = styler;
        _placer = placer;
    }

    public OverlayPane Create(MonitorInfo monitor)
    {
        var viewModel = new OverlayViewModel(_settings);
        var window = new OverlayWindow { DataContext = viewModel };
        return new OverlayPane(window, viewModel, _styler, _placer, monitor);
    }
}
