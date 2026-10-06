namespace Acetato.Domain;

/// <summary>
/// Construye la punta de la flecha libre a partir del recorrido del trazo. El vértice
/// es el último punto; la dirección sale del tramo final (no del último punto, que
/// suele temblar). Tipo puro (sin WPF), testeable.
/// </summary>
public static class ArrowHeadBuilder
{
    private const double MinHeadLength = 16d;
    private const double HeadLengthPerThickness = 3d;
    private const double WingWidthRatio = 0.6d;
    private const double FinalStretchRatio = 1.5d;
    private const double Epsilon = 1e-6;

    /// <summary>
    /// Calcula la punta para <paramref name="path"/>. Devuelve <c>false</c> si el
    /// recorrido es más corto que la punta (la flecha se descarta).
    /// </summary>
    public static bool TryBuild(IReadOnlyList<StrokePoint> path, double thickness, out ArrowHead head)
    {
        ArgumentNullException.ThrowIfNull(path);
        head = default;

        double headLength = Math.Max(MinHeadLength, HeadLengthPerThickness * thickness);
        if (path.Count < 2 || PathLength(path) < headLength)
        {
            return false;
        }

        var tip = path[^1];
        var stretchStart = FinalStretchStart(path, headLength * FinalStretchRatio);
        var (ux, uy) = Direction(path, stretchStart, tip);
        head = Assemble(tip, ux, uy, headLength);
        return true;
    }

    // Base de la punta a `headLength` hacia atrás del vértice; las alas salen
    // perpendiculares a la dirección, a ±WingWidthRatio × largo.
    private static ArrowHead Assemble(StrokePoint tip, double ux, double uy, double headLength)
    {
        double baseX = tip.X - (ux * headLength);
        double baseY = tip.Y - (uy * headLength);
        double wing = headLength * WingWidthRatio;

        return new ArrowHead(
            Left: new StrokePoint(baseX + (uy * wing), baseY - (ux * wing)),
            Tip: tip,
            Right: new StrokePoint(baseX - (uy * wing), baseY + (ux * wing)));
    }

    // Punto del recorrido situado a `stretchLength` (medido sobre el trazo) del final.
    // Se interpola dentro del segmento para no depender de lo espaciados que estén
    // los puntos: con un ratón rápido el último punto podría quedar casi encima del final.
    private static StrokePoint FinalStretchStart(IReadOnlyList<StrokePoint> path, double stretchLength)
    {
        double remaining = stretchLength;
        for (int i = path.Count - 1; i > 0; i--)
        {
            double segment = Distance(path[i - 1], path[i]);
            if (segment >= remaining)
            {
                double t = remaining / segment;
                return new StrokePoint(
                    path[i].X + ((path[i - 1].X - path[i].X) * t),
                    path[i].Y + ((path[i - 1].Y - path[i].Y) * t));
            }

            remaining -= segment;
        }

        return path[0];
    }

    // Vector unitario inicio-del-tramo → vértice. Si el tramo se cierra sobre sí mismo
    // (cuerda nula), se usa el último segmento con longitud.
    private static (double X, double Y) Direction(IReadOnlyList<StrokePoint> path, StrokePoint from, StrokePoint to)
    {
        double chord = Distance(from, to);
        if (chord > Epsilon)
        {
            return ((to.X - from.X) / chord, (to.Y - from.Y) / chord);
        }

        for (int i = path.Count - 1; i > 0; i--)
        {
            double segment = Distance(path[i - 1], path[i]);
            if (segment > Epsilon)
            {
                return ((path[i].X - path[i - 1].X) / segment, (path[i].Y - path[i - 1].Y) / segment);
            }
        }

        return (1d, 0d);
    }

    private static double PathLength(IReadOnlyList<StrokePoint> path)
    {
        double total = 0d;
        for (int i = 1; i < path.Count; i++)
        {
            total += Distance(path[i - 1], path[i]);
        }

        return total;
    }

    private static double Distance(StrokePoint a, StrokePoint b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }
}
