using Acetato.Domain;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Monitors;

/// <summary>
/// Escala de Windows de un monitor (100 %, 125 %…) para sistema.txt (HU-33). Lee los DPI
/// efectivos del monitor que contiene el centro del recuadro. El interop vive en
/// Infrastructure.
/// </summary>
internal static class MonitorScale
{
    private const int BaseDpi = 96;
    private const int Percent = 100;

    /// <summary>Escala en porcentaje; 100 si Windows no la informa.</summary>
    public static int PercentOf(MonitorInfo monitor)
    {
        var center = new NativePoint { X = monitor.X + (monitor.Width / 2), Y = monitor.Y + (monitor.Height / 2) };
        nint handle = NativeMethods.MonitorFromPoint(center, NativeMethods.MonitorDefaultToNearest);
        int result = NativeMethods.GetDpiForMonitor(handle, NativeMethods.MdtEffectiveDpi, out uint dpiX, out _);
        return result == 0 && dpiX > 0 ? (int)(dpiX * Percent / BaseDpi) : Percent;
    }
}
