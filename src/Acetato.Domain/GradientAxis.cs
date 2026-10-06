using System.Runtime.InteropServices;

namespace Acetato.Domain;

/// <summary>
/// Eje del degradado de un trazo (HU-19): el color inicial va en <see cref="Start"/> y
/// el final en <see cref="End"/>, en coordenadas del lienzo. Normalmente es del primer
/// al último punto del trazo; en una figura cerrada (inicio ≈ fin, como el rectángulo)
/// ese eje no tendría longitud, así que corre en diagonal del recuadro que la encierra.
/// Tipo puro (sin WPF), testeable.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct GradientAxis(StrokePoint Start, StrokePoint End)
{
    // Inicio y fin a menos de este múltiplo del grosor cuentan como figura cerrada.
    private const double ClosedFigureThicknessFactor = 2d;

    /// <summary>Calcula el eje del degradado de <paramref name="path"/> con el grosor dado.</summary>
    public static GradientAxis For(IReadOnlyList<StrokePoint> path, double thickness)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (path.Count == 0)
        {
            return default;
        }

        var first = path[0];
        var last = path[^1];
        return Distance(first, last) >= ClosedFigureThicknessFactor * thickness
            ? new GradientAxis(first, last)
            : Diagonal(path);
    }

    // Esquina superior izquierda → inferior derecha del recuadro de los puntos.
    private static GradientAxis Diagonal(IReadOnlyList<StrokePoint> path)
    {
        double left = double.MaxValue;
        double top = double.MaxValue;
        double right = double.MinValue;
        double bottom = double.MinValue;
        foreach (var point in path)
        {
            left = Math.Min(left, point.X);
            top = Math.Min(top, point.Y);
            right = Math.Max(right, point.X);
            bottom = Math.Max(bottom, point.Y);
        }

        return new GradientAxis(new StrokePoint(left, top), new StrokePoint(right, bottom));
    }

    private static double Distance(StrokePoint a, StrokePoint b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }
}
