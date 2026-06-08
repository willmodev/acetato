using Acetato.Application.Abstractions;

namespace Acetato.Application.Overlay;

/// <summary>
/// Orquesta la visibilidad y el modo del overlay y de la barra flotante
/// (HU-01/HU-02/HU-10). La superficie del overlay puede agrupar varias ventanas
/// (una por monitor, HU-09); este controlador es agnóstico de su número: delega
/// el estilado y el click-through en la propia superficie. Al mostrarse arranca
/// en modo dibujo (la capa captura los clics); el modo normal activa el
/// click-through. La barra se muestra y oculta junto al overlay. Es agnóstico de
/// plataforma: depende solo de puertos.
/// </summary>
public sealed class OverlayController : IOverlayController
{
    private readonly IOverlaySurface _surface;
    private readonly IToolbarSurface _toolbar;
    private readonly IToolbarWindowStyler _toolbarStyler;
    private bool _toolbarStylesApplied;
    private bool _drawingMode;

    public OverlayController(
        IOverlaySurface surface,
        IToolbarSurface toolbar,
        IToolbarWindowStyler toolbarStyler)
    {
        _surface = surface;
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
        // Modo dibujo = la capa captura los clics = sin click-through.
        _surface.SetClickThrough(!enabled);
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
        // La superficie aplica sus estilos de forma idempotente a cada ventana.
        _surface.ApplyStyles();
        _toolbarStylesApplied = TryApplyToolbarStyles(_toolbarStylesApplied, _toolbar.Handle);
    }

    private bool TryApplyToolbarStyles(bool alreadyApplied, nint handle)
    {
        if (alreadyApplied || handle == nint.Zero)
        {
            return alreadyApplied;
        }

        _toolbarStyler.ApplyToolbarStyles(handle);
        return true;
    }
}
