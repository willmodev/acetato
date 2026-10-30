using Acetato.Application.Abstractions;

namespace Acetato.Application.Hotkeys;

/// <summary>
/// Calcula qué atajos deben estar registrados (HU-21): siempre los del grupo
/// <see cref="HotkeyScope.Always"/>, y los de <see cref="HotkeyScope.DrawingMode"/>
/// solo si se piden, junto con los fijos de dibujo (HU-24). Las acciones sin
/// atajo se omiten.
/// </summary>
internal static class ActiveHotkeys
{
    /// <summary>Asignaciones activas, en el orden del catálogo.</summary>
    public static List<HotkeyAssignment> Select(HotkeyMap map, bool includeDrawing) =>
    [
        .. HotkeyCatalog.All
            .Where(definition => includeDrawing || definition.Scope == HotkeyScope.Always)
            .Select(definition => (definition.Action, Chord: map.ChordOf(definition.Action)))
            .Where(entry => entry.Chord.HasValue)
            .Select(entry => new HotkeyAssignment(entry.Action, entry.Chord!.Value)),
        .. includeDrawing ? FixedHotkeys.Drawing : [],
    ];
}
