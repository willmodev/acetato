using System.Windows;
using System.Windows.Controls;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Entrada de una selección borrada con Supr (SPEC 11). Deshacerla devuelve de una vez
/// sus trazos, cada uno a su capa, y sus textos, cada uno a su sitio entre los hijos
/// del lienzo.
/// </summary>
internal sealed class SelectionDeleted : IHistoryEntry
{
    private readonly StrokesErased _strokes;
    private readonly InkCanvas _canvas;
    private readonly List<(UIElement Element, int Index)> _elements;

    public SelectionDeleted(StrokesErased strokes, InkCanvas canvas, IEnumerable<(UIElement Element, int Index)> elements)
    {
        _strokes = strokes;
        _canvas = canvas;
        // De menor a mayor índice: al insertar en orden, cada uno cae en su sitio.
        _elements = [.. elements.OrderBy(entry => entry.Index)];
    }

    public void Undo()
    {
        _strokes.Undo();
        foreach (var (element, index) in _elements)
        {
            if (!_canvas.Children.Contains(element))
            {
                _canvas.Children.Insert(Math.Min(index, _canvas.Children.Count), element);
            }
        }
    }
}
