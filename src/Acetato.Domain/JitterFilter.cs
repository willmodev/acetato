namespace Acetato.Domain;

/// <summary>
/// Filtro incremental del temblor del pulso (HU-20), para la flecha en vivo. Cada punto
/// nuevo se acerca al anterior filtrado con una media exponencial (<c>smoothing</c> es
/// el peso del punto nuevo: 1 = sin filtro) y los puntos más cerca que
/// <c>minDistance</c> del último aceptado se descartan (ruido del mouse quieto).
/// Tipo puro (sin WPF), testeable.
/// </summary>
public sealed class JitterFilter
{
    private readonly double _smoothing;
    private readonly double _minDistance;
    private StrokePoint? _last;

    public JitterFilter(double smoothing, double minDistance)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(smoothing);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(smoothing, 1d);
        ArgumentOutOfRangeException.ThrowIfNegative(minDistance);
        _smoothing = smoothing;
        _minDistance = minDistance;
    }

    /// <summary>
    /// Filtra <paramref name="raw"/>. Devuelve <c>false</c> si el punto se descarta por
    /// estar demasiado cerca del último aceptado. El primer punto pasa tal cual.
    /// </summary>
    public bool TryAdd(StrokePoint raw, out StrokePoint filtered)
    {
        if (_last is not { } last)
        {
            _last = filtered = raw;
            return true;
        }

        filtered = new StrokePoint(
            last.X + (_smoothing * (raw.X - last.X)),
            last.Y + (_smoothing * (raw.Y - last.Y)));
        if (last.DistanceTo(filtered) < _minDistance)
        {
            filtered = default;
            return false;
        }

        _last = filtered;
        return true;
    }

    /// <summary>Olvida el recorrido: el próximo punto empieza un trazo nuevo.</summary>
    public void Reset() => _last = null;
}
