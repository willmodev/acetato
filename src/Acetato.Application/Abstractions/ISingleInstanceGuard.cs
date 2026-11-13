namespace Acetato.Application.Abstractions;

/// <summary>
/// Una sola copia de Acetato por sesión de Windows (HU-31). La primera copia reclama el
/// lugar y escucha; las siguientes le avisan y se cierran. El adaptador vive en
/// Infrastructure.
/// </summary>
public interface ISingleInstanceGuard : IDisposable
{
    /// <summary>
    /// La segunda copia pidió activar el dibujo. Llega en un hilo de fondo, no en el de la
    /// interfaz.
    /// </summary>
    public event EventHandler? ActivationRequested;

    /// <summary>Reclama el lugar; <c>false</c> si ya había otra copia abierta en la sesión.</summary>
    public bool TryClaim();

    /// <summary>Avisa a la copia que ya estaba abierta (la segunda copia, antes de cerrarse).</summary>
    public void NotifyOwner();
}
