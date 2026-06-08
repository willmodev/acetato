using System.Runtime.InteropServices;
using Acetato.Application.Abstractions;
using Acetato.Domain;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Overlay;

/// <summary>
/// Posiciona una ventana del overlay sobre un monitor en píxeles físicos (HU-09)
/// vía SetWindowPlacement: fija rcNormalPosition al rectángulo del monitor y pide
/// maximizar, de modo que Windows lo haga en ESE monitor y WPF adopte su DPI →
/// cobertura exacta sin desfase, en escalado igual o mixto. El interop vive en
/// Infrastructure.
/// </summary>
public sealed class Win32OverlayWindowPlacer : IOverlayWindowPlacer
{
    public void MaximizeOnMonitor(nint windowHandle, MonitorInfo monitor)
    {
        if (windowHandle == nint.Zero)
        {
            throw new ArgumentException("El manejador de ventana no es válido.", nameof(windowHandle));
        }

        var placement = new WindowPlacementNative
        {
            Length = (uint)Marshal.SizeOf<WindowPlacementNative>(),
            ShowCmd = NativeMethods.SwShowMaximized,
            NormalPosition = new Rect
            {
                Left = monitor.X,
                Top = monitor.Y,
                Right = monitor.X + monitor.Width,
                Bottom = monitor.Y + monitor.Height,
            },
        };

        NativeMethods.SetWindowPlacement(windowHandle, ref placement);
    }
}
