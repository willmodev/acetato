using Acetato.Domain;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Hotkeys;

/// <summary>
/// Traduce las combinaciones del Domain a los valores de RegisterHotKey (HU-21):
/// <see cref="HotkeyKey"/> → virtual-key code y <see cref="HotkeyModifiers"/> →
/// MOD_*. Todos los atajos llevan MOD_NOREPEAT (no se repiten al mantener).
/// </summary>
internal static class HotkeyKeyMap
{
    private static readonly Dictionary<HotkeyKey, uint> VirtualKeys = BuildVirtualKeys();

    /// <summary>Virtual-key code de la tecla.</summary>
    public static uint ToVirtualKey(HotkeyKey key) => VirtualKeys[key];

    /// <summary>Banderas MOD_* de los modificadores, con MOD_NOREPEAT.</summary>
    public static uint ToModifiers(HotkeyModifiers modifiers)
    {
        uint result = NativeMethods.ModNoRepeat;
        result |= modifiers.HasFlag(HotkeyModifiers.Ctrl) ? NativeMethods.ModControl : 0;
        result |= modifiers.HasFlag(HotkeyModifiers.Alt) ? NativeMethods.ModAlt : 0;
        result |= modifiers.HasFlag(HotkeyModifiers.Shift) ? NativeMethods.ModShift : 0;
        result |= modifiers.HasFlag(HotkeyModifiers.Win) ? NativeMethods.ModWin : 0;
        return result;
    }

    private static Dictionary<HotkeyKey, uint> BuildVirtualKeys()
    {
        var keys = new Dictionary<HotkeyKey, uint>
        {
            [HotkeyKey.Space] = NativeMethods.VkSpace,
            [HotkeyKey.Back] = NativeMethods.VkBack,
            [HotkeyKey.Delete] = NativeMethods.VkDelete,
            [HotkeyKey.Insert] = NativeMethods.VkInsert,
            [HotkeyKey.Home] = NativeMethods.VkHome,
            [HotkeyKey.End] = NativeMethods.VkEnd,
            [HotkeyKey.PageUp] = NativeMethods.VkPrior,
            [HotkeyKey.PageDown] = NativeMethods.VkNext,
            [HotkeyKey.Left] = NativeMethods.VkLeft,
            [HotkeyKey.Up] = NativeMethods.VkUp,
            [HotkeyKey.Right] = NativeMethods.VkRight,
            [HotkeyKey.Down] = NativeMethods.VkDown,
        };
        AddRange(keys, HotkeyKey.A, HotkeyKey.Z, NativeMethods.VkA);
        AddRange(keys, HotkeyKey.D0, HotkeyKey.D9, NativeMethods.Vk0);
        AddRange(keys, HotkeyKey.F1, HotkeyKey.F12, NativeMethods.VkF1);
        AddRange(keys, HotkeyKey.NumPad0, HotkeyKey.NumPad9, NativeMethods.VkNumPad0);
        return keys;
    }

    // Series contiguas tanto en el enum como en los virtual-key codes.
    private static void AddRange(Dictionary<HotkeyKey, uint> keys, HotkeyKey first, HotkeyKey last, uint firstCode)
    {
        for (var key = first; key <= last; key++)
        {
            keys[key] = firstCode + (uint)(key - first);
        }
    }
}
