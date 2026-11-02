using Acetato.Application.Abstractions;
using Acetato.Domain;

namespace Acetato.Application.Hotkeys;

/// <summary>
/// Atajos fijos (HU-24): no están en el catálogo, no se guardan ni salen en
/// Ajustes. Esc para el Puntero, que se registra solo en modo dibujo, y Supr para
/// borrar la selección (SPEC 11), solo en modo dibujo y con la herramienta Mover:
/// fuera de eso, ambas teclas llegan a las demás apps (y Supr, al texto en edición).
/// </summary>
public static class FixedHotkeys
{
    /// <summary>Esc → salir del modo dibujo sin ocultar nada.</summary>
    public static HotkeyAssignment PointerEsc { get; } =
        new(HotkeyAction.EnterPointerMode, new KeyChord(HotkeyModifiers.None, HotkeyKey.Escape));

    /// <summary>Supr → borrar lo seleccionado con la herramienta Mover.</summary>
    public static HotkeyAssignment SelectionDelete { get; } =
        new(HotkeyAction.DeleteSelection, new KeyChord(HotkeyModifiers.None, HotkeyKey.Delete));

    /// <summary>Atajos fijos que se registran junto al grupo «Solo al dibujar».</summary>
    public static IReadOnlyList<HotkeyAssignment> Drawing { get; } = [PointerEsc];

    /// <summary>Atajos fijos que se suman a los de dibujo mientras la herramienta es Mover.</summary>
    public static IReadOnlyList<HotkeyAssignment> Selection { get; } = [SelectionDelete];

    /// <summary>Todos los atajos fijos.</summary>
    public static IReadOnlyList<HotkeyAssignment> All { get; } = [.. Drawing, .. Selection];

    /// <summary>Combinación fija de <paramref name="action"/>; <c>null</c> si no es fija.</summary>
    public static KeyChord? ChordOf(HotkeyAction action) =>
        All.FirstOrDefault(assignment => assignment.Action == action)?.Chord;
}
