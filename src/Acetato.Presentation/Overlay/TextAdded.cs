using System.Windows;
using System.Windows.Controls;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Entrada de texto fijado en el historial (HU-13). Deshacerla es quitar el
/// <see cref="UIElement"/> de los hijos de su <see cref="InkCanvas"/>. Se guarda
/// la referencia al lienzo porque el padre lógico de un hijo del InkCanvas es el
/// propio InkCanvas (no un Panel), así que quitarlo hay que hacerlo por su
/// colección <c>Children</c>.
/// </summary>
internal sealed class TextAdded : IHistoryEntry
{
    private readonly InkCanvas _canvas;
    private readonly UIElement _element;

    public TextAdded(InkCanvas canvas, UIElement element)
    {
        _canvas = canvas;
        _element = element;
    }

    public void Undo() => _canvas.Children.Remove(_element);
}
