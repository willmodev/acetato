using System.Windows;
using System.Windows.Interop;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Limita la zona de la ventana que recibe el mouse al elemento marcado. La barra
/// deja un margen para su sombra y, en una ventana con transparencia, Windows trata
/// como propio cualquier píxel con algo de alfa: la sombra le robaba el puntero al lienzo
/// a medio centímetro de la barra y el cursor del lápiz pasaba a flecha.
/// Fuera del elemento la ventana responde «transparente» a la prueba de impacto
/// (WM_NCHITTEST) y el mouse sigue a la ventana de debajo del mismo hilo: la capa de
/// dibujo. No es P/Invoke: solo un gancho de mensajes de WPF.
/// </summary>
public static class HitAreaBehavior
{
    private const int WmNcHitTest = 0x0084;
    private const int HtTransparent = -1;

    /// <summary>Marca el elemento cuyo rectángulo es la única zona que recibe el mouse.</summary>
    public static readonly DependencyProperty IsHitAreaProperty =
        DependencyProperty.RegisterAttached(
            "IsHitArea",
            typeof(bool),
            typeof(HitAreaBehavior),
            new PropertyMetadata(false, OnIsHitAreaChanged));

    // El gancho se guarda en el elemento: HwndSource no lo mantiene vivo por sí solo.
    private static readonly DependencyProperty HookProperty =
        DependencyProperty.RegisterAttached("Hook", typeof(HwndSourceHook), typeof(HitAreaBehavior));

    public static void SetIsHitArea(DependencyObject element, bool value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(IsHitAreaProperty, value);
    }

    public static bool GetIsHitArea(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (bool)element.GetValue(IsHitAreaProperty);
    }

    /// <summary>Punto de pantalla (píxeles físicos, con signo) que trae el lParam del mensaje.</summary>
    internal static Point ScreenPoint(nint lParam)
    {
        int packed = unchecked((int)(long)lParam);
        return new Point((short)(packed & 0xFFFF), (short)((packed >> 16) & 0xFFFF));
    }

    private static void OnIsHitAreaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element || !(bool)e.NewValue)
        {
            return;
        }

        element.Loaded -= OnLoaded;
        element.Loaded += OnLoaded;
        if (element.IsLoaded)
        {
            Attach(element);
        }
    }

    private static void OnLoaded(object sender, RoutedEventArgs e) => Attach((FrameworkElement)sender);

    private static void Attach(FrameworkElement element)
    {
        if (element.GetValue(HookProperty) is not null
            || PresentationSource.FromVisual(element) is not HwndSource source)
        {
            return;
        }

        HwndSourceHook hook = (nint _, int msg, nint _, nint lParam, ref bool handled) =>
            FilterHitTest(element, msg, lParam, ref handled);
        element.SetValue(HookProperty, hook);
        source.AddHook(hook);
    }

    private static nint FilterHitTest(FrameworkElement element, int msg, nint lParam, ref bool handled)
    {
        if (msg != WmNcHitTest || !GetIsHitArea(element) || IsInside(element, ScreenPoint(lParam)))
        {
            return nint.Zero;
        }

        handled = true;
        return HtTransparent;
    }

    private static bool IsInside(FrameworkElement element, Point screen)
    {
        // Sin fuente de presentación no se puede convertir: mejor dejar que reciba.
        if (PresentationSource.FromVisual(element) is null)
        {
            return true;
        }

        return new Rect(element.RenderSize).Contains(element.PointFromScreen(screen));
    }
}
