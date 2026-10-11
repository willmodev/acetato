using System.Windows.Input;
using Acetato.Domain;

namespace Acetato.Presentation.Settings;

/// <summary>
/// Traduce las teclas de WPF a las del Domain (HU-21). Solo las de la lista
/// blanca de <see cref="HotkeyKey"/>: una tecla de símbolo (<c>ñ</c>, <c>+</c>…)
/// devuelve <c>null</c> y la UI la rechaza como «Tecla no admitida».
/// </summary>
internal static class WpfKeyMap
{
    private static readonly Dictionary<Key, HotkeyKey> Keys = BuildKeys();

    private static readonly HashSet<Key> ModifierKeysAlone =
    [
        Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt,
        Key.LeftShift, Key.RightShift, Key.LWin, Key.RWin,
    ];

    /// <summary>Tecla del Domain; <c>null</c> si no está admitida.</summary>
    public static HotkeyKey? ToHotkeyKey(Key key) => Keys.TryGetValue(key, out var hotkey) ? hotkey : null;

    /// <summary>Modificadores del Domain a partir de los de WPF.</summary>
    public static HotkeyModifiers ToModifiers(ModifierKeys modifiers)
    {
        var result = HotkeyModifiers.None;
        result |= modifiers.HasFlag(ModifierKeys.Control) ? HotkeyModifiers.Ctrl : HotkeyModifiers.None;
        result |= modifiers.HasFlag(ModifierKeys.Alt) ? HotkeyModifiers.Alt : HotkeyModifiers.None;
        result |= modifiers.HasFlag(ModifierKeys.Shift) ? HotkeyModifiers.Shift : HotkeyModifiers.None;
        result |= modifiers.HasFlag(ModifierKeys.Windows) ? HotkeyModifiers.Win : HotkeyModifiers.None;
        return result;
    }

    /// <summary>Indica si la tecla es un modificador suelto (no termina la captura).</summary>
    public static bool IsModifier(Key key) => ModifierKeysAlone.Contains(key);

    private static Dictionary<Key, HotkeyKey> BuildKeys()
    {
        var keys = new Dictionary<Key, HotkeyKey>
        {
            [Key.Space] = HotkeyKey.Space,
            [Key.Back] = HotkeyKey.Back,
            [Key.Delete] = HotkeyKey.Delete,
            [Key.Insert] = HotkeyKey.Insert,
            [Key.Home] = HotkeyKey.Home,
            [Key.End] = HotkeyKey.End,
            [Key.PageUp] = HotkeyKey.PageUp,
            [Key.PageDown] = HotkeyKey.PageDown,
            [Key.Left] = HotkeyKey.Left,
            [Key.Up] = HotkeyKey.Up,
            [Key.Right] = HotkeyKey.Right,
            [Key.Down] = HotkeyKey.Down,
        };
        AddRange(keys, Key.A, HotkeyKey.A, HotkeyKey.Z);
        AddRange(keys, Key.D0, HotkeyKey.D0, HotkeyKey.D9);
        AddRange(keys, Key.F1, HotkeyKey.F1, HotkeyKey.F12);
        AddRange(keys, Key.NumPad0, HotkeyKey.NumPad0, HotkeyKey.NumPad9);
        return keys;
    }

    // Series contiguas tanto en Key (WPF) como en HotkeyKey.
    private static void AddRange(Dictionary<Key, HotkeyKey> keys, Key firstKey, HotkeyKey first, HotkeyKey last)
    {
        for (var hotkey = first; hotkey <= last; hotkey++)
        {
            keys[firstKey + (hotkey - first)] = hotkey;
        }
    }
}
