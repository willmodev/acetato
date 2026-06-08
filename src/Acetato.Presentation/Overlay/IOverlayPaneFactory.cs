using Acetato.Domain;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Crea una <see cref="OverlayPane"/> (ventana + ViewModel) para un monitor
/// dado (HU-09). Permite al agregador construir una pane por pantalla sin acoplar
/// la creación de ventanas WPF a la lógica de composición.
/// </summary>
public interface IOverlayPaneFactory
{
    /// <summary>Crea una pane que cubrirá el monitor dado.</summary>
    public OverlayPane Create(MonitorInfo monitor);
}
