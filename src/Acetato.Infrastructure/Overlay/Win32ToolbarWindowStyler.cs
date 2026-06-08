using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Overlay;

/// <summary>
/// Aplica los estilos extendidos de la barra flotante (HU-10): WS_EX_NOACTIVATE
/// para que no robe el foco a la app de abajo ni al overlay, WS_EX_TOOLWINDOW
/// para quedar fuera del Alt-Tab y WS_EX_LAYERED para componerse por capa. Además
/// la excluye de cualquier captura de pantalla (HU-12) sin ocultarla e intenta el
/// blur acrílico (DWM). El interop vive en Infrastructure.
/// </summary>
public sealed class Win32ToolbarWindowStyler : IToolbarWindowStyler
{
    public void ApplyToolbarStyles(nint windowHandle)
    {
        if (windowHandle == nint.Zero)
        {
            throw new ArgumentException("El manejador de ventana no es válido.", nameof(windowHandle));
        }

        long current = NativeMethods.GetWindowLongPtr(windowHandle, NativeMethods.GwlExStyle);
        long updated = current
            | NativeMethods.WsExNoActivate
            | NativeMethods.WsExToolWindow
            | NativeMethods.WsExLayered;
        NativeMethods.SetWindowLongPtr(windowHandle, NativeMethods.GwlExStyle, (nint)updated);

        // La barra sigue visible en pantalla pero no aparece en las capturas (HU-12).
        // Best-effort: en builds sin soporte simplemente no surte efecto.
        NativeMethods.SetWindowDisplayAffinity(windowHandle, NativeMethods.WdaExcludeFromCapture);

        TryEnableAcrylic(windowHandle);
    }

    // Blur acrílico vía DWM (Windows 11 22H2+). Best-effort: en versiones previas
    // devuelve un HRESULT de error que se ignora y queda el glass de respaldo.
    private static void TryEnableAcrylic(nint windowHandle)
    {
        int backdrop = NativeMethods.DwmsbtTransientWindow;
        _ = NativeMethods.DwmSetWindowAttribute(
            windowHandle, NativeMethods.DwmwaSystemBackdropType, ref backdrop, sizeof(int));
    }
}
