using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Monitors;

/// <summary>
/// Lee la posición del cursor vía Win32 (HU-09) en píxeles físicos del
/// escritorio virtual. El interop vive en Infrastructure.
/// </summary>
public sealed class Win32CursorPositionProvider : ICursorPositionProvider
{
    public (int X, int Y) GetCursorPosition()
    {
        return NativeMethods.GetCursorPos(out NativePoint point)
            ? (point.X, point.Y)
            : (0, 0);
    }
}
