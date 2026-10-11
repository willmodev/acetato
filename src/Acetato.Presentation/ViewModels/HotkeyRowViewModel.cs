using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Presentation.Settings;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// Fila de un atajo en Ajustes (HU-21): nombre de la acción y combinación
/// actual. Refleja el estado del <see cref="HotkeyCoordinator"/>, que es la
/// única fuente de verdad.
/// </summary>
internal sealed partial class HotkeyRowViewModel : ObservableObject
{
    private const string NoChordText = "Sin atajo";

    private readonly HotkeyCoordinator _coordinator;

    /// <summary>Combinación en texto: «Ctrl+Z» o «Sin atajo».</summary>
    [ObservableProperty]
    private string _chordText = NoChordText;

    /// <summary>Conserva el atajo por defecto.</summary>
    [ObservableProperty]
    private bool _isDefault;

    /// <summary>La combinación no se pudo registrar (otra app la tiene).</summary>
    [ObservableProperty]
    private bool _isUnavailable;

    public HotkeyRowViewModel(HotkeyAction action, HotkeyCoordinator coordinator)
    {
        Action = action;
        Label = HotkeyActionLabels.Of(action);
        _coordinator = coordinator;
        Refresh();
    }

    /// <summary>Acción de la fila.</summary>
    public HotkeyAction Action { get; }

    /// <summary>Nombre de la acción: «Deshacer», «Tinta roja»…</summary>
    public string Label { get; }

    /// <summary>Relee el estado del coordinador.</summary>
    public void Refresh()
    {
        var map = _coordinator.Map;
        ChordText = map.ChordOf(Action) is { } chord ? KeyChordText.Format(chord) : NoChordText;
        IsDefault = map.IsDefault(Action);
        IsUnavailable = _coordinator.Unavailable.Contains(Action);
    }
}
