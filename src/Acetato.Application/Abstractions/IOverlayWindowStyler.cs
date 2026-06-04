namespace Acetato.Application.Abstractions;

/// <summary>
/// Aplica los estilos de ventana nativos que necesita el overlay (HU-01/HU-02).
/// La implementación (P/Invoke) vive en Infrastructure; aquí solo el contrato.
/// </summary>
public interface IOverlayWindowStyler
{
    /// <summary>Aplica los estilos extendidos del overlay al manejador dado.</summary>
    public void ApplyOverlayStyles(nint windowHandle);
}
