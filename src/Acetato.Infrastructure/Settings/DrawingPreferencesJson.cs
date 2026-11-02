using System.Text.Json.Nodes;
using Acetato.Application.Abstractions;

namespace Acetato.Infrastructure.Settings;

/// <summary>
/// Sección de opciones de dibujo de settings.json:
/// <c>"drawing": { "inkPairs": true, "freeResize": false }</c> (HU-27, HU-28). Una sección
/// ausente, una clave ausente o un valor que no es booleano valen lo predeterminado
/// (apagado), clave por clave: un archivo de antes de HU-28 se lee igual.
/// </summary>
internal static class DrawingPreferencesJson
{
    /// <summary>Nombre de la sección de las opciones de dibujo en settings.json.</summary>
    public const string SectionKey = "drawing";

    private const string InkPairsKey = "inkPairs";
    private const string FreeResizeKey = "freeResize";

    /// <summary>Sección <c>drawing</c> con las opciones dadas.</summary>
    public static JsonObject ToSection(DrawingPreferences preferences) =>
        new() { [InkPairsKey] = preferences.InkPairs, [FreeResizeKey] = preferences.FreeResize };

    /// <summary>Opciones leídas del objeto raíz del archivo.</summary>
    public static DrawingPreferences Read(JsonObject root)
    {
        if (root[SectionKey] is not JsonObject section)
        {
            return DrawingPreferences.Defaults;
        }

        return new DrawingPreferences(
            InkPairs: Flag(section, InkPairsKey, DrawingPreferences.Defaults.InkPairs),
            FreeResize: Flag(section, FreeResizeKey, DrawingPreferences.Defaults.FreeResize));
    }

    // Booleano de la clave; si falta o no es booleano, el predeterminado.
    private static bool Flag(JsonObject section, string key, bool fallback) =>
        section[key] is JsonValue value && value.TryGetValue(out bool flag) ? flag : fallback;
}
