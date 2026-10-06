namespace Acetato.Domain;

/// <summary>
/// Fuente de enteros al azar inyectable. Permite que el sorteo de pares de degradado
/// sea determinista en los tests (secuencia fija) sin depender de <see cref="Random"/>.
/// </summary>
public interface IRandomSource
{
    /// <summary>Entero uniforme en [0, <paramref name="maxExclusive"/>).</summary>
    public int NextInt(int maxExclusive);
}
