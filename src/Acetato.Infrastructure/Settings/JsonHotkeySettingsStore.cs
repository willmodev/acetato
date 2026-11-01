using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;

namespace Acetato.Infrastructure.Settings;

/// <summary>
/// Guarda los atajos en la sección <c>hotkeys</c> de %AppData%\Acetato\settings.json
/// (HU-21) a través de <see cref="SettingsFile"/>, que conserva las demás secciones. Un
/// archivo ilegible se renombra a settings.corrupt.json para no perderlo y la app arranca
/// con los predeterminados; este almacén es el único que lo aparta y avisa. Nunca lanza:
/// los ajustes no deben impedir que Acetato arranque ni que un atajo cambie.
/// </summary>
public sealed class JsonHotkeySettingsStore : IHotkeySettingsStore
{
    private readonly SettingsFile _file;

    /// <summary>Usa la carpeta de la app en %AppData%.</summary>
    public JsonHotkeySettingsStore()
        : this(SettingsFile.DefaultDirectory)
    {
    }

    /// <summary>Usa <paramref name="directory"/> (los tests pasan una carpeta temporal).</summary>
    public JsonHotkeySettingsStore(string directory) => _file = new SettingsFile(directory);

    public HotkeySettingsLoad Load()
    {
        string? json = _file.ReadText();
        if (json is null)
        {
            return new HotkeySettingsLoad(HotkeyMap.Defaults, WasCorrupt: false);
        }

        if (HotkeySettingsJson.TryDeserialize(json, out var map))
        {
            return new HotkeySettingsLoad(map, WasCorrupt: false);
        }

        _file.MoveAsideCorrupt();
        return new HotkeySettingsLoad(HotkeyMap.Defaults, WasCorrupt: true);
    }

    public void Save(HotkeyMap map)
    {
        ArgumentNullException.ThrowIfNull(map);
        _file.WriteSection(HotkeySettingsJson.SectionKey, HotkeySettingsJson.ToSection(map));
    }
}
