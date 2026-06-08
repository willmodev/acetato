using System.Runtime.InteropServices;

namespace Acetato.Infrastructure.Interop;

/// <summary>
/// Equivalente administrado de la estructura POINT de Win32 (x, y en píxeles
/// físicos). Diseño secuencial para coincidir con la API.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativePoint
{
    internal int X;
    internal int Y;
}
