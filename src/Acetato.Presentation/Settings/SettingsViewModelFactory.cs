using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Hotkeys;
using Acetato.Presentation.ViewModels;

namespace Acetato.Presentation.Settings;

/// <summary>
/// Crea el ViewModel de Ajustes con lo que necesita cada bloque: atajos (HU-21), Dibujo
/// (HU-27) y Acerca de (HU-33). Aparte de <see cref="SettingsWindowLauncher"/> para que
/// ninguno pase de cinco parámetros (S107).
/// </summary>
public sealed class SettingsViewModelFactory
{
    private readonly HotkeyCoordinator _coordinator;
    private readonly IWindowDragHandler _dragHandler;
    private readonly IDrawingSettings _drawing;
    private readonly IDrawingPreferencesStore _preferences;
    private readonly SettingsAboutServices _about;

    public SettingsViewModelFactory(
        HotkeyCoordinator coordinator,
        IWindowDragHandler dragHandler,
        IDrawingSettings drawing,
        IDrawingPreferencesStore preferences,
        SettingsAboutServices about)
    {
        _coordinator = coordinator;
        _dragHandler = dragHandler;
        _drawing = drawing;
        _preferences = preferences;
        _about = about;
    }

    /// <summary>Un ViewModel nuevo por cada apertura de la ventana.</summary>
    internal SettingsViewModel Create() => new(_coordinator, _dragHandler, _drawing, _preferences, _about);
}
