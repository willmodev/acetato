using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Acetato.Domain;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Trazo propio del overlay (HU-19). Pinta el cuerpo con el estilo normal del lienzo y,
/// si es una flecha, añade la punta abierta como una polilínea aparte (extremos y
/// uniones redondeados). Así la punta no pasa por el suavizado del lápiz, que
/// deformaría el vértice, y la flecha sigue siendo UN solo trazo para el historial
/// y el borrador.
/// </summary>
internal sealed class StyledStroke : Stroke
{
    public StyledStroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes, ArrowHead? head)
        : base(stylusPoints, drawingAttributes)
    {
        Head = head;
    }

    /// <summary>Punta de la flecha; <c>null</c> si el trazo no es una flecha.</summary>
    public ArrowHead? Head { get; }

    /// <summary>
    /// Copia profunda que conserva la punta. WPF clona trazos por su cuenta (p. ej. al
    /// copiar/seleccionar, HU-14); sin este override la copia perdería la punta.
    /// </summary>
    public override Stroke Clone() =>
        new StyledStroke(StylusPoints.Clone(), DrawingAttributes.Clone(), Head);

    /// <summary>
    /// Recuadro del cuerpo ampliado con el de la punta. Si la punta quedara fuera, el
    /// lienzo no la invalidaría al borrar o deshacer y dejaría restos en pantalla.
    /// </summary>
    public override Rect GetBounds()
    {
        var bounds = base.GetBounds();
        if (Head is not { } head)
        {
            return bounds;
        }

        var headBounds = new Rect(ToPoint(head.Left), ToPoint(head.Right));
        headBounds.Union(ToPoint(head.Tip));
        double reach = PenThickness(DrawingAttributes) / 2d;
        headBounds.Inflate(reach, reach);
        return Rect.Union(bounds, headBounds);
    }

    protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
    {
        ArgumentNullException.ThrowIfNull(drawingContext);
        ArgumentNullException.ThrowIfNull(drawingAttributes);

        base.DrawCore(drawingContext, drawingAttributes);
        if (Head is { } head)
        {
            DrawHead(drawingContext, head, drawingAttributes);
        }
    }

    private static void DrawHead(DrawingContext context, ArrowHead head, DrawingAttributes attributes)
    {
        var geometry = new StreamGeometry();
        using (var geometryContext = geometry.Open())
        {
            geometryContext.BeginFigure(ToPoint(head.Left), isFilled: false, isClosed: false);
            geometryContext.PolyLineTo([ToPoint(head.Tip), ToPoint(head.Right)], isStroked: true, isSmoothJoin: false);
        }

        geometry.Freeze();
        context.DrawGeometry(brush: null, CreatePen(attributes), geometry);
    }

    private static Pen CreatePen(DrawingAttributes attributes)
    {
        var brush = new SolidColorBrush(attributes.Color);
        brush.Freeze();
        var pen = new Pen(brush, PenThickness(attributes))
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round,
        };
        pen.Freeze();
        return pen;
    }

    // Punta circular (Width == Height en esta app); con una punta elíptica se usa la mayor.
    private static double PenThickness(DrawingAttributes attributes) => Math.Max(attributes.Width, attributes.Height);

    private static Point ToPoint(StrokePoint point) => new(point.X, point.Y);
}
