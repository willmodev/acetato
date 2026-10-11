using Acetato.Application.Abstractions;
using Acetato.Domain;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Hotkeys;

/// <summary>
/// Atajos registrados con RegisterHotKey (HU-21). Con hWnd = 0 cada registro
/// queda atado al hilo que lo hace, así que esta clase SOLO se usa desde el hilo
/// de atajos de <see cref="Win32GlobalHotkeyService"/>. No es thread-safe a
/// propósito: el hilo único la serializa.
/// </summary>
internal sealed class HotkeyRegistry
{
    // Id del registro de prueba de IsAvailable: fuera del rango de las acciones.
    private const int ProbeId = 0xBFFF;

    private readonly Dictionary<int, HotkeyAction> _registered = [];

    /// <summary>
    /// Suelta lo registrado y registra <paramref name="assignments"/>. Cada
    /// registro es independiente: si uno falla, los demás siguen.
    /// </summary>
    public List<HotkeyAction> Apply(IReadOnlyList<HotkeyAssignment> assignments)
    {
        UnregisterAll();
        var failed = new List<HotkeyAction>();
        foreach (var assignment in assignments)
        {
            int id = IdOf(assignment.Action);
            if (TryRegister(id, assignment.Chord))
            {
                _registered[id] = assignment.Action;
            }
            else
            {
                failed.Add(assignment.Action);
            }
        }

        return failed;
    }

    /// <summary>
    /// Registra y suelta al instante. Falla si otra app o este mismo hilo ya
    /// tienen la combinación.
    /// </summary>
    public static bool IsAvailable(KeyChord chord)
    {
        if (!TryRegister(ProbeId, chord))
        {
            return false;
        }

        NativeMethods.UnregisterHotKey(nint.Zero, ProbeId);
        return true;
    }

    /// <summary>Suelta todos los atajos registrados.</summary>
    public void UnregisterAll()
    {
        foreach (int id in _registered.Keys)
        {
            NativeMethods.UnregisterHotKey(nint.Zero, id);
        }

        _registered.Clear();
    }

    /// <summary>Acción del atajo con ese id (el wParam de WM_HOTKEY); <c>null</c> si no está.</summary>
    public HotkeyAction? Find(int id) =>
        _registered.TryGetValue(id, out var action) ? action : null;

    // Id estable por acción: nunca 0 y nunca choca con el de prueba.
    private static int IdOf(HotkeyAction action) => (int)action + 1;

    private static bool TryRegister(int id, KeyChord chord) =>
        NativeMethods.RegisterHotKey(
            nint.Zero,
            id,
            HotkeyKeyMap.ToModifiers(chord.Modifiers),
            HotkeyKeyMap.ToVirtualKey(chord.Key));
}
