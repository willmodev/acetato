using Acetato.Application.Abstractions;

namespace Acetato.Application.Overlay;

/// <summary>
/// Orquesta la visibilidad y el modo del overlay (HU-01/HU-02). Aplica los
/// estilos nativos la primera vez que la superficie obtiene manejador. Al
/// mostrarse arranca en modo dibujo (captura clics); el modo normal activa el
/// click-through. Es agnóstico de plataforma: depende solo de puertos.
/// </summary>
public sealed class OverlayController : IOverlayController
{
    private readonly IOverlaySurface _surface;
    private readonly IOverlayWindowStyler _styler;
    private bool _stylesApplied;
    private bool _drawingMode;

    public OverlayController(IOverlaySurface surface, IOverlayWindowStyler styler)
    {
        _surface = surface;
        _styler = styler;
    }

    public bool IsVisible => _surface.IsVisible;

    public bool IsDrawingMode => _drawingMode;

    public void Toggle()
    {
        if (_surface.IsVisible)
        {
            _surface.Hide();
            return;
        }

        _surface.Show();
        EnsureNativeStyles();
        SetDrawingMode(true);
    }

    public void SetDrawingMode(bool enabled)
    {
        _drawingMode = enabled;

        nint handle = _surface.Handle;
        if (handle == nint.Zero)
        {
            return;
        }

        // Modo dibujo = la capa captura los clics = sin click-through.
        _styler.SetClickThrough(handle, !enabled);
    }

    private void EnsureNativeStyles()
    {
        if (_stylesApplied)
        {
            return;
        }

        nint handle = _surface.Handle;
        if (handle == nint.Zero)
        {
            return;
        }

        _styler.ApplyOverlayStyles(handle);
        _stylesApplied = true;
    }
}
