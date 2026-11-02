using Acetato.Domain;

namespace Acetato.Application.Abstractions;

/// <summary>
/// Lo que el usuario eligió en la última sesión y Acetato recupera al arrancar (HU-29).
/// </summary>
/// <param name="Ink">Última tinta sólida.</param>
/// <param name="Gradient">Modo Degradado (neón) activo.</param>
/// <param name="ThicknessIndex">Paso de grosor en la escala.</param>
/// <param name="Tool">Herramienta activa.</param>
/// <param name="ToolbarCollapsed">Barra plegada con el ojo.</param>
/// <param name="ToolbarPlacement">Dónde dejó el usuario la barra; <c>null</c> si nunca la movió.</param>
public sealed record LastSession(
    TintaColor Ink,
    bool Gradient,
    int ThicknessIndex,
    ToolKind Tool,
    bool ToolbarCollapsed,
    ToolbarPlacement? ToolbarPlacement)
{
    /// <summary>Lo de fábrica: Lápiz rojo fino y barra desplegada en su sitio de siempre.</summary>
    public static LastSession Defaults { get; } = new(
        TintaColor.Red, Gradient: false, ThicknessScale.DefaultIndex, ToolKind.Pencil, ToolbarCollapsed: false, ToolbarPlacement: null);
}
