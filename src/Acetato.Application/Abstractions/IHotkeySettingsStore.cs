using Acetato.Application.Hotkeys;

namespace Acetato.Application.Abstractions;

/// <summary>
/// Persistencia de los atajos entre sesiones (HU-21). La implementación (JSON en
/// %AppData%\Acetato) vive en Infrastructure. Nunca lanza: un archivo ausente o
/// dañado se resuelve con los predeterminados.
/// </summary>
public interface IHotkeySettingsStore
{
    /// <summary>Lee los atajos guardados.</summary>
    public HotkeySettingsLoad Load();

    /// <summary>Guarda el mapa (solo lo que difiere del predeterminado).</summary>
    public void Save(HotkeyMap map);
}
