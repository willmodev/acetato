using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Application.Tests;

public sealed class HotkeyCoordinatorTests
{
    private static readonly HotkeyAction[] AlwaysActions = [HotkeyAction.Toggle, HotkeyAction.DrawingModeToggle];

    private readonly IGlobalHotkeyService _hotkeys = Substitute.For<IGlobalHotkeyService>();
    private readonly IOverlayController _overlay = Substitute.For<IOverlayController>();
    private readonly IHotkeySettingsStore _store = Substitute.For<IHotkeySettingsStore>();
    private readonly List<IReadOnlyList<HotkeyAssignment>> _applied = [];

    public HotkeyCoordinatorTests()
    {
        _hotkeys.Apply(Arg.Do<IReadOnlyList<HotkeyAssignment>>(_applied.Add)).Returns([]);
        _store.Load().Returns(new HotkeySettingsLoad(HotkeyMap.Defaults, WasCorrupt: false));
    }

    private IReadOnlyList<HotkeyAction> LastAppliedActions => [.. _applied[^1].Select(assignment => assignment.Action)];

    private HotkeyCoordinator CreateStarted(bool visible, bool drawing)
    {
        SetOverlay(visible, drawing);
        var coordinator = new HotkeyCoordinator(_hotkeys, _overlay, _store, Substitute.For<IHotkeyNotifier>());
        coordinator.Start();
        return coordinator;
    }

    private void SetOverlay(bool visible, bool drawing)
    {
        _overlay.IsVisible.Returns(visible);
        _overlay.IsDrawingMode.Returns(drawing);
    }

    private void RaiseStateChanged(bool visible, bool drawing)
    {
        SetOverlay(visible, drawing);
        _overlay.StateChanged += Raise.Event();
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(false, false)]
    [InlineData(true, false)]
    public void Hidden_or_click_through_applies_only_the_always_group(bool visible, bool drawing)
    {
        using var coordinator = CreateStarted(visible, drawing);

        LastAppliedActions.Should().Equal(AlwaysActions);
    }

    [Fact]
    public void Visible_in_drawing_mode_applies_every_action()
    {
        using var coordinator = CreateStarted(visible: true, drawing: true);

        LastAppliedActions.Should().Equal(HotkeyCatalog.All.Select(definition => definition.Action));
    }

    [Fact]
    public void Entering_drawing_mode_adds_the_drawing_group_and_leaving_removes_it()
    {
        using var coordinator = CreateStarted(visible: false, drawing: false);

        RaiseStateChanged(visible: true, drawing: true);
        LastAppliedActions.Should().HaveCount(HotkeyCatalog.All.Count);

        RaiseStateChanged(visible: true, drawing: false);
        LastAppliedActions.Should().Equal(AlwaysActions);
    }

    [Fact]
    public void A_state_change_that_keeps_the_same_set_does_not_reapply()
    {
        using var coordinator = CreateStarted(visible: true, drawing: false);

        RaiseStateChanged(visible: false, drawing: false);

        _applied.Should().HaveCount(1);
    }

    [Fact]
    public void Applies_the_chords_of_the_map()
    {
        using var coordinator = CreateStarted(visible: false, drawing: false);

        _applied[^1].Should().Contain(new HotkeyAssignment(
            HotkeyAction.Toggle, HotkeyCatalog.Of(HotkeyAction.Toggle).Default));
    }

    [Fact]
    public void Start_loads_the_saved_map_and_applies_it()
    {
        var saved = HotkeyMap.Defaults.With(HotkeyAction.Toggle, new KeyChord(HotkeyModifiers.Alt, HotkeyKey.F9));
        _store.Load().Returns(new HotkeySettingsLoad(saved, WasCorrupt: false));

        using var coordinator = CreateStarted(visible: false, drawing: false);

        coordinator.Map.Should().BeSameAs(saved);
        _applied[^1].Should().Contain(new HotkeyAssignment(HotkeyAction.Toggle, new KeyChord(HotkeyModifiers.Alt, HotkeyKey.F9)));
    }

    [Fact]
    public void An_action_without_chord_is_not_applied()
    {
        _store.Load().Returns(new HotkeySettingsLoad(HotkeyMap.Defaults.With(HotkeyAction.Undo, null), WasCorrupt: false));

        using var coordinator = CreateStarted(visible: true, drawing: true);

        LastAppliedActions.Should().NotContain(HotkeyAction.Undo).And.HaveCount(HotkeyCatalog.All.Count - 1);
    }

    [Fact]
    public void After_dispose_state_changes_are_ignored()
    {
        var coordinator = CreateStarted(visible: false, drawing: false);
        coordinator.Dispose();

        RaiseStateChanged(visible: true, drawing: true);

        _applied.Should().HaveCount(1);
    }
}
