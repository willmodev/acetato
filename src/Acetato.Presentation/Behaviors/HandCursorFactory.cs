using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Cursores de la herramienta Mover (SPEC 11): la mano abierta (Lucide <c>hand</c>) y,
/// mientras se arrastra, la cerrada (Lucide <c>grab</c>). Son cursores reales de Windows,
/// a la escala de cada monitor, con trazo claro y borde oscuro (los tokens del aro del
/// cursor de tinta) para verse sobre cualquier fondo. Se crean una vez por escala.
/// </summary>
internal static class HandCursorFactory
{
    // Lado de un cursor estándar de Windows a 100 %, en DIP, y rejilla de los iconos Lucide.
    private const double LogicalSize = 32d;
    private const double IconGrid = 24d;
    private const double Margin = 2d;
    private const double IconStroke = 2d;

    private static readonly Dictionary<(bool Grabbing, int Pixels), Cursor> Cache = [];

    /// <summary>Mano abierta para la escala dada (1 = 100 %).</summary>
    public static Cursor Open(double scale) => For(grabbing: false, scale);

    /// <summary>Mano cerrada, mientras se arrastra, para la escala dada.</summary>
    public static Cursor Grab(double scale) => For(grabbing: true, scale);

    private static Cursor For(bool grabbing, double scale)
    {
        int pixels = Math.Max(16, (int)Math.Round(LogicalSize * scale));
        var key = (grabbing, pixels);
        if (!Cache.TryGetValue(key, out var cursor))
        {
            cursor = Create(grabbing, pixels);
            Cache[key] = cursor;
        }

        return cursor;
    }

    private static Cursor Create(bool grabbing, int pixels)
    {
        var icon = (Geometry)System.Windows.Application.Current.Resources[grabbing ? "Icon.Grab" : "Icon.Move"];
        var bytes = CursorFile.Write(Render(icon, pixels), pixels, pixels / 2);
        using var stream = new MemoryStream(bytes);
        return new Cursor(stream);
    }

    // El icono escalado a la imagen: primero el borde oscuro (trazo más ancho) y encima
    // el trazo claro. Devuelve BGRA con alfa sin premultiplicar, fila a fila.
    private static byte[] Render(Geometry icon, int pixels)
    {
        double factor = (pixels - (2d * Margin)) / IconGrid;
        var geometry = icon.Clone();
        geometry.Transform = new MatrixTransform(factor, 0d, 0d, factor, Margin, Margin);
        double stroke = IconStroke * factor;
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            context.DrawGeometry(null, RoundPen(Token("Cursor.Ring.Dark.Color"), stroke + 2d), geometry);
            context.DrawGeometry(null, RoundPen(Token("Cursor.Ring.Light.Color"), stroke), geometry);
        }

        var bitmap = new RenderTargetBitmap(pixels, pixels, 96d, 96d, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var straight = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0d);
        var bgra = new byte[pixels * pixels * 4];
        straight.CopyPixels(bgra, pixels * 4, 0);
        return bgra;
    }

    private static Pen RoundPen(Color color, double thickness) => new(new SolidColorBrush(color), thickness)
    {
        StartLineCap = PenLineCap.Round,
        EndLineCap = PenLineCap.Round,
        LineJoin = PenLineJoin.Round,
    };

    private static Color Token(string key) => (Color)System.Windows.Application.Current.Resources[key];
}
