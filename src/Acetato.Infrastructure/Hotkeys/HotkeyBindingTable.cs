using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Hotkeys;

/// <summary>
/// Tabla de atajos: una fila por acción. Aislar los datos aquí mantiene el
/// servicio pequeño y permite escalar a N atajos sin tocar su complejidad.
/// Todos comparten Ctrl+Alt (coherente con la convención del proyecto) y no se
/// repiten al mantener la tecla pulsada (ModNoRepeat).
/// </summary>
internal static class HotkeyBindingTable
{
    private const uint Modifiers = NativeMethods.ModControl | NativeMethods.ModAlt | NativeMethods.ModNoRepeat;

    /// <summary>Atajos registrados, en orden de id.</summary>
    public static IReadOnlyList<HotkeyBinding> All { get; } =
    [
        new(1, HotkeyAction.Toggle, Modifiers, NativeMethods.VkD),
        new(2, HotkeyAction.DrawingModeToggle, Modifiers, NativeMethods.VkE),
        new(3, HotkeyAction.Clear, Modifiers, NativeMethods.VkBack),
        new(4, HotkeyAction.ColorRed, Modifiers, NativeMethods.Vk1),
        new(5, HotkeyAction.ColorBlue, Modifiers, NativeMethods.Vk2),
        new(6, HotkeyAction.ColorYellow, Modifiers, NativeMethods.Vk3),
        new(7, HotkeyAction.ColorGreen, Modifiers, NativeMethods.Vk4),
        new(8, HotkeyAction.ColorWhite, Modifiers, NativeMethods.Vk5),
        new(9, HotkeyAction.ColorBlack, Modifiers, NativeMethods.Vk6),
        new(10, HotkeyAction.Undo, Modifiers, NativeMethods.VkZ),
        new(11, HotkeyAction.CycleTool, Modifiers, NativeMethods.VkSpace),
        new(12, HotkeyAction.Capture, Modifiers, NativeMethods.VkS),
        new(13, HotkeyAction.SelectTextTool, Modifiers, NativeMethods.VkT),
    ];

    private static readonly Dictionary<int, HotkeyBinding> ById = All.ToDictionary(binding => binding.Id);

    /// <summary>Busca el binding por id; <c>null</c> si no existe.</summary>
    public static HotkeyBinding? FindById(int id) =>
        ById.TryGetValue(id, out var binding) ? binding : null;
}
