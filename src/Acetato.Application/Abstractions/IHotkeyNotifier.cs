namespace Acetato.Application.Abstractions;

/// <summary>
/// Aviso de problemas con los atajos al arrancar (HU-21): atajos que otra app
/// ya tiene o un settings.json dañado. Un atajo que deja de funcionar sin aviso
/// parece un fallo de Acetato. La notificación del sistema vive en
/// Infrastructure; al hacer clic se piden los Ajustes.
/// </summary>
public interface IHotkeyNotifier
{
    /// <summary>El usuario hizo clic en el aviso: abrir Ajustes. Puede llegar desde otro hilo.</summary>
    public event EventHandler? SettingsRequested;

    /// <summary>Muestra el aviso si hay atajos no disponibles o se descartaron los ajustes.</summary>
    public void NotifyStartupProblems(int unavailableCount, bool settingsWereReset);
}
