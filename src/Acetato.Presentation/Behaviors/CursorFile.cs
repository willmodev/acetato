using System.IO;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Empaqueta una imagen BGRA de 32 bits como archivo <c>.cur</c> en memoria (HU-26),
/// para crear un cursor real de Windows con <c>Cursor(Stream)</c> sin P/Invoke. Una
/// sola imagen con canal alfa. La máscara AND va vacía porque manda el alfa.
/// </summary>
internal static class CursorFile
{
    private const int DirectorySize = 6;
    private const int EntrySize = 16;
    private const int InfoHeaderSize = 40;

    /// <summary>
    /// Bytes del <c>.cur</c> de una imagen cuadrada de <paramref name="size"/> píxeles.
    /// <paramref name="bgra"/> va fila a fila de arriba abajo, con alfa sin premultiplicar.
    /// </summary>
    public static byte[] Write(byte[] bgra, int size, int hotspot)
    {
        ArgumentNullException.ThrowIfNull(bgra);
        int stride = size * 4;
        int maskStride = (size + 31) / 32 * 4;
        int imageBytes = InfoHeaderSize + (stride * size) + (maskStride * size);

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream))
        {
            WriteDirectory(writer, size, hotspot, imageBytes);
            WriteInfoHeader(writer, size);

            // Los bitmaps de Windows guardan las filas de abajo arriba.
            for (int row = size - 1; row >= 0; row--)
            {
                writer.Write(bgra, row * stride, stride);
            }

            writer.Write(new byte[maskStride * size]);
        }

        return stream.ToArray();
    }

    // ICONDIR (tipo 2 = cursor, una imagen) y su ICONDIRENTRY con el punto activo.
    private static void WriteDirectory(BinaryWriter writer, int size, int hotspot, int imageBytes)
    {
        byte side = (byte)(size >= 256 ? 0 : size);
        writer.Write((ushort)0);
        writer.Write((ushort)2);
        writer.Write((ushort)1);
        writer.Write(side);
        writer.Write(side);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((ushort)hotspot);
        writer.Write((ushort)hotspot);
        writer.Write(imageBytes);
        writer.Write(DirectorySize + EntrySize);
    }

    // BITMAPINFOHEADER: la altura cuenta doble (imagen de color + máscara AND).
    private static void WriteInfoHeader(BinaryWriter writer, int size)
    {
        writer.Write(InfoHeaderSize);
        writer.Write(size);
        writer.Write(size * 2);
        writer.Write((ushort)1);
        writer.Write((ushort)32);
        for (int field = 0; field < 6; field++)
        {
            writer.Write(0);
        }
    }
}
