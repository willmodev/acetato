using System.Runtime.InteropServices;

namespace Acetato.Infrastructure.Interop;

/// <summary>
/// Equivalente administrado de la estructura RECT de Win32 (bordes en píxeles
/// físicos). Diseño secuencial para coincidir con la API.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct Rect
{
    internal int Left;
    internal int Top;
    internal int Right;
    internal int Bottom;
}
