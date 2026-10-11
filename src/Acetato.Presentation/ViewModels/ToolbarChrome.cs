using Acetato.Application.Abstractions;
using Acetato.Presentation.Settings;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// Servicios de la barra que no son de dibujo: el arrastre desde el grip
/// (HU-10), abrir Ajustes (HU-21) y los tooltips con atajo (HU-22). Agruparlos
/// mantiene el constructor de <see cref="ToolbarViewModel"/> dentro del tope de
/// parámetros.
/// </summary>
/// <param name="DragHandler">Inicia el arrastre nativo de la ventana.</param>
/// <param name="SettingsLauncher">Abre la ventana Ajustes.</param>
/// <param name="Shortcuts">Crea los tooltips y los mantiene al día.</param>
public sealed record ToolbarChrome(
    IWindowDragHandler DragHandler,
    SettingsWindowLauncher SettingsLauncher,
    ToolbarShortcuts Shortcuts);
