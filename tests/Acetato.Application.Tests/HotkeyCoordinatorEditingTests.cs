using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Application.Tests;

/// <summary>Edición de atajos en el coordinador (HU-21), con el overlay en modo dibujo.</summary>
public sealed class HotkeyCoordinatorEditingTests : IDisposable
{
    private static readonly KeyChord AltZ = new(HotkeyModifiers.Alt, HotkeyKey.Z);

    private readonly IGlobalHotkeyService _hotkeys = Substitute.For<IGlobalHotkeyService>();
    private readonly IOverlayController _overlay = Substitute.For<IOverlayController>();
    private readonly IHotkeySettingsStore _store = Substitute.For<IHotkeySettingsStore>();
    private readonly List<IReadOnlyList<HotkeyAssignment>> _applied = [];
    private readonly HotkeyCoordinator _coordinator;

    public HotkeyCoordinatorEditingTests()
    {
        _hotkeys.Apply(Arg.Do<IReadOnlyList<HotkeyAssignment>>(_applied.Add)).Returns([]);
        _hotkeys.IsAvailable(Arg.Any<KeyChord>()).Returns(true);
        _store.Load().Returns(new HotkeySettingsLoad(HotkeyMap.Defaults, WasCorrupt: false));
        _overlay.IsVisible.Returns(true);
        _overlay.IsDrawingMode.Returns(true);
        _coordinator = new HotkeyCoordinator(_hotkeys, _overlay, _store, Substitute.For<IHotkeyNotifier>());
        _coordinator.Start();
    }

    private static KeyChord DefaultOf(HotkeyAction action) => HotkeyCatalog.Of(action).Default;

    [Fact]
    public void An_invalid_chord_returns_Invalid_and_changes_nothing()
    {
        var result = _coordinator.Assign(HotkeyAction.Undo, new KeyChord(HotkeyModifiers.Shift, HotkeyKey.Z));

        result.Should().Be(new ChordInvalid(KeyChordError.ShiftOnly));
        _coordinator.Map.Should().BeSameAs(HotkeyMap.Defaults);
        _store.DidNotReceive().Save(Arg.Any<HotkeyMap>());
    }

    [Fact]
    public void A_free_chord_is_assigned_applied_and_saved()
    {
        var result = _coordinator.Assign(HotkeyAction.Undo, AltZ);

        result.Should().BeOfType<ChordAssigned>();
        _coordinator.Map.ChordOf(HotkeyAction.Undo).Should().Be(AltZ);
        _applied[^1].Should().Contain(new HotkeyAssignment(HotkeyAction.Undo, AltZ));
        _store.Received(1).Save(_coordinator.Map);
    }

    [Fact]
    public void A_chord_used_by_another_action_returns_TakenByAction()
    {
        var result = _coordinator.Assign(HotkeyAction.Undo, DefaultOf(HotkeyAction.SelectLaserTool));

        result.Should().Be(new ChordTakenByAction(HotkeyAction.SelectLaserTool));
        _coordinator.Map.Should().BeSameAs(HotkeyMap.Defaults);
    }

    [Fact]
    public void Replace_assigns_the_chord_and_leaves_the_owner_without_chord()
    {
        var laser = DefaultOf(HotkeyAction.SelectLaserTool);

        _coordinator.Replace(HotkeyAction.Undo, laser);

        _coordinator.Map.ChordOf(HotkeyAction.Undo).Should().Be(laser);
        _coordinator.Map.ChordOf(HotkeyAction.SelectLaserTool).Should().BeNull();
        _store.Received(1).Save(_coordinator.Map);
    }

    [Fact]
    public void A_chord_taken_outside_returns_TakenByOtherApp_and_changes_nothing()
    {
        _hotkeys.IsAvailable(AltZ).Returns(false);

        var result = _coordinator.Assign(HotkeyAction.Undo, AltZ);

        result.Should().BeOfType<ChordTakenByOtherApp>();
        _coordinator.Map.Should().BeSameAs(HotkeyMap.Defaults);
    }

    [Fact]
    public void Assigning_the_current_chord_changes_nothing()
    {
        _coordinator.Assign(HotkeyAction.Undo, DefaultOf(HotkeyAction.Undo)).Should().BeOfType<ChordAssigned>();

        _store.DidNotReceive().Save(Arg.Any<HotkeyMap>());
    }

    [Fact]
    public void During_capture_nothing_is_registered_and_afterwards_everything_returns()
    {
        _coordinator.BeginCapture();
        _applied[^1].Should().BeEmpty();

        _coordinator.EndCapture();
        _applied[^1].Should().HaveCount(HotkeyCatalog.All.Count);
    }

    [Fact]
    public void Remove_Reset_and_ResetAll_save_each_new_map()
    {
        _coordinator.Remove(HotkeyAction.Capture);
        _coordinator.Map.ChordOf(HotkeyAction.Capture).Should().BeNull();
        _store.Received(1).Save(_coordinator.Map);

        _coordinator.Reset(HotkeyAction.Capture);
        _coordinator.Map.IsDefault(HotkeyAction.Capture).Should().BeTrue();

        _coordinator.Remove(HotkeyAction.Undo);
        _coordinator.ResetAll();
        _coordinator.Map.Overrides.Should().BeEmpty();
        _store.Received(4).Save(Arg.Any<HotkeyMap>());
    }

    [Fact]
    public void Reset_takes_the_default_back_from_the_action_using_it()
    {
        var laser = DefaultOf(HotkeyAction.SelectLaserTool);
        _coordinator.Replace(HotkeyAction.Undo, laser);

        _coordinator.Reset(HotkeyAction.SelectLaserTool);

        _coordinator.Map.ChordOf(HotkeyAction.SelectLaserTool).Should().Be(laser);
        _coordinator.Map.ChordOf(HotkeyAction.Undo).Should().BeNull();
    }

    [Fact]
    public void Each_change_raises_Changed()
    {
        int raised = 0;
        _coordinator.Changed += (_, _) => raised++;

        _coordinator.Assign(HotkeyAction.Undo, AltZ);
        _coordinator.Remove(HotkeyAction.Capture);

        raised.Should().Be(2);
    }

    [Fact]
    public void Actions_that_fail_to_register_are_marked_unavailable()
    {
        _hotkeys.Apply(Arg.Any<IReadOnlyList<HotkeyAssignment>>()).Returns([HotkeyAction.Undo]);

        _coordinator.Assign(HotkeyAction.Capture, AltZ);

        _coordinator.Unavailable.Should().Equal(HotkeyAction.Undo);
    }

    [Fact]
    public void Start_probes_every_chord_and_marks_the_taken_ones()
    {
        var hotkeys = Substitute.For<IGlobalHotkeyService>();
        hotkeys.IsAvailable(Arg.Any<KeyChord>()).Returns(true);
        hotkeys.IsAvailable(DefaultOf(HotkeyAction.Capture)).Returns(false);
        hotkeys.Apply(Arg.Any<IReadOnlyList<HotkeyAssignment>>()).Returns([]);
        _overlay.IsVisible.Returns(false);
        using var coordinator = new HotkeyCoordinator(hotkeys, _overlay, _store, Substitute.For<IHotkeyNotifier>());

        coordinator.Start();

        coordinator.Unavailable.Should().Equal(HotkeyAction.Capture);
        hotkeys.Received(HotkeyCatalog.All.Count).IsAvailable(Arg.Any<KeyChord>());
    }

    [Fact]
    public void Removing_an_unavailable_chord_clears_the_mark()
    {
        _hotkeys.Apply(Arg.Any<IReadOnlyList<HotkeyAssignment>>()).Returns([HotkeyAction.Undo]);
        _coordinator.BeginCapture();
        _coordinator.EndCapture();
        _hotkeys.Apply(Arg.Any<IReadOnlyList<HotkeyAssignment>>()).Returns([]);

        _coordinator.Remove(HotkeyAction.Undo);

        _coordinator.Unavailable.Should().BeEmpty();
    }

    public void Dispose() => _coordinator.Dispose();
}
