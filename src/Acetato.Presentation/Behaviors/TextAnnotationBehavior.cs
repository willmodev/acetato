using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Acetato.Domain;
using Acetato.Presentation.ViewModels;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Comportamiento adjunto que anota con texto (HU-13) sobre el InkCanvas sin
/// lógica en el code-behind. Con la herramienta Texto activa, el cursor pasa a
/// I-beam y al hacer clic crea un <see cref="TextBox"/> multilínea en el punto
/// (con un placeholder "Texto" mientras está vacío) y le da foco; al confirmar
/// (clic fuera o Esc) lo reemplaza por un <see cref="TextBlock"/> fijo y lo
/// registra en el historial unificado, salvo que esté vacío (se descarta). Color
/// (tinta activa) y tamaño (derivado del grosor) salen del ViewModel.
/// </summary>
public static class TextAnnotationBehavior
{
    /// <summary>Herramienta activa (enlazada al ViewModel del overlay).</summary>
    public static readonly DependencyProperty ToolProperty =
        DependencyProperty.RegisterAttached(
            "Tool",
            typeof(ToolKind),
            typeof(TextAnnotationBehavior),
            new PropertyMetadata(ToolKind.Pencil, OnToolChanged));

    public static void SetTool(DependencyObject element, ToolKind value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(ToolProperty, value);
    }

    public static ToolKind GetTool(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (ToolKind)element.GetValue(ToolProperty);
    }

    private static void OnToolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not InkCanvas canvas)
        {
            return;
        }

        // Re-suscribir es idempotente: evita enganchar el manejador dos veces.
        canvas.PreviewMouseLeftButtonDown -= OnMouseDown;
        canvas.PreviewMouseLeftButtonDown += OnMouseDown;
        ApplyCursor(canvas, (ToolKind)e.NewValue == ToolKind.Text);
    }

    // I-beam cuando Texto está activa; por defecto en el resto. ForceCursor es
    // necesario porque el InkCanvas fija su propio cursor e ignora Cursor a secas.
    private static void ApplyCursor(InkCanvas canvas, bool isText)
    {
        canvas.Cursor = isText ? Cursors.IBeam : null;
        canvas.ForceCursor = isText;
    }

    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        if (GetTool(canvas) != ToolKind.Text || canvas.DataContext is not OverlayViewModel viewModel)
        {
            return;
        }

        StartEditing(canvas, viewModel, e.GetPosition(canvas));
        e.Handled = true;
    }

    private static void StartEditing(InkCanvas canvas, OverlayViewModel viewModel, Point point)
    {
        var editor = CreateEditor(viewModel);
        var placeholder = CreatePlaceholder(editor);
        AddChild(canvas, placeholder, point);
        AddChild(canvas, editor, point);

        editor.TextChanged += (_, _) =>
            placeholder.Visibility = editor.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        editor.PreviewKeyDown += (_, args) => OnEditorKeyDown(canvas, editor, placeholder, args);
        editor.LostKeyboardFocus += (_, _) => Fix(canvas, editor, placeholder);
        _ = editor.Focus();
    }

    private static TextBox CreateEditor(OverlayViewModel viewModel) => new()
    {
        AcceptsReturn = true, // Enter = salto de línea (multilínea)
        TextWrapping = TextWrapping.NoWrap,
        Background = Brushes.Transparent,
        BorderThickness = new Thickness(0),
        Padding = new Thickness(0),
        MinWidth = 4d,
        Foreground = new SolidColorBrush(viewModel.ActiveInkColor),
        FontSize = viewModel.ActiveFontSize,
        FontFamily = BrandFont(),
        FontWeight = BrandWeight(),
    };

    // Placeholder "Texto" que se auto-dimensiona; tinta tenue del design system
    // (Fg.Disabled). No captura el mouse: el clic y el foco van al TextBox encima.
    private static TextBlock CreatePlaceholder(TextBox editor) => new()
    {
        Text = "Texto",
        FontFamily = editor.FontFamily,
        FontWeight = editor.FontWeight,
        FontSize = editor.FontSize,
        Foreground = (Brush)System.Windows.Application.Current.Resources["Fg.Disabled"],
        IsHitTestVisible = false,
    };

    private static void AddChild(InkCanvas canvas, UIElement child, Point point)
    {
        InkCanvas.SetLeft(child, point.X);
        InkCanvas.SetTop(child, point.Y);
        canvas.Children.Add(child);
    }

    private static void OnEditorKeyDown(InkCanvas canvas, TextBox editor, UIElement placeholder, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Fix(canvas, editor, placeholder);
            e.Handled = true;
        }
    }

    // Fija el texto: descarta si está vacío; si no, lo reemplaza por un TextBlock
    // y lo registra en el historial. El guard evita fijar dos veces (Esc dispara
    // también la pérdida de foco al quitar el editor).
    private static void Fix(InkCanvas canvas, TextBox editor, UIElement placeholder)
    {
        if (!canvas.Children.Contains(editor))
        {
            return;
        }

        canvas.Children.Remove(editor);
        canvas.Children.Remove(placeholder);
        if (string.IsNullOrWhiteSpace(editor.Text))
        {
            return;
        }

        var label = CreateLabel(editor);
        InkCanvas.SetLeft(label, InkCanvas.GetLeft(editor));
        InkCanvas.SetTop(label, InkCanvas.GetTop(editor));
        canvas.Children.Add(label);
        (canvas.DataContext as OverlayViewModel)?.RecordText(canvas, label);
    }

    private static TextBlock CreateLabel(TextBox editor) => new()
    {
        Text = editor.Text,
        Foreground = editor.Foreground,
        FontSize = editor.FontSize,
        FontFamily = editor.FontFamily,
        FontWeight = editor.FontWeight,
    };

    // Tipografía de marca desde el design system (Resources/Tokens.xaml).
    private static FontFamily BrandFont() =>
        (FontFamily)System.Windows.Application.Current.Resources["Font.Ui"];

    private static FontWeight BrandWeight() =>
        (FontWeight)System.Windows.Application.Current.Resources["Annotation.FontWeight"];
}
