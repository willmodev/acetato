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

    public ToolbarWindowSurface(
        ToolbarWindow window,
        IMonitorService monitors,
        ICursorPositionProvider cursor)
    {
        _window = window;
        _monitors = monitors;
        _cursor = cursor;
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
        // ActualWidth ya está disponible tras Show (SizeToContent): centrar en X.
        _window.Left = left + ((width - _window.ActualWidth) / 2d);
    }

    public void Hide() => _window.Hide();

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
