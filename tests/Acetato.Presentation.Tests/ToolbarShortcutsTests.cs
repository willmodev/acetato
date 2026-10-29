using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using Acetato.Presentation.ViewModels;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>Tooltips de la barra con el atajo vigente (HU-22).</summary>
public sealed class ToolbarShortcutsTests : IDisposable
{
    private static readonly KeyChord AltZ = new(HotkeyModifiers.Alt, HotkeyKey.Z);

    private readonly HotkeyCoordinator _coordinator = TestCoordinators.Started();
    private readonly ToolbarShortcuts _shortcuts;

    public ToolbarShortcutsTests() => _shortcuts = new ToolbarShortcuts(_coordinator);

    [Fact]
    public void A_hint_shows_the_name_and_the_current_chord()
    {
        var hint = _shortcuts.Hint("Lápiz", HotkeyAction.SelectPencilTool);

        hint.Label.Should().Be("Lápiz");
        hint.Shortcut.Should().Be("Ctrl+P");
        hint.HasShortcut.Should().BeTrue();
    }

    [Fact]
    public void The_pointer_hint_shows_its_fixed_esc()
    {
        var hint = _shortcuts.Hint("Puntero", HotkeyAction.EnterPointerMode);

        hint.Shortcut.Should().Be("Esc");
    }

    [Fact]
    public void A_button_without_action_shows_only_the_name()
    {
        var hint = _shortcuts.Hint("Ajustes");

        hint.Shortcut.Should().BeNull();
        hint.HasShortcut.Should().BeFalse();
        hint.ToString().Should().Be("Ajustes");
    }

    [Fact]
    public void Changing_a_chord_in_settings_updates_the_hint()
    {
        var hint = _shortcuts.Hint("Deshacer", HotkeyAction.Undo);

        _coordinator.Assign(HotkeyAction.Undo, AltZ);

        hint.Shortcut.Should().Be("Alt+Z");
    }

    [Fact]
    public void Removing_a_chord_leaves_only_the_name()
    {
        var hint = _shortcuts.Hint("Deshacer", HotkeyAction.Undo);

        _coordinator.Remove(HotkeyAction.Undo);

        hint.HasShortcut.Should().BeFalse();
    }

    [Fact]
    public void Hints_created_before_start_show_the_saved_chords()
    {
        using var coordinator = TestCoordinators.Create(HotkeyMap.Defaults.With(HotkeyAction.Undo, AltZ));
        using var shortcuts = new ToolbarShortcuts(coordinator);
        var hint = shortcuts.Hint("Deshacer", HotkeyAction.Undo);

        coordinator.Start();

        hint.Shortcut.Should().Be("Alt+Z");
    }

    [Fact]
    public void After_dispose_hints_stop_following_the_coordinator()
    {
        var hint = _shortcuts.Hint("Deshacer", HotkeyAction.Undo);

        _shortcuts.Dispose();
        _coordinator.Assign(HotkeyAction.Undo, AltZ);

        hint.Shortcut.Should().Be("Ctrl+Z");
    }

    public void Dispose()
    {
        _shortcuts.Dispose();
        _coordinator.Dispose();
    }
}
