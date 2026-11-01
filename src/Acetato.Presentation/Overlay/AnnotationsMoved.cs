using System.Windows;
using System.Windows.Ink;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Entrada de trazos y textos movidos, con la mano o con Ctrl (SPEC 11). Deshacerla
/// les aplica el desplazamiento inverso.
/// </summary>
internal sealed class AnnotationsMoved : IHistoryEntry
{
    private readonly IReadOnlyList<Stroke> _strokes;
    private readonly IReadOnlyList<UIElement> _elements;
    private readonly Vector _offset;

    public AnnotationsMoved(IReadOnlyList<Stroke> strokes, IReadOnlyList<UIElement> elements, Vector offset)
    {
        _strokes = strokes;
        _elements = elements;
        _offset = offset;
    }

    public void Undo() => AnnotationOffset.Apply(_strokes, _elements, -_offset);
}
