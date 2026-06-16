using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using Acetato.Application.Drawing;
using Acetato.Domain;
using Acetato.Presentation.Overlay;
using Acetato.Presentation.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// ViewModel de una pane del overlay (una por monitor, HU-09). Posee los trazos
/// de SU pantalla (enlazados al InkCanvas) y los atributos de dibujo activos
/// (color/grosor). Traduce el estado agnóstico de <see cref="IDrawingSettings"/>
/// —compartido entre panes— a los tipos de WPF. Las acciones que abarcan varias
/// pantallas (limpiar, deshacer, captura, color, herramienta) las orquesta el
/// OverlayBroadcaster. El code-behind solo llama a InitializeComponent.
/// </summary>
public sealed partial class OverlayViewModel : ObservableObject, IDisposable
{
    private readonly IDrawingSettings _settings;
    private readonly AnnotationHistory _history;

    /// <summary>Trazos dibujados en esta pantalla; enlazado a <c>InkCanvas.Strokes</c>.</summary>
    public StrokeCollection Strokes { get; } = [];

    /// <summary>
    /// Atributos del próximo trazo; enlazado a <c>InkCanvas.DefaultDrawingAttributes</c>.
    /// El InkCanvas los clona al fijar cada trazo, así que cambiar color/grosor no
    /// altera los trazos ya dibujados (HU-05/06).
    /// </summary>
    public DrawingAttributes ActiveAttributes { get; } = new() { FitToCurve = true };

    /// <summary>Herramienta activa; el behavior de formas la lee para dibujar (HU-11).</summary>
    [ObservableProperty]
    private ToolKind _activeTool = ToolKind.Pencil;

    /// <summary>Color de la tinta activa para el texto; igual que los trazos (HU-13).</summary>
    public Color ActiveInkColor => ActiveAttributes.Color;

    /// <summary>Tamaño de fuente del texto, derivado del grosor activo (HU-13).</summary>
    public double ActiveFontSize => FontScale.FromThicknessIndex(_settings.ThicknessIndex);

    /// <summary>
    /// Modo de edición del InkCanvas según la herramienta: lápiz = tinta libre,
    /// borrador = borrar trazos, formas = ninguno (las dibuja el behavior).
    /// </summary>
    [ObservableProperty]
    private InkCanvasEditingMode _editingMode = InkCanvasEditingMode.Ink;

    public OverlayViewModel(IDrawingSettings settings)
    {
        _settings = settings;
        _settings.Changed += OnSettingsChanged;
        _history = new AnnotationHistory(Strokes);
        SyncAttributes();
        SyncTool();
    }

    /// <summary>Borra trazos y textos de esta pantalla. No falla si ya está vacía (HU-04/HU-13).</summary>
    [RelayCommand]
    private void Clear() => _history.Clear();

    /// <summary>Registra un texto recién fijado en el historial de esta pane (HU-13).</summary>
    public void RecordText(InkCanvas canvas, UIElement element) => _history.RecordText(canvas, element);

    /// <summary>Sube un paso de grosor (HU-06); enlazado a la rueda del mouse.</summary>
    [RelayCommand]
    private void IncreaseThickness() => _settings.IncreaseThickness();

    /// <summary>Baja un paso de grosor (HU-06); enlazado a la rueda del mouse.</summary>
    [RelayCommand]
    private void DecreaseThickness() => _settings.DecreaseThickness();

    /// <summary>Deshace la última anotación (trazo o texto) de esta pantalla (HU-07/HU-13).</summary>
    [RelayCommand]
    private void Undo() => _history.UndoLast();

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        SyncAttributes();
        SyncTool();
    }

    private void SyncAttributes()
    {
        ActiveAttributes.Color = TintaColorMap.Resolve(_settings.Color);
        ActiveAttributes.Width = _settings.Thickness;
        ActiveAttributes.Height = _settings.Thickness;
    }

    private void SyncTool()
    {
        ActiveTool = _settings.SelectedTool;
        EditingMode = ToEditingMode(_settings.SelectedTool);
    }

    private static InkCanvasEditingMode ToEditingMode(ToolKind tool) => tool switch
    {
        ToolKind.Eraser => InkCanvasEditingMode.EraseByStroke,
        ToolKind.Pencil => InkCanvasEditingMode.Ink,
        _ => InkCanvasEditingMode.None, // formas: las dibuja el behavior
    };

    public void Dispose()
    {
        _settings.Changed -= OnSettingsChanged;
        _history.Dispose();
    }
}
