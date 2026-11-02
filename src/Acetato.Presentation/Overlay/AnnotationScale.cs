using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using Acetato.Domain;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Lleva trazos y textos de un recuadro a otro (HU-28). Lo comparten el redimensionado
/// en curso y su Deshacer. Los trazos se transforman sin tocar su grosor
/// (<c>applyToStylusTip: false</c>). Los textos colocan su esquina con la misma matriz
/// y escalan su letra por igual en los dos ejes, para no deformarse.
/// </summary>
internal static class AnnotationScale
{
    private const double Epsilon = 1e-6;

    /// <summary>Transforma <paramref name="strokes"/> y <paramref name="elements"/> de <paramref name="from"/> a <paramref name="to"/>.</summary>
    public static void Apply(IEnumerable<Stroke> strokes, IEnumerable<UIElement> elements, SelectionBox from, SelectionBox to)
    {
        var (scaleX, scaleY) = Scales(from, to);
        var matrix = new Matrix();
        matrix.Translate(-from.X, -from.Y);
        matrix.Scale(scaleX, scaleY);
        matrix.Translate(to.X, to.Y);

        foreach (var stroke in strokes)
        {
            stroke.Transform(matrix, applyToStylusTip: false);
        }

        // La letra crece según el área: igual que la escala en modo parejo y, en modo
        // libre, un término medio entre los dos ejes.
        double fontScale = Math.Sqrt(scaleX * scaleY);
        foreach (var element in elements)
        {
            var corner = matrix.Transform(new Point(PositionOf(InkCanvas.GetLeft(element)), PositionOf(InkCanvas.GetTop(element))));
            InkCanvas.SetLeft(element, corner.X);
            InkCanvas.SetTop(element, corner.Y);
            element.SetValue(TextElement.FontSizeProperty, (double)element.GetValue(TextElement.FontSizeProperty) * fontScale);
        }
    }

    // Escala de cada eje; un eje sin tamaño se queda como está.
    private static (double X, double Y) Scales(SelectionBox from, SelectionBox to) =>
        (from.Width < Epsilon ? 1d : to.Width / from.Width, from.Height < Epsilon ? 1d : to.Height / from.Height);

    // Un hijo sin posición fijada (NaN) está en el origen del lienzo.
    private static double PositionOf(double value) => double.IsNaN(value) ? 0d : value;
}
