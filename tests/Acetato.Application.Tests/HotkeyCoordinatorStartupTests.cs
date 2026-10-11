using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using NSubstitute;
using Xunit;

namespace Acetato.Application.Tests;

/// <summary>Aviso de problemas al arrancar (HU-21).</summary>
public sealed class HotkeyCoordinatorStartupTests
{
    private readonly IGlobalHotkeyService _hotkeys = Substitute.For<IGlobalHotkeyService>();
    private readonly IOverlayController _overlay = Substitute.For<IOverlayController>();
    private readonly IHotkeySettingsStore _store = Substitute.For<IHotkeySettingsStore>();
    private readonly IHotkeyNotifier _notifier = Substitute.For<IHotkeyNotifier>();

    public HotkeyCoordinatorStartupTests()
    {
        _hotkeys.Apply(Arg.Any<IReadOnlyList<HotkeyAssignment>>()).Returns([]);
        _hotkeys.IsAvailable(Arg.Any<KeyChord>()).Returns(true);
        _store.Load().Returns(new HotkeySettingsLoad(HotkeyMap.Defaults, WasCorrupt: false));
    }

    private HotkeyCoordinator Start()
    {
        var coordinator = new HotkeyCoordinator(_hotkeys, _overlay, _store, _notifier);
        coordinator.Start();
        return coordinator;
    }

    [Fact]
    public void A_clean_start_does_not_notify()
    {
        using var coordinator = Start();

        _notifier.DidNotReceiveWithAnyArgs().NotifyStartupProblems(default, default);
    }

    [Fact]
    public void Taken_chords_are_notified_with_their_count()
    {
        _hotkeys.IsAvailable(HotkeyCatalog.Of(HotkeyAction.Toggle).Default).Returns(false);
        _hotkeys.IsAvailable(HotkeyCatalog.Of(HotkeyAction.Capture).Default).Returns(false);
        // Mostrar/ocultar se registra al arrancar: si otra app lo tiene, también falla ahí.
        _hotkeys.Apply(Arg.Any<IReadOnlyList<HotkeyAssignment>>()).Returns([HotkeyAction.Toggle]);

        using var coordinator = Start();

        _notifier.Received(1).NotifyStartupProblems(2, false);
    }

    [Fact]
    public void A_corrupt_settings_file_is_notified()
    {
        _store.Load().Returns(new HotkeySettingsLoad(HotkeyMap.Defaults, WasCorrupt: true));

        using var coordinator = Start();

        _notifier.Received(1).NotifyStartupProblems(0, true);
    }

    [Fact]
    public void Failures_after_start_do_not_notify_again()
    {
        _hotkeys.IsAvailable(HotkeyCatalog.Of(HotkeyAction.Capture).Default).Returns(false);
        using var coordinator = Start();
        _hotkeys.Apply(Arg.Any<IReadOnlyList<HotkeyAssignment>>()).Returns([HotkeyAction.Undo]);
        _overlay.IsVisible.Returns(true);
        _overlay.IsDrawingMode.Returns(true);

        _overlay.StateChanged += Raise.Event();

        _notifier.ReceivedWithAnyArgs(1).NotifyStartupProblems(default, default);
    }
}
