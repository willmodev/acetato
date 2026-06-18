using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Acetato.Domain;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Comportamiento adjunto del puntero láser (HU-15), sin lógica en el code-behind.
/// Con la herramienta <see cref="ToolKind.Laser"/> activa, dibuja un punto/halo
/// luminoso que sigue al cursor sobre una capa propia (<c>LaserLayer</c>) encima
/// del InkCanvas. Es totalmente efímero: no crea <c>Stroke</c> ni toca el
/// historial. Escucha el mouse en el InkCanvas (la capa es <c>IsHitTestVisible=false</c>)
/// y limpia el punto al cambiar de herramienta u ocultar el overlay.
/// </summary>
public static class LaserPointerBehavior
{
    /// <summary>Herramienta activa (enlazada al ViewModel del overlay).</summary>
    public static readonly DependencyProperty ToolProperty =
        DependencyProperty.RegisterAttached(
            "Tool",
            typeof(ToolKind),
            typeof(LaserPointerBehavior),
            new PropertyMetadata(ToolKind.Pencil, OnToolChanged));

    /// <summary>Capa de render del láser (Canvas encima del InkCanvas).</summary>
    public static readonly DependencyProperty LayerProperty =
        DependencyProperty.RegisterAttached(
            "Layer",
            typeof(Canvas),
            typeof(LaserPointerBehavior),
            new PropertyMetadata(null));

    // Estado del láser en curso (por InkCanvas); no es bindable.
    private static readonly DependencyProperty SessionProperty =
        DependencyProperty.RegisterAttached(
            "Session",
            typeof(LaserSession),
            typeof(LaserPointerBehavior),
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

    public static void SetLayer(DependencyObject element, Canvas value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(LayerProperty, value);
    }

    public static Canvas GetLayer(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (Canvas)element.GetValue(LayerProperty);
    }

    private static void OnToolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not InkCanvas canvas)
        {
            return;
        }

        EnsureHandlers(canvas);
        ApplyCursor(canvas, (ToolKind)e.NewValue, (ToolKind)e.OldValue);
        if ((ToolKind)e.NewValue is not ToolKind.Laser)
        {
            ClearSession(canvas);
        }
    }

    // Re-suscribir es idempotente: evita enganchar los manejadores dos veces.
    private static void EnsureHandlers(InkCanvas canvas)
    {
        canvas.PreviewMouseMove -= OnMouseMove;
        canvas.PreviewMouseMove += OnMouseMove;
        canvas.PreviewMouseLeftButtonDown -= OnMouseDown;
        canvas.PreviewMouseLeftButtonDown += OnMouseDown;
        canvas.IsVisibleChanged -= OnVisibleChanged;
        canvas.IsVisibleChanged += OnVisibleChanged;
    }

    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        if (GetTool(canvas) is ToolKind.Laser)
        {
            // Cada pulsación es un trazo independiente: no se conecta con el anterior.
            GetOrCreateSession(canvas)?.BeginStroke();
        }
    }

    // Oculta el cursor del SO con Láser activo (solo se ve el halo). ForceCursor es
    // necesario porque el InkCanvas fija su propio cursor e ignora Cursor a secas.
    // Solo soltamos el cursor forzado al SALIR de Láser, para no pisar el de Texto.
    private static void ApplyCursor(InkCanvas canvas, ToolKind newTool, ToolKind oldTool)
    {
        if (newTool is ToolKind.Laser)
        {
            canvas.Cursor = Cursors.None;
            canvas.ForceCursor = true;
        }
        else if (oldTool is ToolKind.Laser)
        {
            canvas.Cursor = null;
            canvas.ForceCursor = false;
        }
    }

    private static void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is InkCanvas canvas && !canvas.IsVisible)
        {
            ClearSession(canvas);
        }
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        var canvas = (InkCanvas)sender;
        if (GetTool(canvas) is not ToolKind.Laser)
        {
            return;
        }

        var session = GetOrCreateSession(canvas);
        if (session is null)
        {
            return;
        }

        var position = e.GetPosition(canvas);
        session.MoveTo(position);
        if (e.LeftButton is MouseButtonState.Pressed)
        {
            session.AddTrailPoint(position);
        }
    }

    private static LaserSession? GetOrCreateSession(InkCanvas canvas)
    {
        if (canvas.GetValue(SessionProperty) is LaserSession existing)
        {
            return existing;
        }

        if (GetLayer(canvas) is not Canvas layer)
        {
            return null;
        }

        var session = new LaserSession(layer, () => canvas.DefaultDrawingAttributes.Color);
        canvas.SetValue(SessionProperty, session);
        return session;
    }

    private static void ClearSession(InkCanvas canvas)
    {
        if (canvas.GetValue(SessionProperty) is LaserSession session)
        {
            session.Clear();
            canvas.SetValue(SessionProperty, null);
        }
    }
}
