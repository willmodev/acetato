using Acetato.Application.Abstractions;
using Acetato.Domain;

namespace Acetato.Application.Overlay;

/// <summary>
/// Implementación en memoria de <see cref="IToolbarLayout"/> (HU-25, HU-29). Una sola
/// instancia por app: así la barra sigue plegada y en su sitio al ocultar y volver a
/// mostrar la capa. Entre sesiones lo conserva <see cref="Session.SessionMemory"/>.
/// </summary>
public sealed class ToolbarLayout : IToolbarLayout
{
    public bool IsCollapsed { get; private set; }

    public ToolbarPlacement? Placement { get; private set; }

    public event EventHandler? Changed;

    public void ToggleCollapsed()
    {
        IsCollapsed = !IsCollapsed;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Remember(ToolbarPlacement placement) => Placement = placement;
}
