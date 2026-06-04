namespace Acetato.Application.Abstractions;

/// <summary>
/// Aplica los estilos de ventana nativos que necesita el overlay (HU-01/HU-02).
/// La implementación (P/Invoke) vive en Infrastructure; aquí solo el contrato.
/// </summary>
public interface IOverlayWindowStyler
{
    /// <summary>Aplica los estilos extendidos del overlay al manejador dado.</summary>
    public void ApplyOverlayStyles(nint windowHandle);

    /// <summary>
    /// Activa o desactiva el click-through (HU-02): si está activo, los clics
    /// atraviesan la capa hacia la app de abajo; si no, la capa los captura.
    /// </summary>
    public void SetClickThrough(nint windowHandle, bool enabled);
}
