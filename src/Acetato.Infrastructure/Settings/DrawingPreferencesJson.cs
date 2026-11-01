using System.Text.Json.Nodes;
using Acetato.Application.Abstractions;

namespace Acetato.Infrastructure.Settings;

/// <summary>
/// Sección de opciones de dibujo de settings.json (HU-27): <c>"drawing": { "inkPairs": true }</c>.
/// Una sección ausente, una clave ausente o un valor que no es booleano valen lo
/// predeterminado (apagado): un archivo de antes de HU-27 se lee igual.
/// </summary>
internal static class DrawingPreferencesJson
{
    /// <summary>Nombre de la sección de las opciones de dibujo en settings.json.</summary>
    public const string SectionKey = "drawing";

    private const string InkPairsKey = "inkPairs";

    /// <summary>Sección <c>drawing</c> con las opciones dadas.</summary>
    public static JsonObject ToSection(DrawingPreferences preferences) =>
        new() { [InkPairsKey] = preferences.InkPairs };

    /// <summary>Opciones leídas del objeto raíz del archivo.</summary>
    public static DrawingPreferences Read(JsonObject root)
    {
        if (root[SectionKey] is not JsonObject section
            || section[InkPairsKey] is not JsonValue value
            || !value.TryGetValue(out bool inkPairs))
        {
            return DrawingPreferences.Defaults;
        }

        return new DrawingPreferences(inkPairs);
    }
}
