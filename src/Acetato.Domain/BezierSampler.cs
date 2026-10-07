namespace Acetato.Domain;

/// <summary>
/// Convierte una cadena de curvas Bézier en puntos densos (HU-20). El trazo se guarda
/// como puntos, así que la flecha suavizada vuelve a puntos para que historial, borrador
/// y captura sigan funcionando igual. Tipo puro (sin WPF), testeable.
/// </summary>
public static class BezierSampler
{
    // Subdivisiones para estimar la longitud de cada curva (precisión de sobra para
    // decidir cuántas muestras tomar).
    private const int LengthSteps = 16;

    /// <summary>
    /// Muestrea <paramref name="curves"/> con unos <paramref name="spacing"/> DIP entre
    /// puntos. Incluye el inicio de la primera curva y el final de la última; el punto
    /// compartido entre dos curvas seguidas no se repite.
    /// </summary>
    public static IReadOnlyList<StrokePoint> Sample(IReadOnlyList<CubicBezier> curves, double spacing)
    {
        ArgumentNullException.ThrowIfNull(curves);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(spacing);
        if (curves.Count == 0)
        {
            return [];
        }

        var points = new List<StrokePoint> { curves[0].P0 };
        foreach (var curve in curves)
        {
            int samples = Math.Max(1, (int)Math.Ceiling(ApproximateLength(curve) / spacing));
            for (int i = 1; i <= samples; i++)
            {
                points.Add(curve.At((double)i / samples));
            }
        }

        return points;
    }

    private static double ApproximateLength(CubicBezier curve)
    {
        double length = 0d;
        var previous = curve.P0;
        for (int i = 1; i <= LengthSteps; i++)
        {
            var current = curve.At((double)i / LengthSteps);
            length += previous.DistanceTo(current);
            previous = current;
        }

        return length;
    }
}
