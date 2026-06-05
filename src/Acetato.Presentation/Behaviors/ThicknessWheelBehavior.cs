using System.Windows;
using System.Windows.Input;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Comportamiento adjunto que traduce la rueda del mouse a comandos de grosor
/// (HU-06): rueda arriba aumenta, rueda abajo disminuye. Permite controlar el
/// grosor con la rueda sin lógica en el code-behind (MVVM estricto).
/// </summary>
public static class ThicknessWheelBehavior
{
    /// <summary>Comando ejecutado al girar la rueda hacia arriba.</summary>
    public static readonly DependencyProperty IncreaseCommandProperty =
        DependencyProperty.RegisterAttached(
            "IncreaseCommand",
            typeof(ICommand),
            typeof(ThicknessWheelBehavior),
            new PropertyMetadata(null, OnCommandsChanged));

    /// <summary>Comando ejecutado al girar la rueda hacia abajo.</summary>
    public static readonly DependencyProperty DecreaseCommandProperty =
        DependencyProperty.RegisterAttached(
            "DecreaseCommand",
            typeof(ICommand),
            typeof(ThicknessWheelBehavior),
            new PropertyMetadata(null, OnCommandsChanged));

    public static void SetIncreaseCommand(DependencyObject element, ICommand? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(IncreaseCommandProperty, value);
    }

    public static ICommand? GetIncreaseCommand(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (ICommand?)element.GetValue(IncreaseCommandProperty);
    }

    public static void SetDecreaseCommand(DependencyObject element, ICommand? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(DecreaseCommandProperty, value);
    }

    public static ICommand? GetDecreaseCommand(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (ICommand?)element.GetValue(DecreaseCommandProperty);
    }

    private static void OnCommandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
        {
            return;
        }

        // Re-suscribir es idempotente: evita enganchar el manejador dos veces
        // cuando se fijan ambos comandos.
        element.PreviewMouseWheel -= OnMouseWheel;
        element.PreviewMouseWheel += OnMouseWheel;
    }

    private static void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var element = (DependencyObject)sender;
        var command = e.Delta > 0 ? GetIncreaseCommand(element) : GetDecreaseCommand(element);
        if (command?.CanExecute(null) == true)
        {
            command.Execute(null);
            e.Handled = true;
        }
    }
}
