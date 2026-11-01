using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Acetato.Domain;
using Acetato.Presentation.Overlay;
using Acetato.Presentation.ViewModels;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Comportamiento adjunto de la herramienta Mover (HU-14, SPEC 11) sin lógica en el
/// code-behind. Con Mover activa el lienzo está en modo selección: apretar sobre un
/// trazo o un texto lo agarra y lo arrastra en el mismo gesto, y al soltar queda
/// seleccionado; apretar dentro de una selección la arrastra entera; apretar en un
/// sitio vacío deja que el lienzo dibuje su lazo. El arrastre lo hace
/// <see cref="AnnotationDrag"/> y se registra en el historial de la pane, así Deshacer
/// lo revierte. Bloquea Cortar, Copiar, Pegar y Supr del lienzo, que el historial no
/// controla: borrar va por el atajo fijo Supr (SPEC 11).
/// </summary>
public static class MoveToolBehavior
{
    /// <summary>Herramienta activa (enlazada al ViewModel del overlay).</summary>
    public static readonly DependencyProperty ToolProperty =
        DependencyProperty.RegisterAttached(
            "Tool",
            typeof(ToolKind),
            typeof(MoveToolBehavior),
            new PropertyMetadata(ToolKind.Move, OnToolChanged));

    // Arrastre en curso (por InkCanvas); no es bindable.
    private static readonly DependencyProperty SessionProperty =
        DependencyProperty.RegisterAttached(
            "Session",
            typeof(MoveSession),
            typeof(MoveToolBehavior),
            new PropertyMetadata(null));

    // Gestos del lienzo que cambiarían trazos a espaldas del historial.
    private static readonly RoutedUICommand[] BlockedCommands =
    [
        ApplicationCommands.Cut, ApplicationCommands.Copy, ApplicationCommands.Paste, ApplicationCommands.Delete,
    ];

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

    /// <summary>
    /// Quita la selección de <paramref name="canvas"/>: al cambiar de herramienta, al pasar
    /// a Puntero y al ocultar la capa (SPEC 11). Conserva el modo de edición enlazado.
    /// </summary>
    public static void ClearSelection(InkCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        if (canvas.GetSelectedStrokes().Count == 0 && canvas.GetSelectedElements().Count == 0)
        {
            return;
        }

        var mode = canvas.EditingMode;
        canvas.Select(new StrokeCollection());
        canvas.SetCurrentValue(InkCanvas.EditingModeProperty, mode);
    }

    private static void OnToolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not InkCanvas canvas)
        {
            return;
        }

        // Re-suscribir es idempotente: evita enganchar los manejadores dos veces.
        canvas.PreviewMouseLeftButtonDown -= OnMouseDown;
        canvas.PreviewMouseLeftButtonDown += OnMouseDown;
        canvas.PreviewMouseMove -= OnMouseMove;
        canvas.PreviewMouseMove += OnMouseMove;
        canvas.PreviewMouseLeftButtonUp -= OnMouseUp;
        canvas.PreviewMouseLeftButtonUp += OnMouseUp;
        canvas.PreviewKeyDown -= OnKeyDown;
        canvas.PreviewKeyDown += OnKeyDown;

        if ((ToolKind)e.NewValue != ToolKind.Move)
        {
            ClearSelection(canvas);
        }
    }

    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        if (GetTool(canvas) != ToolKind.Move)
        {
            return;
        }

        var point = e.GetPosition(canvas);
        var targets = SelectionUnder(canvas, point) ?? AnnotationHitTest.At(canvas, point);
        if (targets.IsEmpty)
        {
            return; // sitio vacío: el lienzo dibuja el lazo
        }

        canvas.Select(new StrokeCollection(targets.Strokes), targets.Elements);
        StartDrag(canvas, targets, point, keepSelected: true);
        e.Handled = true;
    }

    // La selección actual si el punto cae dentro de su recuadro; null si no.
    private static GrabTargets? SelectionUnder(InkCanvas canvas, Point point)
    {
        var bounds = canvas.GetSelectionBounds();
        if (bounds.IsEmpty || !bounds.Contains(point))
        {
            return null;
        }

        return new GrabTargets([.. canvas.GetSelectedStrokes()], [.. canvas.GetSelectedElements()]);
    }

    private static void StartDrag(InkCanvas canvas, GrabTargets targets, Point start, bool keepSelected)
    {
        var drag = new AnnotationDrag(targets.Strokes, targets.Elements);
        canvas.SetValue(SessionProperty, new MoveSession(drag, targets, start, keepSelected));
        _ = canvas.CaptureMouse();
        Mouse.OverrideCursor = HandCursorFactory.Grab(VisualTreeHelper.GetDpi(canvas).DpiScaleX);
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        if (canvas.GetValue(SessionProperty) is not MoveSession session)
        {
            return;
        }

        var point = e.GetPosition(canvas);
        session.Drag.MoveBy(point - session.Last);
        session.Last = point;
        e.Handled = true;
    }

    private static void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        if (canvas.GetValue(SessionProperty) is not MoveSession session)
        {
            return;
        }

        canvas.SetValue(SessionProperty, null);
        canvas.ReleaseMouseCapture();
        Mouse.OverrideCursor = null;
        if (session.Drag.Finish() is { } moved)
        {
            (canvas.DataContext as OverlayViewModel)?.RecordMove(moved);
        }

        if (session.KeepSelected)
        {
            // Re-seleccionar recoloca el recuadro de la selección sobre lo movido.
            canvas.Select(new StrokeCollection(session.Targets.Strokes), session.Targets.Elements);
        }

        e.Handled = true;
    }

    private static void OnKeyDown(object sender, KeyEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        if (GetTool(canvas) == ToolKind.Move && IsBlockedGesture(canvas, e))
        {
            e.Handled = true;
        }
    }

    private static bool IsBlockedGesture(InkCanvas canvas, KeyEventArgs e) =>
        BlockedCommands
            .SelectMany(command => command.InputGestures.OfType<KeyGesture>())
            .Any(gesture => gesture.Matches(canvas, e));

    // Estado de un arrastre de la mano en curso.
    private sealed class MoveSession
    {
        public MoveSession(AnnotationDrag drag, GrabTargets targets, Point start, bool keepSelected)
        {
            Drag = drag;
            Targets = targets;
            Last = start;
            KeepSelected = keepSelected;
        }

        public AnnotationDrag Drag { get; }

        public GrabTargets Targets { get; }

        public bool KeepSelected { get; }

        public Point Last { get; set; }
    }
}
