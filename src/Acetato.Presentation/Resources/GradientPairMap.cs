using System.Windows.Media;
using Acetato.Domain;

namespace Acetato.Presentation.Resources;

/// <summary>
/// Traduce un <see cref="GradientPair"/> de dominio a sus dos <see cref="Color"/> de WPF
/// (inicio y fin): un par neón por <see cref="TintaGradienteMap"/> y un par de tinta por
/// <see cref="TintaColorMap"/>, de la tinta a su vecina (HU-27).
/// </summary>
public static class GradientPairMap
{
    /// <summary>Colores de inicio y fin del par <paramref name="pair"/>.</summary>
    public static (Color Start, Color End) Resolve(GradientPair pair)
    {
        if (pair.Ink is { } ink)
        {
            return (TintaColorMap.Resolve(ink), TintaColorMap.ResolveEnd(ink));
        }

        // Un par por defecto (sin neón ni tinta) cae al primer par neón, como TintaGradienteMap.
        return TintaGradienteMap.Resolve(pair.Neon ?? TintaGradiente.BlueCyan);
    }
}
