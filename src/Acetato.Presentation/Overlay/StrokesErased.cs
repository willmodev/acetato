using System.Windows.Ink;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Entrada de trazos quitados con el Borrador (SPEC 11). Deshacerla los devuelve a
/// su índice original, así cada trazo vuelve a su misma capa (delante o detrás de
/// los demás). Un mismo gesto del Borrador puede quitar varios a la vez.
/// </summary>
internal sealed class StrokesErased : IHistoryEntry
{
    private readonly StrokeCollection _strokes;
    private readonly List<ErasedStroke> _erased;

    /// <param name="strokes">Colección de la que salieron.</param>
    /// <param name="erased">Cada trazo con el índice que tenía antes de borrarse.</param>
    public StrokesErased(StrokeCollection strokes, IEnumerable<ErasedStroke> erased)
    {
        _strokes = strokes;
        // De menor a mayor índice: al insertar en orden, cada uno cae en su sitio.
        _erased = [.. erased.OrderBy(entry => entry.Index)];
    }

    public void Undo()
    {
        foreach (var (stroke, index) in _erased)
        {
            if (!_strokes.Contains(stroke))
            {
                _strokes.Insert(Math.Min(index, _strokes.Count), stroke);
            }
        }
    }
}
