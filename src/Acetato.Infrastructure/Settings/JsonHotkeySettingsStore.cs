using System.IO;
using System.Text;
using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;

namespace Acetato.Infrastructure.Settings;

/// <summary>
/// Guarda los atajos en %AppData%\Acetato\settings.json (HU-21). La escritura
/// es atómica (archivo temporal y luego reemplazo) para no dejar un JSON a
/// medias. Un archivo ilegible se renombra a settings.corrupt.json para no
/// perderlo y la app arranca con los predeterminados. Nunca lanza: los ajustes
/// no deben impedir que Acetato arranque ni que un atajo cambie.
/// </summary>
public sealed class JsonHotkeySettingsStore : IHotkeySettingsStore
{
    private const string FileName = "settings.json";
    private const string CorruptFileName = "settings.corrupt.json";
    private const string TempSuffix = ".tmp";

    // UTF-8 sin BOM: el archivo se puede editar a mano con cualquier editor.
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private readonly string _directory;

    /// <summary>Usa la carpeta de la app en %AppData%.</summary>
    public JsonHotkeySettingsStore()
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Acetato"))
    {
    }

    /// <summary>Usa <paramref name="directory"/> (los tests pasan una carpeta temporal).</summary>
    public JsonHotkeySettingsStore(string directory) => _directory = directory;

    private string FilePath => Path.Combine(_directory, FileName);

    public HotkeySettingsLoad Load()
    {
        string? json = TryReadAllText(FilePath);
        if (json is null)
        {
            return new HotkeySettingsLoad(HotkeyMap.Defaults, WasCorrupt: false);
        }

        if (HotkeySettingsJson.TryDeserialize(json, out var map))
        {
            return new HotkeySettingsLoad(map, WasCorrupt: false);
        }

        MoveAsideCorrupt();
        return new HotkeySettingsLoad(HotkeyMap.Defaults, WasCorrupt: true);
    }

    public void Save(HotkeyMap map)
    {
        ArgumentNullException.ThrowIfNull(map);
        string tempPath = FilePath + TempSuffix;
        try
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(tempPath, HotkeySettingsJson.Serialize(map), Utf8NoBom);
            File.Move(tempPath, FilePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Best-effort: el cambio ya está aplicado en memoria; solo no se recordará.
        }
    }

    // null = no hay archivo o no se pudo abrir (se tratan igual: predeterminados).
    private static string? TryReadAllText(string path)
    {
        try
        {
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private void MoveAsideCorrupt()
    {
        try
        {
            File.Move(FilePath, Path.Combine(_directory, CorruptFileName), overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Si no se puede renombrar, el siguiente guardado lo sobrescribe.
        }
    }
}
