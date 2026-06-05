using System.Windows.Ink;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Deshacer el último trazo (HU-07). Opera sobre la <see cref="StrokeCollection"/>
/// como única fuente de verdad: quita el último trazo añadido si lo hay y no
/// falla si está vacía. Coexiste con la limpieza total (HU-04): tras un Clear no
/// queda nada que deshacer (el backlog no pide rehacer).
/// </summary>
internal sealed class UndoStack
{
    private readonly StrokeCollection _strokes;

    public UndoStack(StrokeCollection strokes) => _strokes = strokes;

    /// <summary>Quita el último trazo; sin trazos no hace nada.</summary>
    public void UndoLast()
    {
        if (_strokes.Count == 0)
        {
            return;
        }

        _strokes.RemoveAt(_strokes.Count - 1);
    }
}
