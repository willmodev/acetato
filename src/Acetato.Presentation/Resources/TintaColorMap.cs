using System.Windows.Media;
using Acetato.Domain;

namespace Acetato.Presentation.Resources;

/// <summary>
/// Traduce la <see cref="TintaColor"/> de dominio al <see cref="Color"/> de WPF
/// leyendo el token correspondiente (<c>Ink.*.Color</c>) de <c>Tokens.xaml</c>.
/// El mapeo color→pintura vive SOLO en Presentation: ni Domain ni Application
/// conocen WPF.
/// </summary>
public static class TintaColorMap
{
    /// <summary>Color WPF del token de tinta para la <paramref name="color"/> dada.</summary>
    public static Color Resolve(TintaColor color) => Lookup($"Ink.{Name(color)}.Color");

    /// <summary>Vecina fija de la <paramref name="color"/> dada (<c>Ink.*.End.Color</c>, HU-27).</summary>
    public static Color ResolveEnd(TintaColor color) => Lookup($"Ink.{Name(color)}.End.Color");

    private static Color Lookup(string key) =>
        (Color)System.Windows.Application.Current.Resources[key];

    // Un valor fuera del enum (cast inválido) cae a la tinta roja.
    private static string Name(TintaColor color) => color switch
    {
        TintaColor.Red => "Red",
        TintaColor.Blue => "Blue",
        TintaColor.Yellow => "Yellow",
        TintaColor.Green => "Green",
        TintaColor.White => "White",
        TintaColor.Black => "Black",
        _ => "Red",
    };
}
