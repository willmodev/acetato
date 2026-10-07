using System.Windows.Ink;
using System.Windows.Input;
using Acetato.Domain;
using Acetato.Presentation.Overlay;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Estado de un trazo libre en curso (HU-20), por InkCanvas: acumula los puntos y arma
/// el trazo a mostrar en vivo y el que queda fijo al soltar. Atributos (color, grosor) y
/// par de degradado se fijan al empezar: girar la rueda a mitad del trazo solo afecta
/// al siguiente. No toca el lienzo ni el historial.
/// </summary>
internal sealed class FreehandSession
{
    private readonly DrawingAttributes _attributes;
    private readonly TintaGradiente? _gradient;
    private readonly List<StrokePoint> _points = [];

    public FreehandSession(ToolKind tool, DrawingAttributes attributes, TintaGradiente? gradient)
    {
        Tool = tool;
        _attributes = attributes.Clone();
        _gradient = gradient;
    }

    public ToolKind Tool { get; }

    /// <summary>true si el trazo va en degradado (al fijarlo hay que sortear el par siguiente).</summary>
    public bool IsGradient => _gradient is not null;

    public int PointCount => _points.Count;

    /// <summary>Acumula un punto del recorrido; los repetidos (mouse quieto) se ignoran.</summary>
    public bool Add(StrokePoint raw)
    {
        if (_points.Count > 0 && _points[^1] == raw)
        {
            return false;
        }

        _points.Add(raw);
        return true;
    }

    /// <summary>Trazo a pintar en vivo: el mismo aspecto que tendrá al soltar.</summary>
    public Stroke? BuildPreview() => BuildStroke();

    /// <summary>Trazo que queda fijo al soltar; <c>null</c> si no hay nada que fijar.</summary>
    public Stroke? BuildFinal() => BuildStroke();

    // Cada llamada usa una colección de puntos (y atributos) nueva: un Stroke se suscribe
    // a los cambios de ambos, así que compartirlos entre trazos acumularía manejadores.
    private Stroke? BuildStroke()
    {
        if (_points.Count == 0)
        {
            return null;
        }

        var stylusPoints = ToStylusPoints(_points);
        return _gradient is null
            ? new Stroke(stylusPoints, _attributes.Clone())
            : new StyledStroke(stylusPoints, _attributes.Clone(), head: null, _gradient);
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
