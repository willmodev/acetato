using System.Windows;
using System.Windows.Interop;
using Acetato.Application.Abstractions;
using Acetato.Domain;
using Acetato.Presentation.Views;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Adaptador WPF de <see cref="IToolbarSurface"/>: envuelve la
/// <see cref="ToolbarWindow"/>. Al mostrarse la sitúa en el monitor bajo el
/// cursor (HU-09), centrada en horizontal y arriba, para que aparezca donde el
/// usuario está trabajando. El usuario puede reubicarla arrastrando el grip.
/// </summary>
public sealed class ToolbarWindowSurface : IToolbarSurface
{
    // Fracción de la altura del monitor a la que cae el borde superior de la barra.
    private const double TopFraction = 0.06;

    private readonly ToolbarWindow _window;
    private readonly IMonitorService _monitors;
    private readonly ICursorPositionProvider _cursor;
    private readonly IOverlayCanvasProvider _canvases;

    public ToolbarWindowSurface(
        ToolbarWindow window,
        IMonitorService monitors,
        ICursorPositionProvider cursor,
        IOverlayCanvasProvider canvases)
    {
        _window = window;
        _monitors = monitors;
        _cursor = cursor;
        _canvases = canvases;
    }

    public bool IsVisible => _window.IsVisible;

    public nint Handle => new WindowInteropHelper(_window).Handle;

    public void Show()
    {
        var (monitor, scale) = ResolveActiveMonitor();
        double left = DipConverter.ToDip(monitor.X, scale);
        double width = DipConverter.ToDip(monitor.Width, scale);
        double top = DipConverter.ToDip(monitor.Y, scale)
            + (DipConverter.ToDip(monitor.Height, scale) * TopFraction);

        _window.WindowStartupLocation = WindowStartupLocation.Manual;
        _window.Left = left;
        _window.Top = top;
        _window.Show();
        // Hacer la barra "owned" de la pane de este monitor DESPUÉS de Show: WPF, al
        // mostrar, reescribe el owner nativo según su propiedad Window.Owner (null) y
        // borraría el que se fije antes. Window.Owner de WPF no admite cambios tras
        // Show, así que se usa el owner NATIVO aquí. Una ventana owner nunca puede
        // tapar a su owned → la barra queda siempre clicable por encima del lienzo.
        OwnToPaneOf(monitor);
        // ActualWidth ya está disponible tras Show (SizeToContent): centrar en X.
        _window.Left = left + ((width - _window.ActualWidth) / 2d);
    }

    public void Hide()
    {
        // Soltar el owner antes de ocultar: si cambia la topología, las panes se
        // recrean (cierran su ventana) y al destruir una ventana owner Windows
        // destruiría su owned (la barra). Sin owner en ese momento, queda a salvo.
        new WindowInteropHelper(_window).Owner = nint.Zero;
        _window.Hide();
    }

    // Fija el owner NATIVO (HWND) de la barra a la pane del monitor dado (ya mostrada,
    // su HWND existe). Owner nativo y no Window.Owner: WPF no deja cambiar Window.Owner
    // tras Show, y la barra se muestra/oculta repetidamente.
    private void OwnToPaneOf(MonitorInfo monitor)
    {
        new WindowInteropHelper(_window).Owner = FindPaneHandle(monitor);
    }

    private nint FindPaneHandle(MonitorInfo monitor)
    {
        var pane = _canvases.Canvases.FirstOrDefault(c => c.Monitor.Equals(monitor));
        return pane?.Handle ?? nint.Zero;
    }

    // Monitor que contiene el cursor y la escala del primario para sembrar la
    // posición antes de que WPF asigne el DPI por-monitor.
    private (MonitorInfo Monitor, double Scale) ResolveActiveMonitor()
    {
        var monitors = _monitors.GetMonitors();
        var (x, y) = _cursor.GetCursorPosition();
        var monitor = monitors[MonitorPicker.IndexAt(monitors, x, y)];
        return (monitor, DisplayMetrics.PrimaryScale(monitors));
    }
}
