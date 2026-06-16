using System.Windows.Media;
using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Overlay;
using Acetato.Domain;
using Acetato.Presentation.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// ViewModel de la barra flotante (HU-10). Las acciones de dibujo (limpiar,
/// deshacer, captura) van por el <see cref="OverlayBroadcaster"/>, que las
/// reparte entre las panes de cada monitor (HU-09); color/grosor/herramienta van
/// por el estado compartido. Refleja el estado activo (tinta/grosor) para
/// resaltarlo con el acento. Gestiona los popovers y el cierre. El code-behind
/// solo llama a InitializeComponent.
/// </summary>
public sealed partial class ToolbarViewModel : ObservableObject, IDisposable
{
    private readonly IDrawingSettings _settings;
    private readonly IOverlayController _controller;
    private readonly OverlayBroadcaster _broadcaster;

    /// <summary>Pincel de la tinta activa (para el botón de color de la barra).</summary>
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
        IWindowDragHandler dragHandler)
    {
        _broadcaster = broadcaster;
        _settings = settings;
        _controller = controller;
        DragHandler = dragHandler;
        Swatches = BuildSwatches();
        Thicknesses = BuildThicknesses();
        Tools = BuildTools();
        _settings.Changed += OnSettingsChanged;
        SyncActiveState();
    }

    /// <summary>Inicia el arrastre nativo de la ventana desde el grip.</summary>
    public IWindowDragHandler DragHandler { get; }

    /// <summary>Las 6 tintas del popover de color.</summary>
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

    /// <summary>Elige una tinta y cierra el popover (HU-05).</summary>
    [RelayCommand]
    private void PickColor(TintaColor color)
    {
        _settings.SelectColor(color);
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

    /// <summary>Oculta el overlay y la barra (botón Cerrar).</summary>
    [RelayCommand]
    private void Close() => _controller.Toggle();

    private static List<InkSwatchItem> BuildSwatches()
    {
        TintaColor[] colors =
        [
            TintaColor.Red, TintaColor.Blue, TintaColor.Yellow,
            TintaColor.Green, TintaColor.White, TintaColor.Black,
        ];
        var items = new List<InkSwatchItem>(colors.Length);
        foreach (var color in colors)
        {
            items.Add(new InkSwatchItem(color, new SolidColorBrush(TintaColorMap.Resolve(color))));
        }

        return items;
    }

    private static List<ThicknessItem> BuildThicknesses()
    {
        var items = new List<ThicknessItem>(ThicknessScale.Steps.Count);
        for (int index = ThicknessScale.MinIndex; index <= ThicknessScale.MaxIndex; index++)
        {
            items.Add(new ThicknessItem(index, 6d + (index * 4d)));
        }

        return items;
    }

    // Herramientas de la barra; las no implementadas van deshabilitadas (placeholder).
    private static List<ToolItem> BuildTools() =>
    [
        new(ToolKind.Select, "Seleccionar", Glyph("Icon.Select"), false),
        new(ToolKind.Pencil, "Lápiz", Glyph("Icon.Pencil"), true),
        new(ToolKind.Eraser, "Borrador", Glyph("Icon.Eraser"), true),
        new(ToolKind.Line, "Línea", Glyph("Icon.Line"), true),
        new(ToolKind.Arrow, "Flecha", Glyph("Icon.Arrow"), true),
        new(ToolKind.Rectangle, "Rectángulo", Glyph("Icon.Rectangle"), true),
        new(ToolKind.Text, "Texto", Glyph("Icon.Text"), true),
    ];

    // Resuelve la geometría del icono desde los recursos (igual que las tintas).
    private static Geometry Glyph(string key) =>
        (Geometry)System.Windows.Application.Current.Resources[key];

    private void OnSettingsChanged(object? sender, EventArgs e) => SyncActiveState();

    private void SyncActiveState()
    {
        ActiveInkBrush = new SolidColorBrush(TintaColorMap.Resolve(_settings.Color));
        foreach (var swatch in Swatches)
        {
            swatch.IsSelected = swatch.Color == _settings.Color;
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

    public void Dispose() => _settings.Changed -= OnSettingsChanged;
}
