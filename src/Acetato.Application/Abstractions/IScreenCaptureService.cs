namespace Acetato.Application.Abstractions;

/// <summary>
/// Puerto de captura de pantalla (HU-12). El adaptador de plataforma copia el
/// monitor primario —ya compuesto con los trazos del overlay y sin la barra
/// flotante— y lo guarda como PNG en la ruta indicada. El interop vive en
/// Infrastructure.
/// </summary>
public interface IScreenCaptureService
{
    /// <summary>Guarda la pantalla del monitor primario como PNG en <paramref name="filePath"/>.</summary>
    public void CaptureToPng(string filePath);
}
