using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Desplaza trazos y textos del lienzo (SPEC 11). Lo comparten el arrastre de la
/// herramienta Mover (y de Ctrl) y su Deshacer, que aplica el desplazamiento inverso.
/// Los trazos se mueven con <see cref="Stroke.Transform"/> y los textos con su
/// posición en el <see cref="InkCanvas"/>.
/// </summary>
internal static class AnnotationOffset
{
    /// <summary>Mueve <paramref name="strokes"/> y <paramref name="elements"/> por <paramref name="offset"/>.</summary>
    public static void Apply(IEnumerable<Stroke> strokes, IEnumerable<UIElement> elements, Vector offset)
    {
        var translation = new Matrix();
        translation.Translate(offset.X, offset.Y);
        foreach (var stroke in strokes)
        {
            stroke.Transform(translation, applyToStylusTip: false);
        }

        foreach (var element in elements)
        {
            InkCanvas.SetLeft(element, PositionOf(InkCanvas.GetLeft(element)) + offset.X);
            InkCanvas.SetTop(element, PositionOf(InkCanvas.GetTop(element)) + offset.Y);
        }
    }

    // Un hijo sin posición fijada (NaN) está en el origen del lienzo.
    private static double PositionOf(double value) => double.IsNaN(value) ? 0d : value;
}
