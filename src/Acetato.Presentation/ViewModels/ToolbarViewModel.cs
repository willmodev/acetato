using System.Windows.Media;
using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Overlay;
using Acetato.Domain;
using Acetato.Presentation.Resources;
using Acetato.Presentation.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// ViewModel de la barra flotante (HU-10). Las acciones de dibujo (limpiar,
/// deshacer, captura) van por el <see cref="OverlayBroadcaster"/>, que las
/// reparte entre las panes de cada monitor (HU-09); color/grosor/herramienta van
/// por el estado compartido. Refleja el estado activo (tinta/grosor) para
/// resaltarlo con el acento. Gestiona los popovers y el cierre. Cada botón
/// tiene un tooltip con su nombre y, si su acción tiene atajo, la combinación
/// vigente (HU-22). El code-behind solo llama a InitializeComponent.
/// </summary>
public sealed partial class ToolbarViewModel : ObservableObject, IDisposable
{
    private readonly IDrawingSettings _settings;
    private readonly IOverlayController _controller;
    private readonly OverlayBroadcaster _broadcaster;
    private readonly SettingsWindowLauncher _settingsLauncher;

    /// <summary>Pincel de la tinta activa, o el degradado de muestra (para el botón de color de la barra).</summary>
    [ObservableProperty]
    private Brush _activeInkBrush = Brushes.Transparent;

    /// <summary>Indica si el popover de tintas está abierto.</summary>
    [ObservableProperty]
    private bool _isInkPopoverOpen;

    /// <summary>Indica si el popover de grosor está abierto.</summary>
    [ObservableProperty]
    private bool _isThicknessPopoverOpen;

    public ToolbarViewModel(
        OverlayBroadcaster broadcaster,
        IDrawingSettings settings,
        IOverlayController controller,
        ToolbarChrome chrome)
    {
        ArgumentNullException.ThrowIfNull(chrome);
        _broadcaster = broadcaster;
        _settingsLauncher = chrome.SettingsLauncher;
        _settings = settings;
        _controller = controller;
        DragHandler = chrome.DragHandler;
        Swatches = InkSwatchFactory.Create();
        Thicknesses = BuildThicknesses();
        var shortcuts = chrome.Shortcuts;
        Tools = BuildTools(shortcuts);
        MoveHint = shortcuts.Hint("Mover");
        ColorHint = shortcuts.Hint("Color");
        ThicknessHint = shortcuts.Hint("Grosor");
        UndoHint = shortcuts.Hint("Deshacer", HotkeyAction.Undo);
        ClearHint = shortcuts.Hint("Limpiar todo", HotkeyAction.Clear);
        CaptureHint = shortcuts.Hint("Captura", HotkeyAction.Capture);
        SettingsHint = shortcuts.Hint("Ajustes");
        CloseHint = shortcuts.Hint("Cerrar", HotkeyAction.Toggle);
        _settings.Changed += OnSettingsChanged;
        SyncActiveState();
    }

    /// <summary>Inicia el arrastre nativo de la ventana desde el grip.</summary>
    public IWindowDragHandler DragHandler { get; }

    /// <summary>Tooltip del grip de arrastre.</summary>
    public ShortcutHint MoveHint { get; }

    /// <summary>Tooltip del botón de tinta.</summary>
    public ShortcutHint ColorHint { get; }

    /// <summary>Tooltip del botón de grosor.</summary>
    public ShortcutHint ThicknessHint { get; }

    /// <summary>Tooltip de Deshacer, con su atajo.</summary>
    public ShortcutHint UndoHint { get; }

    /// <summary>Tooltip del botón que borra todos los trazos, con su atajo.</summary>
    public ShortcutHint ClearHint { get; }

    /// <summary>Tooltip de Captura, con su atajo.</summary>
    public ShortcutHint CaptureHint { get; }

    /// <summary>Tooltip de Ajustes.</summary>
    public ShortcutHint SettingsHint { get; }

    /// <summary>Tooltip de Cerrar, con el atajo de Mostrar/ocultar (hace lo mismo).</summary>
    public ShortcutHint CloseHint { get; }

    /// <summary>Las 6 tintas del popover de color y, al final, la muestra del modo Degradado.</summary>
    public IReadOnlyList<InkSwatchItem> Swatches { get; }

    /// <summary>Los pasos de grosor del popover.</summary>
    public IReadOnlyList<ThicknessItem> Thicknesses { get; }

    /// <summary>Las herramientas de la barra (HU-11).</summary>
    public IReadOnlyList<ToolItem> Tools { get; }

    /// <summary>Abre o cierra el popover de tintas (cierra el de grosor).</summary>
    [RelayCommand]
    private void ToggleInkPopover()
    {
        IsThicknessPopoverOpen = false;
        IsInkPopoverOpen = !IsInkPopoverOpen;
    }

    /// <summary>Abre o cierra el popover de grosor (cierra el de tintas).</summary>
    [RelayCommand]
    private void ToggleThicknessPopover()
    {
        IsInkPopoverOpen = false;
        IsThicknessPopoverOpen = !IsThicknessPopoverOpen;
    }

    /// <summary>
    /// Elige una tinta sólida o el modo Degradado y cierra el popover (HU-05/HU-19).
    /// </summary>
    [RelayCommand]
    private void PickSwatch(InkSwatchItem swatch)
    {
        if (swatch.Color is { } color)
        {
            _settings.SelectColor(color);
        }
        else
        {
            _settings.SelectGradient();
        }

        IsInkPopoverOpen = false;
    }

    /// <summary>Elige un grosor por índice y cierra el popover (HU-06/HU-10).</summary>
    [RelayCommand]
    private void PickThickness(int index)
    {
        _settings.SelectThickness(index);
        IsThicknessPopoverOpen = false;
    }

    /// <summary>Selecciona la herramienta activa (HU-11).</summary>
    [RelayCommand]
    private void PickTool(ToolKind tool) => _settings.SelectTool(tool);

    /// <summary>Deshace el último trazo de la pantalla bajo el cursor (HU-07/HU-09).</summary>
    [RelayCommand]
    private void Undo() => _broadcaster.Undo();

    /// <summary>Borra los trazos de todas las pantallas (HU-04/HU-09).</summary>
    [RelayCommand]
    private void Clear() => _broadcaster.Clear();

    /// <summary>Guarda el escritorio virtual anotado como PNG (HU-12).</summary>
    [RelayCommand]
    private Task Capture() => _broadcaster.CaptureAsync();

    /// <summary>Abre la ventana Ajustes (HU-21).</summary>
    [RelayCommand]
    private void OpenSettings() => _settingsLauncher.Open();

    /// <summary>Oculta el overlay y la barra (botón Cerrar).</summary>
    [RelayCommand]
    private void Close() => _controller.Toggle();

    private static List<ThicknessItem> BuildThicknesses()
    {
        var items = new List<ThicknessItem>(ThicknessScale.Steps.Count);
        for (int index = ThicknessScale.MinIndex; index <= ThicknessScale.MaxIndex; index++)
        {
            items.Add(new ThicknessItem(index, 6d + (index * 4d)));
        }

        return items;
    }

    // Herramientas de la barra con el atajo que las elige (HU-22); las no
    // implementadas van deshabilitadas (placeholder) y sin atajo.
    private static List<ToolItem> BuildTools(ToolbarShortcuts shortcuts) =>
    [
        new(ToolKind.Select, shortcuts.Hint("Seleccionar"), Glyph("Icon.Select"), false),
        new(ToolKind.Pencil, shortcuts.Hint("Lápiz", HotkeyAction.SelectPencilTool), Glyph("Icon.Pencil"), true),
        new(ToolKind.Laser, shortcuts.Hint("Láser", HotkeyAction.SelectLaserTool), Glyph("Icon.Laser"), true),
        new(ToolKind.Eraser, shortcuts.Hint("Borrador", HotkeyAction.SelectEraserTool), Glyph("Icon.Eraser"), true),
        new(ToolKind.Line, shortcuts.Hint("Línea", HotkeyAction.SelectLineTool), Glyph("Icon.Line"), true),
        new(ToolKind.Arrow, shortcuts.Hint("Flecha", HotkeyAction.SelectArrowTool), Glyph("Icon.Arrow"), true),
        new(ToolKind.Rectangle, shortcuts.Hint("Rectángulo", HotkeyAction.SelectRectangleTool), Glyph("Icon.Rectangle"), true),
        new(ToolKind.Text, shortcuts.Hint("Texto", HotkeyAction.SelectTextTool), Glyph("Icon.Text"), true),
    ];

    // Resuelve la geometría del icono desde los recursos (igual que las tintas).
    private static Geometry Glyph(string key) =>
        (Geometry)System.Windows.Application.Current.Resources[key];

    private void OnSettingsChanged(object? sender, EventArgs e) => SyncActiveState();

    private void SyncActiveState()
    {
        ActiveInkBrush = _settings.IsGradient
            ? InkSwatchFactory.GradientSample
            : new SolidColorBrush(TintaColorMap.Resolve(_settings.Color));
        foreach (var swatch in Swatches)
        {
            swatch.IsSelected = IsActive(swatch);
        }

        foreach (var step in Thicknesses)
        {
            step.IsSelected = step.Index == _settings.ThicknessIndex;
        }

        foreach (var tool in Tools)
        {
            tool.IsSelected = tool.Kind == _settings.SelectedTool;
        }
    }

    // La muestra de degradado está activa con el modo; una tinta, solo fuera del modo.
    private bool IsActive(InkSwatchItem swatch) =>
        swatch.IsGradient ? _settings.IsGradient : !_settings.IsGradient && swatch.Color == _settings.Color;

    public void Dispose() => _settings.Changed -= OnSettingsChanged;
}
