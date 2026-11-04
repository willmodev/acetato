using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Acetato.Infrastructure.Settings;

/// <summary>
/// Guarda los atajos en la sección <c>hotkeys</c> de %AppData%\Acetato\settings.json
/// (HU-21) a través de <see cref="SettingsFile"/>, que conserva las demás secciones. Un
/// archivo ilegible se renombra a settings.corrupt.json para no perderlo y la app arranca
/// con los predeterminados; este almacén es el único que lo aparta, avisa y lo anota en el
/// registro (HU-32). Nunca lanza: los ajustes no deben impedir que Acetato arranque ni que
/// un atajo cambie.
/// </summary>
public sealed partial class JsonHotkeySettingsStore : IHotkeySettingsStore
{
    private readonly SettingsFile _file;
    private readonly ILogger _logger;

    /// <summary>Usa la carpeta de la app en %AppData%.</summary>
    public JsonHotkeySettingsStore(ILogger<JsonHotkeySettingsStore> logger)
        : this(SettingsFile.DefaultDirectory, logger)
    {
    }

    /// <summary>Usa <paramref name="directory"/> sin registro (tests).</summary>
    public JsonHotkeySettingsStore(string directory)
        : this(directory, NullLogger<JsonHotkeySettingsStore>.Instance)
    {
    }

    /// <summary>Usa <paramref name="directory"/> (los tests pasan una carpeta temporal).</summary>
    public JsonHotkeySettingsStore(string directory, ILogger<JsonHotkeySettingsStore> logger)
    {
        _logger = logger;
        _file = new SettingsFile(directory, logger);
    }

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

        LogCorrupt();
        _file.MoveAsideCorrupt();
        return new HotkeySettingsLoad(HotkeyMap.Defaults, WasCorrupt: true);
    }

    public void Save(HotkeyMap map)
    {
        ArgumentNullException.ThrowIfNull(map);
        _file.WriteSection(HotkeySettingsJson.SectionKey, HotkeySettingsJson.ToSection(map));
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "settings.json está dañado: se aparta como settings.corrupt.json y se usan los ajustes predeterminados")]
    private partial void LogCorrupt();
}
