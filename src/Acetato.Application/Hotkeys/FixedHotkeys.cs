using Acetato.Application.Abstractions;
using Acetato.Domain;

namespace Acetato.Application.Hotkeys;

/// <summary>
/// Atajos fijos (HU-24): no están en el catálogo, no se guardan ni salen en
/// Ajustes. Hoy solo Esc para el Puntero, que se registra solo en modo dibujo:
/// fuera de él, Esc llega a las demás apps.
/// </summary>
public static class FixedHotkeys
{
    /// <summary>Esc → salir del modo dibujo sin ocultar nada.</summary>
    public static HotkeyAssignment PointerEsc { get; } =
        new(HotkeyAction.EnterPointerMode, new KeyChord(HotkeyModifiers.None, HotkeyKey.Escape));

    /// <summary>Atajos fijos que se registran junto al grupo «Solo al dibujar».</summary>
    public static IReadOnlyList<HotkeyAssignment> Drawing { get; } = [PointerEsc];

    /// <summary>Combinación fija de <paramref name="action"/>; <c>null</c> si no es fija.</summary>
    public static KeyChord? ChordOf(HotkeyAction action) =>
        Drawing.FirstOrDefault(assignment => assignment.Action == action)?.Chord;
}
