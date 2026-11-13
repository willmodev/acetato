using System.ComponentModel;
using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Hotkeys;
using Acetato.Presentation.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// ViewModel de la ventana Ajustes (HU-21): el bloque «Dibujo» (HU-27), los atajos,
/// en dos bloques: «Siempre activos» y «Solo al dibujar», y «Acerca de» (HU-33). Solo una
/// fila captura a la vez (<see cref="CapturingRow"/>, que recibe las teclas). Escucha
/// al coordinador para refrescar las filas; se desecha al cerrar la ventana para
/// no dejar suscripciones vivas ni los atajos en pausa.
/// </summary>
internal sealed partial class SettingsViewModel : ObservableObject, IDisposable
{
    /// <summary>Pista de «Reportar problema» antes de usarlo.</summary>
    public const string ReportHint = "Crea un .zip en el escritorio para enviarlo a soporte";

    /// <summary>Pista cuando el reporte quedó guardado.</summary>
    public const string ReportSaved = "Reporte guardado en el escritorio";

    /// <summary>Pista cuando el reporte no se pudo crear.</summary>
    public const string ReportFailed = "No se pudo crear el reporte";

    private readonly HotkeyCoordinator _coordinator;
    private readonly List<HotkeyRowViewModel> _rows;
    private readonly IDrawingSettings _drawing;
    private readonly IDrawingPreferencesStore _preferences;
    private readonly SettingsAboutServices _about;

    /// <summary>Fila que está capturando una combinación; <c>null</c> si ninguna.</summary>
    [ObservableProperty]
    private HotkeyRowViewModel? _capturingRow;

    /// <summary>Se pide confirmar la restauración global (en la propia ventana, sin diálogo del sistema).</summary>
    [ObservableProperty]
    private bool _isConfirmingResetAll;

    /// <summary>Pista bajo «Reportar problema»: qué hace o cómo terminó (HU-33).</summary>
    [ObservableProperty]
    private string _reportStatus = ReportHint;

    public SettingsViewModel(
        HotkeyCoordinator coordinator,
        IWindowDragHandler dragHandler,
        IDrawingSettings drawing,
        IDrawingPreferencesStore preferences,
        SettingsAboutServices about)
    {
        ArgumentNullException.ThrowIfNull(about);
        _coordinator = coordinator;
        _drawing = drawing;
        _preferences = preferences;
        _about = about;
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

    /// <summary>
    /// Tintas degradadas (HU-27): se aplica al momento al estado de dibujo y se guarda.
    /// Restaurar los atajos no lo cambia.
    /// </summary>
    public bool InkPairs
    {
        get => _drawing.InkPairsEnabled;
        set
        {
            if (value == _drawing.InkPairsEnabled)
            {
                return;
            }

            _drawing.SetInkPairs(value);
            SavePreferences();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Transformación libre (HU-28): al redimensionar, cada eje se estira por su lado.
    /// Se aplica al momento y se guarda. Restaurar los atajos no la cambia.
    /// </summary>
    public bool FreeResize
    {
        get => _drawing.FreeResize;
        set
        {
            if (value == _drawing.FreeResize)
            {
                return;
            }

            _drawing.SetFreeResize(value);
            SavePreferences();
            OnPropertyChanged();
        }
    }

    /// <summary>Filas del bloque «Siempre activos».</summary>
    public IReadOnlyList<HotkeyRowViewModel> AlwaysRows { get; }

    /// <summary>Filas del bloque «Solo al dibujar».</summary>
    public IReadOnlyList<HotkeyRowViewModel> DrawingRows { get; }

    /// <summary>«Acetato 0.9.0»: la versión de pantalla, sin el código del commit (HU-33).</summary>
    public string VersionText => $"Acetato {_about.AppInfo.Version.Display}";

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

    /// <summary>Pide confirmación para restaurar todos los atajos.</summary>
    [RelayCommand]
    private void ResetAll()
    {
        CapturingRow?.CancelCaptureCommand.Execute(null);
        IsConfirmingResetAll = true;
    }

    /// <summary>Devuelve las 15 acciones a su atajo por defecto.</summary>
    [RelayCommand]
    private void ConfirmResetAll()
    {
        IsConfirmingResetAll = false;
        _coordinator.ResetAll();
    }

    [RelayCommand]
    private void CancelResetAll() => IsConfirmingResetAll = false;

    /// <summary>Deja la versión de pantalla en el portapapeles (HU-33).</summary>
    [RelayCommand]
    private void CopyVersion() => _about.Clipboard.SetText(_about.AppInfo.Version.Display);

    /// <summary>
    /// Arma el .zip para soporte (HU-33). Mientras corre, el botón queda desactivado (el
    /// comando no admite dos ejecuciones a la vez).
    /// </summary>
    [RelayCommand]
    private async Task ReportProblemAsync(CancellationToken cancellationToken)
    {
        // Vuelve al hilo de la interfaz: la pista está enlazada a la ventana.
        var result = await _about.Reports.CreateAsync(cancellationToken).ConfigureAwait(true);
        ReportStatus = result.Succeeded ? ReportSaved : ReportFailed;
    }

    // Guarda el bloque Dibujo entero: cada opción conserva la otra.
    private void SavePreferences() =>
        _preferences.Save(new DrawingPreferences(_drawing.InkPairsEnabled, _drawing.FreeResize));

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
