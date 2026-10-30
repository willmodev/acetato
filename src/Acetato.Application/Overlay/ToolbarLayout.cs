using Acetato.Application.Abstractions;

namespace Acetato.Application.Overlay;

/// <summary>
/// Implementación en memoria de <see cref="IToolbarLayout"/> (HU-25). Una sola
/// instancia por app: así la barra sigue plegada al ocultar y volver a mostrar
/// la capa, y vuelve desplegada al reiniciar.
/// </summary>
public sealed class ToolbarLayout : IToolbarLayout
{
    public bool IsCollapsed { get; private set; }

    public event EventHandler? Changed;

    public void ToggleCollapsed()
    {
        IsCollapsed = !IsCollapsed;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
