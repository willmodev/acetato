using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// ViewModel de la ventana Ajustes (HU-21). Por ahora una sola sección,
/// Atajos, en dos bloques: «Siempre activos» y «Solo al dibujar». Escucha al
/// coordinador para refrescar las filas; se desecha al cerrar la ventana para
/// no dejar suscripciones vivas.
/// </summary>
internal sealed partial class SettingsViewModel : ObservableObject, IDisposable
{
    private readonly HotkeyCoordinator _coordinator;
    private readonly List<HotkeyRowViewModel> _rows;

    public SettingsViewModel(HotkeyCoordinator coordinator, IWindowDragHandler dragHandler)
    {
        _coordinator = coordinator;
        DragHandler = dragHandler;
        _rows = [.. HotkeyCatalog.All.Select(definition => new HotkeyRowViewModel(definition.Action, coordinator))];
        AlwaysRows = RowsOf(HotkeyScope.Always);
        DrawingRows = RowsOf(HotkeyScope.DrawingMode);
        _coordinator.Changed += OnCoordinatorChanged;
    }

    /// <summary>Se pide cerrar la ventana (botón Cerrar).</summary>
    public event EventHandler? CloseRequested;

    /// <summary>Inicia el arrastre nativo de la ventana desde el título.</summary>
    public IWindowDragHandler DragHandler { get; }

    /// <summary>Filas del bloque «Siempre activos».</summary>
    public IReadOnlyList<HotkeyRowViewModel> AlwaysRows { get; }

    /// <summary>Filas del bloque «Solo al dibujar».</summary>
    public IReadOnlyList<HotkeyRowViewModel> DrawingRows { get; }

    public void Dispose() => _coordinator.Changed -= OnCoordinatorChanged;

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
}
