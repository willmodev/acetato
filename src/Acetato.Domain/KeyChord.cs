using System.Runtime.InteropServices;

namespace Acetato.Domain;

/// <summary>
/// Combinación de teclas de un atajo (HU-21): modificadores + tecla final.
/// <see cref="ToString"/> y <see cref="TryParse"/> usan un formato invariante
/// (<c>"Ctrl+Shift+Z"</c>) pensado para settings.json; el texto en español para
/// la UI vive en Presentation. No valida las reglas de uso: eso es
/// <see cref="KeyChordRules"/>.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct KeyChord(HotkeyModifiers Modifiers, HotkeyKey Key)
{
    private const char Separator = '+';

    // Orden canónico de los modificadores al escribir.
    private static readonly (HotkeyModifiers Flag, string Name)[] ModifierNames =
    [
        (HotkeyModifiers.Ctrl, "Ctrl"),
        (HotkeyModifiers.Alt, "Alt"),
        (HotkeyModifiers.Shift, "Shift"),
        (HotkeyModifiers.Win, "Win"),
    ];

    // Nombres de tecla aceptados (los del enum). Un diccionario evita que
    // Enum.TryParse acepte números ("5") como si fueran teclas.
    private static readonly Dictionary<string, HotkeyKey> KeysByName =
        Enum.GetValues<HotkeyKey>().ToDictionary(key => key.ToString(), StringComparer.OrdinalIgnoreCase);

    /// <summary>Texto invariante, p. ej. <c>"Ctrl+Alt+D"</c> o <c>"F8"</c>.</summary>
    public override string ToString()
    {
        var parts = new List<string>(ModifierNames.Length + 1);
        foreach (var (flag, name) in ModifierNames)
        {
            if (Modifiers.HasFlag(flag))
            {
                parts.Add(name);
            }
        }

        parts.Add(Key.ToString());
        return string.Join(Separator, parts);
    }

    /// <summary>
    /// Lee el formato de <see cref="ToString"/> (sin distinguir mayúsculas).
    /// Falla con texto vacío, teclas desconocidas o modificadores repetidos.
    /// </summary>
    public static bool TryParse(string? text, out KeyChord chord)
    {
        chord = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string[] tokens = text.Split(Separator, StringSplitOptions.TrimEntries);
        if (!KeysByName.TryGetValue(tokens[^1], out var key)
            || !TryParseModifiers(tokens.AsSpan(0, tokens.Length - 1), out var modifiers))
        {
            return false;
        }

        chord = new KeyChord(modifiers, key);
        return true;
    }

    private static bool TryParseModifiers(ReadOnlySpan<string> tokens, out HotkeyModifiers modifiers)
    {
        modifiers = HotkeyModifiers.None;
        foreach (string token in tokens)
        {
            var flag = FindModifier(token);
            if (flag == HotkeyModifiers.None || modifiers.HasFlag(flag))
            {
                return false;
            }

            modifiers |= flag;
        }

        return true;
    }

    private static HotkeyModifiers FindModifier(string token)
    {
        foreach (var (flag, name) in ModifierNames)
        {
            if (string.Equals(token, name, StringComparison.OrdinalIgnoreCase))
            {
                return flag;
            }
        }

        return HotkeyModifiers.None;
    }
}
