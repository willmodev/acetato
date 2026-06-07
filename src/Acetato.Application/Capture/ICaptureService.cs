namespace Acetato.Application.Capture;

/// <summary>
/// Caso de uso de captura de pantalla anotada (HU-12). Guarda un PNG con la
/// pantalla y los trazos del overlay y devuelve su ruta.
/// </summary>
public interface ICaptureService
{
    /// <summary>Captura la pantalla anotada y la guarda como PNG.</summary>
    public Task<CaptureResult> CaptureAsync();
}
