using Acetato.Application.Abstractions;

namespace Acetato.Application.Hotkeys;

/// <summary>
/// Caso de uso central de los atajos (HU-21): guarda el mapa y decide qué
/// atajos están registrados según el estado del overlay. Los del grupo
/// «Siempre» viven mientras la app corre; los de «Al dibujar», solo con el
/// overlay visible y en modo dibujo, para no quitarles Ctrl+Z o Ctrl+S a las
/// demás apps. Carga el mapa guardado al arrancar. Agnóstico de plataforma:
/// depende solo de puertos.
/// </summary>
public sealed class HotkeyCoordinator : IDisposable
{
    private readonly IGlobalHotkeyService _hotkeys;
    private readonly IOverlayController _overlay;
    private readonly IHotkeySettingsStore _store;
    private List<HotkeyAssignment> _applied = [];
    private bool _started;

    public HotkeyCoordinator(IGlobalHotkeyService hotkeys, IOverlayController overlay, IHotkeySettingsStore store)
    {
        _hotkeys = hotkeys;
        _overlay = overlay;
        _store = store;
    }

    /// <summary>Asignación actual.</summary>
    public HotkeyMap Map { get; private set; } = HotkeyMap.Defaults;

    /// <summary>
    /// Carga los atajos guardados, aplica los activos y empieza a seguir el
    /// estado del overlay.
    /// </summary>
    public void Start()
    {
        if (_started)
        {
            return;
        }

        _started = true;
        Map = _store.Load().Map;
        _overlay.StateChanged += OnOverlayStateChanged;
        Refresh(force: true);
    }

    public void Dispose() => _overlay.StateChanged -= OnOverlayStateChanged;

    private void OnOverlayStateChanged(object? sender, EventArgs e) => Refresh(force: false);

    private bool IsDrawingActive => _overlay.IsVisible && _overlay.IsDrawingMode;

    // Recalcula el conjunto activo y solo lo reaplica si cambió (ocultar en
    // click-through, por ejemplo, no toca nada).
    private void Refresh(bool force)
    {
        var active = ActiveHotkeys.Select(Map, IsDrawingActive);
        if (!force && active.SequenceEqual(_applied))
        {
            return;
        }

        _ = _hotkeys.Apply(active);
        _applied = active;
    }
}
