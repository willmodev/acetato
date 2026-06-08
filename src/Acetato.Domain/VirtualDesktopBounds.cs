using System.Runtime.InteropServices;

namespace Acetato.Domain;

/// <summary>
/// Rectángulo que abarca todos los monitores (el "escritorio virtual"), en
/// píxeles físicos (HU-09). Sirve para capturar la pantalla completa en
/// multi-monitor. El origen (X, Y) puede ser negativo si hay monitores a la
/// izquierda o arriba del primario. Tipo puro (sin WPF ni Win32), testeable.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct VirtualDesktopBounds(int X, int Y, int Width, int Height)
{
    /// <summary>
    /// Unión de los bounds de todos los monitores. Devuelve un rectángulo vacío
    /// si la lista está vacía.
    /// </summary>
    public static VirtualDesktopBounds Union(IReadOnlyList<MonitorInfo> monitors)
    {
        ArgumentNullException.ThrowIfNull(monitors);

        if (monitors.Count == 0)
        {
            return default;
        }

        int left = int.MaxValue;
        int top = int.MaxValue;
        int right = int.MinValue;
        int bottom = int.MinValue;

        foreach (var monitor in monitors)
        {
            left = Math.Min(left, monitor.X);
            top = Math.Min(top, monitor.Y);
            right = Math.Max(right, monitor.X + monitor.Width);
            bottom = Math.Max(bottom, monitor.Y + monitor.Height);
        }

        return new VirtualDesktopBounds(left, top, right - left, bottom - top);
    }
}
