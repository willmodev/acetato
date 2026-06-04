namespace Acetato.Application.Abstractions;

/// <summary>
/// Controla el ciclo de vida y el modo del overlay transparente (HU-01/HU-02):
/// mostrar/ocultar la capa y alternar entre click-through y captura de clics.
/// </summary>
public interface IOverlayController
{
    /// <summary>Indica si la capa está visible.</summary>
    public bool IsVisible { get; }

    /// <summary>Muestra u oculta la capa de dibujo.</summary>
    public void Toggle();

    /// <summary>
    /// Activa el modo dibujo (la capa captura los clics) o el modo normal
    /// (los clics atraviesan hacia la app de abajo).
    /// </summary>
    public void SetDrawingMode(bool enabled);
}
