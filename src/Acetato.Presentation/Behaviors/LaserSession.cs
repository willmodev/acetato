using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Acetato.Domain;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Estado de render del láser para una pane: el punto/halo que sigue al cursor y
/// la estela efímera, dibujados sobre la capa propia (<c>LaserLayer</c>). Redibuja
/// por frame (<see cref="CompositionTarget.Rendering"/>) aplicando
/// <see cref="LaserFade"/> y descarta los puntos expirados. Totalmente efímero: no
/// crea <c>Stroke</c> ni toca el historial. El color se lee en vivo (reactivo a la
/// tinta activa). <see cref="Clear"/> desuscribe el render y vacía la capa.
/// </summary>
internal sealed class LaserSession
{
    // Tamaños en DIP y vida de la estela (afinado de marca). El color sale SIEMPRE
    // de la tinta activa (paleta Ink.*), nunca hardcodeado.
    private const double HaloDiameter = 22d;
    private const double HaloRadius = HaloDiameter / 2d;
    private const double TrailThickness = 6d;
    private const double TrailLifetimeMs = 2000d;

    private readonly Canvas _layer;
    private readonly Func<Color> _colorProvider;
    private readonly Ellipse _halo;
    private readonly List<LaserPoint> _trail = [];
    private readonly List<Line> _segments = [];

    private Point? _haloPosition;
    private bool _haloAttached;
    private bool _newStroke;

    public LaserSession(Canvas layer, Func<Color> colorProvider)
    {
        _layer = layer;
        _colorProvider = colorProvider;
        _halo = new Ellipse
        {
            Width = HaloDiameter,
            Height = HaloDiameter,
            IsHitTestVisible = false,
        };
        CompositionTarget.Rendering += OnRendering;
    }

    /// <summary>Actualiza la posición (DIP) a la que sigue el punto/halo.</summary>
    public void MoveTo(Point position) => _haloPosition = position;

    /// <summary>Marca el inicio de un trazo nuevo (un clic distinto): el próximo
    /// punto no se conectará con el trazo anterior.</summary>
    public void BeginStroke() => _newStroke = true;

    /// <summary>Acumula un punto de estela en la posición (DIP) actual.</summary>
    public void AddTrailPoint(Point position)
    {
        _trail.Add(new LaserPoint(position, Stopwatch.GetTimestamp(), _newStroke));
        _newStroke = false;
    }

    /// <summary>Desuscribe el render y quita el punto y la estela de la capa.</summary>
    public void Clear()
    {
        CompositionTarget.Rendering -= OnRendering;
        RemoveSegments();
        _layer.Children.Remove(_halo);
        _haloAttached = false;
        _trail.Clear();
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        long now = Stopwatch.GetTimestamp();
        _trail.RemoveAll(point => AgeMs(point, now) >= TrailLifetimeMs);
        var color = _colorProvider();
        RenderHalo(color);
        RenderTrail(now, color);
    }

    private void RenderHalo(Color color)
    {
        if (_haloPosition is not Point position)
        {
            return;
        }

        if (!_haloAttached)
        {
            _ = _layer.Children.Add(_halo);
            _haloAttached = true;
        }

        _halo.Fill = BuildHaloBrush(color);
        Canvas.SetLeft(_halo, position.X - HaloRadius);
        Canvas.SetTop(_halo, position.Y - HaloRadius);
    }

    private void RenderTrail(long now, Color color)
    {
        RemoveSegments();
        if (_trail.Count < 2)
        {
            return;
        }

        var stroke = new SolidColorBrush(color);
        stroke.Freeze();
        for (int i = 1; i < _trail.Count; i++)
        {
            // No conectar entre clics distintos: el inicio de un trazo no se une al anterior.
            if (_trail[i].StartsStroke)
            {
                continue;
            }

            double opacity = LaserFade.OpacityForAge(AgeMs(_trail[i], now), TrailLifetimeMs);
            // Estela tipo cometa: se afina hacia la cola (más vieja).
            double thickness = TrailThickness * (0.35d + (0.65d * opacity));
            _segments.Add(BuildSegment(_trail[i - 1].Position, _trail[i].Position, stroke, opacity, thickness));
        }

        foreach (var segment in _segments)
        {
            _ = _layer.Children.Add(segment);
        }
    }

    private void RemoveSegments()
    {
        foreach (var segment in _segments)
        {
            _layer.Children.Remove(segment);
        }

        _segments.Clear();
    }

    private static Line BuildSegment(Point from, Point to, Brush stroke, double opacity, double thickness) => new()
    {
        X1 = from.X,
        Y1 = from.Y,
        X2 = to.X,
        Y2 = to.Y,
        Stroke = stroke,
        StrokeThickness = thickness,
        StrokeStartLineCap = PenLineCap.Round,
        StrokeEndLineCap = PenLineCap.Round,
        Opacity = opacity,
        IsHitTestVisible = false,
    };

    private static double AgeMs(LaserPoint point, long nowTicks) =>
        (nowTicks - point.CreatedAtTicks) * 1000d / Stopwatch.Frequency;

    /// <summary>Pincel radial: núcleo brillante → glow del color → borde transparente.</summary>
    private static RadialGradientBrush BuildHaloBrush(Color color)
    {
        var core = Color.FromArgb(255, 255, 255, 255);
        var inner = Color.FromArgb(240, color.R, color.G, color.B);
        var glow = Color.FromArgb(130, color.R, color.G, color.B);
        var edge = Color.FromArgb(0, color.R, color.G, color.B);
        var brush = new RadialGradientBrush
        {
            GradientStops =
            [
                new GradientStop(core, 0d),
                new GradientStop(core, 0.18d), // núcleo intenso y pequeño
                new GradientStop(inner, 0.4d),
                new GradientStop(glow, 0.7d),
                new GradientStop(edge, 1d), // glow suave hasta el borde
            ],
        };
        brush.Freeze();
        return brush;
    }
}
