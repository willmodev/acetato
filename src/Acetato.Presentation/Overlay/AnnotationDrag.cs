using System.Windows;
using System.Windows.Ink;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Un arrastre de trazos y textos en curso (SPEC 11). Los mueve a cada paso del
/// mouse y, al terminar, entrega la entrada <see cref="AnnotationsMoved"/> con el
/// desplazamiento total. Lo usan la herramienta Mover y el atajo de mantener Ctrl,
/// para no duplicar el código del arrastre.
/// </summary>
internal sealed class AnnotationDrag
{
    private readonly List<Stroke> _strokes;
    private readonly List<UIElement> _elements;
    private Vector _total;

    public AnnotationDrag(IEnumerable<Stroke> strokes, IEnumerable<UIElement> elements)
    {
        _strokes = [.. strokes];
        _elements = [.. elements];
    }

    /// <summary>Mueve lo arrastrado por <paramref name="delta"/> y lo acumula.</summary>
    public void MoveBy(Vector delta)
    {
        if (delta == default)
        {
            return;
        }

        AnnotationOffset.Apply(_strokes, _elements, delta);
        _total += delta;
    }

    /// <summary>
    /// Termina el arrastre: la entrada para el historial, o <c>null</c> si al final
    /// nada cambió de sitio (un clic sin arrastrar no ensucia el historial).
    /// </summary>
    public AnnotationsMoved? Finish() =>
        _total == default ? null : new AnnotationsMoved(_strokes, _elements, _total);
}
