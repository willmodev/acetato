using Acetato.Application.Abstractions;

namespace Acetato.Application.Hotkeys;

/// <summary>
/// Acciones cuyo atajo no se pudo registrar (HU-21): otra app ya tiene la
/// combinación. Se actualiza con el resultado de cada registro y, para los
/// atajos que Acetato aún no registró (grupo Al dibujar fuera de modo dibujo),
/// con una prueba en el sistema.
/// </summary>
internal sealed class HotkeyAvailability
{
    private readonly IGlobalHotkeyService _hotkeys;
    private readonly HashSet<HotkeyAction> _unavailable = [];

    public HotkeyAvailability(IGlobalHotkeyService hotkeys) => _hotkeys = hotkeys;

    /// <summary>Acciones marcadas «No disponible».</summary>
    public IReadOnlySet<HotkeyAction> Unavailable => _unavailable;

    /// <summary>
    /// Anota el resultado de registrar <paramref name="applied"/>. Las acciones
    /// que no se intentaron conservan su estado. Devuelve si algo cambió.
    /// </summary>
    public bool Record(IReadOnlyList<HotkeyAssignment> applied, IReadOnlyList<HotkeyAction> failed)
    {
        bool changed = false;
        foreach (var action in applied.Select(assignment => assignment.Action))
        {
            changed |= Set(action, failed.Contains(action));
        }

        return changed;
    }

    /// <summary>
    /// Prueba en el sistema la combinación de cada acción. Solo es fiable para
    /// combinaciones que Acetato no tiene registradas. Devuelve si algo cambió.
    /// </summary>
    public bool Probe(HotkeyMap map, IEnumerable<HotkeyAction> actions)
    {
        bool changed = false;
        foreach (var action in actions)
        {
            bool taken = map.ChordOf(action) is { } chord && !_hotkeys.IsAvailable(chord);
            changed |= Set(action, taken);
        }

        return changed;
    }

    /// <summary>Olvida el estado de las acciones (su combinación cambió).</summary>
    public void Forget(IEnumerable<HotkeyAction> actions) => _unavailable.ExceptWith(actions);

    private bool Set(HotkeyAction action, bool unavailable) =>
        unavailable ? _unavailable.Add(action) : _unavailable.Remove(action);
}
