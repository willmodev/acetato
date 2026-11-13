using System.Windows;
using System.Windows.Media;
using Acetato.Domain;

namespace Acetato.Presentation.Resources;

/// <summary>
/// Pincel del par de degradado en diagonal sobre el recuadro del elemento: del color de inicio
/// arriba a la izquierda al de fin abajo a la derecha, como se ve en las figuras cerradas
/// (HU-19). Lo usan el cursor de tinta (HU-26) y el texto (spec 15). El pincel es relativo al
/// recuadro, así se adapta solo cuando el texto crece al escribir o cambia de tamaño.
/// </summary>
internal static class DiagonalGradientBrush
{
    /// <summary>Pincel congelado con los colores de <paramref name="pair"/>.</summary>
    public static LinearGradientBrush Create(GradientPair pair)
    {
        var (start, end) = GradientPairMap.Resolve(pair);
        var brush = new LinearGradientBrush(start, end, new Point(0d, 0d), new Point(1d, 1d));
        brush.Freeze();
        return brush;
    }
}
