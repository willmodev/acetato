using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Ink;

namespace Acetato.Presentation.Behaviors;

/// <summary>Trazos y textos que la mano agarra en un arrastre (SPEC 11).</summary>
[StructLayout(LayoutKind.Auto)]
internal readonly record struct GrabTargets(IReadOnlyList<Stroke> Strokes, IReadOnlyList<UIElement> Elements)
{
    /// <summary>No hay nada que agarrar.</summary>
    public bool IsEmpty => Strokes.Count == 0 && Elements.Count == 0;
}
