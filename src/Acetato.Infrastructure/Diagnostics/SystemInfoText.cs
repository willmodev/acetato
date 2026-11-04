using System.Globalization;
using System.Text;
using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Monitors;

namespace Acetato.Infrastructure.Diagnostics;

/// <summary>
/// Contenido de sistema.txt del reporte (HU-33): versión completa de Acetato, Windows con
/// su compilación, .NET, idioma y cada monitor con su resolución y escala. Sin rutas ni
/// nombres; la redacción de usuario y equipo se aplica igual al guardar.
/// </summary>
public static class SystemInfoText
{
    /// <summary>Texto con una línea por dato.</summary>
    public static string Build(IAppInfo appInfo, IMonitorService monitors)
    {
        ArgumentNullException.ThrowIfNull(appInfo);
        ArgumentNullException.ThrowIfNull(monitors);
        var text = new StringBuilder();
        var culture = CultureInfo.InvariantCulture;
        text.AppendLine(culture, $"Acetato: {appInfo.Version.Full}");
        text.AppendLine(culture, $"Windows: {SystemDescription.WindowsVersion()}");
        text.AppendLine(culture, $".NET: {SystemDescription.RuntimeVersion}");
        text.AppendLine(culture, $"Idioma: {SystemDescription.Language}");

        var list = monitors.GetMonitors();
        text.AppendLine(culture, $"Monitores: {list.Count}");
        for (int index = 0; index < list.Count; index++)
        {
            var monitor = list[index];
            string primary = monitor.IsPrimary ? ", principal" : string.Empty;
            text.AppendLine(
                culture,
                $"  {index + 1}. {monitor.Width} × {monitor.Height} px, escala {MonitorScale.PercentOf(monitor)} %{primary}");
        }

        return text.ToString();
    }
}
