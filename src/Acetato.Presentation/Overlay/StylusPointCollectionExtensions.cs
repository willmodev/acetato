using System.Windows.Input;
using Acetato.Domain;

namespace Acetato.Presentation.Overlay;

/// <summary>Puente entre los puntos del lienzo (WPF) y los de dominio.</summary>
internal static class StylusPointCollectionExtensions
{
    /// <summary>Recorrido del trazo como puntos de dominio, en el mismo orden.</summary>
    public static List<StrokePoint> ToStrokePoints(this StylusPointCollection points)
    {
        ArgumentNullException.ThrowIfNull(points);

        var path = new List<StrokePoint>(points.Count);
        foreach (var point in points)
        {
            path.Add(new StrokePoint(point.X, point.Y));
        }

        return path;
    }
}
