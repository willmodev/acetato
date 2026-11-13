using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using Acetato.Application.Abstractions;
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

    /// <summary>
    /// Última tinta sólida elegida. La lee el láser (HU-15), que no admite degradado (HU-19), y el
    /// texto (HU-13) cuando no hay degradado: con él toma el par (spec 15). Coincide con el color de <see cref="ActiveAttributes"/>:
    /// el degradado del trazo libre ya se pinta en vivo (HU-20).
    /// </summary>
    public Color SolidInkColor => TintaColorMap.Resolve(_settings.Color);

    /// <summary>
    /// Par de degradado que tomará el próximo trazo: neón (HU-19) o de tinta (HU-27);
    /// <c>null</c> = tinta plana.
    /// Los behaviors que crean trazos lo reciben por <c>InkGradient.Pair</c>.
    /// </summary>
    public GradientPair? ActiveGradient => _settings.NextStrokePair;

    /// <summary>
    /// Tinta, par del próximo trazo y grosor que pinta el cursor de tinta (HU-26).
    /// Enlazado a <c>ToolCursorBehavior.Tip</c>.
    /// </summary>
    public InkCursorTip CursorTip => IsHighlighter
        ? new(SolidInkColor, Gradient: null, HighlighterWidth.From(_settings.Thickness), Highlighter: true)
        : new(SolidInkColor, ActiveGradient, _settings.Thickness);

    /// <summary>
    /// Estado de Ctrl para mover con Ctrl desde cualquier herramienta (SPEC 11). Enlazado a
    /// <c>MoveToolBehavior.KeyState</c>.
    /// </summary>
    public IModifierKeyState KeyState { get; }

    /// <summary>
    /// Transformación libre al redimensionar una selección (HU-28). Enlazado a
    /// <c>SelectionResizeBehavior.Free</c>.
    /// </summary>
    public bool FreeResize => _settings.FreeResize;

    /// <summary>Tamaño de fuente del texto, derivado del grosor activo (HU-13).</summary>
    public double ActiveFontSize => FontScale.FromThicknessIndex(_settings.ThicknessIndex);

    /// <summary>
    /// Modo de edición del InkCanvas según la herramienta: borrador = borrar trazos; el
    /// resto = ninguno (lápiz y flecha los dibuja FreehandStrokeBehavior en vivo, HU-20;
    /// formas, texto y láser, sus behaviors).
    /// </summary>
    [ObservableProperty]
    private InkCanvasEditingMode _editingMode = InkCanvasEditingMode.None;

    public OverlayViewModel(IDrawingSettings settings, IModifierKeyState? keyState = null)
    {
        _settings = settings;
        KeyState = keyState ?? WpfModifierKeyState.Instance;
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

    /// <summary>Registra un arrastre de la mano o de Ctrl en el historial de esta pane (SPEC 11).</summary>
    internal void RecordMove(AnnotationsMoved moved) => _history.Record(moved);

    /// <summary>Registra un cambio de tamaño de la selección en el historial de esta pane (HU-28).</summary>
    internal void RecordResize(AnnotationsResized resized) => _history.Record(resized);

    /// <summary>Borra trazos y textos seleccionados como una sola acción que Deshacer revierte (SPEC 11).</summary>
    internal void DeleteSelection(InkCanvas canvas, IReadOnlyList<Stroke> strokes, IReadOnlyList<UIElement> elements) =>
        _history.DeleteSelection(canvas, strokes, elements);

    /// <summary>
    /// Un trazo se fijó en degradado: sortea el par del siguiente (HU-19). Enlazado a
    /// <c>InkGradient.UsedCommand</c>.
    /// </summary>
    [RelayCommand]
    private void GradientUsed() => _settings.AdvanceGradient();

    /// <summary>Sube un paso de grosor (HU-06); enlazado a la rueda del mouse.</summary>
    [RelayCommand]
    private void IncreaseThickness() => _settings.IncreaseThickness();

    /// <summary>Baja un paso de grosor (HU-06); enlazado a la rueda del mouse.</summary>
    [RelayCommand]
    private void DecreaseThickness() => _settings.DecreaseThickness();

    /// <summary>
    /// Deshace la última acción de esta pantalla: un trazo o texto dibujado, o lo quitado
    /// con el Borrador (HU-07/HU-13, SPEC 11).
    /// </summary>
    [RelayCommand]
    private void Undo() => _history.UndoLast();

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(ActiveGradient));
        OnPropertyChanged(nameof(CursorTip));
        OnPropertyChanged(nameof(FreeResize));
        SyncAttributes();
        SyncTool();
    }

    private bool IsHighlighter => _settings.SelectedTool == ToolKind.Highlighter;

    private void SyncAttributes()
    {
        // Siempre la tinta sólida: el degradado lo pinta StyledStroke con su par (HU-20).
        ActiveAttributes.Color = SolidInkColor;
        // El resaltador (HU-34) es translúcido y tres veces más ancho que el grosor elegido.
        ActiveAttributes.IsHighlighter = IsHighlighter;
        double width = IsHighlighter ? HighlighterWidth.From(_settings.Thickness) : _settings.Thickness;
        ActiveAttributes.Width = width;
        ActiveAttributes.Height = width;
    }

    private void SyncTool()
    {
        ActiveTool = _settings.SelectedTool;
        EditingMode = ToEditingMode(_settings.SelectedTool);
        // Solo con el Borrador, quitar un trazo es una acción que Deshacer revierte (SPEC 11).
        _history.RecordsErasures = _settings.SelectedTool == ToolKind.Eraser;
    }

    private static InkCanvasEditingMode ToEditingMode(ToolKind tool) => tool switch
    {
        ToolKind.Eraser => InkCanvasEditingMode.EraseByStroke,
        ToolKind.Move => InkCanvasEditingMode.Select, // el lazo de la mano (SPEC 11)
        _ => InkCanvasEditingMode.None, // lápiz y flecha (captura propia, HU-20), formas, texto y láser: los dibuja su behavior
    };

    public void Dispose()
    {
        _settings.Changed -= OnSettingsChanged;
        _history.Dispose();
    }
}
