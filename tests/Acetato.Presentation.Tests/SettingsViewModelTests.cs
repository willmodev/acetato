using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using Acetato.Presentation.ViewModels;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>Ventana Ajustes (HU-21): bloques, captura única, quitar y restaurar.</summary>
public sealed class SettingsViewModelTests : IDisposable
{
    private readonly IGlobalHotkeyService _hotkeys = Substitute.For<IGlobalHotkeyService>();
    private readonly IHotkeySettingsStore _store = Substitute.For<IHotkeySettingsStore>();
    private readonly IDrawingPreferencesStore _preferences = Substitute.For<IDrawingPreferencesStore>();
    private readonly DrawingSettings _drawing = new();
    private readonly List<IReadOnlyList<HotkeyAssignment>> _applied = [];
    private readonly HotkeyCoordinator _coordinator;
    private readonly SettingsViewModel _settings;

    public SettingsViewModelTests()
    {
        _hotkeys.Apply(Arg.Do<IReadOnlyList<HotkeyAssignment>>(_applied.Add)).Returns([]);
        _hotkeys.IsAvailable(Arg.Any<KeyChord>()).Returns(true);
        _store.Load().Returns(new HotkeySettingsLoad(HotkeyMap.Defaults, WasCorrupt: false));
        _coordinator = new HotkeyCoordinator(_hotkeys, Substitute.For<IOverlayController>(), _store, Substitute.For<IHotkeyNotifier>());
        _coordinator.Start();
        _settings = new SettingsViewModel(_coordinator, Substitute.For<IWindowDragHandler>(), _drawing, _preferences);
    }

    private HotkeyRowViewModel Row(HotkeyAction action) =>
        _settings.AlwaysRows.Concat(_settings.DrawingRows).Single(row => row.Action == action);

    [Fact]
    public void Lists_the_twenty_five_actions_in_two_blocks()
    {
        _settings.AlwaysRows.Select(row => row.Label).Should().Equal("Mostrar/ocultar", "Modo dibujo", "Plegar barra");
        _settings.DrawingRows.Should().HaveCount(22);
    }

    [Fact]
    public void Only_one_row_captures_at_a_time()
    {
        var undo = Row(HotkeyAction.Undo);
        var capture = Row(HotkeyAction.Capture);

        undo.StartCaptureCommand.Execute(null);
        capture.StartCaptureCommand.Execute(null);

        undo.IsCapturing.Should().BeFalse();
        _settings.CapturingRow.Should().BeSameAs(capture);
        _applied[^1].Should().BeEmpty();
    }

    [Fact]
    public void Remove_leaves_the_row_without_chord_and_Reset_restores_it()
    {
        var undo = Row(HotkeyAction.Undo);

        undo.RemoveCommand.Execute(null);
        undo.ChordText.Should().Be("Sin atajo");
        undo.HasChord.Should().BeFalse();
        undo.IsDefault.Should().BeFalse();

        undo.ResetCommand.Execute(null);
        undo.ChordText.Should().Be("Ctrl+Z");
        undo.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void ResetAll_asks_for_confirmation_before_restoring()
    {
        Row(HotkeyAction.Undo).RemoveCommand.Execute(null);

        _settings.ResetAllCommand.Execute(null);
        _settings.IsConfirmingResetAll.Should().BeTrue();
        _coordinator.Map.Overrides.Should().NotBeEmpty();

        _settings.ConfirmResetAllCommand.Execute(null);
        _settings.IsConfirmingResetAll.Should().BeFalse();
        _coordinator.Map.Overrides.Should().BeEmpty();
    }

    [Fact]
    public void Cancelling_ResetAll_changes_nothing()
    {
        Row(HotkeyAction.Undo).RemoveCommand.Execute(null);

        _settings.ResetAllCommand.Execute(null);
        _settings.CancelResetAllCommand.Execute(null);

        _settings.IsConfirmingResetAll.Should().BeFalse();
        _coordinator.Map.Overrides.Should().ContainKey(HotkeyAction.Undo);
    }

    [Fact]
    public void Closing_while_capturing_resumes_the_hotkeys()
    {
        Row(HotkeyAction.Undo).StartCaptureCommand.Execute(null);

        _settings.Dispose();

        _applied[^1].Should().NotBeEmpty();
    }

    [Fact]
    public void Ink_pairs_start_as_the_drawing_state()
    {
        _settings.InkPairs.Should().BeFalse();
    }

    [Fact]
    public void Turning_on_ink_pairs_applies_it_and_saves_it()
    {
        var raised = new List<string?>();
        _settings.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _settings.InkPairs = true;

        _drawing.InkPairsEnabled.Should().BeTrue();
        _preferences.Received(1).Save(new DrawingPreferences(InkPairs: true));
        raised.Should().Contain(nameof(SettingsViewModel.InkPairs));
    }

    [Fact]
    public void Setting_the_same_value_does_not_save_again()
    {
        _settings.InkPairs = false;

        _preferences.DidNotReceiveWithAnyArgs().Save(default!);
    }

    [Fact]
    public void Reset_all_restores_the_hotkeys_and_keeps_ink_pairs()
    {
        _settings.InkPairs = true;
        Row(HotkeyAction.Undo).RemoveCommand.Execute(null);

        _settings.ResetAllCommand.Execute(null);
        _settings.ConfirmResetAllCommand.Execute(null);

        Row(HotkeyAction.Undo).HasChord.Should().BeTrue();
        _settings.InkPairs.Should().BeTrue();
        _drawing.InkPairsEnabled.Should().BeTrue();
    }

    public void Dispose()
    {
        _settings.Dispose();
        _coordinator.Dispose();
    }
}
