using Acetato.Application.Abstractions;
using Acetato.Domain;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Superficie de overlay para multi-monitor (HU-09): agrega una
/// <see cref="OverlayPane"/> por monitor e implementa <see cref="IOverlaySurface"/>
/// (el controlador la trata como una sola superficie) y
/// <see cref="IOverlayCanvasProvider"/> (el broadcaster reparte las acciones). Las
/// panes se crean al primer mostrar y se reconstruyen si la topología de
/// pantallas cambia.
/// </summary>
public sealed class MultiMonitorOverlaySurface : IOverlaySurface, IOverlayCanvasProvider, IDisposable
{
    private readonly IOverlayPaneFactory _factory;
    private readonly IMonitorService _monitors;
    private readonly List<OverlayPane> _panes = [];
    private IReadOnlyList<MonitorInfo> _builtFor = [];

    public MultiMonitorOverlaySurface(IOverlayPaneFactory factory, IMonitorService monitors)
    {
        _factory = factory;
        _monitors = monitors;
    }

    public bool IsVisible => _panes.Count > 0 && _panes[0].IsVisible;

    public IReadOnlyList<IOverlayCanvas> Canvases => _panes;

    public void Show()
    {
        EnsurePanes();
        foreach (var pane in _panes)
        {
            pane.Show();
        }
    }

    public void Hide()
    {
        foreach (var pane in _panes)
        {
            pane.Hide();
        }
    }

    public void ApplyStyles()
    {
        foreach (var pane in _panes)
        {
            pane.ApplyStyles();
        }
    }

    public void SetClickThrough(bool clickThrough)
    {
        foreach (var pane in _panes)
        {
            pane.SetClickThrough(clickThrough);
        }
    }

    public void Dispose() => DisposePanes();

    // Crea una pane por monitor; reconstruye si la topología cambió (HU-09).
    private void EnsurePanes()
    {
        var monitors = _monitors.GetMonitors();
        if (_panes.Count > 0 && monitors.SequenceEqual(_builtFor))
        {
            return;
        }

        DisposePanes();
        foreach (var monitor in monitors)
        {
            _panes.Add(_factory.Create(monitor));
        }

        _builtFor = monitors;
    }

    private void DisposePanes()
    {
        foreach (var pane in _panes)
        {
            pane.Dispose();
        }

        _panes.Clear();
        _builtFor = [];
    }
}
