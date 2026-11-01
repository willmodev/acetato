namespace Acetato.Application.Abstractions;

/// <summary>
/// Opciones de dibujo que se recuerdan entre sesiones (bloque «Dibujo» de Ajustes).
/// </summary>
/// <param name="InkPairs">Tintas degradadas encendido: cada tinta va hacia su vecina (HU-27).</param>
public sealed record DrawingPreferences(bool InkPairs)
{
    /// <summary>Lo que vale sin nada guardado: Tintas degradadas apagado.</summary>
    public static DrawingPreferences Defaults { get; } = new(InkPairs: false);
}
