using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Sitio y tamaño de letra de un texto antes de redimensionarlo (HU-28). Deshacer lo
/// devuelve tal cual, sin el error que dejaría escalar la letra de vuelta.
/// </summary>
internal sealed record TextSnapshot(UIElement Element, double Left, double Top, double FontSize)
{
    /// <summary>Foto del estado actual de <paramref name="element"/>.</summary>
    public static TextSnapshot Of(UIElement element) => new(
        element,
        InkCanvas.GetLeft(element),
        InkCanvas.GetTop(element),
        (double)element.GetValue(TextElement.FontSizeProperty));

    /// <summary>Devuelve el texto a la foto.</summary>
    public void Restore()
    {
        InkCanvas.SetLeft(Element, Left);
        InkCanvas.SetTop(Element, Top);
        Element.SetValue(TextElement.FontSizeProperty, FontSize);
    }
}
