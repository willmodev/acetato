namespace Acetato.Application.Abstractions;

/// <summary>
/// Opciones de dibujo que se recuerdan entre sesiones (bloque «Dibujo» de Ajustes).
/// </summary>
/// <param name="InkPairs">Tintas degradadas encendido: cada tinta va hacia su vecina (HU-27).</param>
/// <param name="FreeResize">
/// Transformación libre: al cambiar el tamaño de una selección cada eje se estira por su
/// lado; apagada, crece parejo (HU-28).
/// </param>
public sealed record DrawingPreferences(bool InkPairs, bool FreeResize = false)
{
    /// <summary>Lo que vale sin nada guardado: Tintas degradadas y Transformación libre apagadas.</summary>
    public static DrawingPreferences Defaults { get; } = new(InkPairs: false, FreeResize: false);
}
