namespace Acetato.Domain;

/// <summary>
/// Escala discreta de grosores de trazo (px), del más fino al más grueso,
/// portada del design system. El grosor se mueve por pasos con tope en ambos
/// extremos: nunca se sale del rango. Tipo puro (sin WPF), testeable.
/// </summary>
public static class ThicknessScale
{
    /// <summary>Pasos de grosor en píxeles, de menor a mayor.</summary>
    public static IReadOnlyList<double> Steps { get; } = [3d, 5d, 8d, 12d, 18d];

    /// <summary>Índice del grosor mínimo.</summary>
    public static int MinIndex => 0;

    /// <summary>Índice del grosor máximo.</summary>
    public static int MaxIndex => Steps.Count - 1;

    /// <summary>Índice por defecto (coincide con el grosor inicial, 3 px).</summary>
    public static int DefaultIndex => 0;

    /// <summary>Siguiente paso; en el máximo se mantiene.</summary>
    public static int Next(int index) => Math.Clamp(index + 1, MinIndex, MaxIndex);

    /// <summary>Paso anterior; en el mínimo se mantiene.</summary>
    public static int Previous(int index) => Math.Clamp(index - 1, MinIndex, MaxIndex);

    /// <summary>Grosor (px) del índice dado, con clamp al rango válido.</summary>
    public static double At(int index) => Steps[Math.Clamp(index, MinIndex, MaxIndex)];
}
