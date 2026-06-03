namespace Acetato.Application.Abstractions;

/// <summary>
/// Registro de atajos globales del sistema (HU-01). La implementación
/// (RegisterHotKey) vive en Infrastructure; aquí solo el contrato.
/// </summary>
public interface IGlobalHotkeyService
{
    /// <summary>Se dispara cuando el usuario pulsa el atajo de activación.</summary>
    event EventHandler ToggleRequested;

    /// <summary>Registra los atajos globales. Idempotente.</summary>
    void Register();

    /// <summary>Libera los atajos registrados.</summary>
    void Unregister();
}
