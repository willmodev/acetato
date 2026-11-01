using System.Windows.Ink;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Entrada de trazo dibujado en el historial (HU-13). Deshacerla es quitar el
/// <see cref="Stroke"/> de su <see cref="StrokeCollection"/>.
/// </summary>
internal sealed class StrokeAdded : IHistoryEntry
{
    private readonly StrokeCollection _strokes;

    public StrokeAdded(StrokeCollection strokes, Stroke stroke)
    {
        _strokes = strokes;
        Stroke = stroke;
    }

    /// <summary>Trazo representado; lo usa el historial para sincronizar con la colección.</summary>
    public Stroke Stroke { get; }

    public void Undo() => _strokes.Remove(Stroke);
}
