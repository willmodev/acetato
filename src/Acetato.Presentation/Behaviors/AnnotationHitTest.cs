using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Busca la anotación que hay bajo el cursor (SPEC 11): un texto del lienzo o, si no,
/// el trazo de más arriba. Lo usan la mano y el atajo de mantener Ctrl para saber qué
/// agarrar.
/// </summary>
internal static class AnnotationHitTest
{
    // Diámetro mínimo alrededor del cursor, en DIP: un trazo fino se agarra sin puntería fina.
    private const double Reach = 8d;

    /// <summary>Lo que hay bajo <paramref name="point"/>; vacío si es un sitio sin dibujo.</summary>
    public static GrabTargets At(InkCanvas canvas, Point point)
    {
        if (TextAt(canvas, point) is { } element)
        {
            return new GrabTargets([], [element]);
        }

        return StrokeAt(canvas.Strokes, point) is { } stroke
            ? new GrabTargets([stroke], [])
            : GrabTargets.None;
    }

    // De arriba abajo: el último hijo añadido es el que se ve encima.
    private static UIElement? TextAt(InkCanvas canvas, Point point)
    {
        for (int i = canvas.Children.Count - 1; i >= 0; i--)
        {
            var child = canvas.Children[i];
            var bounds = new Rect(new Point(Position(InkCanvas.GetLeft(child)), Position(InkCanvas.GetTop(child))), child.RenderSize);
            if (bounds.Contains(point))
            {
                return child;
            }
        }

        return null;
    }

    // El trazo de más arriba (el último de la colección) entre los que toca el cursor.
    private static Stroke? StrokeAt(StrokeCollection strokes, Point point)
    {
        var hits = strokes.HitTest(point, Reach);
        for (int i = strokes.Count - 1; i >= 0; i--)
        {
            if (hits.Contains(strokes[i]))
            {
                return strokes[i];
            }
        }

        return null;
    }

    private static double Position(double value) => double.IsNaN(value) ? 0d : value;
}
