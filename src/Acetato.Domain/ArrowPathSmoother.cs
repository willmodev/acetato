namespace Acetato.Domain;

/// <summary>
/// Suavizado final de la flecha libre (HU-20): ajusta el recorrido a curvas Bézier y lo
/// vuelve a muestrear como puntos densos. La tolerancia crece con el grosor: un trazo
/// grueso disimula menos el temblor y admite una curva más limpia. Tipo puro, testeable.
/// </summary>
public static class ArrowPathSmoother
{
    // Valores iniciales, ajustables a ojo (spec 04): tolerancia = base + factor × grosor.
    private const double BaseTolerance = 4d;
    private const double TolerancePerThickness = 0.5d;
    private const double SampleSpacing = 2d;

    /// <summary>Recorrido suavizado; con menos de tres puntos se devuelve tal cual.</summary>
    public static IReadOnlyList<StrokePoint> Smooth(IReadOnlyList<StrokePoint> path, double thickness)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (path.Count < 3)
        {
            return path;
        }

        var curves = BezierFitter.Fit(path, ToleranceFor(thickness));
        return curves.Count == 0 ? path : BezierSampler.Sample(curves, SampleSpacing);
    }

    /// <summary>Error máximo permitido entre el recorrido y la curva, en DIP.</summary>
    public static double ToleranceFor(double thickness) => BaseTolerance + (TolerancePerThickness * thickness);
}
