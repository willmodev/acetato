using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class MonitorPickerTests
{
    private static readonly IReadOnlyList<MonitorInfo> TwoMonitors =
    [
        new MonitorInfo(0, 0, 1920, 1080, true),
        new MonitorInfo(1920, 0, 1280, 1024, false),
    ];

    [Fact]
    public void IndexAt_finds_the_monitor_under_the_point()
    {
        MonitorPicker.IndexAt(TwoMonitors, 100, 100).Should().Be(0);
        MonitorPicker.IndexAt(TwoMonitors, 2000, 200).Should().Be(1);
    }

    [Fact]
    public void IndexAt_returns_the_primary_when_no_monitor_contains_the_point()
    {
        // Punto fuera de ambos monitores (por debajo del segundo): cae al primario.
        MonitorPicker.IndexAt(TwoMonitors, 2000, 2000).Should().Be(0);
    }

    [Fact]
    public void IndexAt_assigns_the_shared_edge_to_the_right_monitor()
    {
        // x == 1920 pertenece al segundo monitor (límite derecho exclusivo).
        MonitorPicker.IndexAt(TwoMonitors, 1920, 0).Should().Be(1);
    }
}
