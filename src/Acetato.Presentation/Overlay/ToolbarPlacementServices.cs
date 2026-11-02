using Acetato.Application.Abstractions;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Lo que la barra necesita para volver a su sitio (HU-29): dónde la dejó el usuario y
/// cómo leer y fijar su posición en píxeles físicos. Va junto para no pasar de cinco
/// parámetros en <see cref="ToolbarWindowSurface"/> (S107).
/// </summary>
/// <param name="Layout">Estado compartido de la barra, con su sitio recordado.</param>
/// <param name="Placer">Lee y fija la posición de la ventana en píxeles físicos.</param>
public sealed record ToolbarPlacementServices(IToolbarLayout Layout, IToolbarWindowPlacer Placer);
