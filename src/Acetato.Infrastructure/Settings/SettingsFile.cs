using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace Acetato.Infrastructure.Settings;

/// <summary>
/// El archivo %AppData%\Acetato\settings.json, compartido por los atajos (HU-21) y las
/// opciones de dibujo (HU-27): <c>{ "version": 1, "hotkeys": { … }, "drawing": { … } }</c>.
/// Cada almacén escribe SOLO su sección y conserva las demás, así guardar los atajos no
/// borra las opciones de dibujo ni al revés. La escritura es atómica (archivo temporal y
/// luego reemplazo) y nunca lanza: el cambio ya está aplicado en memoria. Lo que falla
/// se anota como advertencia en el registro (HU-32).
/// </summary>
internal sealed partial class SettingsFile
{
    private const int CurrentVersion = 1;
    private const string VersionKey = "version";
    private const string FileName = "settings.json";
    private const string CorruptFileName = "settings.corrupt.json";
    private const string TempSuffix = ".tmp";

    // UTF-8 sin BOM: el archivo se puede editar a mano con cualquier editor.
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    private readonly string _directory;
    private readonly ILogger _logger;

    /// <param name="directory">Carpeta del archivo (los tests pasan una temporal).</param>
    /// <param name="logger">Registro del almacén que usa el archivo.</param>
    public SettingsFile(string directory, ILogger logger)
    {
        _directory = directory;
        _logger = logger;
    }

    /// <summary>Carpeta de la app en %AppData%.</summary>
    public static string DefaultDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Acetato");

    private string FilePath => Path.Combine(_directory, FileName);

    /// <summary>Texto del archivo; <c>null</c> si no existe o no se pudo abrir (se tratan igual).</summary>
    public string? ReadText()
    {
        try
        {
            return File.Exists(FilePath) ? File.ReadAllText(FilePath, Encoding.UTF8) : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            LogReadFailed(ex);
            return null;
        }
    }

    /// <summary>Objeto raíz del archivo; <c>null</c> si no existe, no se abre o no es un objeto JSON.</summary>
    public JsonObject? ReadRoot()
    {
        string? json = ReadText();
        if (json is null)
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(json) as JsonObject;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Reemplaza la sección <paramref name="key"/> y conserva el resto del archivo.</summary>
    public void WriteSection(string key, JsonNode section)
    {
        var root = ReadRoot() ?? [];
        root[VersionKey] = CurrentVersion;
        root[key] = section;
        string tempPath = FilePath + TempSuffix;
        try
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(tempPath, root.ToJsonString(Indented), Utf8NoBom);
            File.Move(tempPath, FilePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Best-effort: el cambio ya está aplicado en memoria; solo no se recordará.
            LogWriteFailed(key, ex);
        }
    }

    /// <summary>Aparta un archivo ilegible a settings.corrupt.json para no perderlo.</summary>
    public void MoveAsideCorrupt()
    {
        try
        {
            File.Move(FilePath, Path.Combine(_directory, CorruptFileName), overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Si no se puede renombrar, el siguiente guardado lo sobrescribe.
            LogMoveAsideFailed(ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "No se pudo leer settings.json: se usan los valores predeterminados")]
    private partial void LogReadFailed(Exception exception);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "No se pudo guardar la sección {Section} de settings.json: el cambio no se recordará")]
    private partial void LogWriteFailed(string section, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No se pudo apartar settings.json dañado: el siguiente guardado lo reemplaza")]
    private partial void LogMoveAsideFailed(Exception exception);
}
