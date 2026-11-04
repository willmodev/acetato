using Acetato.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Acetato.Infrastructure.Settings;

/// <summary>
/// Guarda las opciones de dibujo en la sección <c>drawing</c> de
/// %AppData%\Acetato\settings.json (HU-27), sin tocar los atajos. Un archivo ilegible
/// vale <see cref="DrawingPreferences.Defaults"/> y no se aparta ni se avisa: de eso se
/// encarga el almacén de atajos, para no avisar dos veces del mismo archivo. Nunca lanza.
/// </summary>
public sealed class JsonDrawingPreferencesStore : IDrawingPreferencesStore
{
    private readonly SettingsFile _file;

    /// <summary>Usa la carpeta de la app en %AppData%.</summary>
    public JsonDrawingPreferencesStore(ILogger<JsonDrawingPreferencesStore> logger)
        : this(SettingsFile.DefaultDirectory, logger)
    {
    }

    /// <summary>Usa <paramref name="directory"/> sin registro (tests).</summary>
    public JsonDrawingPreferencesStore(string directory)
        : this(directory, NullLogger<JsonDrawingPreferencesStore>.Instance)
    {
    }

    /// <summary>Usa <paramref name="directory"/> (los tests pasan una carpeta temporal).</summary>
    public JsonDrawingPreferencesStore(string directory, ILogger<JsonDrawingPreferencesStore> logger) =>
        _file = new SettingsFile(directory, logger);

    public DrawingPreferences Load() =>
        _file.ReadRoot() is { } root ? DrawingPreferencesJson.Read(root) : DrawingPreferences.Defaults;

    public void Save(DrawingPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        _file.WriteSection(DrawingPreferencesJson.SectionKey, DrawingPreferencesJson.ToSection(preferences));
    }
}
