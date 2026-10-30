namespace Acetato.Application.Abstractions;

/// <summary>
/// Estado plegado de la barra (HU-25), compartido por el botón del ojo y el
/// atajo «Plegar barra» sin que se conozcan entre sí. Solo dura la sesión: al
/// arrancar la app la barra está desplegada.
/// </summary>
public interface IToolbarLayout
{
    /// <summary>Indica si la barra está plegada a asa, separador y ojo.</summary>
    public bool IsCollapsed { get; }

    /// <summary>Se dispara cada vez que la barra se pliega o se despliega.</summary>
    public event EventHandler? Changed;

    /// <summary>Pliega la barra si está desplegada, y al revés.</summary>
    public void ToggleCollapsed();
}
