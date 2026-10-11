namespace Acetato.Domain;

/// <summary>
/// Reglas de una combinación de atajo (HU-21). Un atajo global sin modificador
/// le roba esa tecla a todas las apps, por eso se exige Ctrl, Alt o Win, salvo
/// F1–F12 solas. Mayús no basta: en modo dibujo impediría escribir mayúsculas
/// en una anotación de Texto. Alt+F4 y Alt+Espacio son de Windows aunque el
/// registro no lo impida.
/// </summary>
public static class KeyChordRules
{
    private const HotkeyModifiers Required = HotkeyModifiers.Ctrl | HotkeyModifiers.Alt | HotkeyModifiers.Win;

    /// <summary>Valida la combinación; <c>null</c> si es aceptable.</summary>
    public static KeyChordError? Validate(KeyChord chord)
    {
        if ((chord.Modifiers & Required) == HotkeyModifiers.None)
        {
            return ValidateWithoutRequired(chord);
        }

        return IsReserved(chord) ? KeyChordError.SystemShortcut : null;
    }

    // Sin Ctrl/Alt/Win: solo una F1–F12 sola es válida. Con Mayús el motivo es
    // que Mayús no basta (también con una F), sin ella falta el modificador.
    private static KeyChordError? ValidateWithoutRequired(KeyChord chord)
    {
        if (chord.Modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            return KeyChordError.ShiftOnly;
        }

        return IsFunctionKey(chord.Key) ? null : KeyChordError.MissingModifier;
    }

    private static bool IsFunctionKey(HotkeyKey key) => key is >= HotkeyKey.F1 and <= HotkeyKey.F12;

    private static bool IsReserved(KeyChord chord) =>
        chord.Modifiers == HotkeyModifiers.Alt && chord.Key is HotkeyKey.F4 or HotkeyKey.Space;
}
