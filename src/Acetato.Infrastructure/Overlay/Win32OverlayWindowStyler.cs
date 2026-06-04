using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Overlay;

/// <summary>
/// Aplica los estilos extendidos del overlay (HU-01): WS_EX_LAYERED para la
/// composición por capa y WS_EX_TOOLWINDOW para mantener la ventana fuera del
/// Alt-Tab. El interop vive en Infrastructure.
/// </summary>
public sealed class Win32OverlayWindowStyler : IOverlayWindowStyler
{
    public void ApplyOverlayStyles(nint windowHandle)
    {
        if (windowHandle == nint.Zero)
        {
            throw new ArgumentException("El manejador de ventana no es válido.", nameof(windowHandle));
        }

        long current = NativeMethods.GetWindowLongPtr(windowHandle, NativeMethods.GwlExStyle);
        long updated = current | NativeMethods.WsExLayered | NativeMethods.WsExToolWindow;
        NativeMethods.SetWindowLongPtr(windowHandle, NativeMethods.GwlExStyle, (nint)updated);
    }

    public void SetClickThrough(nint windowHandle, bool enabled)
    {
        if (windowHandle == nint.Zero)
        {
            throw new ArgumentException("El manejador de ventana no es válido.", nameof(windowHandle));
        }

        long current = NativeMethods.GetWindowLongPtr(windowHandle, NativeMethods.GwlExStyle);
        long updated = enabled
            ? current | NativeMethods.WsExTransparent
            : current & ~NativeMethods.WsExTransparent;
        NativeMethods.SetWindowLongPtr(windowHandle, NativeMethods.GwlExStyle, (nint)updated);
    }
}
