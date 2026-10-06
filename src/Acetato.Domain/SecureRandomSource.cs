using System.Security.Cryptography;

namespace Acetato.Domain;

/// <summary>
/// Implementación de producción de <see cref="IRandomSource"/> sobre
/// <see cref="RandomNumberGenerator"/>: segura entre hilos y sin estado, por lo que
/// una única instancia compartida sirve a todos los monitores.
/// </summary>
public sealed class SecureRandomSource : IRandomSource
{
    /// <summary>Instancia compartida (el tipo no tiene estado).</summary>
    public static SecureRandomSource Instance { get; } = new();

    /// <inheritdoc />
    public int NextInt(int maxExclusive) => RandomNumberGenerator.GetInt32(maxExclusive);
}
