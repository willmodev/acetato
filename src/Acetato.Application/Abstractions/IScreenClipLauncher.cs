namespace Acetato.Application.Abstractions;

/// <summary>
/// Abre el recorte de pantalla del sistema (HU-23). La herramienta elige el
/// área, la copia al portapapeles y muestra su propio aviso. Los trazos salen en
/// la imagen y la barra no (queda excluida de las capturas). El adaptador vive
/// en Infrastructure.
/// </summary>
public interface IScreenClipLauncher
{
    /// <summary>Abre la selección de área del recorte de pantalla.</summary>
    public void Launch();
}
