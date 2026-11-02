using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Application.Tests;

/// <summary>
/// Supr borra la selección de Mover (SPEC 11): es un atajo fijo que solo se registra en
/// modo dibujo y con la herramienta Mover; con otra herramienta o en modo puntero, Supr
/// llega a las demás apps y al texto en edición.
/// </summary>
public sealed class HotkeyCoordinatorSelectionTests
{
    private readonly IGlobalHotkeyService _hotkeys = Substitute.For<IGlobalHotkeyService>();
    private readonly IOverlayController _overlay = Substitute.For<IOverlayController>();
    private readonly IHotkeySettingsStore _store = Substitute.For<IHotkeySettingsStore>();
    private readonly DrawingSettings _drawing = new();
    private readonly List<IReadOnlyList<HotkeyAssignment>> _applied = [];

    public HotkeyCoordinatorSelectionTests()
    {
        _hotkeys.Apply(Arg.Do<IReadOnlyList<HotkeyAssignment>>(_applied.Add)).Returns([]);
        _store.Load().Returns(new HotkeySettingsLoad(HotkeyMap.Defaults, WasCorrupt: false));
    }

    private bool SupprApplied => _applied[^1].Any(assignment => assignment.Action == HotkeyAction.DeleteSelection);

    [Fact]
    public void Suppr_is_applied_in_drawing_mode_with_the_move_tool()
    {
        _drawing.SelectTool(ToolKind.Move);
        using var coordinator = CreateStarted(drawing: true);

        _applied[^1].Should().ContainSingle(assignment => assignment.Action == HotkeyAction.DeleteSelection)
            .Which.Chord.Should().Be(new KeyChord(HotkeyModifiers.None, HotkeyKey.Delete));
    }

    [Fact]
    public void Suppr_is_not_applied_with_another_tool()
    {
        using var coordinator = CreateStarted(drawing: true);

        SupprApplied.Should().BeFalse();
    }

    [Fact]
    public void Suppr_is_not_applied_in_pointer_mode_even_with_the_move_tool()
    {
        _drawing.SelectTool(ToolKind.Move);
        using var coordinator = CreateStarted(drawing: false);

        SupprApplied.Should().BeFalse();
    }

    [Fact]
    public void Choosing_the_move_tool_adds_suppr_and_leaving_it_frees_the_key()
    {
        using var coordinator = CreateStarted(drawing: true);

        _drawing.SelectTool(ToolKind.Move);
        SupprApplied.Should().BeTrue();

        _drawing.SelectTool(ToolKind.Pencil);
        SupprApplied.Should().BeFalse();
    }

    [Fact]
    public void Suppr_stays_out_of_the_catalog_shown_in_settings()
    {
        HotkeyCatalog.All.Should().NotContain(definition => definition.Action == HotkeyAction.DeleteSelection);
        FixedHotkeys.ChordOf(HotkeyAction.DeleteSelection).Should().Be(new KeyChord(HotkeyModifiers.None, HotkeyKey.Delete));
    }

    private HotkeyCoordinator CreateStarted(bool drawing)
    {
        _overlay.IsVisible.Returns(true);
        _overlay.IsDrawingMode.Returns(drawing);
        var coordinator = new HotkeyCoordinator(_hotkeys, _overlay, _store, Substitute.For<IHotkeyNotifier>(), _drawing);
        coordinator.Start();
        return coordinator;
    }
}
