using Acetato.Application.Abstractions;
using Acetato.Domain;

namespace Acetato.Application.Hotkeys;

/// <summary>
/// Caso de uso central de los atajos (HU-21): guarda el mapa y decide qué
/// atajos están registrados según el estado del overlay y la pausa de captura.
/// Los del grupo «Siempre» viven mientras la app corre; los de «Al dibujar»,
/// solo con el overlay visible y en modo dibujo, para no quitarles Ctrl+Z o
/// Ctrl+S a las demás apps. Mientras se captura una combinación en Ajustes no
/// hay ninguno registrado. Cada cambio se guarda al instante. Agnóstico de
/// plataforma: depende solo de puertos.
/// </summary>
public sealed class HotkeyCoordinator : IDisposable
{
    private readonly IGlobalHotkeyService _hotkeys;
    private readonly IOverlayController _overlay;
    private readonly IHotkeySettingsStore _store;
    private readonly IHotkeyNotifier _notifier;
    private readonly HotkeyAvailability _availability;
    private List<HotkeyAssignment> _applied = [];
    private bool _started;
    private bool _capturing;

    public HotkeyCoordinator(
        IGlobalHotkeyService hotkeys,
        IOverlayController overlay,
        IHotkeySettingsStore store,
        IHotkeyNotifier notifier)
    {
        _hotkeys = hotkeys;
        _overlay = overlay;
        _store = store;
        _notifier = notifier;
        _availability = new HotkeyAvailability(hotkeys);
    }

    /// <summary>Asignación actual.</summary>
    public HotkeyMap Map { get; private set; } = HotkeyMap.Defaults;

    /// <summary>Acciones cuyo atajo no se pudo registrar (otra app lo tiene).</summary>
    public IReadOnlySet<HotkeyAction> Unavailable => _availability.Unavailable;

    /// <summary>Cambió el mapa o la disponibilidad de algún atajo.</summary>
    public event EventHandler? Changed;

    private bool IsDrawingActive => _overlay.IsVisible && _overlay.IsDrawingMode;

    /// <summary>
    /// Carga los atajos guardados, prueba todas sus combinaciones (también las
    /// de dibujo, que aún no se registran), aplica las activas, empieza a seguir
    /// el estado del overlay y avisa si algo falló. Solo aquí se avisa: lo que
    /// falle después se marca «No disponible» sin repetir la notificación.
    /// </summary>
    public void Start()
    {
        if (_started)
        {
            return;
        }

        _started = true;
        var load = _store.Load();
        Map = load.Map;
        _ = _availability.Probe(Map, HotkeyCatalog.All.Select(definition => definition.Action));
        _overlay.StateChanged += OnOverlayStateChanged;
        _ = Refresh(force: true);
        if (Unavailable.Count > 0 || load.WasCorrupt)
        {
            _notifier.NotifyStartupProblems(Unavailable.Count, load.WasCorrupt);
        }
    }

    /// <summary>Intenta asignar la combinación; solo cambia algo si devuelve <see cref="ChordAssigned"/>.</summary>
    public AssignResult Assign(HotkeyAction action, KeyChord chord)
    {
        if (KeyChordRules.Validate(chord) is { } error)
        {
            return new ChordInvalid(error);
        }

        if (Map.ChordOf(action) == chord)
        {
            return new ChordAssigned();
        }

        if (Map.OwnerOf(chord) is { } owner)
        {
            return new ChordTakenByAction(owner);
        }

        if (!_hotkeys.IsAvailable(chord))
        {
            return new ChordTakenByOtherApp();
        }

        Commit(Map.With(action, chord));
        return new ChordAssigned();
    }

    /// <summary>Asigna la combinación y deja sin atajo a la acción que la tenía.</summary>
    public void Replace(HotkeyAction action, KeyChord chord) => Commit(WithTakeover(action, chord));

    /// <summary>Deja la acción sin atajo (la combinación queda libre para otras apps).</summary>
    public void Remove(HotkeyAction action) => Commit(Map.With(action, null));

    /// <summary>
    /// Devuelve la acción a su predeterminado. Si otra acción lo usa, esa queda
    /// sin atajo (como al reemplazar), para que nunca se repita una combinación.
    /// </summary>
    public void Reset(HotkeyAction action) => Replace(action, HotkeyCatalog.Of(action).Default);

    /// <summary>Devuelve todas las acciones a su predeterminado.</summary>
    public void ResetAll() => Commit(HotkeyMap.Defaults);

    /// <summary>Pausa todos los atajos mientras se captura una combinación.</summary>
    public void BeginCapture()
    {
        _capturing = true;
        _ = Refresh(force: false);
    }

    /// <summary>Reanuda los atajos que correspondan al estado del overlay.</summary>
    public void EndCapture()
    {
        _capturing = false;
        RefreshAndNotify();
    }

    public void Dispose() => _overlay.StateChanged -= OnOverlayStateChanged;

    private void OnOverlayStateChanged(object? sender, EventArgs e) => RefreshAndNotify();

    private void RefreshAndNotify()
    {
        if (Refresh(force: false))
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    private HotkeyMap WithTakeover(HotkeyAction action, KeyChord chord)
    {
        var owner = Map.OwnerOf(chord);
        var map = owner is { } other && other != action ? Map.With(other, null) : Map;
        return map.With(action, chord);
    }

    // Aplica el mapa nuevo: guarda, reaplica y revisa la disponibilidad de las
    // acciones que cambiaron (las no registradas se prueban en el sistema).
    private void Commit(HotkeyMap map)
    {
        var changed = HotkeyCatalog.All
            .Select(definition => definition.Action)
            .Where(action => Map.ChordOf(action) != map.ChordOf(action))
            .ToList();
        Map = map;
        _store.Save(map);
        _availability.Forget(changed);
        _ = Refresh(force: false);
        _ = _availability.Probe(map, changed.Where(action => !IsApplied(action)));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private bool IsApplied(HotkeyAction action) => _applied.Exists(assignment => assignment.Action == action);

    // Recalcula el conjunto activo y solo lo reaplica si cambió (ocultar en
    // click-through, por ejemplo, no toca nada). Devuelve si cambió la
    // disponibilidad de algún atajo.
    private bool Refresh(bool force)
    {
        var active = _capturing ? [] : ActiveHotkeys.Select(Map, IsDrawingActive);
        if (!force && active.SequenceEqual(_applied))
        {
            return false;
        }

        var failed = _hotkeys.Apply(active);
        _applied = active;
        return _availability.Record(active, failed);
    }
}
