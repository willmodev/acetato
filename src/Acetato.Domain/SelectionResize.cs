namespace Acetato.Domain;

/// <summary>
/// Nuevo recuadro de una selección al jalar uno de sus tiradores (HU-28). Siempre se
/// calcula desde el recuadro del inicio del gesto y el desplazamiento total del mouse,
/// para que no se acumule error. En modo parejo ancho y alto crecen en la misma
/// proporción; en modo libre, cada eje por su lado. El lado opuesto al tirador queda
/// fijo (desde un lado, en modo parejo, el centro de ese lado opuesto). Un lado no baja
/// de <see cref="MinSide"/> salvo que ya fuera más corto, y la selección no se voltea.
/// Tipo puro (sin WPF), testeable.
/// </summary>
public static class SelectionResize
{
    /// <summary>Lado mínimo (DIP) al encoger una selección.</summary>
    public const double MinSide = 8d;

    private const double Epsilon = 1e-6;

    /// <summary>
    /// Recuadro resultante de jalar <paramref name="handle"/> de <paramref name="start"/>
    /// un total de (<paramref name="dx"/>, <paramref name="dy"/>).
    /// </summary>
    public static SelectionBox Resize(SelectionBox start, ResizeHandle handle, double dx, double dy, bool free)
    {
        var (hx, hy) = Axes(handle);
        return free ? Free(start, hx, hy, dx, dy) : Even(start, hx, hy, dx, dy);
    }

    // Libre: cada eje que mueve el tirador se estira por su cuenta.
    private static SelectionBox Free(SelectionBox start, int hx, int hy, double dx, double dy)
    {
        double width = hx == 0 ? start.Width : Shrinkable(start.Width + (hx * dx), start.Width);
        double height = hy == 0 ? start.Height : Shrinkable(start.Height + (hy * dy), start.Height);
        return new SelectionBox(
            Origin(start.X, start.Width, width, hx, centered: false),
            Origin(start.Y, start.Height, height, hy, centered: false),
            width,
            height);
    }

    // Parejo: una sola escala para los dos ejes, la del eje que más crece.
    private static SelectionBox Even(SelectionBox start, int hx, int hy, double dx, double dy)
    {
        double? scaleX = AxisScale(start.Width, hx, dx);
        double? scaleY = AxisScale(start.Height, hy, dy);
        if (scaleX is null && scaleY is null)
        {
            return start;
        }

        double scale = Math.Max(scaleX ?? double.MinValue, scaleY ?? double.MinValue);
        double minScale = Math.Min(1d, MinSide / Math.Max(start.Width, start.Height));
        scale = Math.Max(scale, minScale);

        double width = start.Width * scale;
        double height = start.Height * scale;
        return new SelectionBox(
            Origin(start.X, start.Width, width, hx, centered: true),
            Origin(start.Y, start.Height, height, hy, centered: true),
            width,
            height);
    }

    // Escala de un eje; null si el tirador no lo mueve o el eje no tiene tamaño.
    private static double? AxisScale(double side, int direction, double delta)
    {
        if (direction == 0 || side < Epsilon)
        {
            return null;
        }

        return (side + (direction * delta)) / side;
    }

    // Un lado no baja del mínimo; si ya era más corto, no se encoge más.
    private static double Shrinkable(double candidate, double original) =>
        Math.Max(candidate, Math.Min(MinSide, original));

    // Origen del eje: fijo si el tirador mueve el borde final, desplazado si mueve el
    // inicial, y centrado (solo en modo parejo) si el tirador no mueve este eje.
    private static double Origin(double origin, double oldSide, double newSide, int direction, bool centered) =>
        direction switch
        {
            < 0 => origin + oldSide - newSide,
            > 0 => origin,
            _ => centered ? origin + ((oldSide - newSide) / 2d) : origin,
        };

    // Qué borde mueve el tirador en cada eje: -1 el inicial, 1 el final, 0 ninguno.
    private static (int X, int Y) Axes(ResizeHandle handle) => handle switch
    {
        ResizeHandle.TopLeft => (-1, -1),
        ResizeHandle.Top => (0, -1),
        ResizeHandle.TopRight => (1, -1),
        ResizeHandle.Right => (1, 0),
        ResizeHandle.BottomRight => (1, 1),
        ResizeHandle.Bottom => (0, 1),
        ResizeHandle.BottomLeft => (-1, 1),
        ResizeHandle.Left => (-1, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(handle), handle, "Tirador desconocido."),
    };
}
