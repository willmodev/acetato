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
    public static Color Resolve(TintaColor color) =>
        (Color)System.Windows.Application.Current.Resources[ResourceKey(color)];

    private static string ResourceKey(TintaColor color) => color switch
    {
        TintaColor.Red => "Ink.Red.Color",
        TintaColor.Blue => "Ink.Blue.Color",
        TintaColor.Yellow => "Ink.Yellow.Color",
        TintaColor.Green => "Ink.Green.Color",
        TintaColor.White => "Ink.White.Color",
        TintaColor.Black => "Ink.Black.Color",
        _ => "Ink.Red.Color",
    };
}
