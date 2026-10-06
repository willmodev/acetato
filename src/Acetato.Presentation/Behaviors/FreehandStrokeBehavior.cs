using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using Acetato.Domain;
using Acetato.Presentation.Overlay;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Comportamiento adjunto que da estilo a los trazos a mano alzada (HU-19) sin lógica
/// en el code-behind. El InkCanvas traza libre (lápiz y flecha); al soltar
/// (<see cref="InkCanvas.StrokeCollected"/>) este behavior reemplaza el trazo por un
/// <see cref="StyledStroke"/>: con punta si la herramienta es Flecha (o lo descarta si es
/// más corto que la punta) y con el par de degradado si el modo Degradado está activo.
/// Un lápiz sólido se deja tal cual. La baja y el alta pasan por la colección de trazos,
/// así que el historial neto es UNA entrada (o ninguna si se descarta).
/// </summary>
public static class FreehandStrokeBehavior
{
    /// <summary>
    /// Herramienta activa (enlazada al ViewModel del overlay). El valor por defecto es
    /// Select, que no traza: así el primer enlace con Lápiz (la herramienta inicial) SÍ
    /// dispara el cambio y engancha el manejador. Con Lápiz como defecto no habría
    /// cambio y el degradado del lápiz no se aplicaría hasta cambiar de herramienta.
    /// </summary>
    public static readonly DependencyProperty ToolProperty =
        DependencyProperty.RegisterAttached(
            "Tool",
            typeof(ToolKind),
            typeof(FreehandStrokeBehavior),
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

        // Re-suscribir es idempotente: evita enganchar el manejador dos veces.
        canvas.StrokeCollected -= OnStrokeCollected;
        canvas.StrokeCollected += OnStrokeCollected;
    }

    private static void OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        var tool = GetTool(canvas);
        var gradient = InkGradient.GetPair(canvas);
        if (!NeedsStyling(tool, gradient))
        {
            return;
        }

        bool kept = Restyle(canvas, e.Stroke, tool, gradient);
        if (kept && gradient is not null)
        {
            InkGradient.NotifyUsed(canvas);
        }
    }

    // La flecha siempre se reemplaza (lleva punta); el lápiz solo si hay degradado.
    private static bool NeedsStyling(ToolKind tool, TintaGradiente? gradient) =>
        tool == ToolKind.Arrow || (tool == ToolKind.Pencil && gradient is not null);

    // Quita el trazo recién recogido y pone en su lugar el trazo con estilo (mismos
    // puntos y atributos, ya clonados por el lienzo). Devuelve false si se descartó.
    private static bool Restyle(InkCanvas canvas, Stroke collected, ToolKind tool, TintaGradiente? gradient)
    {
        _ = canvas.Strokes.Remove(collected);

        ArrowHead? head = null;
        if (tool == ToolKind.Arrow)
        {
            var path = collected.StylusPoints.ToStrokePoints();
            if (!ArrowHeadBuilder.TryBuild(path, collected.DrawingAttributes.Width, out var built))
            {
                return false;
            }

            head = built;
        }

        canvas.Strokes.Add(new StyledStroke(
            collected.StylusPoints.Clone(), collected.DrawingAttributes.Clone(), head, gradient));
        return true;
    }
}
