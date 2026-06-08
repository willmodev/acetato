using System.Windows;
using Acetato.Domain;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Utilidades de escala de pantalla para sembrar posiciones de ventana antes de
/// que WPF asigne el DPI por-monitor (HU-09). Compartido por el overlay y la
/// barra para no duplicar el cálculo.
/// </summary>
internal static class DisplayMetrics
{
    /// <summary>
    /// Factor de escala del monitor primario (físico / DIP). SystemParameters da
    /// el ancho del primario en DIP; con su ancho físico se obtiene la escala.
    /// Devuelve 1 si no hay datos válidos.
    /// </summary>
    public static double PrimaryScale(IReadOnlyList<MonitorInfo> monitors)
    {
        ArgumentNullException.ThrowIfNull(monitors);

        var primary = monitors.FirstOrDefault(m => m.IsPrimary);
        double dipWidth = SystemParameters.PrimaryScreenWidth;
        if (primary.Width <= 0 || dipWidth <= 0)
        {
            return 1d;
        }

        return primary.Width / dipWidth;
    }
}
