namespace Acetato.Application.Abstractions;

/// <summary>
/// Puerto que resuelve la carpeta donde se guardan las capturas (HU-12). El
/// adaptador la sitúa en Imágenes\Acetato; aislarlo mantiene el caso de uso
/// agnóstico del sistema de archivos del SO y testeable.
/// </summary>
public interface ICaptureDirectoryProvider
{
    /// <summary>Devuelve la carpeta de destino de las capturas (sin garantizar que exista).</summary>
    public string GetCaptureDirectory();
}
