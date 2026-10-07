using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Acetato.Domain;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Cursor del InkCanvas según la herramienta, en UN solo lugar (HU-20): cruz fina para
/// las que dibujan (lápiz, flecha, línea, rectángulo), I-beam para Texto (HU-13), oculto
/// con Láser (solo se ve el halo, HU-15) y el del propio InkCanvas para el resto
/// (borrador). Antes cada behavior fijaba su cursor y se pisaban entre sí según el orden
/// de enlace. ForceCursor es necesario porque el InkCanvas ignora Cursor a secas.
/// </summary>
public static class ToolCursorBehavior
{
    /// <summary>Herramienta activa (enlazada al ViewModel del overlay).</summary>
    public static readonly DependencyProperty ToolProperty =
        DependencyProperty.RegisterAttached(
            "Tool",
            typeof(ToolKind),
            typeof(ToolCursorBehavior),
            new PropertyMetadata(ToolKind.Select, OnToolChanged));

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

    private static void OnToolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not InkCanvas canvas)
        {
            return;
        }

        var cursor = CursorFor((ToolKind)e.NewValue);
        canvas.Cursor = cursor;
        canvas.ForceCursor = cursor is not null;
    }

    private static Cursor? CursorFor(ToolKind tool) => tool switch
    {
        ToolKind.Pencil or ToolKind.Arrow or ToolKind.Line or ToolKind.Rectangle => Cursors.Cross,
        ToolKind.Text => Cursors.IBeam,
        ToolKind.Laser => Cursors.None,
        _ => null,
    };
}
