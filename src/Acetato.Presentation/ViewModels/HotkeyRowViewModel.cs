using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using Acetato.Presentation.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// Fila de un atajo en Ajustes (HU-21): nombre de la acción, combinación actual
/// y su edición. Al pulsar la combinación entra en captura (todos los atajos en
/// pausa) hasta recibir una válida, <c>Esc</c> o un conflicto con otra acción,
/// que ofrece Reemplazar. El <see cref="HotkeyCoordinator"/> es la única fuente
/// de verdad; la fila solo refleja su estado.
/// </summary>
internal sealed partial class HotkeyRowViewModel : ObservableObject
{
    private readonly HotkeyCoordinator _coordinator;
    private KeyChord? _pendingChord;

    /// <summary>Combinación actual en texto: «Ctrl+Z» o «Sin atajo».</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChordText))]
    private string _currentChordText = HotkeyMessages.NoChord;

    /// <summary>La fila espera una combinación.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChordText))]
    private bool _isCapturing;

    /// <summary>Conserva el atajo por defecto (si no, se ofrece Restaurar).</summary>
    [ObservableProperty]
    private bool _isDefault;

    /// <summary>Tiene combinación (si no, no hay nada que quitar).</summary>
    [ObservableProperty]
    private bool _hasChord;

    /// <summary>La combinación no se pudo registrar (otra app la tiene).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowsUnavailable))]
    private bool _isUnavailable;

    /// <summary>Error o conflicto en línea; <c>null</c> si no hay.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMessage))]
    [NotifyPropertyChangedFor(nameof(ShowsUnavailable))]
    private string? _message;

    /// <summary>El conflicto es con otra acción: se ofrece Reemplazar.</summary>
    [ObservableProperty]
    private bool _canReplace;

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

    /// <summary>Texto de la combinación: la actual o «Pulsa la combinación…».</summary>
    public string ChordText => IsCapturing ? HotkeyMessages.Capturing : CurrentChordText;

    /// <summary>Hay un mensaje que mostrar bajo la fila.</summary>
    public bool HasMessage => Message is not null;

    /// <summary>Se muestra «No disponible» (un mensaje en curso tiene prioridad).</summary>
    public bool ShowsUnavailable => IsUnavailable && !HasMessage;

    /// <summary>Relee el estado del coordinador.</summary>
    public void Refresh()
    {
        var map = _coordinator.Map;
        var chord = map.ChordOf(Action);
        HasChord = chord.HasValue;
        CurrentChordText = chord is { } current ? KeyChordText.Format(current) : HotkeyMessages.NoChord;
        IsDefault = map.IsDefault(Action);
        IsUnavailable = _coordinator.Unavailable.Contains(Action);
    }

    /// <summary>Entra en captura: pausa todos los atajos y espera la combinación.</summary>
    [RelayCommand]
    private void StartCapture()
    {
        if (IsCapturing)
        {
            return;
        }

        ClearMessage();
        // Primero el estado (Ajustes cancela otra fila que capture) y luego la pausa.
        IsCapturing = true;
        _coordinator.BeginCapture();
    }

    /// <summary>
    /// Recibe la combinación pulsada; <c>null</c> = tecla no admitida. Si no se
    /// asigna por un error, la captura sigue para probar otra.
    /// </summary>
    [RelayCommand]
    private void Capture(KeyChord? chord)
    {
        if (!IsCapturing)
        {
            return;
        }

        if (chord is not { } pressed)
        {
            Message = HotkeyMessages.UnsupportedKey;
            return;
        }

        Show(_coordinator.Assign(Action, pressed), pressed);
    }

    /// <summary>Cancela la captura o el conflicto pendiente sin cambios.</summary>
    [RelayCommand]
    private void CancelCapture()
    {
        ClearMessage();
        EndCapture();
    }

    /// <summary>Asigna la combinación en conflicto y deja sin atajo a la otra acción.</summary>
    [RelayCommand]
    private void Replace()
    {
        if (_pendingChord is { } chord)
        {
            ClearMessage();
            _coordinator.Replace(Action, chord);
        }
    }

    /// <summary>Deja la acción sin atajo; la combinación queda libre para otras apps.</summary>
    [RelayCommand]
    private void Remove()
    {
        CancelCapture();
        _coordinator.Remove(Action);
    }

    /// <summary>Devuelve la acción a su atajo por defecto.</summary>
    [RelayCommand]
    private void Reset()
    {
        CancelCapture();
        _coordinator.Reset(Action);
    }

    private void Show(AssignResult result, KeyChord chord)
    {
        switch (result)
        {
            case ChordInvalid invalid:
                Message = HotkeyMessages.Of(invalid.Error);
                break;
            case ChordTakenByOtherApp:
                Message = HotkeyMessages.TakenByOtherApp;
                break;
            case ChordTakenByAction taken:
                EndCapture();
                Message = HotkeyMessages.TakenBy(taken.Owner);
                CanReplace = true;
                _pendingChord = chord;
                break;
            default:
                ClearMessage();
                EndCapture();
                break;
        }
    }

    private void EndCapture()
    {
        if (!IsCapturing)
        {
            return;
        }

        IsCapturing = false;
        _coordinator.EndCapture();
    }

    private void ClearMessage()
    {
        Message = null;
        CanReplace = false;
        _pendingChord = null;
    }
}
