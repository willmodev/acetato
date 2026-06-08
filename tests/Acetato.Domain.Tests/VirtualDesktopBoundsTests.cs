using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class VirtualDesktopBoundsTests
{
    [Fact]
    public void Union_of_no_monitors_is_empty()
    {
        VirtualDesktopBounds.Union([]).Should().Be(default(VirtualDesktopBounds));
    }

    [Fact]
    public void Union_of_a_single_monitor_is_that_monitor()
    {
        var monitor = new MonitorInfo(0, 0, 1920, 1080, true);

        VirtualDesktopBounds.Union([monitor])
            .Should().Be(new VirtualDesktopBounds(0, 0, 1920, 1080));
    }

    [Fact]
    public void Union_covers_two_side_by_side_monitors()
    {
        var primary = new MonitorInfo(0, 0, 1920, 1080, true);
        var secondary = new MonitorInfo(1920, 0, 1280, 1024, false);

        VirtualDesktopBounds.Union([primary, secondary])
            .Should().Be(new VirtualDesktopBounds(0, 0, 3200, 1080));
    }

    [Fact]
    public void Union_handles_a_monitor_to_the_left_with_negative_origin()
    {
        var primary = new MonitorInfo(0, 0, 1920, 1080, true);
        var left = new MonitorInfo(-1280, -200, 1280, 1024, false);

        VirtualDesktopBounds.Union([primary, left])
            .Should().Be(new VirtualDesktopBounds(-1280, -200, 3200, 1280));
    }
}
