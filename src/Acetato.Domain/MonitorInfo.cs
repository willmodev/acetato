using System.Runtime.InteropServices;

namespace Acetato.Domain;

/// <summary>
/// Geometría de un monitor en píxeles físicos, en coordenadas del escritorio
/// virtual (HU-09). X/Y pueden ser negativos si el monitor está a la izquierda o
/// arriba del primario. Tipo puro (sin WPF ni Win32), testeable.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct MonitorInfo(int X, int Y, int Width, int Height, bool IsPrimary);
