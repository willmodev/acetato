namespace Acetato.Domain;

/// <summary>
/// Los 6 pares neón del modo Degradado. Como <see cref="TintaColor"/>, son tintas
/// de dibujo (trazos del usuario), no colores de chrome. Los hex concretos de cada
/// par (inicio/fin) viven en el design system.
/// </summary>
public enum TintaGradiente
{
    BlueCyan,
    YellowLime,
    PinkRed,
    VioletPink,
    GreenCyan,
    OrangePink,
}
