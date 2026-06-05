namespace Acetato.Application.Abstractions;

/// <summary>
/// Registro de atajos globales del sistema (HU-01). La implementación
/// (RegisterHotKey) vive en Infrastructure; aquí solo el contrato. Expone un
/// único evento con la acción solicitada para escalar a N atajos sin cambiar
/// la interfaz.
/// </summary>
public interface IGlobalHotkeyService
{
    /// <summary>Se dispara cuando el usuario pulsa un atajo global registrado.</summary>
    public event EventHandler<HotkeyActionEventArgs> ActionRequested;

    /// <summary>Registra los atajos globales. Idempotente.</summary>
    public void Register();

    /// <summary>Libera los atajos registrados.</summary>
    public void Unregister();
}
