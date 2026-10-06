using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using Acetato.Domain;
using Acetato.Presentation.Overlay;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Comportamiento adjunto que da estilo a los trazos a mano alzada (HU-19) sin lógica
/// en el code-behind. Con la herramienta Flecha, el InkCanvas traza libre como el
/// lápiz; al soltar (<see cref="InkCanvas.StrokeCollected"/>) este behavior reemplaza
/// el trazo por un <see cref="StyledStroke"/> con punta, o lo descarta si es más corto
/// que la punta. La baja y el alta pasan por la colección de trazos, así que el
/// historial neto es UNA entrada (o ninguna si se descarta).
/// </summary>
public static class FreehandStrokeBehavior
{
    /// <summary>Herramienta activa (enlazada al ViewModel del overlay).</summary>
    public static readonly DependencyProperty ToolProperty =
        DependencyProperty.RegisterAttached(
            "Tool",
            typeof(ToolKind),
            typeof(FreehandStrokeBehavior),
            new PropertyMetadata(ToolKind.Pencil, OnToolChanged));

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

        // Re-suscribir es idempotente: evita enganchar el manejador dos veces.
        canvas.StrokeCollected -= OnStrokeCollected;
        canvas.StrokeCollected += OnStrokeCollected;
    }

    private static void OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        if (GetTool(canvas) != ToolKind.Arrow)
        {
            return;
        }

        ReplaceWithArrow(canvas, e.Stroke);
    }

    // Quita el trazo recién recogido y, si da para una flecha, pone en su lugar el
    // trazo con punta (mismos puntos y atributos, ya clonados por el lienzo).
    private static void ReplaceWithArrow(InkCanvas canvas, Stroke collected)
    {
        var path = ToPath(collected.StylusPoints);
        _ = canvas.Strokes.Remove(collected);

        if (!ArrowHeadBuilder.TryBuild(path, collected.DrawingAttributes.Width, out var head))
        {
            return;
        }

        canvas.Strokes.Add(new StyledStroke(collected.StylusPoints.Clone(), collected.DrawingAttributes.Clone(), head));
    }

    private static List<StrokePoint> ToPath(StylusPointCollection points)
    {
        var path = new List<StrokePoint>(points.Count);
        foreach (var point in points)
        {
            path.Add(new StrokePoint(point.X, point.Y));
        }

        return path;
    }
}
