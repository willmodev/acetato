namespace Acetato.Presentation.Overlay;

/// <summary>
/// Una acción deshacible de una pane (HU-13, SPEC 11): dibujar un trazo o un texto,
/// borrar con el Borrador, mover o borrar una selección. Abstrae el «cómo se revierte»
/// para que el historial deshaga «la última acción, sea cual sea» sin conocer su tipo.
/// </summary>
internal interface IHistoryEntry
{
    /// <summary>Revierte la acción sobre su lienzo.</summary>
    public void Undo();
}
