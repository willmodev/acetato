namespace Acetato.Application.Abstractions;

/// <summary>
/// Superficie del overlay vista por la capa de aplicación: visibilidad, estilado
/// y click-through, sin acoplarse a WPF. Puede agrupar varias ventanas (una por
/// monitor, HU-09); por eso encapsula el estilado y el click-through en lugar de
/// exponer un único manejador. El adaptador concreto vive en Presentation.
/// </summary>
public interface IOverlaySurface
{
    /// <summary>Indica si la superficie está visible.</summary>
    public bool IsVisible { get; }

    /// <summary>Muestra la superficie (todas sus ventanas).</summary>
    public void Show();

    /// <summary>Oculta la superficie (todas sus ventanas).</summary>
    public void Hide();

    /// <summary>
    /// Aplica los estilos de ventana nativos del overlay (HU-01/HU-02) a todas
    /// sus ventanas. Idempotente: aplicarlo varias veces no tiene efecto extra.
    /// </summary>
    public void ApplyStyles();

    /// <summary>
    /// Activa o desactiva el click-through (HU-02) en todas sus ventanas: si está
    /// activo, los clics atraviesan la capa hacia la app de abajo; si no, la capa
    /// los captura (modo dibujo).
    /// </summary>
    public void SetClickThrough(bool clickThrough);
}
