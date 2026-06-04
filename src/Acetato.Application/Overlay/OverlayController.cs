using Acetato.Application.Abstractions;

namespace Acetato.Application.Overlay;

/// <summary>
/// Orquesta la visibilidad del overlay (HU-01). Aplica los estilos nativos la
/// primera vez que la superficie obtiene manejador. Es agnóstico de plataforma:
/// depende solo de puertos (la superficie y el styler).
/// </summary>
public sealed class OverlayController : IOverlayController
{
    private readonly IOverlaySurface _surface;
    private readonly IOverlayWindowStyler _styler;
    private bool _stylesApplied;

    public OverlayController(IOverlaySurface surface, IOverlayWindowStyler styler)
    {
        _surface = surface;
        _styler = styler;
    }

    public bool IsVisible => _surface.IsVisible;

    public void Toggle()
    {
        if (_surface.IsVisible)
        {
            _surface.Hide();
            return;
        }

        _surface.Show();
        EnsureNativeStyles();
    }

    public void SetDrawingMode(bool enabled)
    {
        // HU-02: alternará WS_EX_TRANSPARENT (click-through). Aún sin implementar.
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
