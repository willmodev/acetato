namespace Acetato.Application.Abstractions;

/// <summary>
/// Registro de atajos globales del sistema (HU-01). La implementación
/// (RegisterHotKey) vive en Infrastructure; aquí solo el contrato.
/// </summary>
public interface IGlobalHotkeyService
{
    /// <summary>Se dispara cuando el usuario pulsa el atajo de activación.</summary>
    public event EventHandler ToggleRequested;

    /// <summary>Se dispara al pulsar el atajo de cambio de modo (dibujo/normal).</summary>
    public event EventHandler DrawingModeToggleRequested;

    /// <summary>Se dispara al pulsar el atajo de limpiar todos los trazos.</summary>
    public event EventHandler ClearRequested;

    /// <summary>Registra los atajos globales. Idempotente.</summary>
    public void Register();

    /// <summary>Libera los atajos registrados.</summary>
    public void Unregister();
}
