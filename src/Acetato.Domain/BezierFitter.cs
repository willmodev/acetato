namespace Acetato.Domain;

/// <summary>
/// Ajusta un recorrido a mano alzada a una cadena de curvas Bézier cúbicas (HU-20), con
/// el algoritmo de Schneider ("An Algorithm for Automatically Fitting Digitized Curves",
/// Graphics Gems, 1990): tangentes en los extremos, parametrización por longitud de
/// cuerda, mínimos cuadrados y reparametrización de Newton. Si una curva se aleja de los
/// puntos más que la tolerancia, el tramo se parte en el punto de mayor error: así una
/// esquina marcada se conserva en vez de redondearse. Tipo puro (sin WPF), testeable.
/// </summary>
public static class BezierFitter
{
    // Rondas de reparametrización antes de partir el tramo, y margen de error (× la
    // tolerancia) por debajo del cual vale la pena intentarlas.
    private const int MaxReparameterizations = 4;
    private const double ReparameterizeErrorFactor = 4d;

    /// <summary>
    /// Ajusta <paramref name="path"/> con un error máximo de <paramref name="tolerance"/>
    /// DIP entre cada punto y la curva. Con menos de dos puntos distintos devuelve una
    /// lista vacía.
    /// </summary>
    public static IReadOnlyList<CubicBezier> Fit(IReadOnlyList<StrokePoint> path, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tolerance);

        var points = BezierFitMath.WithoutDuplicates(path);
        var curves = new List<CubicBezier>();
        if (points.Count < 2)
        {
            return curves;
        }

        double window = 2d * tolerance;
        var whole = new FitSegment(
            0,
            points.Count - 1,
            BezierFitMath.TangentFrom(points, 0, +1, window),
            BezierFitMath.TangentFrom(points, points.Count - 1, -1, window));
        FitSegment(new FitContext(points, tolerance * tolerance, window, curves), whole);
        return curves;
    }

    private static void FitSegment(FitContext context, FitSegment segment)
    {
        var points = context.Points;
        if (segment.Count == 2)
        {
            context.Curves.Add(BezierFitMath.Heuristic(points, segment));
            return;
        }

        var u = BezierFitMath.ChordLengthParameters(points, segment);
        var curve = BezierFitMath.LeastSquares(points, segment, u);
        var (error, split) = BezierFitMath.MaxError(points, segment, curve, u);
        if (error <= context.ErrorSquared)
        {
            context.Curves.Add(curve);
            return;
        }

        if (error <= context.ErrorSquared * ReparameterizeErrorFactor * ReparameterizeErrorFactor
            && TryReparameterize(context, segment, u, ref curve))
        {
            context.Curves.Add(curve);
            return;
        }

        Split(context, segment, split);
    }

    // Afina los parámetros con Newton y reajusta; true si alguna ronda queda en tolerancia
    // (entonces `curve` es la curva afinada).
    private static bool TryReparameterize(FitContext context, FitSegment segment, double[] u, ref CubicBezier curve)
    {
        var current = curve;
        for (int round = 0; round < MaxReparameterizations; round++)
        {
            u = BezierFitMath.Reparameterize(context.Points, segment, current, u);
            current = BezierFitMath.LeastSquares(context.Points, segment, u);
            if (BezierFitMath.MaxError(context.Points, segment, current, u).Error <= context.ErrorSquared)
            {
                curve = current;
                return true;
            }
        }

        return false;
    }

    // Parte el tramo en `split`: la tangente allí es común a ambas mitades (curva
    // continua) y apunta hacia atrás para la izquierda y hacia adelante para la derecha.
    private static void Split(FitContext context, FitSegment segment, int split)
    {
        var center = BezierFitMath.CenterTangent(context.Points, split, context.TangentWindow);
        FitSegment(context, segment with { Last = split, TangentEnd = center });
        FitSegment(context, segment with { First = split, TangentStart = BezierFitMath.Negate(center) });
    }

    // Datos fijos durante un ajuste, para no arrastrarlos como parámetros en la recursión.
    private sealed record FitContext(
        IReadOnlyList<StrokePoint> Points, double ErrorSquared, double TangentWindow, List<CubicBezier> Curves);
}
