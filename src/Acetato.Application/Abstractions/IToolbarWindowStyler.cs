namespace Acetato.Application.Abstractions;

/// <summary>
/// Aplica los estilos de ventana nativos que necesita la barra flotante (HU-10):
/// fuera del Alt-Tab, siempre encima y, en especial, que no robe el foco a la app
/// de abajo ni al overlay. La implementación (P/Invoke) vive en Infrastructure.
/// </summary>
public interface IToolbarWindowStyler
{
    /// <summary>Aplica los estilos extendidos de la barra al manejador dado.</summary>
    public void ApplyToolbarStyles(nint windowHandle);
}
