using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using Acetato.Domain;
using Acetato.Presentation.Overlay;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Comportamiento adjunto del trazo a mano alzada (HU-19/HU-20) sin lógica en el
/// code-behind. Lápiz y flecha no usan el trazado nativo del InkCanvas: este behavior
/// captura el mouse, acumula los puntos en una <see cref="FreehandSession"/> y pinta el
/// trazo en curso en la <see cref="LiveStrokeLayer"/> con su aspecto final (degradado y
/// punta de flecha incluidos). Al soltar —o si se pierde el mouse a mitad del trazo— el
/// trazo entra a la colección del lienzo UNA vez: una sola entrada en el historial. Una
/// flecha más corta que su punta no deja nada.
/// </summary>
public static class FreehandStrokeBehavior
{
    /// <summary>
    /// Herramienta activa (enlazada al ViewModel del overlay). El valor por defecto es
    /// Select, que no traza: así el primer enlace con Lápiz (la herramienta inicial) SÍ
    /// dispara el cambio y engancha los manejadores.
    /// </summary>
    public static readonly DependencyProperty ToolProperty =
        DependencyProperty.RegisterAttached(
            "Tool",
            typeof(ToolKind),
            typeof(FreehandStrokeBehavior),
            new PropertyMetadata(ToolKind.Select, OnToolChanged));

    /// <summary>Capa donde se pinta el trazo en curso (encima del InkCanvas).</summary>
    public static readonly DependencyProperty LayerProperty =
        DependencyProperty.RegisterAttached(
            "Layer",
            typeof(LiveStrokeLayer),
            typeof(FreehandStrokeBehavior),
            new PropertyMetadata(null));

    // Trazo en curso (por InkCanvas); no es bindable.
    private static readonly DependencyProperty SessionProperty =
        DependencyProperty.RegisterAttached(
            "Session",
            typeof(FreehandSession),
            typeof(FreehandStrokeBehavior),
            new PropertyMetadata(null));

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

    public static void SetLayer(DependencyObject element, LiveStrokeLayer? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(LayerProperty, value);
    }

    public static LiveStrokeLayer? GetLayer(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (LiveStrokeLayer?)element.GetValue(LayerProperty);
    }

    private static void OnToolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not InkCanvas canvas)
        {
            return;
        }

        EnsureHandlers(canvas);
        Commit(canvas); // cambiar de herramienta a mitad del trazo fija lo dibujado
    }

    // Re-suscribir es idempotente: evita enganchar los manejadores dos veces.
    private static void EnsureHandlers(InkCanvas canvas)
    {
        canvas.PreviewMouseLeftButtonDown -= OnMouseDown;
        canvas.PreviewMouseLeftButtonDown += OnMouseDown;
        canvas.PreviewMouseMove -= OnMouseMove;
        canvas.PreviewMouseMove += OnMouseMove;
        canvas.PreviewMouseLeftButtonUp -= OnMouseUp;
        canvas.PreviewMouseLeftButtonUp += OnMouseUp;
        canvas.LostMouseCapture -= OnLostMouseCapture;
        canvas.LostMouseCapture += OnLostMouseCapture;
        canvas.IsVisibleChanged -= OnVisibleChanged;
        canvas.IsVisibleChanged += OnVisibleChanged;
    }

    // Herramientas que se trazan con captura propia y pintado en vivo.
    private static bool IsLiveTool(ToolKind tool) => tool is ToolKind.Pencil or ToolKind.Arrow;

    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        var tool = GetTool(canvas);
        if (!IsLiveTool(tool) || GetLayer(canvas) is not { } layer)
        {
            return;
        }

        var session = new FreehandSession(tool, canvas.DefaultDrawingAttributes, InkGradient.GetPair(canvas));
        _ = session.Add(ToStrokePoint(e.GetPosition(canvas)));
        canvas.SetValue(SessionProperty, session);
        layer.Show(session.BuildPreview);
        _ = canvas.CaptureMouse();
        e.Handled = true;
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        if (GetSession(canvas) is { } session && session.Add(ToStrokePoint(e.GetPosition(canvas))))
        {
            GetLayer(canvas)?.Refresh();
        }
    }

    private static void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        if (GetSession(canvas) is not null)
        {
            Commit(canvas);
            e.Handled = true;
        }
    }

    // Se perdió el mouse a mitad del trazo (click-through, otra ventana): fijar lo dibujado.
    private static void OnLostMouseCapture(object sender, MouseEventArgs e) => Commit((InkCanvas)sender);

    private static void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is InkCanvas canvas && !canvas.IsVisible)
        {
            Commit(canvas);
        }
    }

    // Cierra la sesión: vacía la capa en vivo y fija el trazo final en el lienzo. La
    // sesión se quita ANTES de soltar la captura para que LostMouseCapture no repita.
    private static void Commit(InkCanvas canvas)
    {
        if (GetSession(canvas) is not { } session)
        {
            return;
        }

        canvas.SetValue(SessionProperty, null);
        if (canvas.IsMouseCaptured)
        {
            canvas.ReleaseMouseCapture();
        }

        if (GetLayer(canvas) is { } layer)
        {
            layer.Clear();
            layer.Stats.Report(session.PointCount);
        }

        if (session.BuildFinal() is not { } stroke)
        {
            return;
        }

        canvas.Strokes.Add(stroke);
        if (session.IsGradient)
        {
            InkGradient.NotifyUsed(canvas); // sortea el par del siguiente trazo
        }
    }

    private static FreehandSession? GetSession(InkCanvas canvas) =>
        (FreehandSession?)canvas.GetValue(SessionProperty);

    private static StrokePoint ToStrokePoint(Point point) => new(point.X, point.Y);
}
