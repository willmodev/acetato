using Acetato.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Acetato.Infrastructure.Settings;

/// <summary>
/// Guarda la última sesión en la sección <c>session</c> de %AppData%\Acetato\settings.json
/// (HU-29), sin tocar los atajos ni las opciones de dibujo. Un archivo ilegible vale
/// <see cref="LastSession.Defaults"/> y no se aparta ni se avisa: de eso se encarga el
/// almacén de atajos, para no avisar dos veces del mismo archivo. Nunca lanza.
/// </summary>
public sealed class JsonLastSessionStore : ILastSessionStore
{
    private readonly SettingsFile _file;

    /// <summary>Usa la carpeta de la app en %AppData%.</summary>
    public JsonLastSessionStore(ILogger<JsonLastSessionStore> logger)
        : this(SettingsFile.DefaultDirectory, logger)
    {
    }

    /// <summary>Usa <paramref name="directory"/> sin registro (tests).</summary>
    public JsonLastSessionStore(string directory)
        : this(directory, NullLogger<JsonLastSessionStore>.Instance)
    {
    }

    /// <summary>Usa <paramref name="directory"/> (los tests pasan una carpeta temporal).</summary>
    public JsonLastSessionStore(string directory, ILogger<JsonLastSessionStore> logger) =>
        _file = new SettingsFile(directory, logger);

    public LastSession Load() =>
        _file.ReadRoot() is { } root ? LastSessionJson.Read(root) : LastSession.Defaults;

    public void Save(LastSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        _file.WriteSection(LastSessionJson.SectionKey, LastSessionJson.ToSection(session));
    }
}
