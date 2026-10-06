using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Media;
using Acetato.Domain;
using Acetato.Presentation.Resources;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Fabrica el pincel de degradado de un trazo (HU-19). El degradado se pinta en
/// coordenadas absolutas del lienzo, sobre el eje que calcula <see cref="GradientAxis"/>.
/// Los colores de cada par (<see cref="GradientStopCollection"/>) se resuelven una sola
/// vez desde los tokens y se comparten, congelados, entre todos los trazos del mismo par.
/// </summary>
internal static class StrokeGradientBrush
{
    private static readonly ConcurrentDictionary<TintaGradiente, GradientStopCollection> StopsByPair = new();

    /// <summary>Pincel congelado del par <paramref name="pair"/> para un trazo con ese recorrido y grosor.</summary>
    public static Brush Create(TintaGradiente pair, IReadOnlyList<StrokePoint> path, double thickness)
    {
        var axis = GradientAxis.For(path, thickness);
        var brush = new LinearGradientBrush(
            StopsByPair.GetOrAdd(pair, BuildStops),
            new Point(axis.Start.X, axis.Start.Y),
            new Point(axis.End.X, axis.End.Y))
        {
            MappingMode = BrushMappingMode.Absolute,
        };
        brush.Freeze();
        return brush;
    }

    private static GradientStopCollection BuildStops(TintaGradiente pair)
    {
        var (start, end) = TintaGradienteMap.Resolve(pair);
        var stops = new GradientStopCollection { new GradientStop(start, 0d), new GradientStop(end, 1d) };
        stops.Freeze();
        return stops;
    }
}
