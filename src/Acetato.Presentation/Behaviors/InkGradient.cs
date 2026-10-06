using System.Windows;
using System.Windows.Input;
using Acetato.Domain;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Propiedades adjuntas del modo Degradado (HU-19), enlazadas al ViewModel del overlay,
/// que comparten los behaviors que crean trazos (formas y trazo libre) sin duplicarlas:
/// el par que tomará el próximo trazo (<c>null</c> en modo sólido) y el comando con el
/// que avisan de que ya fijaron un trazo, para sortear el par siguiente.
/// </summary>
public static class InkGradient
{
    /// <summary>Par de degradado del próximo trazo; <c>null</c> = tinta sólida.</summary>
    public static readonly DependencyProperty PairProperty =
        DependencyProperty.RegisterAttached(
            "Pair",
            typeof(TintaGradiente?),
            typeof(InkGradient),
            new PropertyMetadata(null));

    /// <summary>Comando que se ejecuta al fijar un trazo en degradado.</summary>
    public static readonly DependencyProperty UsedCommandProperty =
        DependencyProperty.RegisterAttached(
            "UsedCommand",
            typeof(ICommand),
            typeof(InkGradient),
            new PropertyMetadata(null));

    public static void SetPair(DependencyObject element, TintaGradiente? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(PairProperty, value);
    }

    public static TintaGradiente? GetPair(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (TintaGradiente?)element.GetValue(PairProperty);
    }

    public static void SetUsedCommand(DependencyObject element, ICommand? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(UsedCommandProperty, value);
    }

    public static ICommand? GetUsedCommand(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (ICommand?)element.GetValue(UsedCommandProperty);
    }

    /// <summary>Avisa de que se fijó un trazo en degradado: el par siguiente se sortea.</summary>
    internal static void NotifyUsed(DependencyObject element)
    {
        var command = GetUsedCommand(element);
        if (command?.CanExecute(null) == true)
        {
            command.Execute(null);
        }
    }
}
