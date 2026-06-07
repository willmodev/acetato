using System.Drawing;
using System.Drawing.Imaging;
using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Capture;

/// <summary>
/// Adaptador de captura por GDI (HU-12). Copia el monitor primario —ya compuesto
/// con los trazos del overlay; la barra se excluye con WDA_EXCLUDEFROMCAPTURE— y
/// lo guarda como PNG. Usa Graphics.CopyFromScreen para evitar el P/Invoke BitBlt
/// de 9 parámetros (S107). El interop vive en Infrastructure.
/// </summary>
public sealed class GdiScreenCaptureService : IScreenCaptureService
{
    public void CaptureToPng(string filePath)
    {
        int width = NativeMethods.GetSystemMetrics(NativeMethods.SmCxScreen);
        int height = NativeMethods.GetSystemMetrics(NativeMethods.SmCyScreen);

        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
        }

        bitmap.Save(filePath, ImageFormat.Png);
    }
}
