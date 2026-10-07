namespace Acetato.Domain;

/// <summary>
/// Cálculos del ajuste Bézier de <see cref="BezierFitter"/> (HU-20), separados para que
/// cada pieza quede corta y testeable a través del ajuste: tangentes, parametrización,
/// mínimos cuadrados, error máximo y paso de Newton. Los puntos se usan como vectores.
/// </summary>
internal static class BezierFitMath
{
    private const double Epsilon = 1e-9;

    /// <summary>Copia del recorrido sin puntos consecutivos repetidos (romperían la parametrización).</summary>
    public static List<StrokePoint> WithoutDuplicates(IReadOnlyList<StrokePoint> path)
    {
        var points = new List<StrokePoint>(path.Count);
        foreach (var point in path)
        {
            if (points.Count == 0 || points[^1].DistanceTo(point) > Epsilon)
            {
                points.Add(point);
            }
        }

        return points;
    }

    /// <summary>
    /// Tangente unitaria en el extremo <paramref name="index"/>, hacia el interior
    /// (<paramref name="step"/> +1 o −1). Mira un punto a unos <paramref name="window"/>
    /// DIP en vez del vecino inmediato, que con el temblor del pulso torcería la tangente.
    /// </summary>
    public static StrokePoint TangentFrom(IReadOnlyList<StrokePoint> points, int index, int step, double window) =>
        Normalize(Subtract(Reach(points, index, step, window), points[index]));

    /// <summary>Tangente en un punto interior de corte, apuntando hacia atrás (hacia el inicio).</summary>
    public static StrokePoint CenterTangent(IReadOnlyList<StrokePoint> points, int index, double window) =>
        Normalize(Subtract(Reach(points, index, -1, window), Reach(points, index, +1, window)));

    public static StrokePoint Negate(StrokePoint v) => new(-v.X, -v.Y);

    /// <summary>Curva de un tramo de dos puntos: controles a un tercio de la cuerda.</summary>
    public static CubicBezier Heuristic(IReadOnlyList<StrokePoint> points, FitSegment segment)
    {
        var first = points[segment.First];
        var last = points[segment.Last];
        double third = first.DistanceTo(last) / 3d;
        return Build(first, last, segment, third, third);
    }

    /// <summary>Parámetro t de cada punto del tramo, proporcional a la longitud recorrida.</summary>
    public static double[] ChordLengthParameters(IReadOnlyList<StrokePoint> points, FitSegment segment)
    {
        var u = new double[segment.Count];
        for (int i = 1; i < u.Length; i++)
        {
            u[i] = u[i - 1] + points[segment.First + i - 1].DistanceTo(points[segment.First + i]);
        }

        double total = u[^1];
        for (int i = 1; i < u.Length; i++)
        {
            u[i] /= total;
        }

        return u;
    }

    /// <summary>
    /// Curva del tramo con extremos y tangentes fijos: resuelve por mínimos cuadrados qué
    /// tan lejos van los controles. Si la solución es degenerada, cae a la heurística.
    /// </summary>
    public static CubicBezier LeastSquares(IReadOnlyList<StrokePoint> points, FitSegment segment, double[] u)
    {
        var first = points[segment.First];
        var last = points[segment.Last];
        double c00 = 0d, c01 = 0d, c11 = 0d, x0 = 0d, x1 = 0d;
        for (int i = 0; i < u.Length; i++)
        {
            var (b0, b1, b2, b3) = Bernstein(u[i]);
            var a0 = Scale(segment.TangentStart, b1);
            var a1 = Scale(segment.TangentEnd, b2);
            c00 += Dot(a0, a0);
            c01 += Dot(a0, a1);
            c11 += Dot(a1, a1);
            var tmp = Subtract(points[segment.First + i], Add(Scale(first, b0 + b1), Scale(last, b2 + b3)));
            x0 += Dot(a0, tmp);
            x1 += Dot(a1, tmp);
        }

        double det = (c00 * c11) - (c01 * c01);
        double alphaStart = Math.Abs(det) < Epsilon ? 0d : ((x0 * c11) - (x1 * c01)) / det;
        double alphaEnd = Math.Abs(det) < Epsilon ? 0d : ((c00 * x1) - (c01 * x0)) / det;
        double minAlpha = 1e-6 * first.DistanceTo(last);
        return alphaStart < minAlpha || alphaEnd < minAlpha
            ? Heuristic(points, segment)
            : Build(first, last, segment, alphaStart, alphaEnd);
    }

    /// <summary>
    /// Mayor distancia (al cuadrado) entre un punto interior del tramo y la curva en su
    /// parámetro, y el índice de ese punto (donde se partiría el tramo).
    /// </summary>
    public static (double Error, int Split) MaxError(
        IReadOnlyList<StrokePoint> points, FitSegment segment, CubicBezier curve, double[] u)
    {
        double maxError = 0d;
        int split = segment.First + (segment.Count / 2);
        for (int i = 1; i < u.Length - 1; i++)
        {
            var diff = Subtract(curve.At(u[i]), points[segment.First + i]);
            double error = Dot(diff, diff);
            if (error >= maxError)
            {
                maxError = error;
                split = segment.First + i;
            }
        }

        return (maxError, split);
    }

    /// <summary>Un paso de Newton por punto: acerca cada t al punto de la curva más próximo.</summary>
    public static double[] Reparameterize(
        IReadOnlyList<StrokePoint> points, FitSegment segment, CubicBezier curve, double[] u)
    {
        var refined = new double[u.Length];
        for (int i = 0; i < u.Length; i++)
        {
            refined[i] = NewtonStep(curve, points[segment.First + i], u[i]);
        }

        return refined;
    }

    // Raíz de (Q(t) − P) · Q'(t) = 0 con un paso de Newton; t queda en [0, 1].
    private static double NewtonStep(CubicBezier curve, StrokePoint point, double t)
    {
        var diff = Subtract(curve.At(t), point);
        var (d1, d2) = Derivatives(curve, t);
        double denominator = Dot(d1, d1) + Dot(diff, d2);
        return Math.Abs(denominator) < Epsilon ? t : Math.Clamp(t - (Dot(diff, d1) / denominator), 0d, 1d);
    }

    // Primera y segunda derivada de la curva en t.
    private static (StrokePoint First, StrokePoint Second) Derivatives(CubicBezier c, double t)
    {
        var q0 = Scale(Subtract(c.P1, c.P0), 3d);
        var q1 = Scale(Subtract(c.P2, c.P1), 3d);
        var q2 = Scale(Subtract(c.P3, c.P2), 3d);
        double u = 1d - t;
        var first = Add(Add(Scale(q0, u * u), Scale(q1, 2d * u * t)), Scale(q2, t * t));
        var second = Add(Scale(Subtract(q1, q0), 2d * u), Scale(Subtract(q2, q1), 2d * t));
        return (first, second);
    }

    private static CubicBezier Build(StrokePoint first, StrokePoint last, FitSegment segment, double alphaStart, double alphaEnd) =>
        new(first, Add(first, Scale(segment.TangentStart, alphaStart)), Add(last, Scale(segment.TangentEnd, alphaEnd)), last);

    // Primer punto a `window` DIP o más de `index` avanzando con `step`; el extremo si no hay.
    private static StrokePoint Reach(IReadOnlyList<StrokePoint> points, int index, int step, double window)
    {
        int i = index + step;
        while (i > 0 && i < points.Count - 1 && points[index].DistanceTo(points[i]) < window)
        {
            i += step;
        }

        return points[Math.Clamp(i, 0, points.Count - 1)];
    }

    private static (double B0, double B1, double B2, double B3) Bernstein(double t)
    {
        double u = 1d - t;
        return (u * u * u, 3d * u * u * t, 3d * u * t * t, t * t * t);
    }

    private static StrokePoint Normalize(StrokePoint v)
    {
        double length = Math.Sqrt(Dot(v, v));
        return length < Epsilon ? new StrokePoint(1d, 0d) : Scale(v, 1d / length);
    }

    private static StrokePoint Add(StrokePoint a, StrokePoint b) => new(a.X + b.X, a.Y + b.Y);

    private static StrokePoint Subtract(StrokePoint a, StrokePoint b) => new(a.X - b.X, a.Y - b.Y);

    private static StrokePoint Scale(StrokePoint v, double factor) => new(v.X * factor, v.Y * factor);

    private static double Dot(StrokePoint a, StrokePoint b) => (a.X * b.X) + (a.Y * b.Y);
}
