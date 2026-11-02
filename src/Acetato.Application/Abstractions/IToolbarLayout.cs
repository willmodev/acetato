using Acetato.Domain;

namespace Acetato.Application.Abstractions;

/// <summary>
/// Estado de la barra compartido sin que sus partes se conozcan entre sí: plegada
/// (HU-25), que comparten el botón del ojo y el atajo «Plegar barra», y el sitio donde
/// el usuario la dejó (HU-29). Al arrancar, la última sesión los recupera.
/// </summary>
public interface IToolbarLayout
{
    /// <summary>Indica si la barra está plegada a asa, separador y ojo.</summary>
    public bool IsCollapsed { get; }

    /// <summary>Dónde dejó el usuario la barra; <c>null</c> si nunca la movió.</summary>
    public ToolbarPlacement? Placement { get; }

    /// <summary>Se dispara cada vez que la barra se pliega o se despliega.</summary>
    public event EventHandler? Changed;

    /// <summary>Pliega la barra si está desplegada, y al revés.</summary>
    public void ToggleCollapsed();

    /// <summary>
    /// Recuerda dónde quedó la barra. No dispara <see cref="Changed"/>: se llama a cada
    /// paso del arrastre y se guarda al salir.
    /// </summary>
    public void Remember(ToolbarPlacement placement);
}
