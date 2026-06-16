using System.Windows;
using System.Windows.Controls;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Entrada de texto en el historial unificado (HU-13). Deshacerla es quitar el
/// <see cref="UIElement"/> de los hijos de su <see cref="InkCanvas"/>. Se guarda
/// la referencia al lienzo porque el padre lógico de un hijo del InkCanvas es el
/// propio InkCanvas (no un Panel), así que quitarlo hay que hacerlo por su
/// colección <c>Children</c>.
/// </summary>
internal sealed class TextAnnotation : IAnnotation
{
    private readonly InkCanvas _canvas;
    private readonly UIElement _element;

    public TextAnnotation(InkCanvas canvas, UIElement element)
    {
        _canvas = canvas;
        _element = element;
    }

    public void Remove() => _canvas.Children.Remove(_element);
}
