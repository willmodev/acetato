using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Acetato.Application.Abstractions;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Comportamiento adjunto que arranca el arrastre nativo de la ventana al pulsar
/// el grip (HU-10), sin lógica en el code-behind. El P/Invoke vive en el
/// <see cref="IWindowDragHandler"/> (Infrastructure); aquí solo se resuelve el
/// manejador de la ventana (WPF) y se delega.
/// </summary>
public static class WindowDragBehavior
{
    /// <summary>Manejador de arrastre a invocar al pulsar el elemento.</summary>
    public static readonly DependencyProperty DragHandlerProperty =
        DependencyProperty.RegisterAttached(
            "DragHandler",
            typeof(IWindowDragHandler),
            typeof(WindowDragBehavior),
            new PropertyMetadata(null, OnDragHandlerChanged));

    public static void SetDragHandler(DependencyObject element, IWindowDragHandler? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(DragHandlerProperty, value);
    }

    public static IWindowDragHandler? GetDragHandler(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (IWindowDragHandler?)element.GetValue(DragHandlerProperty);
    }

    private static void OnDragHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
        {
            return;
        }

        // Re-suscribir es idempotente: evita enganchar el manejador dos veces.
        element.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
        element.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var element = (DependencyObject)sender;
        var handler = GetDragHandler(element);
        var window = Window.GetWindow(element);
        if (handler is null || window is null)
        {
            return;
        }

        nint handle = new WindowInteropHelper(window).Handle;
        if (handle != nint.Zero)
        {
            handler.BeginDrag(handle);
        }
    }
}
