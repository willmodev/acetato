namespace Acetato.Domain;

/// <summary>
/// Construye los puntos de una forma (HU-11) a partir del inicio y el fin del
/// arrastre. Devuelve una polilínea de <see cref="StrokePoint"/> que la capa de
/// presentación convierte en un trazo del lienzo (así las formas reutilizan
/// undo/limpiar/color/grosor). Tipo puro (sin WPF), testeable.
/// </summary>
public static class ShapeBuilder
{
    private const double MinCornerRadius = 8d;
    private const double CornerRadiusPerThickness = 2d;
    private const int CornerSegments = 6;
    private const double Epsilon = 1e-6;

    /// <summary>
    /// Puntos de la forma según la herramienta; las no-forma caen a línea.
    /// <paramref name="thickness"/> (grosor activo, DIP) escala el radio de las esquinas.
    /// </summary>
    public static IReadOnlyList<StrokePoint> Build(ToolKind tool, StrokePoint start, StrokePoint end, double thickness) => tool switch
    {
        ToolKind.Rectangle => Rectangle(start, end, thickness),
        _ => [start, end],
    };

    // Rectángulo normalizado (funciona arrastrando en cualquier dirección), cerrado y
    // con esquinas redondeadas: cada una es un arco de CornerSegments segmentos.
    private static List<StrokePoint> Rectangle(StrokePoint a, StrokePoint b, double thickness)
    {
        double left = Math.Min(a.X, b.X);
        double right = Math.Max(a.X, b.X);
        double top = Math.Min(a.Y, b.Y);
        double bottom = Math.Max(a.Y, b.Y);
        double radius = CornerRadius(right - left, bottom - top, thickness);

        if (radius < Epsilon)
        {
            return [new(left, top), new(right, top), new(right, bottom), new(left, bottom), new(left, top)];
        }

        var points = new List<StrokePoint>((4 * (CornerSegments + 1)) + 1);
        AddCorner(points, new StrokePoint(right - radius, top + radius), radius, -90d);
        AddCorner(points, new StrokePoint(right - radius, bottom - radius), radius, 0d);
        AddCorner(points, new StrokePoint(left + radius, bottom - radius), radius, 90d);
        AddCorner(points, new StrokePoint(left + radius, top + radius), radius, 180d);
        points.Add(points[0]);
        return points;
    }

    // Crece con el grosor, con piso de MinCornerRadius y tope en la mitad del lado menor
    // (así un rectángulo muy pequeño no se deforma).
    private static double CornerRadius(double width, double height, double thickness)
    {
        double desired = Math.Max(MinCornerRadius, CornerRadiusPerThickness * thickness);
        return Math.Min(desired, Math.Min(width, height) / 2d);
    }

    // Arco de 90° con centro en `center`, desde `startDegrees` (eje Y hacia abajo).
    // Incluye ambos extremos: el tramo recto entre esquinas queda implícito.
    private static void AddCorner(List<StrokePoint> points, StrokePoint center, double radius, double startDegrees)
    {
        for (int i = 0; i <= CornerSegments; i++)
        {
            double angle = (startDegrees + (90d * i / CornerSegments)) * Math.PI / 180d;
            points.Add(new StrokePoint(
                center.X + (radius * Math.Cos(angle)),
                center.Y + (radius * Math.Sin(angle))));
        }
    }
}
