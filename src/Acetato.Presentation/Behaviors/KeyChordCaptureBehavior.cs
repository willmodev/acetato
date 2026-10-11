using System.Windows;
using System.Windows.Input;
using Acetato.Domain;
using Acetato.Presentation.Settings;
using Acetato.Presentation.ViewModels;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Comportamiento adjunto de la ventana Ajustes (HU-21): con una fila
/// capturando, intercepta <c>PreviewKeyDown</c>, traduce la tecla y los
/// modificadores a <see cref="KeyChord"/> e invoca el comando de la fila. Las
/// teclas modificadoras solas no terminan la captura y <c>Esc</c> la cancela.
/// Marca la tecla como manejada para que no actúe en la ventana (Alt+F4 no la
/// cierra, Espacio no pulsa el botón). Si la ventana pierde el foco, la captura
/// se cancela para no dejar los atajos en pausa. Sin lógica en el code-behind.
/// </summary>
internal static class KeyChordCaptureBehavior
{
    /// <summary>Fila que recibe la combinación; <c>null</c> = sin captura.</summary>
    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.RegisterAttached(
            "Target",
            typeof(HotkeyRowViewModel),
            typeof(KeyChordCaptureBehavior),
            new PropertyMetadata(null, OnTargetChanged));

    public static void SetTarget(DependencyObject element, HotkeyRowViewModel? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(TargetProperty, value);
    }

    public static HotkeyRowViewModel? GetTarget(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (HotkeyRowViewModel?)element.GetValue(TargetProperty);
    }

    private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window)
        {
            return;
        }

        // Re-suscribir es idempotente: evita enganchar los manejadores dos veces.
        window.PreviewKeyDown -= OnPreviewKeyDown;
        window.PreviewKeyDown += OnPreviewKeyDown;
        window.Deactivated -= OnDeactivated;
        window.Deactivated += OnDeactivated;
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (GetTarget((DependencyObject)sender) is not { } row)
        {
            return;
        }

        e.Handled = true;
        var key = RealKey(e);
        if (key == Key.Escape)
        {
            row.CancelCaptureCommand.Execute(null);
        }
        else if (!WpfKeyMap.IsModifier(key))
        {
            row.CaptureCommand.Execute(ToChord(key));
        }
    }

    private static void OnDeactivated(object? sender, EventArgs e)
    {
        if (sender is DependencyObject window && GetTarget(window) is { } row)
        {
            row.CancelCaptureCommand.Execute(null);
        }
    }

    // Con Alt pulsado WPF entrega Key.System y la tecla real en SystemKey; las
    // teclas muertas e IME también llegan envueltas.
    private static Key RealKey(KeyEventArgs e) => e.Key switch
    {
        Key.System => e.SystemKey,
        Key.ImeProcessed => e.ImeProcessedKey,
        Key.DeadCharProcessed => e.DeadCharProcessedKey,
        _ => e.Key,
    };

    private static KeyChord? ToChord(Key key) =>
        WpfKeyMap.ToHotkeyKey(key) is { } hotkey
            ? new KeyChord(WpfKeyMap.ToModifiers(Keyboard.Modifiers), hotkey)
            : null;
}
