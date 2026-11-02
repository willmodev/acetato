using System.Windows.Ink;
using Acetato.Domain;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Entrada de trazos y textos redimensionados (HU-28). Deshacerla devuelve los trazos
/// a su recuadro original y los textos a su sitio y su letra de antes.
/// </summary>
internal sealed class AnnotationsResized : IHistoryEntry
{
    private readonly IReadOnlyList<Stroke> _strokes;
    private readonly IReadOnlyList<TextSnapshot> _texts;
    private readonly SelectionBox _final;
    private readonly SelectionBox _original;

    public AnnotationsResized(IReadOnlyList<Stroke> strokes, IReadOnlyList<TextSnapshot> texts, SelectionBox final, SelectionBox original)
    {
        _strokes = strokes;
        _texts = texts;
        _final = final;
        _original = original;
    }

    public void Undo()
    {
        AnnotationScale.Apply(_strokes, [], _final, _original);
        foreach (var text in _texts)
        {
            text.Restore();
        }
    }
}
