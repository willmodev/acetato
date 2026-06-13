using Acetato.Domain;

namespace Acetato.Application.Abstractions;

/// <summary>
/// Lienzo de un solo monitor (HU-09): una pane del overlay. Permite acciones
/// dirigidas por monitor (limpiar, deshacer) sin que la capa de aplicación
/// conozca WPF. El adaptador concreto vive en Presentation.
/// </summary>
public interface IOverlayCanvas
{
    /// <summary>Monitor que cubre este lienzo (píxeles físicos).</summary>
    public MonitorInfo Monitor { get; }

    /// <summary>
    /// Manejador nativo (HWND) de la ventana de esta pane. Permite que la barra
    /// flotante se haga ventana "owned" de la pane de su monitor y quede siempre
    /// por encima del lienzo sin pelear el orden Z (0 si aún no existe).
    /// </summary>
    public nint Handle { get; }

    /// <summary>Borra todos los trazos de este lienzo (HU-04).</summary>
    public void Clear();

    /// <summary>Deshace el último trazo de este lienzo (HU-07).</summary>
    public void Undo();
}
