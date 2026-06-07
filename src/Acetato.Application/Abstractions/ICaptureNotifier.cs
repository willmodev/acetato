namespace Acetato.Application.Abstractions;

/// <summary>
/// Puerto de notificación de captura (HU-12). El adaptador muestra un aviso del
/// sistema al guardar y, al activarlo, abre la imagen (como el recorte de
/// Windows). El interop vive en Infrastructure.
/// </summary>
public interface ICaptureNotifier
{
    /// <summary>Notifica que se guardó una captura en <paramref name="filePath"/>.</summary>
    public void NotifyCaptured(string filePath);
}
