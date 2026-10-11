using System.IO;
using System.Text;
using System.Text.Json;
using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;

namespace Acetato.Infrastructure.Settings;

/// <summary>
/// Formato de settings.json (HU-21): <c>{ "version": 1, "hotkeys": { "Undo": "Alt+Z",
/// "Capture": null } }</c>. Clave ausente = predeterminado y <c>null</c> = sin atajo.
/// Una acción desconocida o una combinación inválida se ignora (vale el
/// predeterminado); solo un JSON ilegible se considera dañado.
/// </summary>
internal static class HotkeySettingsJson
{
    private const int CurrentVersion = 1;
    private const string VersionKey = "version";
    private const string HotkeysKey = "hotkeys";

    // Nombres de acción aceptados. Un diccionario evita que Enum.TryParse
    // acepte números ("3") como acciones.
    private static readonly Dictionary<string, HotkeyAction> ActionsByName =
        Enum.GetValues<HotkeyAction>().ToDictionary(action => action.ToString(), StringComparer.Ordinal);

    /// <summary>JSON con la versión y las diferencias del mapa, en orden estable.</summary>
    public static string Serialize(HotkeyMap map)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteNumber(VersionKey, CurrentVersion);
            writer.WriteStartObject(HotkeysKey);
            foreach (var (action, chord) in map.Overrides.OrderBy(entry => entry.Key))
            {
                writer.WriteString(action.ToString(), chord?.ToString());
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
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

        if (!root.TryGetProperty(HotkeysKey, out var hotkeys))
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
