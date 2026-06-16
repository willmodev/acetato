namespace Acetato.Domain;

/// <summary>
/// Mapea el índice del grosor activo (<see cref="ThicknessScale"/>) a un tamaño de
/// fuente en puntos para las anotaciones de texto. Así el texto crece con el grosor
/// sin necesidad de un control propio. Tipo puro (sin WPF), testeable.
/// </summary>
public static class FontScale
{
    /// <summary>Tamaños de fuente (pt) por índice de grosor, de menor a mayor.</summary>
    public static IReadOnlyList<double> Sizes { get; } = [16d, 22d, 30d, 42d, 60d];

    /// <summary>Tamaño de fuente (pt) del índice de grosor dado, con clamp al rango válido.</summary>
    public static double FromThicknessIndex(int index) =>
        Sizes[Math.Clamp(index, 0, Sizes.Count - 1)];
}
