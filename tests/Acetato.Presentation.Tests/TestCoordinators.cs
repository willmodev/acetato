using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using NSubstitute;

namespace Acetato.Presentation.Tests;

/// <summary>Coordinadores de atajos con dobles: combinaciones libres y, salvo que se pase, sin cambios guardados.</summary>
internal static class TestCoordinators
{
    /// <summary>Coordinador sin arrancar; carga <paramref name="saved"/> (o los predeterminados) al llamar a Start.</summary>
    public static HotkeyCoordinator Create(HotkeyMap? saved = null)
    {
        var hotkeys = Substitute.For<IGlobalHotkeyService>();
        hotkeys.Apply(Arg.Any<IReadOnlyList<HotkeyAssignment>>()).Returns([]);
        hotkeys.IsAvailable(Arg.Any<KeyChord>()).Returns(true);
        var store = Substitute.For<IHotkeySettingsStore>();
        store.Load().Returns(new HotkeySettingsLoad(saved ?? HotkeyMap.Defaults, WasCorrupt: false));
        return new HotkeyCoordinator(hotkeys, Substitute.For<IOverlayController>(), store, Substitute.For<IHotkeyNotifier>());
    }

    /// <summary>Coordinador ya arrancado con los predeterminados.</summary>
    public static HotkeyCoordinator Started()
    {
        var coordinator = Create();
        coordinator.Start();
        return coordinator;
    }
}
