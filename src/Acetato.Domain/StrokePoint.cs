using System.Runtime.InteropServices;

namespace Acetato.Domain;

/// <summary>
/// Punto de un trazo, en coordenadas lógicas (independientes de DPI).
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct StrokePoint(double X, double Y)
{
    /// <summary>Distancia euclídea hasta <paramref name="other"/>.</summary>
    public double DistanceTo(StrokePoint other)
    {
        double dx = other.X - X;
        double dy = other.Y - Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }
}
