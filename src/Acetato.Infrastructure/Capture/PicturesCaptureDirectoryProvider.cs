using Acetato.Application.Abstractions;

namespace Acetato.Infrastructure.Capture;

/// <summary>
/// Resuelve la carpeta de capturas (HU-12) en Imágenes\Acetato del usuario. El
/// caso de uso asegura su existencia antes de guardar.
/// </summary>
public sealed class PicturesCaptureDirectoryProvider : ICaptureDirectoryProvider
{
    private const string SubfolderName = "Acetato";

    public string GetCaptureDirectory()
    {
        string pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        return Path.Combine(pictures, SubfolderName);
    }
}
