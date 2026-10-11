using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Application.Tests;

public sealed class HotkeyCatalogTests
{
    [Fact]
    public void Every_action_has_exactly_one_definition()
    {
        HotkeyCatalog.All.Select(definition => definition.Action)
            .Should().BeEquivalentTo(Enum.GetValues<HotkeyAction>())
            .And.OnlyHaveUniqueItems();
    }

    [Fact]
    public void No_default_chord_is_repeated()
    {
        HotkeyCatalog.All.Select(definition => definition.Default).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Every_default_passes_the_chord_rules()
    {
        HotkeyCatalog.All.Should().AllSatisfy(definition =>
            KeyChordRules.Validate(definition.Default).Should().BeNull());
    }

    [Fact]
    public void Show_and_drawing_mode_are_always_active_and_the_rest_only_while_drawing()
    {
        HotkeyCatalog.All.Where(definition => definition.Scope == HotkeyScope.Always)
            .Select(definition => definition.Action)
            .Should().Equal(HotkeyAction.Toggle, HotkeyAction.DrawingModeToggle);
    }

    [Fact]
    public void Of_returns_the_definition_of_the_action()
    {
        HotkeyCatalog.Of(HotkeyAction.Undo).Action.Should().Be(HotkeyAction.Undo);
    }
}
