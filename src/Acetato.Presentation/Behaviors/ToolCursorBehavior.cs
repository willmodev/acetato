using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Acetato.Domain;
using Acetato.Presentation.ViewModels;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Cursor del InkCanvas según la herramienta, en UN solo lugar (HU-20): círculo de
/// tinta para las que dibujan (lápiz, flecha, línea, rectángulo; HU-26), I-beam para
/// Texto (HU-13), oculto con Láser (solo se ve el halo, HU-15) y el del propio
/// InkCanvas para el resto (borrador). Antes cada behavior fijaba su cursor y se
/// pisaban entre sí según el orden de enlace. ForceCursor es necesario porque el
/// InkCanvas ignora Cursor a secas. El círculo se rehace al cambiar la tinta o el
/// grosor. También al cargar la pane y al entrar el mouse: el enlace llega antes de
/// que la pane esté en su monitor, cuando la escala aún es la del monitor principal.
/// </summary>
public static class ToolCursorBehavior
{
    // Herramientas que pintan tinta: su cursor es el círculo de tinta (HU-26).
    private static readonly HashSet<ToolKind> InkTools =
    [
        ToolKind.Pencil, ToolKind.Arrow, ToolKind.Line, ToolKind.Rectangle, ToolKind.Ellipse,
    ];

    /// <summary>Herramienta activa (enlazada al ViewModel del overlay).</summary>
    public static readonly DependencyProperty ToolProperty =
        DependencyProperty.RegisterAttached(
            "Tool",
            typeof(ToolKind),
            typeof(ToolCursorBehavior),
            new PropertyMetadata(ToolKind.Move, OnCursorInputChanged));

    /// <summary>Tinta y grosor del círculo (HU-26), enlazados al ViewModel del overlay.</summary>
    public static readonly DependencyProperty TipProperty =
        DependencyProperty.RegisterAttached(
            "Tip",
            typeof(InkCursorTip),
            typeof(ToolCursorBehavior),
            new PropertyMetadata(null, OnCursorInputChanged));

    public static void SetTool(DependencyObject element, ToolKind value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(ToolProperty, value);
    }

    public static ToolKind GetTool(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (ToolKind)element.GetValue(ToolProperty);
    }

    public static void SetTip(DependencyObject element, InkCursorTip? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(TipProperty, value);
    }

    public static InkCursorTip? GetTip(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (InkCursorTip?)element.GetValue(TipProperty);
    }

    /// <summary>
    /// Cursor de la herramienta: círculo de tinta para las que dibujan si hay
    /// <paramref name="tip"/>; <c>null</c> = el propio del InkCanvas.
    /// </summary>
    internal static Cursor? CursorFor(ToolKind tool, InkCursorTip? tip, double scale) => tool switch
    {
        ToolKind.Text => Cursors.IBeam,
        ToolKind.Laser => Cursors.None,
        ToolKind.Move => HandCursorFactory.Open(scale),
        _ when InkTools.Contains(tool) => tip is null ? Cursors.Cross : InkCursorFactory.For(tip, scale),
        _ => null,
    };

    private static void OnCursorInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not InkCanvas canvas)
        {
            return;
        }

        // Quitar antes de añadir: cada manejador queda una sola vez aunque cambien ambas propiedades.
        canvas.Loaded -= OnRefresh;
        canvas.Loaded += OnRefresh;
        canvas.MouseEnter -= OnRefresh;
        canvas.MouseEnter += OnRefresh;
        Apply(canvas);
    }

    // Recalcula con la escala del monitor donde ya está la pane (HU-09/HU-26).
    private static void OnRefresh(object sender, RoutedEventArgs e) => Apply((InkCanvas)sender);

    /// <summary>Vuelve a poner el cursor de la herramienta (al soltar Ctrl tras la mano, SPEC 11).</summary>
    internal static void Refresh(InkCanvas canvas) => Apply(canvas);

    private static void Apply(InkCanvas canvas)
    {
        var cursor = CursorFor(GetTool(canvas), GetTip(canvas), VisualTreeHelper.GetDpi(canvas).DpiScaleX);
        canvas.Cursor = cursor;
        canvas.ForceCursor = cursor is not null;
        // En modo selección (Mover) el lienzo pone sus propios cursores si no se le pide el nuestro.
        canvas.UseCustomCursor = cursor is not null;
    }
}
