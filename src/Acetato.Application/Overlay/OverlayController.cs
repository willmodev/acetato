using Acetato.Application.Abstractions;

namespace Acetato.Application.Overlay;

/// <summary>
/// Orquesta la visibilidad y el modo del overlay y de la barra flotante
/// (HU-01/HU-02/HU-10). Aplica los estilos nativos de cada superficie la primera
/// vez que obtiene manejador. Al mostrarse arranca en modo dibujo (la capa
/// captura los clics); el modo normal activa el click-through del overlay. La
/// barra se muestra y oculta junto al overlay. Es agnóstico de plataforma:
/// depende solo de puertos.
/// </summary>
public sealed class OverlayController : IOverlayController
{
    private readonly IOverlaySurface _surface;
    private readonly IOverlayWindowStyler _styler;
    private readonly IToolbarSurface _toolbar;
    private readonly IToolbarWindowStyler _toolbarStyler;
    private bool _overlayStylesApplied;
    private bool _toolbarStylesApplied;
    private bool _drawingMode;

    public OverlayController(
        IOverlaySurface surface,
        IOverlayWindowStyler styler,
        IToolbarSurface toolbar,
        IToolbarWindowStyler toolbarStyler)
    {
        _surface = surface;
        _styler = styler;
        _toolbar = toolbar;
        _toolbarStyler = toolbarStyler;
    }

    public bool IsVisible => _surface.IsVisible;

    public bool IsDrawingMode => _drawingMode;

    public void Toggle()
    {
        if (_surface.IsVisible)
        {
            HideAll();
            return;
        }

        ShowAll();
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

    private void HideAll()
    {
        _surface.Hide();
        _toolbar.Hide();
    }

    private void ShowAll()
    {
        // El overlay primero; la barra después para quedar encima entre ventanas topmost.
        _surface.Show();
        _toolbar.Show();
        EnsureNativeStyles();
        SetDrawingMode(true);
    }

    private void EnsureNativeStyles()
    {
        _overlayStylesApplied = TryApplyStyles(_overlayStylesApplied, _surface.Handle, _styler.ApplyOverlayStyles);
        _toolbarStylesApplied = TryApplyStyles(_toolbarStylesApplied, _toolbar.Handle, _toolbarStyler.ApplyToolbarStyles);
    }

    private static bool TryApplyStyles(bool alreadyApplied, nint handle, Action<nint> apply)
    {
        if (alreadyApplied || handle == nint.Zero)
        {
            return alreadyApplied;
        }

        apply(handle);
        return true;
    }
}
