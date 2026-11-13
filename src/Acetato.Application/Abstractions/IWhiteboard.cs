using Acetato.Domain;

namespace Acetato.Application.Abstractions;

/// <summary>
/// La pizarra (HU-35): una hoja blanca o negra que tapa un monitor para explicar algo
/// aparte. Un solo botón o atajo la recorre: apagada → blanca → negra → apagada. Solo
/// puede haber una prendida a la vez, en el monitor donde estaba el cursor al prenderla.
/// </summary>
public interface IWhiteboard
{
    /// <summary>Estado actual de la pizarra.</summary>
    public WhiteboardMode Mode { get; }

    /// <summary>Se dispara cuando cambia <see cref="Mode"/>.</summary>
    public event EventHandler? Changed;

    /// <summary>
    /// Pasa al estado siguiente. Si estaba apagada, la prende en el monitor bajo el cursor;
    /// si ya estaba prendida, cambia el color en ese mismo monitor.
    /// </summary>
    public void Cycle();

    /// <summary>
    /// Avisa que el próximo paso a modo puntero lo provoca el recorte de pantalla (HU-23):
    /// la pizarra se queda prendida para que el recorte la capture. Cualquier otra salida del
    /// modo dibujo, y ocultar la capa, la apagan.
    /// </summary>
    public void KeepThroughNextPointerMode();
}
