namespace Acetato.Domain;

/// <summary>
/// Elige el monitor que contiene un punto (p.ej. el cursor) para decidir el
/// monitor "activo" en multi-monitor (HU-09): dónde colocar la barra o a qué
/// lienzo dirigir una acción. Tipo puro (sin WPF ni Win32), testeable.
/// </summary>
public static class MonitorPicker
{
    /// <summary>
    /// Índice del monitor que contiene el punto (x, y) en píxeles físicos. Si
    /// ningún monitor lo contiene (huecos entre pantallas o fuera de todas),
    /// devuelve el índice del primario; 0 si no hay primario marcado.
    /// </summary>
    public static int IndexAt(IReadOnlyList<MonitorInfo> monitors, int x, int y)
    {
        ArgumentNullException.ThrowIfNull(monitors);

        for (int i = 0; i < monitors.Count; i++)
        {
            if (Contains(monitors[i], x, y))
            {
                return i;
            }
        }

        return PrimaryIndex(monitors);
    }

    // Límites izquierdo/superior inclusivos, derecho/inferior exclusivos: así el
    // borde compartido entre dos monitores pertenece al de la derecha/abajo.
    private static bool Contains(MonitorInfo monitor, int x, int y) =>
        x >= monitor.X && x < monitor.X + monitor.Width &&
        y >= monitor.Y && y < monitor.Y + monitor.Height;

    private static int PrimaryIndex(IReadOnlyList<MonitorInfo> monitors)
    {
        for (int i = 0; i < monitors.Count; i++)
        {
            if (monitors[i].IsPrimary)
            {
                return i;
            }
        }

        return 0;
    }
}
