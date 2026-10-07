using System.Windows.Ink;
using System.Windows.Input;
using Acetato.Domain;
using Acetato.Presentation.Overlay;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Estado de un trazo libre en curso (HU-20), por InkCanvas: acumula los puntos y arma
/// el trazo a mostrar en vivo y el que queda fijo al soltar. Atributos (color, grosor) y
/// par de degradado se fijan al empezar: girar la rueda a mitad del trazo solo afecta
/// al siguiente. La flecha filtra el temblor en vivo y lleva su punta desde que el
/// recorrido es más largo que ella. No toca el lienzo ni el historial.
/// </summary>
internal sealed class FreehandSession
{
    // Filtro leve del temblor de la flecha (valores iniciales del spec 04).
    private const double ArrowSmoothing = 0.5d;
    private const double ArrowMinDistance = 1d;

    private readonly DrawingAttributes _attributes;
    private readonly TintaGradiente? _gradient;
    private readonly JitterFilter? _filter;
    private readonly List<StrokePoint> _points = [];

    public FreehandSession(ToolKind tool, DrawingAttributes attributes, TintaGradiente? gradient)
    {
        Tool = tool;
        _attributes = attributes.Clone();
        _gradient = gradient;
        _filter = tool == ToolKind.Arrow ? new JitterFilter(ArrowSmoothing, ArrowMinDistance) : null;
    }

    public ToolKind Tool { get; }

    /// <summary>true si el trazo va en degradado (al fijarlo hay que sortear el par siguiente).</summary>
    public bool IsGradient => _gradient is not null;

    public int PointCount => _points.Count;

    private double Thickness => Math.Max(_attributes.Width, _attributes.Height);

    /// <summary>
    /// Acumula un punto del recorrido. Devuelve false si se ignoró: repetido (mouse quieto)
    /// o, en la flecha, descartado por el filtro de temblor.
    /// </summary>
    public bool Add(StrokePoint raw)
    {
        var point = raw;
        if (_filter is not null && !_filter.TryAdd(raw, out point))
        {
            return false;
        }

        if (_points.Count > 0 && _points[^1] == point)
        {
            return false;
        }

        _points.Add(point);
        return true;
    }

    /// <summary>
    /// Trazo a pintar en vivo, con el aspecto que tendrá al soltar. La flecha muestra su
    /// cuerpo sin punta mientras el recorrido sea más corto que la punta.
    /// </summary>
    public Stroke? BuildPreview()
    {
        if (Tool != ToolKind.Arrow)
        {
            return BuildStroke(_points, head: null);
        }

        return BuildStroke(_points, ArrowHeadBuilder.TryBuild(_points, Thickness, out var head) ? head : null);
    }

    /// <summary>
    /// Trazo que queda fijo al soltar; <c>null</c> si no hay nada que fijar (sin puntos, o
    /// una flecha más corta que su punta: un clic suelto no deja nada).
    /// </summary>
    public Stroke? BuildFinal()
    {
        if (Tool != ToolKind.Arrow)
        {
            return BuildStroke(_points, head: null);
        }

        return ArrowHeadBuilder.TryBuild(_points, Thickness, out var head) ? BuildStroke(_points, head) : null;
    }

    // Cada llamada usa una colección de puntos (y atributos) nueva: un Stroke se suscribe
    // a los cambios de ambos, así que compartirlos entre trazos acumularía manejadores.
    private Stroke? BuildStroke(List<StrokePoint> path, ArrowHead? head)
    {
        if (path.Count == 0)
        {
            return null;
        }

        var stylusPoints = ToStylusPoints(path);
        return _gradient is null && head is null
            ? new Stroke(stylusPoints, _attributes.Clone())
            : new StyledStroke(stylusPoints, _attributes.Clone(), head, _gradient);
    }

    private static StylusPointCollection ToStylusPoints(List<StrokePoint> path)
    {
        var stylusPoints = new StylusPointCollection(path.Count);
        foreach (var point in path)
        {
            stylusPoints.Add(new StylusPoint(point.X, point.Y));
        }

        return stylusPoints;
    }
}
