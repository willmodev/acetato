using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

/// <summary>Posición recordada de la barra y su monitor (HU-29).</summary>
public sealed class ToolbarPlacementTests
{
    private static readonly MonitorInfo Primary = new(0, 0, 1920, 1080, IsPrimary: true);
    private static readonly MonitorInfo Side = new(1920, 0, 2560, 1440, IsPrimary: false);

    [Fact]
    public void Its_monitor_still_connected_is_home()
    {
        var placement = new ToolbarPlacement(Side, 2500, 80);

        placement.FindHome([Primary, Side]).Should().Be(Side);
    }

    [Fact]
    public void Being_primary_or_not_does_not_count()
    {
        var placement = new ToolbarPlacement(Side with { IsPrimary = true }, 2500, 80);

        placement.FindHome([Primary, Side]).Should().Be(Side);
    }

    [Fact]
    public void A_disconnected_monitor_is_not_home()
    {
        var placement = new ToolbarPlacement(Side, 2500, 80);

        placement.FindHome([Primary]).Should().BeNull();
    }

    [Fact]
    public void A_monitor_with_another_resolution_is_not_home()
    {
        var placement = new ToolbarPlacement(Side, 2500, 80);

        placement.FindHome([Primary, Side with { Width = 1920, Height = 1080 }]).Should().BeNull();
    }
}
