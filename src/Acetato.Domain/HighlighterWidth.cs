namespace Acetato.Domain;

/// <summary>
/// Ancho del Resaltador (HU-34): tres veces el grosor elegido, porque un marcador es
/// más ancho que un lápiz. Con 3 px pinta 9 px y con 18 px pinta 54 px. Así se
/// reutiliza la escala de grosores sin un control nuevo. Tipo puro (sin WPF), testeable.
/// </summary>
public static class HighlighterWidth
{
    /// <summary>Veces que el resaltador multiplica el grosor elegido.</summary>
    public const double Factor = 3d;

    /// <summary>Ancho (px) del trazo del resaltador para el grosor dado.</summary>
    public static double From(double thickness) => thickness * Factor;
}
