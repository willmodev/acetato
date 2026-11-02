using System.Windows;
using System.Windows.Ink;
using Acetato.Domain;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Un cambio de tamaño de trazos y textos en curso (HU-28). A cada paso del mouse los
/// lleva del recuadro actual al nuevo y, al terminar, entrega la entrada
/// <see cref="AnnotationsResized"/> que Deshacer revierte de una vez.
/// </summary>
internal sealed class AnnotationResize
{
    private readonly List<Stroke> _strokes;
    private readonly List<UIElement> _elements;
    private readonly List<TextSnapshot> _texts;
    private readonly SelectionBox _start;

    public AnnotationResize(IEnumerable<Stroke> strokes, IEnumerable<UIElement> elements, SelectionBox start)
    {
        _strokes = [.. strokes];
        _elements = [.. elements];
        _texts = [.. _elements.Select(TextSnapshot.Of)];
        _start = start;
        Current = start;
    }

    /// <summary>Recuadro en el que está ahora lo redimensionado.</summary>
    public SelectionBox Current { get; private set; }

    /// <summary>Lleva lo redimensionado a <paramref name="box"/>.</summary>
    public void ResizeTo(SelectionBox box)
    {
        if (box == Current)
        {
            return;
        }

        AnnotationScale.Apply(_strokes, _elements, Current, box);
        Current = box;
    }

    /// <summary>
    /// Termina el cambio: la entrada para el historial, o <c>null</c> si al final el
    /// tamaño es el del principio (un clic en un tirador no ensucia el historial).
    /// </summary>
    public AnnotationsResized? Finish() =>
        Current == _start ? null : new AnnotationsResized(_strokes, _texts, Current, _start);
}
