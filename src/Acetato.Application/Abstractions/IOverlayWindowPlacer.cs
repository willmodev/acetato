using Acetato.Domain;

namespace Acetato.Application.Abstractions;

/// <summary>
/// Posiciona una ventana del overlay cubriendo un monitor (HU-09) en píxeles
/// físicos. Necesario porque fijar la posición en DIP desde WPF descoloca la
/// ventana en monitores con DPI distinto (la manda al primario). El interop vive
/// en Infrastructure.
/// </summary>
public interface IOverlayWindowPlacer
{
    /// <summary>Maximiza la ventana sobre el monitor dado (coordenadas físicas).</summary>
    public void MaximizeOnMonitor(nint windowHandle, MonitorInfo monitor);
}
