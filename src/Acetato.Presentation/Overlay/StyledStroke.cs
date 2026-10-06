using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Acetato.Domain;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Trazo propio del overlay (HU-19). Pinta el cuerpo con el estilo normal del lienzo
/// (o con un degradado neón, si lo tiene) y, si es una flecha, añade la punta abierta
/// como una polilínea aparte (extremos y uniones redondeados). Así la punta no pasa por
/// el suavizado del lápiz, que deformaría el vértice, y la flecha sigue siendo UN solo
/// trazo para el historial y el borrador. Conserva su par de degradado para siempre.
/// </summary>
internal sealed class StyledStroke : Stroke
{
    private Brush? _gradientBrush;

    public StyledStroke(
        StylusPointCollection stylusPoints,
        DrawingAttributes drawingAttributes,
        ArrowHead? head,
        TintaGradiente? gradient = null)
        : base(stylusPoints, drawingAttributes)
    {
        Head = head;
        Gradient = gradient;
    }

    /// <summary>Punta de la flecha; <c>null</c> si el trazo no es una flecha.</summary>
    public ArrowHead? Head { get; }

    /// <summary>Par de degradado del trazo; <c>null</c> = tinta sólida (<c>DrawingAttributes.Color</c>).</summary>
    public TintaGradiente? Gradient { get; }

    /// <summary>
    /// Copia profunda que conserva punta y degradado. WPF clona trazos por su cuenta (p. ej.
    /// al copiar/seleccionar, HU-14); sin este override la copia perdería ambos.
    /// </summary>
    public override Stroke Clone() =>
        new StyledStroke(StylusPoints.Clone(), DrawingAttributes.Clone(), Head, Gradient);

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

        var gradientBrush = GradientBrush();
        if (gradientBrush is null)
        {
            base.DrawCore(drawingContext, drawingAttributes);
        }
        else
        {
            drawingContext.DrawGeometry(gradientBrush, pen: null, GetGeometry(drawingAttributes));
        }

        if (Head is { } head)
        {
            DrawHead(drawingContext, head, drawingAttributes, gradientBrush);
        }
    }

    // Pincel de degradado del trazo, congelado y cacheado: se arma una vez (el trazo es
    // inmutable una vez fijado) en vez de en cada repintado. null si el trazo es sólido.
    private Brush? GradientBrush()
    {
        if (Gradient is not { } pair)
        {
            return null;
        }

        return _gradientBrush ??= StrokeGradientBrush.Create(
            pair, StylusPoints.ToStrokePoints(), PenThickness(DrawingAttributes));
    }

    private static void DrawHead(DrawingContext context, ArrowHead head, DrawingAttributes attributes, Brush? gradientBrush)
    {
        var geometry = new StreamGeometry();
        using (var geometryContext = geometry.Open())
        {
            geometryContext.BeginFigure(ToPoint(head.Left), isFilled: false, isClosed: false);
            geometryContext.PolyLineTo([ToPoint(head.Tip), ToPoint(head.Right)], isStroked: true, isSmoothJoin: false);
        }

        geometry.Freeze();
        context.DrawGeometry(brush: null, CreatePen(attributes, gradientBrush), geometry);
    }

    private static Pen CreatePen(DrawingAttributes attributes, Brush? gradientBrush)
    {
        var pen = new Pen(gradientBrush ?? SolidBrush(attributes), PenThickness(attributes))
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round,
        };
        pen.Freeze();
        return pen;
    }

    private static SolidColorBrush SolidBrush(DrawingAttributes attributes)
    {
        var brush = new SolidColorBrush(attributes.Color);
        brush.Freeze();
        return brush;
    }

    // Punta circular (Width == Height en esta app); con una punta elíptica se usa la mayor.
    private static double PenThickness(DrawingAttributes attributes) => Math.Max(attributes.Width, attributes.Height);

    private static Point ToPoint(StrokePoint point) => new(point.X, point.Y);
}
