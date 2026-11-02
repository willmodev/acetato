using System.Runtime.InteropServices;

namespace Acetato.Domain;

/// <summary>
/// Dónde dejó el usuario la barra (HU-29): la esquina de su ventana en píxeles físicos
/// del escritorio virtual y el monitor en el que estaba. En píxeles físicos y no en DIP
/// porque, con monitores de distinta escala, una posición en DIP caería en otro sitio.
/// Tipo puro (sin WPF ni Win32), testeable.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct ToolbarPlacement(MonitorInfo Monitor, int X, int Y)
{
    /// <summary>
    /// El monitor de la barra entre los conectados, si sigue con la misma posición y
    /// tamaño; <c>null</c> si no, y entonces la posición guardada ya no sirve. Ser o no el
    /// primario no cuenta.
    /// </summary>
    public MonitorInfo? FindHome(IReadOnlyList<MonitorInfo> monitors)
    {
        ArgumentNullException.ThrowIfNull(monitors);
        var home = Monitor;
        foreach (var monitor in monitors)
        {
            if (monitor.X == home.X && monitor.Y == home.Y && monitor.Width == home.Width && monitor.Height == home.Height)
            {
                return monitor;
            }
        }

        return null;
    }
}
