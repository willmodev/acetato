using System.Runtime.InteropServices;

namespace Acetato.Infrastructure.Interop;

/// <summary>
/// Equivalente administrado de la estructura WINDOWPLACEMENT de Win32. Length debe
/// inicializarse al tamaño en bytes antes de llamar a SetWindowPlacement; ShowCmd
/// indica el estado (p.ej. maximizado) y NormalPosition el rectángulo objetivo en
/// coordenadas físicas.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct WindowPlacementNative
{
    internal uint Length;
    internal uint Flags;
    internal uint ShowCmd;
    internal NativePoint MinPosition;
    internal NativePoint MaxPosition;
    internal Rect NormalPosition;
}
