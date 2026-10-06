using Acetato.Domain;

namespace Acetato.Domain.Tests;

/// <summary>
/// Fuente de azar de prueba: devuelve una secuencia fija (cíclica), acotada al rango
/// pedido. Hace los sorteos deterministas y reproducibles.
/// </summary>
internal sealed class SequenceRandomSource : IRandomSource
{
    private readonly int[] _values;
    private int _position;

    public SequenceRandomSource(params int[] values)
    {
        _values = values.Length > 0 ? values : [0];
    }

    public int NextInt(int maxExclusive)
    {
        var value = _values[_position % _values.Length];
        _position++;
        return value % maxExclusive;
    }
}
