using Acetato.Application.Abstractions;
using Acetato.Domain;

namespace Acetato.Application.Hotkeys;

/// <summary>
/// Asignación actual de atajos (HU-21), inmutable. Solo guarda lo que difiere
/// del catálogo: una acción sin entrada usa su predeterminado y una entrada
/// <c>null</c> significa «sin atajo». Asignar el predeterminado borra la entrada,
/// así <see cref="Overrides"/> es exactamente lo que se persiste. Un
/// predeterminado que el usuario ya dio a otra acción cede ante ella: la acción
/// queda sin atajo (p. ej. «Plegar barra» llegó en HU-25 con <c>Ctrl+Alt+H</c>).
/// </summary>
public sealed class HotkeyMap
{
    private readonly Dictionary<HotkeyAction, KeyChord?> _overrides;

    private HotkeyMap(Dictionary<HotkeyAction, KeyChord?> overrides) => _overrides = overrides;

    /// <summary>Mapa sin cambios: todos los predeterminados del catálogo.</summary>
    public static HotkeyMap Defaults { get; } = new([]);

    /// <summary>Diferencias con el catálogo (lo que se persiste).</summary>
    public IReadOnlyDictionary<HotkeyAction, KeyChord?> Overrides => _overrides;

    /// <summary>Combinación actual de la acción; <c>null</c> si no tiene atajo.</summary>
    public KeyChord? ChordOf(HotkeyAction action)
    {
        if (_overrides.TryGetValue(action, out var chord))
        {
            return chord;
        }

        var fallback = HotkeyCatalog.Of(action).Default;
        return _overrides.ContainsValue(fallback) ? null : fallback;
    }

    /// <summary>Acción que usa la combinación; <c>null</c> si ninguna.</summary>
    public HotkeyAction? OwnerOf(KeyChord chord) =>
        HotkeyCatalog.All
            .Select(definition => (HotkeyAction?)definition.Action)
            .FirstOrDefault(action => ChordOf(action!.Value) == chord);

    /// <summary>Indica si la acción conserva su atajo por defecto.</summary>
    public bool IsDefault(HotkeyAction action) => !_overrides.ContainsKey(action);

    /// <summary>Copia con la acción en <paramref name="chord"/> (<c>null</c> = sin atajo).</summary>
    public HotkeyMap With(HotkeyAction action, KeyChord? chord)
    {
        var overrides = new Dictionary<HotkeyAction, KeyChord?>(_overrides);
        if (chord == HotkeyCatalog.Of(action).Default)
        {
            overrides.Remove(action);
        }
        else
        {
            overrides[action] = chord;
        }

        return new HotkeyMap(overrides);
    }

    /// <summary>Copia con la acción de vuelta a su predeterminado.</summary>
    public HotkeyMap Reset(HotkeyAction action) => With(action, HotkeyCatalog.Of(action).Default);
}
