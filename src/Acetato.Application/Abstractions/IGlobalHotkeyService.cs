using Acetato.Domain;

namespace Acetato.Application.Abstractions;

/// <summary>
/// Registro de atajos globales del sistema (HU-01/HU-21). La implementación
/// (RegisterHotKey) vive en Infrastructure; aquí solo el contrato. Expone un
/// único evento con la acción solicitada para escalar a N atajos sin cambiar
/// la interfaz. Las combinaciones no son fijas: las decide quien llama a
/// <see cref="Apply"/>.
/// </summary>
public interface IGlobalHotkeyService
{
    /// <summary>Se dispara cuando el usuario pulsa un atajo global registrado.</summary>
    public event EventHandler<HotkeyActionEventArgs> ActionRequested;

    /// <summary>Arranca el hilo de atajos, sin registrar combinaciones. Idempotente.</summary>
    public void Register();

    /// <summary>Libera los atajos registrados y detiene el hilo.</summary>
    public void Unregister();

    /// <summary>
    /// Reemplaza el conjunto registrado por <paramref name="assignments"/>.
    /// Devuelve las acciones que no se pudieron registrar (p. ej. otra app ya
    /// usa la combinación).
    /// </summary>
    public IReadOnlyList<HotkeyAction> Apply(IReadOnlyList<HotkeyAssignment> assignments);

    /// <summary>Prueba si la combinación está libre en el sistema (registra y suelta).</summary>
    public bool IsAvailable(KeyChord chord);
}
