using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Acetato.Application.Abstractions;
using Acetato.Domain;
using Acetato.Presentation.Overlay;
using Acetato.Presentation.ViewModels;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Comportamiento adjunto que mueve lo dibujado (HU-14, SPEC 11) sin lógica en el
/// code-behind, de dos formas:
/// <list type="bullet">
/// <item>Con la herramienta Mover (lienzo en modo selección): apretar sobre un trazo o un
/// texto lo agarra y lo arrastra en el mismo gesto, y al soltar queda seleccionado;
/// dentro de una selección la arrastra entera; en un sitio vacío el lienzo dibuja su lazo.</item>
/// <item>Con cualquier otra herramienta, manteniendo Ctrl: arrastra el dibujo de debajo sin
/// dibujar nada ni dejarlo seleccionado; en un sitio vacío no hace nada.</item>
/// </list>
/// Escucha en la ventana, no en el lienzo: los eventos «Preview» bajan de la ventana al
/// lienzo, así el clic con Ctrl llega aquí antes que al Lápiz, las formas, el láser, el
/// texto o el Borrador, y marcarlo como manejado los deja fuera. El arrastre lo hace
/// <see cref="AnnotationDrag"/> y se registra en el historial de la pane. Bloquea Cortar,
/// Copiar, Pegar y Supr del lienzo, que el historial no controla. Con Mover, un clic en un
/// tirador de la selección cambia su tamaño (HU-28, <see cref="SelectionResizeBehavior"/>).
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

    /// <summary>Estado de Ctrl (enlazado al ViewModel del overlay); sin él, el de WPF.</summary>
    public static readonly DependencyProperty KeyStateProperty =
        DependencyProperty.RegisterAttached(
            "KeyState",
            typeof(IModifierKeyState),
            typeof(MoveToolBehavior),
            new PropertyMetadata(null));

    // Arrastre en curso, mano de Ctrl a la vista y ventana ya escuchada (por InkCanvas).
    private static readonly DependencyProperty SessionProperty =
        DependencyProperty.RegisterAttached("Session", typeof(MoveSession), typeof(MoveToolBehavior), new PropertyMetadata(null));

    private static readonly DependencyProperty HoverProperty =
        DependencyProperty.RegisterAttached("Hover", typeof(bool), typeof(MoveToolBehavior), new PropertyMetadata(false));

    private static readonly DependencyProperty HostProperty =
        DependencyProperty.RegisterAttached("Host", typeof(Window), typeof(MoveToolBehavior), new PropertyMetadata(null));

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

    public static void SetKeyState(DependencyObject element, IModifierKeyState? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(KeyStateProperty, value);
    }

    public static IModifierKeyState? GetKeyState(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (IModifierKeyState?)element.GetValue(KeyStateProperty);
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

    /// <summary>
    /// Lo seleccionado en <paramref name="canvas"/>, que deja de estarlo: Supr lo borra a
    /// continuación (SPEC 11). Vacío si no hay selección.
    /// </summary>
    internal static GrabTargets TakeSelection(InkCanvas canvas)
    {
        var selection = new GrabTargets([.. canvas.GetSelectedStrokes()], [.. canvas.GetSelectedElements()]);
        ClearSelection(canvas);
        return selection;
    }

    private static void OnToolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not InkCanvas canvas)
        {
            return;
        }

        Hook(canvas);
        EndHover(canvas);
        SelectionResizeBehavior.EndHover(canvas);
        if ((ToolKind)e.NewValue != ToolKind.Move)
        {
            ClearSelection(canvas);
        }
    }

    // Escucha la ventana del lienzo una sola vez; si aún no está en una, al cargarse.
    private static void Hook(InkCanvas canvas)
    {
        if (canvas.GetValue(HostProperty) is not null)
        {
            return;
        }

        canvas.Loaded -= OnCanvasLoaded;
        if (Window.GetWindow(canvas) is not { } host)
        {
            canvas.Loaded += OnCanvasLoaded;
            return;
        }

        canvas.SetValue(HostProperty, host);
        host.PreviewMouseLeftButtonDown += (_, e) => OnMouseDown(canvas, e);
        host.PreviewMouseMove += (_, e) => OnMouseMove(canvas, e);
        host.PreviewMouseLeftButtonUp += (_, e) => OnMouseUp(canvas, e);
        host.PreviewKeyDown += (_, e) => OnKeyDown(canvas, e);
        host.PreviewKeyUp += (_, e) => OnKeyUp(canvas, e);
        canvas.MouseLeave += (_, _) => OnMouseLeave(canvas);
        // Si el arrastre se corta (Alt+Tab, Esc, la capa se oculta), se cierra como al soltar.
        canvas.LostMouseCapture += (_, _) => EndGesture(canvas);
    }

    private static void OnCanvasLoaded(object sender, RoutedEventArgs e) => Hook((InkCanvas)sender);

    /// <summary>
    /// Qué hacer con un clic en <paramref name="point"/>. Con Mover: arrastrar lo que hay
    /// debajo (o la selección si cae dentro) y dejarlo seleccionado; en vacío, pasar para
    /// que el lienzo dibuje el lazo. Con otra herramienta y Ctrl: arrastrar lo de debajo
    /// sin seleccionarlo, y en vacío quedarse el clic para que no dibuje. Sin Ctrl, pasar.
    /// </summary>
    internal static GrabDecision Decide(InkCanvas canvas, Point point)
    {
        if (GetTool(canvas) == ToolKind.Move)
        {
            var targets = SelectionUnder(canvas, point) ?? AnnotationHitTest.At(canvas, point);
            return targets.IsEmpty ? GrabDecision.Pass : new GrabDecision(true, targets, KeepSelected: true);
        }

        return WantsCtrlGrab(canvas)
            ? new GrabDecision(true, AnnotationHitTest.At(canvas, point), KeepSelected: false)
            : GrabDecision.Pass;
    }

    private static void OnMouseDown(InkCanvas canvas, MouseButtonEventArgs e)
    {
        var point = e.GetPosition(canvas);
        // Un tirador de la selección cambia su tamaño (HU-28) en vez de moverla.
        if (SelectionResizeBehavior.TryStart(canvas, point))
        {
            e.Handled = true;
            return;
        }

        var decision = Decide(canvas, point);
        if (!decision.Handles)
        {
            return;
        }

        if (!decision.Targets.IsEmpty)
        {
            if (decision.KeepSelected)
            {
                canvas.Select(new StrokeCollection(decision.Targets.Strokes), decision.Targets.Elements);
            }

            StartDrag(canvas, decision.Targets, point, decision.KeepSelected);
        }

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
        Mouse.OverrideCursor = HandCursorFactory.Grab(Scale(canvas));
    }

    private static void OnMouseMove(InkCanvas canvas, MouseEventArgs e)
    {
        if (SelectionResizeBehavior.MoveTo(canvas, e.GetPosition(canvas)))
        {
            e.Handled = true;
            return;
        }

        if (canvas.GetValue(SessionProperty) is MoveSession session)
        {
            var point = e.GetPosition(canvas);
            session.Drag.MoveBy(point - session.Last);
            session.Last = point;
            e.Handled = true;
            return;
        }

        SelectionResizeBehavior.UpdateHover(canvas, e.GetPosition(canvas));
        // Con Ctrl a la vista, el movimiento no llega a las herramientas (p. ej. el láser).
        e.Handled = UpdateHover(canvas);
    }

    private static void OnMouseUp(InkCanvas canvas, MouseButtonEventArgs e) => e.Handled = EndGesture(canvas);

    private static void OnMouseLeave(InkCanvas canvas)
    {
        EndHover(canvas);
        SelectionResizeBehavior.EndHover(canvas);
    }

    // Cierra el gesto en curso, sea un cambio de tamaño o un arrastre. Devuelve si había uno.
    private static bool EndGesture(InkCanvas canvas) => SelectionResizeBehavior.End(canvas) || EndDrag(canvas);

    // Cierra el arrastre: lo registra en el historial y quita la mano cerrada. Devuelve si
    // había uno. La sesión se suelta antes de liberar el mouse, porque liberarlo vuelve a
    // entrar aquí por LostMouseCapture.
    private static bool EndDrag(InkCanvas canvas)
    {
        if (canvas.GetValue(SessionProperty) is not MoveSession session)
        {
            return false;
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

        return true;
    }

    private static void OnKeyDown(InkCanvas canvas, KeyEventArgs e)
    {
        if (IsCtrlKey(e) && canvas.IsMouseOver)
        {
            _ = UpdateHover(canvas);
        }

        if (GetTool(canvas) == ToolKind.Move && IsBlockedGesture(canvas, e))
        {
            e.Handled = true;
        }
    }

    private static void OnKeyUp(InkCanvas canvas, KeyEventArgs e)
    {
        if (IsCtrlKey(e))
        {
            EndHover(canvas);
        }
    }

    // Muestra u oculta la mano de Ctrl según la tecla. Devuelve si la mano está a la vista.
    private static bool UpdateHover(InkCanvas canvas)
    {
        bool wanted = WantsCtrlGrab(canvas);
        bool shown = (bool)canvas.GetValue(HoverProperty);
        if (wanted && !shown)
        {
            canvas.Cursor = HandCursorFactory.Open(Scale(canvas));
            canvas.ForceCursor = true;
            canvas.UseCustomCursor = true;
            canvas.SetValue(HoverProperty, true);
        }
        else if (!wanted && shown)
        {
            EndHover(canvas);
        }

        return wanted;
    }

    // Vuelve el cursor de la herramienta.
    private static void EndHover(InkCanvas canvas)
    {
        if ((bool)canvas.GetValue(HoverProperty))
        {
            canvas.SetValue(HoverProperty, false);
            ToolCursorBehavior.Refresh(canvas);
        }
    }

    // Ctrl agarra con las herramientas de dibujo; con un texto en edición, el clic lo confirma.
    private static bool WantsCtrlGrab(InkCanvas canvas) =>
        GetTool(canvas) != ToolKind.Move
        && (GetKeyState(canvas) ?? WpfModifierKeyState.Instance).IsCtrlDown
        && !TextAnnotationBehavior.IsEditing(canvas);

    private static bool IsCtrlKey(KeyEventArgs e) => e.Key is Key.LeftCtrl or Key.RightCtrl;

    private static bool IsBlockedGesture(InkCanvas canvas, KeyEventArgs e) =>
        BlockedCommands
            .SelectMany(command => command.InputGestures.OfType<KeyGesture>())
            .Any(gesture => gesture.Matches(canvas, e));

    private static double Scale(Visual canvas) => VisualTreeHelper.GetDpi(canvas).DpiScaleX;

    // Estado de un arrastre en curso, con la mano o con Ctrl.
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
