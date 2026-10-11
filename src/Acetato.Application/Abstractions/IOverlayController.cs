namespace Acetato.Application.Abstractions;

/// <summary>
/// Controla el ciclo de vida y el modo del overlay transparente (HU-01/HU-02):
/// mostrar/ocultar la capa y alternar entre click-through y captura de clics.
/// </summary>
public interface IOverlayController
{
    /// <summary>Indica si la capa está visible.</summary>
    public bool IsVisible { get; }

    /// <summary>Indica si la capa está en modo dibujo (captura los clics).</summary>
    public bool IsDrawingMode { get; }

    /// <summary>
    /// Se dispara al mostrar u ocultar la capa y al cambiar de modo (HU-21): los
    /// atajos de dibujo solo se registran con la capa visible y en modo dibujo.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>Muestra u oculta la capa de dibujo.</summary>
    public void Toggle();

    /// <summary>
    /// Activa el modo dibujo (la capa captura los clics) o el modo normal
    /// (los clics atraviesan hacia la app de abajo).
    /// </summary>
    public void SetDrawingMode(bool enabled);
}
