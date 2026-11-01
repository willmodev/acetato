using System.Runtime.InteropServices;

namespace Acetato.Domain;

/// <summary>
/// Par de colores de un trazo en degradado: un par neón del modo Degradado (HU-19) o
/// una tinta sólida que va hacia su vecina fija (Tintas degradadas, HU-27). Exactamente
/// uno de los dos está presente. Es un valor: dos pares iguales comparten pincel y
/// cursor en caché. Los hex de cada color viven en el design system.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct GradientPair
{
    private GradientPair(TintaGradiente? neon, TintaColor? ink)
    {
        Neon = neon;
        Ink = ink;
    }

    /// <summary>Par neón del modo Degradado; <c>null</c> si es un par de tinta.</summary>
    public TintaGradiente? Neon { get; }

    /// <summary>Tinta que va hacia su vecina; <c>null</c> si es un par neón.</summary>
    public TintaColor? Ink { get; }

    /// <summary>Par neón <paramref name="pair"/> del modo Degradado.</summary>
    public static GradientPair FromNeon(TintaGradiente pair) => new(pair, null);

    /// <summary>La tinta <paramref name="ink"/> hacia su vecina (Tintas degradadas).</summary>
    public static GradientPair FromInk(TintaColor ink) => new(null, ink);
}
