using System.Runtime.InteropServices;

namespace Acetato.Domain;

/// <summary>
/// Curva Bézier cúbica (HU-20): extremos <see cref="P0"/> y <see cref="P3"/>, puntos de
/// control <see cref="P1"/> y <see cref="P2"/>. Es la pieza con la que se suaviza la
/// flecha libre. Tipo puro (sin WPF), testeable.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct CubicBezier(StrokePoint P0, StrokePoint P1, StrokePoint P2, StrokePoint P3)
{
    /// <summary>Punto de la curva en <paramref name="t"/> ∈ [0, 1] (forma de Bernstein).</summary>
    public StrokePoint At(double t)
    {
        double u = 1d - t;
        double b0 = u * u * u;
        double b1 = 3d * u * u * t;
        double b2 = 3d * u * t * t;
        double b3 = t * t * t;
        return new StrokePoint(
            (b0 * P0.X) + (b1 * P1.X) + (b2 * P2.X) + (b3 * P3.X),
            (b0 * P0.Y) + (b1 * P1.Y) + (b2 * P2.Y) + (b3 * P3.Y));
    }
}
