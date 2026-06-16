using System.Windows.Ink;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Entrada de trazo en el historial unificado (HU-13). Deshacerla es quitar el
/// <see cref="Stroke"/> de su <see cref="StrokeCollection"/>.
/// </summary>
internal sealed class StrokeAnnotation : IAnnotation
{
    private readonly StrokeCollection _strokes;

    public StrokeAnnotation(StrokeCollection strokes, Stroke stroke)
    {
        _strokes = strokes;
        Stroke = stroke;
    }

    /// <summary>Trazo representado; lo usa el historial para sincronizar con la colección.</summary>
    public Stroke Stroke { get; }

    public void Remove() => _strokes.Remove(Stroke);
}
