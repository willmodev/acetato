using System.Runtime.InteropServices;

namespace Acetato.Infrastructure.Interop;

/// <summary>
/// Equivalente administrado de la estructura MSG de Win32. El diseño secuencial
/// debe coincidir con MSG para que el bucle de mensajes lea bien message/wParam.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeMessage
{
    internal nint Handle;
    internal uint Message;
    internal nint WParam;
    internal nint LParam;
    internal uint Time;
    internal int PointX;
    internal int PointY;
}
