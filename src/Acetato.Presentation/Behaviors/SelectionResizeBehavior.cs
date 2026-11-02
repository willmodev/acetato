using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using Acetato.Domain;
using Acetato.Presentation.Overlay;
using Acetato.Presentation.ViewModels;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Cambiar el tamaño de lo seleccionado con Mover jalando un tirador (HU-28). Los
/// tiradores los dibuja el lienzo (<c>ResizeEnabled</c>), pero el arrastre lo hace este
/// comportamiento: el del lienzo solo pinta un rectángulo de guía, no deja forzar la
/// proporción en vivo y escala la letra de los textos de forma rara. El grosor nunca
/// cambia. No escucha eventos por su cuenta: lo llama <see cref="MoveToolBehavior"/>,
/// que ya recibe el mouse de la ventana antes que el lienzo.
/// </summary>
public static class SelectionResizeBehavior
{
    /// <summary>Transformación libre (cada eje por su lado); apagada = crecer parejo.</summary>
    public static readonly DependencyProperty FreeProperty =
        DependencyProperty.RegisterAttached(
            "Free",
            typeof(bool),
            typeof(SelectionResizeBehavior),
            new PropertyMetadata(false));

    // Cambio de tamaño en curso y cursor de flecha doble a la vista (por InkCanvas).
    private static readonly DependencyProperty SessionProperty =
        DependencyProperty.RegisterAttached("Session", typeof(ResizeSession), typeof(SelectionResizeBehavior), new PropertyMetadata(null));

    private static readonly DependencyProperty HoverProperty =
        DependencyProperty.RegisterAttached("Hover", typeof(bool), typeof(SelectionResizeBehavior), new PropertyMetadata(false));

    public static void SetFree(DependencyObject element, bool value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(FreeProperty, value);
    }

    public static bool GetFree(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (bool)element.GetValue(FreeProperty);
    }

    /// <summary>
    /// Tirador de la selección bajo <paramref name="point"/>; <c>null</c> si no hay
    /// selección, la herramienta no es Mover o el punto no cae en un tirador.
    /// </summary>
    internal static ResizeHandle? HandleAt(InkCanvas canvas, Point point) =>
        MoveToolBehavior.GetTool(canvas) == ToolKind.Move ? ToHandle(canvas.HitTestSelection(point)) : null;

    // Tirador equivalente al resultado del lienzo; null si no es un tirador.
    private static ResizeHandle? ToHandle(InkCanvasSelectionHitResult hit) =>
        hit switch
        {
            InkCanvasSelectionHitResult.TopLeft => ResizeHandle.TopLeft,
            InkCanvasSelectionHitResult.Top => ResizeHandle.Top,
            InkCanvasSelectionHitResult.TopRight => ResizeHandle.TopRight,
            InkCanvasSelectionHitResult.Right => ResizeHandle.Right,
            InkCanvasSelectionHitResult.BottomRight => ResizeHandle.BottomRight,
            InkCanvasSelectionHitResult.Bottom => ResizeHandle.Bottom,
            InkCanvasSelectionHitResult.BottomLeft => ResizeHandle.BottomLeft,
            InkCanvasSelectionHitResult.Left => ResizeHandle.Left,
            _ => null,
        };

    /// <summary>Empieza a redimensionar si el clic cae en un tirador. Devuelve si empezó.</summary>
    internal static bool TryStart(InkCanvas canvas, Point point)
    {
        if (HandleAt(canvas, point) is not { } handle)
        {
            return false;
        }

        var bounds = canvas.GetSelectionBounds();
        var start = new SelectionBox(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        var resize = new AnnotationResize(canvas.GetSelectedStrokes(), canvas.GetSelectedElements(), start);
        canvas.SetValue(SessionProperty, new ResizeSession(resize, handle, point));
        _ = canvas.CaptureMouse();
        Mouse.OverrideCursor = CursorFor(handle);
        return true;
    }

    /// <summary>Sigue el cambio de tamaño hasta <paramref name="point"/>. Devuelve si había uno.</summary>
    internal static bool MoveTo(InkCanvas canvas, Point point)
    {
        if (canvas.GetValue(SessionProperty) is not ResizeSession session)
        {
            return false;
        }

        var box = SelectionResize.Resize(
            session.Start, session.Handle, point.X - session.Origin.X, point.Y - session.Origin.Y, GetFree(canvas));
        session.Resize.ResizeTo(box);
        return true;
    }

    /// <summary>
    /// Cierra el cambio de tamaño: lo registra en el historial y deja seleccionado lo
    /// redimensionado. Devuelve si había uno. La sesión se suelta antes de liberar el
    /// mouse, porque liberarlo vuelve a entrar aquí por LostMouseCapture.
    /// </summary>
    internal static bool End(InkCanvas canvas)
    {
        if (canvas.GetValue(SessionProperty) is not ResizeSession session)
        {
            return false;
        }

        canvas.SetValue(SessionProperty, null);
        canvas.ReleaseMouseCapture();
        Mouse.OverrideCursor = null;
        if (session.Resize.Finish() is { } resized)
        {
            (canvas.DataContext as OverlayViewModel)?.RecordResize(resized);
        }

        // Re-seleccionar recoloca el recuadro y los tiradores sobre el tamaño nuevo.
        canvas.Select(canvas.GetSelectedStrokes(), canvas.GetSelectedElements());
        return true;
    }

    /// <summary>
    /// Pone la flecha doble sobre un tirador y devuelve el cursor de la herramienta al
    /// salir de él. Hace falta porque Mover usa su propio cursor (la mano) y con él el
    /// lienzo ya no cambia el cursor sobre los tiradores.
    /// </summary>
    internal static void UpdateHover(InkCanvas canvas, Point point)
    {
        bool shown = (bool)canvas.GetValue(HoverProperty);
        if (HandleAt(canvas, point) is { } handle)
        {
            canvas.Cursor = CursorFor(handle);
            canvas.SetValue(HoverProperty, true);
        }
        else if (shown)
        {
            EndHover(canvas);
        }
    }

    /// <summary>Quita la flecha doble, si estaba, y vuelve el cursor de la herramienta.</summary>
    internal static void EndHover(InkCanvas canvas)
    {
        if ((bool)canvas.GetValue(HoverProperty))
        {
            canvas.SetValue(HoverProperty, false);
            ToolCursorBehavior.Refresh(canvas);
        }
    }

    // Flecha doble de Windows en la dirección del tirador.
    private static Cursor CursorFor(ResizeHandle handle) => handle switch
    {
        ResizeHandle.TopLeft or ResizeHandle.BottomRight => Cursors.SizeNWSE,
        ResizeHandle.TopRight or ResizeHandle.BottomLeft => Cursors.SizeNESW,
        ResizeHandle.Left or ResizeHandle.Right => Cursors.SizeWE,
        _ => Cursors.SizeNS,
    };

    // Estado de un cambio de tamaño en curso.
    private sealed record ResizeSession(AnnotationResize Resize, ResizeHandle Handle, Point Origin)
    {
        public SelectionBox Start { get; } = Resize.Current;
    }
}
