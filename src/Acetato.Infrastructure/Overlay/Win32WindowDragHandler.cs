using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Overlay;

/// <summary>
/// Arrastra una ventana sin bordes (HU-10) delegando en el gestor de ventanas
/// del sistema: suelta la captura del mouse y envía WM_NCLBUTTONDOWN con
/// HTCAPTION, como si se pulsara su barra de título. No activa la ventana, así
/// que la barra no roba el foco. El interop vive en Infrastructure.
/// </summary>
public sealed class Win32WindowDragHandler : IWindowDragHandler
{
    public void BeginDrag(nint windowHandle)
    {
        if (windowHandle == nint.Zero)
        {
            throw new ArgumentException("El manejador de ventana no es válido.", nameof(windowHandle));
        }

        _ = NativeMethods.ReleaseCapture();
        _ = NativeMethods.SendMessage(windowHandle, NativeMethods.WmNcLButtonDown, NativeMethods.HtCaption, nint.Zero);
    }
}
