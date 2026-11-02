using System.Runtime.InteropServices;
using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Overlay;

/// <summary>
/// Lee y fija la posición de la barra en píxeles físicos (HU-29). Mueve con
/// SetWindowPlacement (2 parámetros, porque MoveWindow y SetWindowPos romperían S107) y
/// sin activar la ventana. SetWindowPlacement usa coordenadas del área de trabajo si la
/// ventana aún no es de herramientas (al primer Show todavía no lo es). Por eso se mide la
/// diferencia con GetWindowRect, que siempre da coordenadas de pantalla, y se descuenta.
/// </summary>
public sealed class Win32ToolbarWindowPlacer : IToolbarWindowPlacer
{
    public (int X, int Y, int Width, int Height)? GetBounds(nint windowHandle)
    {
        if (windowHandle == nint.Zero || !NativeMethods.GetWindowRect(windowHandle, out var rect))
        {
            return null;
        }

        return (rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    public void MoveTo(nint windowHandle, int x, int y)
    {
        if (windowHandle == nint.Zero || !NativeMethods.GetWindowRect(windowHandle, out var screen))
        {
            return;
        }

        var placement = new WindowPlacementNative { Length = (uint)Marshal.SizeOf<WindowPlacementNative>() };
        if (!NativeMethods.GetWindowPlacement(windowHandle, ref placement))
        {
            return;
        }

        // Diferencia entre pantalla y área de trabajo (0 si la barra ya es de herramientas).
        int left = x - (screen.Left - placement.NormalPosition.Left);
        int top = y - (screen.Top - placement.NormalPosition.Top);
        placement.Flags = 0;
        placement.ShowCmd = NativeMethods.SwShowNoActivate;
        placement.NormalPosition = new Rect
        {
            Left = left,
            Top = top,
            Right = left + (screen.Right - screen.Left),
            Bottom = top + (screen.Bottom - screen.Top),
        };

        NativeMethods.SetWindowPlacement(windowHandle, ref placement);
    }
}
