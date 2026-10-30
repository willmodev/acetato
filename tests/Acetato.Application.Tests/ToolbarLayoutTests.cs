using Acetato.Application.Overlay;
using FluentAssertions;
using Xunit;

namespace Acetato.Application.Tests;

/// <summary>Estado plegado de la barra (HU-25).</summary>
public sealed class ToolbarLayoutTests
{
    [Fact]
    public void Starts_expanded()
    {
        new ToolbarLayout().IsCollapsed.Should().BeFalse();
    }

    [Fact]
    public void Toggle_collapses_and_expands_again()
    {
        var layout = new ToolbarLayout();

        layout.ToggleCollapsed();
        var collapsed = layout.IsCollapsed;
        layout.ToggleCollapsed();

        collapsed.Should().BeTrue();
        layout.IsCollapsed.Should().BeFalse();
    }

    [Fact]
    public void Toggle_raises_changed_each_time()
    {
        var layout = new ToolbarLayout();
        var count = 0;
        layout.Changed += (_, _) => count++;

        layout.ToggleCollapsed();
        layout.ToggleCollapsed();

        count.Should().Be(2);
    }
}
