namespace Acetato.Domain;

/// <summary>
/// Tamaño del círculo del cursor de tinta (HU-26): el mismo diámetro que el trazo,
/// con un mínimo para que el grosor más fino se siga viendo. Tipo puro (sin WPF),
/// testeable.
/// </summary>
public static class CursorTipSize
{
    /// <summary>Diámetro mínimo visible del círculo, en DIP.</summary>
    public const double MinDiameter = 6d;

    /// <summary>Diámetro del círculo (DIP) para el grosor dado.</summary>
    public static double Diameter(double thickness) => Math.Max(MinDiameter, thickness);

    /// <summary>
    /// Diámetro en píxeles físicos para la escala del monitor (1 = 100 %, 1,5 = 150 %).
    /// Nunca baja de 1 píxel.
    /// </summary>
    public static int PixelDiameter(double thickness, double scale) =>
        Math.Max(1, (int)Math.Round(Diameter(thickness) * scale, MidpointRounding.AwayFromZero));
}
