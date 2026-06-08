using System.Drawing;
using System.Drawing.Imaging;
using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Capture;

/// <summary>
/// Adaptador de captura por GDI (HU-12). Copia el escritorio virtual completo
/// —todos los monitores (HU-09), ya compuestos con los trazos del overlay; la
/// barra se excluye con WDA_EXCLUDEFROMCAPTURE— y lo guarda como PNG. Usa
/// Graphics.CopyFromScreen para evitar el P/Invoke BitBlt de 9 parámetros (S107).
/// El interop vive en Infrastructure.
/// </summary>
public sealed class GdiScreenCaptureService : IScreenCaptureService
{
    public void CaptureToPng(string filePath)
    {
        // Origen y tamaño del escritorio virtual; el origen puede ser negativo si
        // hay monitores a la izquierda o arriba del primario.
        int x = NativeMethods.GetSystemMetrics(NativeMethods.SmXVirtualScreen);
        int y = NativeMethods.GetSystemMetrics(NativeMethods.SmYVirtualScreen);
        int width = NativeMethods.GetSystemMetrics(NativeMethods.SmCxVirtualScreen);
        int height = NativeMethods.GetSystemMetrics(NativeMethods.SmCyVirtualScreen);

        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(x, y, 0, 0, bitmap.Size);
        }

        bitmap.Save(filePath, ImageFormat.Png);
    }
}
