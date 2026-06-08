using System.Windows.Forms;
using Acetato.Application.Abstractions;
using Acetato.Domain;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Monitors;

/// <summary>
/// Enumera los monitores conectados (HU-09) vía <see cref="Screen.AllScreens"/>,
/// que en un proceso Per-Monitor V2 devuelve los bounds en píxeles físicos y marca
/// el primario. Más fiable que EnumDisplayMonitors con callback de delegado. El
/// interop vive en Infrastructure.
/// </summary>
public sealed class Win32MonitorService : IMonitorService
{
    public IReadOnlyList<MonitorInfo> GetMonitors()
    {
        var screens = Screen.AllScreens;
        var monitors = new List<MonitorInfo>(screens.Length);
        foreach (var screen in screens)
        {
            var bounds = screen.Bounds;
            monitors.Add(new MonitorInfo(bounds.X, bounds.Y, bounds.Width, bounds.Height, screen.Primary));
        }

        return monitors.Count > 0 ? monitors : PrimaryFallback();
    }

    // Si por alguna razón no hay pantallas, recurre al monitor primario.
    private static IReadOnlyList<MonitorInfo> PrimaryFallback()
    {
        int width = NativeMethods.GetSystemMetrics(NativeMethods.SmCxScreen);
        int height = NativeMethods.GetSystemMetrics(NativeMethods.SmCyScreen);
        return [new MonitorInfo(0, 0, width, height, true)];
    }
}
