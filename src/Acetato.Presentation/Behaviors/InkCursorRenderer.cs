using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Dibuja el círculo del cursor de tinta (HU-26) en píxeles físicos: el relleno con
/// la tinta y, alrededor, un aro oscuro y otro claro de 1 píxel para que se vea
/// sobre cualquier fondo. Devuelve BGRA con alfa sin premultiplicar.
/// </summary>
internal static class InkCursorRenderer
{
    private const int RingsWidth = 2;

    /// <summary>Lado de la imagen para un relleno de <paramref name="fillDiameter"/> píxeles.</summary>
    public static int SizeFor(int fillDiameter) => fillDiameter + (2 * RingsWidth);

    /// <summary>Píxeles BGRA de la imagen, fila a fila de arriba abajo.</summary>
    public static byte[] Render(Brush fill, int fillDiameter, Color light, Color dark)
    {
        int size = SizeFor(fillDiameter);
        var center = new Point(size / 2d, size / 2d);
        double radius = fillDiameter / 2d;
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            context.DrawEllipse(null, new Pen(new SolidColorBrush(light), 1d), center, radius + 1.5d, radius + 1.5d);
            context.DrawEllipse(null, new Pen(new SolidColorBrush(dark), 1d), center, radius + 0.5d, radius + 0.5d);
            context.DrawEllipse(fill, null, center, radius, radius);
        }

        var bitmap = new RenderTargetBitmap(size, size, 96d, 96d, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var straight = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0d);
        var pixels = new byte[size * size * 4];
        straight.CopyPixels(pixels, size * 4, 0);
        return pixels;
    }
}
