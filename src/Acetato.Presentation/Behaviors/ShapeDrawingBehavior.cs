using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using Acetato.Domain;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Comportamiento adjunto que dibuja formas (HU-11) sobre el InkCanvas sin lógica
/// en el code-behind. Cuando la herramienta activa es una forma (línea, flecha,
/// rectángulo), captura el mouse y va reemplazando un trazo de previsualización
/// mientras se arrastra; al soltar, el último queda fijo. Cada forma es UN
/// <see cref="Stroke"/>, así reutiliza undo/limpiar/color/grosor. El lápiz y el
/// borrador los maneja el propio InkCanvas (este behavior no actúa).
/// </summary>
public static class ShapeDrawingBehavior
{
    /// <summary>Herramienta activa (enlazada al ViewModel del overlay).</summary>
    public static readonly DependencyProperty ToolProperty =
        DependencyProperty.RegisterAttached(
            "Tool",
            typeof(ToolKind),
            typeof(ShapeDrawingBehavior),
            new PropertyMetadata(ToolKind.Pencil, OnToolChanged));

    // Estado del arrastre en curso (por InkCanvas); no es bindable.
    private static readonly DependencyProperty SessionProperty =
        DependencyProperty.RegisterAttached(
            "Session",
            typeof(ShapeDragSession),
            typeof(ShapeDrawingBehavior),
            new PropertyMetadata(null));

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

        // Re-suscribir es idempotente: evita enganchar los manejadores dos veces.
        canvas.PreviewMouseLeftButtonDown -= OnMouseDown;
        canvas.PreviewMouseLeftButtonDown += OnMouseDown;
        canvas.PreviewMouseMove -= OnMouseMove;
        canvas.PreviewMouseMove += OnMouseMove;
        canvas.PreviewMouseLeftButtonUp -= OnMouseUp;
        canvas.PreviewMouseLeftButtonUp += OnMouseUp;
    }

    private static bool IsShapeTool(ToolKind tool) =>
        tool is ToolKind.Line or ToolKind.Arrow or ToolKind.Rectangle;

    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        if (!IsShapeTool(GetTool(canvas)))
        {
            return;
        }

        var start = ToStrokePoint(e.GetPosition(canvas));
        canvas.SetValue(SessionProperty, new ShapeDragSession(start));
        _ = canvas.CaptureMouse();
        e.Handled = true;
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        if (canvas.GetValue(SessionProperty) is not ShapeDragSession session)
        {
            return;
        }

        UpdatePreview(canvas, session, ToStrokePoint(e.GetPosition(canvas)));
    }

    private static void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        if (canvas.GetValue(SessionProperty) is not ShapeDragSession)
        {
            return;
        }

        canvas.ReleaseMouseCapture();
        canvas.SetValue(SessionProperty, null);
        e.Handled = true;
    }

    private static void UpdatePreview(InkCanvas canvas, ShapeDragSession session, StrokePoint current)
    {
        if (session.Preview is not null)
        {
            canvas.Strokes.Remove(session.Preview);
        }

        var points = ShapeBuilder.Build(GetTool(canvas), session.Start, current);
        var stroke = BuildStroke(points, canvas.DefaultDrawingAttributes);
        canvas.Strokes.Add(stroke);
        session.Preview = stroke;
    }

    private static Stroke BuildStroke(IReadOnlyList<StrokePoint> points, DrawingAttributes baseAttributes)
    {
        var stylusPoints = new StylusPointCollection(points.Count);
        foreach (var point in points)
        {
            stylusPoints.Add(new StylusPoint(point.X, point.Y));
        }

        var attributes = baseAttributes.Clone();
        attributes.FitToCurve = false; // las formas son rectas, sin suavizado
        return new Stroke(stylusPoints, attributes);
    }

    private static StrokePoint ToStrokePoint(Point point) => new(point.X, point.Y);

    // Estado de un arrastre de forma en curso.
    private sealed class ShapeDragSession
    {
        public ShapeDragSession(StrokePoint start) => Start = start;

        public StrokePoint Start { get; }

        public Stroke? Preview { get; set; }
    }
}
