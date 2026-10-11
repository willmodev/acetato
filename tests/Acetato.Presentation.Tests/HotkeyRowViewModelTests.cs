using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using Acetato.Presentation.ViewModels;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>Captura y conflictos de una fila de Ajustes (HU-21) sobre un coordinador real con dobles.</summary>
public sealed class HotkeyRowViewModelTests : IDisposable
{
    private static readonly KeyChord AltZ = new(HotkeyModifiers.Alt, HotkeyKey.Z);

    private readonly IGlobalHotkeyService _hotkeys = Substitute.For<IGlobalHotkeyService>();
    private readonly IOverlayController _overlay = Substitute.For<IOverlayController>();
    private readonly IHotkeySettingsStore _store = Substitute.For<IHotkeySettingsStore>();
    private readonly List<IReadOnlyList<HotkeyAssignment>> _applied = [];
    private readonly HotkeyCoordinator _coordinator;
    private readonly HotkeyRowViewModel _undo;

    public HotkeyRowViewModelTests()
    {
        _hotkeys.Apply(Arg.Do<IReadOnlyList<HotkeyAssignment>>(_applied.Add)).Returns([]);
        _hotkeys.IsAvailable(Arg.Any<KeyChord>()).Returns(true);
        _store.Load().Returns(new HotkeySettingsLoad(HotkeyMap.Defaults, WasCorrupt: false));
        _coordinator = new HotkeyCoordinator(_hotkeys, _overlay, _store, Substitute.For<IHotkeyNotifier>());
        _coordinator.Start();
        _undo = new HotkeyRowViewModel(HotkeyAction.Undo, _coordinator);
    }

    [Fact]
    public void Shows_the_current_chord_in_spanish()
    {
        _undo.Label.Should().Be("Deshacer");
        _undo.ChordText.Should().Be("Ctrl+Z");
        _undo.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void Starting_capture_asks_for_the_chord_and_pauses_every_hotkey()
    {
        _undo.StartCaptureCommand.Execute(null);

        _undo.IsCapturing.Should().BeTrue();
        _undo.ChordText.Should().Be("Pulsa la combinación…");
        _applied[^1].Should().BeEmpty();
    }

    [Fact]
    public void A_valid_chord_is_assigned_and_ends_the_capture()
    {
        _undo.StartCaptureCommand.Execute(null);

        _undo.CaptureCommand.Execute(AltZ);
        _undo.Refresh();

        _undo.IsCapturing.Should().BeFalse();
        _undo.ChordText.Should().Be("Alt+Z");
        _undo.IsDefault.Should().BeFalse();
    }

    [Theory]
    [InlineData(HotkeyModifiers.None, HotkeyKey.Z, "Usa Ctrl, Alt o Win")]
    [InlineData(HotkeyModifiers.Shift, HotkeyKey.Z, "Mayús necesita otro modificador")]
    [InlineData(HotkeyModifiers.Alt, HotkeyKey.F4, "Reservado por Windows")]
    public void An_invalid_chord_shows_why_and_keeps_capturing(HotkeyModifiers modifiers, HotkeyKey key, string message)
    {
        _undo.StartCaptureCommand.Execute(null);

        _undo.CaptureCommand.Execute(new KeyChord(modifiers, key));

        _undo.Message.Should().Be(message);
        _undo.IsCapturing.Should().BeTrue();
    }

    [Fact]
    public void An_unsupported_key_shows_its_message()
    {
        _undo.StartCaptureCommand.Execute(null);

        _undo.CaptureCommand.Execute(null);

        _undo.Message.Should().Be("Tecla no admitida");
    }

    [Fact]
    public void A_function_key_alone_is_accepted()
    {
        _undo.StartCaptureCommand.Execute(null);

        _undo.CaptureCommand.Execute(new KeyChord(HotkeyModifiers.None, HotkeyKey.F8));

        _coordinator.Map.ChordOf(HotkeyAction.Undo).Should().Be(new KeyChord(HotkeyModifiers.None, HotkeyKey.F8));
    }

    [Fact]
    public void A_chord_of_another_action_offers_Replace()
    {
        var laser = HotkeyCatalog.Of(HotkeyAction.SelectLaserTool).Default;
        _undo.StartCaptureCommand.Execute(null);

        _undo.CaptureCommand.Execute(laser);

        _undo.Message.Should().Be("Ya la usa Láser");
        _undo.CanReplace.Should().BeTrue();
        _undo.IsCapturing.Should().BeFalse();

        _undo.ReplaceCommand.Execute(null);

        _coordinator.Map.ChordOf(HotkeyAction.Undo).Should().Be(laser);
        _coordinator.Map.ChordOf(HotkeyAction.SelectLaserTool).Should().BeNull();
        _undo.HasMessage.Should().BeFalse();
    }

    [Fact]
    public void A_chord_taken_by_another_app_is_not_assigned()
    {
        _hotkeys.IsAvailable(AltZ).Returns(false);
        _undo.StartCaptureCommand.Execute(null);

        _undo.CaptureCommand.Execute(AltZ);

        _undo.Message.Should().Be("En uso por otra aplicación");
        _coordinator.Map.IsDefault(HotkeyAction.Undo).Should().BeTrue();
    }

    [Fact]
    public void Cancel_ends_the_capture_without_changes_and_resumes_hotkeys()
    {
        _undo.StartCaptureCommand.Execute(null);

        _undo.CancelCaptureCommand.Execute(null);

        _undo.IsCapturing.Should().BeFalse();
        _undo.ChordText.Should().Be("Ctrl+Z");
        _applied[^1].Should().NotBeEmpty();
    }

    public void Dispose() => _coordinator.Dispose();
}
