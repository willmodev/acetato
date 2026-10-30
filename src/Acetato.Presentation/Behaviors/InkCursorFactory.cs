using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using Acetato.Domain;
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
        var bgra = InkCursorRenderer.Render(new SolidColorBrush(tip.Solid), pixels, light, dark);
        int size = InkCursorRenderer.SizeFor(pixels);
        var bytes = CursorFile.Write(bgra, size, size / 2);
        using var stream = new MemoryStream(bytes);
        return new Cursor(stream);
    }

    private static Color Token(string key) => (Color)System.Windows.Application.Current.Resources[key];
}
