namespace Acetato.Application.Abstractions;

/// <summary>
/// Puerto de captura de pantalla (HU-12). El adaptador de plataforma copia el
/// escritorio virtual completo —todos los monitores (HU-09), ya compuestos con
/// los trazos del overlay y sin la barra flotante— y lo guarda como PNG en la
/// ruta indicada. El interop vive en Infrastructure.
/// </summary>
public interface IScreenCaptureService
{
    /// <summary>Guarda el escritorio virtual completo como PNG en <paramref name="filePath"/>.</summary>
    public void CaptureToPng(string filePath);
}
