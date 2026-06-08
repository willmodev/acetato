namespace Acetato.Domain;

/// <summary>
/// Convierte longitudes de píxeles físicos a unidades independientes de
/// dispositivo (DIP) según el factor de escala DPI de un monitor (HU-09). WPF
/// posiciona y mide en DIP, pero la geometría de los monitores llega en píxeles
/// físicos; sin esta conversión el overlay se desfasaría en monitores con
/// escalado distinto. Tipo puro (sin WPF ni Win32), testeable.
/// </summary>
public static class DipConverter
{
    /// <summary>
    /// Píxeles físicos → DIP. Un factor ≤ 0 se trata como 1 (100 %) para no
    /// dividir por cero ni invertir el signo.
    /// </summary>
    public static double ToDip(double physical, double dpiScale) =>
        physical / (dpiScale > 0 ? dpiScale : 1d);
}
