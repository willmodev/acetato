using System.ComponentModel;
using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// ViewModel de la ventana Ajustes (HU-21). Por ahora una sola sección,
/// Atajos, en dos bloques: «Siempre activos» y «Solo al dibujar». Solo una fila
/// captura a la vez (<see cref="CapturingRow"/>, que recibe las teclas). Escucha
/// al coordinador para refrescar las filas; se desecha al cerrar la ventana para
/// no dejar suscripciones vivas ni los atajos en pausa.
/// </summary>
internal sealed partial class SettingsViewModel : ObservableObject, IDisposable
{
    private readonly HotkeyCoordinator _coordinator;
    private readonly List<HotkeyRowViewModel> _rows;

    /// <summary>Fila que está capturando una combinación; <c>null</c> si ninguna.</summary>
    [ObservableProperty]
    private HotkeyRowViewModel? _capturingRow;

    public SettingsViewModel(HotkeyCoordinator coordinator, IWindowDragHandler dragHandler)
    {
        _coordinator = coordinator;
        DragHandler = dragHandler;
        _rows = [.. HotkeyCatalog.All.Select(definition => new HotkeyRowViewModel(definition.Action, coordinator))];
        AlwaysRows = RowsOf(HotkeyScope.Always);
        DrawingRows = RowsOf(HotkeyScope.DrawingMode);
        _coordinator.Changed += OnCoordinatorChanged;
        foreach (var row in _rows)
        {
            row.PropertyChanged += OnRowPropertyChanged;
        }
    }

    /// <summary>Se pide cerrar la ventana (botón Cerrar).</summary>
    public event EventHandler? CloseRequested;

    /// <summary>Inicia el arrastre nativo de la ventana desde el título.</summary>
    public IWindowDragHandler DragHandler { get; }

    /// <summary>Filas del bloque «Siempre activos».</summary>
    public IReadOnlyList<HotkeyRowViewModel> AlwaysRows { get; }

    /// <summary>Filas del bloque «Solo al dibujar».</summary>
    public IReadOnlyList<HotkeyRowViewModel> DrawingRows { get; }

    // Al cerrar con una fila capturando, los atajos deben volver.
    public void Dispose()
    {
        CapturingRow?.CancelCaptureCommand.Execute(null);
        _coordinator.Changed -= OnCoordinatorChanged;
        foreach (var row in _rows)
        {
            row.PropertyChanged -= OnRowPropertyChanged;
        }
    }

    [RelayCommand]
    private void Close() => CloseRequested?.Invoke(this, EventArgs.Empty);

    private List<HotkeyRowViewModel> RowsOf(HotkeyScope scope) =>
        [.. _rows.Where(row => HotkeyCatalog.Of(row.Action).Scope == scope)];

    private void OnCoordinatorChanged(object? sender, EventArgs e)
    {
        foreach (var row in _rows)
        {
            row.Refresh();
        }
    }

    // Una sola fila captura: al empezar otra, la anterior se cancela.
    private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not HotkeyRowViewModel row
            || !string.Equals(e.PropertyName, nameof(HotkeyRowViewModel.IsCapturing), StringComparison.Ordinal))
        {
            return;
        }

        if (row.IsCapturing)
        {
            var previous = CapturingRow;
            CapturingRow = row;
            previous?.CancelCaptureCommand.Execute(null);
        }
        else if (CapturingRow == row)
        {
            CapturingRow = null;
        }
    }
}
