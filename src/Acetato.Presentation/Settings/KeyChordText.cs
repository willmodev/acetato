using Acetato.Domain;

namespace Acetato.Presentation.Settings;

/// <summary>
/// Texto en español de una combinación (HU-21), p. ej. «Ctrl+Mayús+Z». El
/// formato invariante de settings.json vive en <see cref="KeyChord.ToString"/>;
/// este es solo para la UI.
/// </summary>
internal static class KeyChordText
{
    private const string Separator = "+";

    private static readonly (HotkeyModifiers Flag, string Name)[] ModifierNames =
    [
        (HotkeyModifiers.Ctrl, "Ctrl"),
        (HotkeyModifiers.Alt, "Alt"),
        (HotkeyModifiers.Shift, "Mayús"),
        (HotkeyModifiers.Win, "Win"),
    ];

    private static readonly Dictionary<HotkeyKey, string> KeyNames = BuildKeyNames();

    /// <summary>Texto de la combinación completa.</summary>
    public static string Format(KeyChord chord)
    {
        var parts = ModifierNames
            .Where(modifier => chord.Modifiers.HasFlag(modifier.Flag))
            .Select(modifier => modifier.Name)
            .Append(KeyName(chord.Key));
        return string.Join(Separator, parts);
    }

    /// <summary>Nombre de la tecla final (letras, F1–F12 e Insert tal cual).</summary>
    public static string KeyName(HotkeyKey key) =>
        KeyNames.TryGetValue(key, out string? name) ? name : key.ToString();

    private static Dictionary<HotkeyKey, string> BuildKeyNames()
    {
        var names = new Dictionary<HotkeyKey, string>
        {
            [HotkeyKey.Space] = "Espacio",
            [HotkeyKey.Back] = "Retroceso",
            [HotkeyKey.Delete] = "Supr",
            [HotkeyKey.Home] = "Inicio",
            [HotkeyKey.End] = "Fin",
            [HotkeyKey.PageUp] = "RePág",
            [HotkeyKey.PageDown] = "AvPág",
            [HotkeyKey.Left] = "←",
            [HotkeyKey.Up] = "↑",
            [HotkeyKey.Right] = "→",
            [HotkeyKey.Down] = "↓",
            [HotkeyKey.Escape] = "Esc",
        };
        for (int digit = 0; digit <= 9; digit++)
        {
            names[HotkeyKey.D0 + digit] = digit.ToString(System.Globalization.CultureInfo.InvariantCulture);
            names[HotkeyKey.NumPad0 + digit] = $"Num {digit}";
        }

        return names;
    }
}
