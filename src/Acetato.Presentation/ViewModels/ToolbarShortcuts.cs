using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using Acetato.Presentation.Settings;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// Crea los tooltips de la barra (HU-22) y les pone el atajo vigente de su
/// acción. Al cambiar un atajo en Ajustes (o al cargarse los guardados al
/// arrancar) el coordinador avisa y todos se actualizan sin reiniciar. Los
/// atajos fijos (Esc del Puntero, HU-24) no salen del mapa sino de
/// <see cref="FixedHotkeys"/>.
/// </summary>
public sealed class ToolbarShortcuts : IDisposable
{
    private readonly HotkeyCoordinator _coordinator;
    private readonly List<ShortcutHint> _hints = [];

    public ToolbarShortcuts(HotkeyCoordinator coordinator)
    {
        _coordinator = coordinator;
        _coordinator.Changed += OnCoordinatorChanged;
    }

    /// <summary>Tooltip con el nombre y, si <paramref name="action"/> tiene atajo, la combinación.</summary>
    public ShortcutHint Hint(string label, HotkeyAction? action = null)
    {
        var hint = new ShortcutHint(label, action);
        Refresh(hint);
        _hints.Add(hint);
        return hint;
    }

    public void Dispose() => _coordinator.Changed -= OnCoordinatorChanged;

    private void OnCoordinatorChanged(object? sender, EventArgs e)
    {
        foreach (var hint in _hints)
        {
            Refresh(hint);
        }
    }

    private void Refresh(ShortcutHint hint) =>
        hint.Shortcut = hint.Action is { } action && ChordOf(action) is { } chord
            ? KeyChordText.Format(chord)
            : null;

    private KeyChord? ChordOf(HotkeyAction action) =>
        FixedHotkeys.ChordOf(action) ?? _coordinator.Map.ChordOf(action);
}
