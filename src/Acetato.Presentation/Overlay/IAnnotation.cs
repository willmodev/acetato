namespace Acetato.Presentation.Overlay;

/// <summary>
/// Una anotación deshacible de una pane (HU-13): un trazo o un texto. Abstrae el
/// "cómo se quita" para que el historial unificado deshaga "lo último, sea lo que
/// sea" sin conocer el tipo concreto.
/// </summary>
internal interface IAnnotation
{
    /// <summary>Quita la anotación de su lienzo (trazo → Strokes; texto → Children).</summary>
    public void Remove();
}
