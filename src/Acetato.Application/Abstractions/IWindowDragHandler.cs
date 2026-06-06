namespace Acetato.Application.Abstractions;

/// <summary>
/// Inicia el arrastre nativo de una ventana sin bordes (HU-10): delega el
/// movimiento al gestor de ventanas del sistema, sin activar la ventana. La
/// implementación (P/Invoke) vive en Infrastructure.
/// </summary>
public interface IWindowDragHandler
{
    /// <summary>Comienza a arrastrar la ventana del manejador dado.</summary>
    public void BeginDrag(nint windowHandle);
}
