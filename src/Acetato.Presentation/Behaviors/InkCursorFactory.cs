using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Acetato.Domain;
using Acetato.Presentation.Resources;
using Acetato.Presentation.ViewModels;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Crea el cursor de tinta (HU-26): un cursor real de Windows con el círculo de la
/// tinta y el grosor activos, a la escala del monitor. Cada combinación se crea una
/// sola vez y queda en caché mientras dure la app; son pocas (tintas o pares ×
/// grosores × escalas), así que la caché no crece sin límite.
/// </summary>
internal static class InkCursorFactory
{
    private const byte HighlighterAlpha = 0x80; // 50 %: la transparencia fija del resaltador de WPF

    private static readonly Dictionary<(InkCursorTip Tip, int Pixels), Cursor> Cache = [];

    /// <summary>Cursor de <paramref name="tip"/> para la escala dada (1 = 100 %).</summary>
    public static Cursor For(InkCursorTip tip, double scale)
    {
        ArgumentNullException.ThrowIfNull(tip);
        int pixels = CursorTipSize.PixelDiameter(tip.Thickness, scale);
        var key = (tip, pixels);
        if (!Cache.TryGetValue(key, out var cursor))
        {
            cursor = Create(tip, pixels);
            Cache[key] = cursor;
        }

        return cursor;
    }

    private static Cursor Create(InkCursorTip tip, int pixels)
    {
        var light = Token("Cursor.Ring.Light.Color");
        var dark = Token("Cursor.Ring.Dark.Color");
        var bgra = InkCursorRenderer.Render(FillOf(tip), pixels, light, dark);
        int size = InkCursorRenderer.SizeFor(pixels);
        var bytes = CursorFile.Write(bgra, size, size / 2);
        using var stream = new MemoryStream(bytes);
        return new Cursor(stream);
    }

    // En degradado (neón o de tinta), el par del próximo trazo en diagonal (inicio arriba a la
    // izquierda), como se ve en las figuras cerradas (HU-19).
    internal static Brush FillOf(InkCursorTip tip)
    {
        if (tip.Gradient is not { } pair)
        {
            // El resaltador (HU-34) muestra su tinta al 50 %, como la pinta.
            return new SolidColorBrush(tip.Highlighter ? Color.FromArgb(HighlighterAlpha, tip.Solid.R, tip.Solid.G, tip.Solid.B) : tip.Solid);
        }

        return DiagonalGradientBrush.Create(pair);
    }

    private static Color Token(string key) => (Color)System.Windows.Application.Current.Resources[key];
}
