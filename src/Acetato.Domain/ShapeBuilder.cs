namespace Acetato.Domain;

/// <summary>
/// Construye los puntos de una forma (HU-11) a partir del inicio y el fin del
/// arrastre. Devuelve una polilínea de <see cref="StrokePoint"/> que la capa de
/// presentación convierte en un trazo del lienzo (así las formas reutilizan
/// undo/limpiar/color/grosor). Tipo puro (sin WPF), testeable.
/// </summary>
public static class ShapeBuilder
{
    private const double ArrowHeadLength = 16d;
    private const double ArrowHeadWidth = 9d;

    /// <summary>Puntos de la forma según la herramienta; las no-forma caen a línea.</summary>
    public static IReadOnlyList<StrokePoint> Build(ToolKind tool, StrokePoint start, StrokePoint end) => tool switch
    {
        ToolKind.Rectangle => Rectangle(start, end),
        ToolKind.Arrow => Arrow(start, end),
        _ => [start, end],
    };

    // Rectángulo normalizado (funciona arrastrando en cualquier dirección), cerrado.
    private static IReadOnlyList<StrokePoint> Rectangle(StrokePoint a, StrokePoint b)
    {
        double left = Math.Min(a.X, b.X);
        double right = Math.Max(a.X, b.X);
        double top = Math.Min(a.Y, b.Y);
        double bottom = Math.Max(a.Y, b.Y);

        return
        [
            new(left, top), new(right, top), new(right, bottom),
            new(left, bottom), new(left, top),
        ];
    }

    // Eje inicio→fin más la punta. Se retraza la punta (fin→ala→fin→ala) para que
    // toda la flecha sea UN solo trazo (un undo la quita entera).
    private static IReadOnlyList<StrokePoint> Arrow(StrokePoint start, StrokePoint end)
    {
        double dx = end.X - start.X;
        double dy = end.Y - start.Y;
        double length = Math.Sqrt((dx * dx) + (dy * dy));
        if (length < 1e-6)
        {
            return [start, end];
        }

        double ux = dx / length;
        double uy = dy / length;
        double baseX = end.X - (ux * ArrowHeadLength);
        double baseY = end.Y - (uy * ArrowHeadLength);
        var left = new StrokePoint(baseX - (uy * ArrowHeadWidth), baseY + (ux * ArrowHeadWidth));
        var right = new StrokePoint(baseX + (uy * ArrowHeadWidth), baseY - (ux * ArrowHeadWidth));

        return [start, end, left, end, right];
    }
}
