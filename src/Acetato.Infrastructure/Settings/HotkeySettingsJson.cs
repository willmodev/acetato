using System.Text.Json;
using System.Text.Json.Nodes;
using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;

namespace Acetato.Infrastructure.Settings;

/// <summary>
/// Sección de atajos de settings.json (HU-21): <c>{ "version": 1, "hotkeys": { "Undo": "Alt+Z",
/// "Capture": null } }</c>. Clave ausente = predeterminado y <c>null</c> = sin atajo.
/// Una acción desconocida o una combinación inválida se ignora (vale el
/// predeterminado); solo un JSON ilegible se considera dañado.
/// </summary>
internal static class HotkeySettingsJson
{
    /// <summary>Nombre de la sección de los atajos en settings.json.</summary>
    public const string SectionKey = "hotkeys";

    // Nombres de acción aceptados. Un diccionario evita que Enum.TryParse
    // acepte números ("3") como acciones.
    private static readonly Dictionary<string, HotkeyAction> ActionsByName =
        Enum.GetValues<HotkeyAction>().ToDictionary(action => action.ToString(), StringComparer.Ordinal);

    /// <summary>Sección <c>hotkeys</c> con las diferencias del mapa, en orden estable.</summary>
    public static JsonObject ToSection(HotkeyMap map)
    {
        var section = new JsonObject();
        foreach (var (action, chord) in map.Overrides.OrderBy(entry => entry.Key))
        {
            section[action.ToString()] = chord?.ToString();
        }

        return section;
    }

    /// <summary>Lee el JSON; <c>false</c> si es ilegible o no tiene la forma esperada.</summary>
    public static bool TryDeserialize(string json, out HotkeyMap map)
    {
        map = HotkeyMap.Defaults;
        try
        {
            using var document = JsonDocument.Parse(json);
            return TryReadRoot(document.RootElement, ref map);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryReadRoot(JsonElement root, ref HotkeyMap map)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!root.TryGetProperty(SectionKey, out var hotkeys))
        {
            return true;
        }

        if (hotkeys.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var entry in hotkeys.EnumerateObject())
        {
            map = Apply(map, entry);
        }

        return true;
    }

    private static HotkeyMap Apply(HotkeyMap map, JsonProperty entry)
    {
        if (!ActionsByName.TryGetValue(entry.Name, out var action))
        {
            return map;
        }

        return entry.Value.ValueKind switch
        {
            JsonValueKind.Null => map.With(action, null),
            JsonValueKind.String when TryReadChord(entry.Value.GetString(), out var chord) => map.With(action, chord),
            _ => map,
        };
    }

    private static bool TryReadChord(string? text, out KeyChord chord) =>
        KeyChord.TryParse(text, out chord) && KeyChordRules.Validate(chord) is null;
}
