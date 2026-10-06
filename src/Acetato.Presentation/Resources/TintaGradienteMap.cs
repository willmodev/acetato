using System.Windows.Media;
using Acetato.Domain;

namespace Acetato.Presentation.Resources;

/// <summary>
/// Traduce el <see cref="TintaGradiente"/> de dominio a sus dos <see cref="Color"/> de
/// WPF (inicio y fin) leyendo los tokens <c>Ink.Gradient.&lt;Par&gt;.Start/End.Color</c>
/// de <c>Tokens.xaml</c>. Igual que <see cref="TintaColorMap"/>, el mapeo vive SOLO en
/// Presentation: ni Domain ni Application conocen WPF.
/// </summary>
public static class TintaGradienteMap
{
    /// <summary>Colores de inicio y fin del par <paramref name="pair"/>.</summary>
    public static (Color Start, Color End) Resolve(TintaGradiente pair)
    {
        // Un valor fuera del enum (cast inválido) cae al primer par, como TintaColorMap.
        var known = Enum.IsDefined(pair) ? pair : TintaGradiente.BlueCyan;
        return (Lookup(known, "Start"), Lookup(known, "End"));
    }

    private static Color Lookup(TintaGradiente pair, string end) =>
        (Color)System.Windows.Application.Current.Resources[$"Ink.Gradient.{pair}.{end}.Color"];
}
