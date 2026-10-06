namespace Acetato.Domain;

/// <summary>
/// Sortea el par de degradado del siguiente trazo. Nunca repite el par actual, para
/// que dos trazos vecinos no se confundan. La fuente de azar se inyecta para que los
/// tests sean deterministas. Tipo puro (sin WPF), testeable.
/// </summary>
public static class GradientPicker
{
    private static readonly int PairCount = Enum.GetValues<TintaGradiente>().Length;

    /// <summary>
    /// Devuelve un par al azar distinto de <paramref name="current"/>. Se sortea entre
    /// los pares restantes (uniforme) saltando el índice del actual.
    /// </summary>
    public static TintaGradiente Next(TintaGradiente current, IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        var index = random.NextInt(PairCount - 1);
        if (index >= (int)current)
        {
            index++;
        }

        return (TintaGradiente)index;
    }
}
