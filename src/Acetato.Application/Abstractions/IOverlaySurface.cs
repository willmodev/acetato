namespace Acetato.Application.Abstractions;

/// <summary>
/// Superficie del overlay vista por la capa de aplicación: visibilidad y
/// manejador nativo, sin acoplarse a WPF. El adaptador concreto (la ventana)
/// vive en Presentation.
/// </summary>
public interface IOverlaySurface
{
    /// <summary>Indica si la superficie está visible.</summary>
    public bool IsVisible { get; }

    /// <summary>Manejador nativo de la ventana (0 si aún no existe).</summary>
    public nint Handle { get; }

    /// <summary>Muestra la superficie.</summary>
    public void Show();

    /// <summary>Oculta la superficie.</summary>
    public void Hide();
}
