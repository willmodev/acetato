namespace Acetato.Application.Abstractions;

/// <summary>
/// Superficie de la barra de herramientas flotante (HU-10) vista por la capa de
/// aplicación: visibilidad y manejador nativo, sin acoplarse a WPF. El adaptador
/// concreto (la ventana) vive en Presentation.
/// </summary>
public interface IToolbarSurface
{
    /// <summary>Indica si la barra está visible.</summary>
    public bool IsVisible { get; }

    /// <summary>Manejador nativo de la ventana (0 si aún no existe).</summary>
    public nint Handle { get; }

    /// <summary>Muestra la barra.</summary>
    public void Show();

    /// <summary>Oculta la barra.</summary>
    public void Hide();
}
