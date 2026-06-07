using System.Globalization;
using Acetato.Application.Abstractions;

namespace Acetato.Application.Capture;

/// <summary>
/// Caso de uso de captura (HU-12): compone la ruta de destino con marca de
/// tiempo, asegura la carpeta y delega la captura del monitor primario en el
/// puerto de plataforma. Agnóstico de WPF/Win32; la E/S corre fuera del hilo de
/// UI para no provocar saltos al guardar.
/// </summary>
public sealed class CaptureService : ICaptureService
{
    private const string FilePrefix = "acetato-";
    private const string TimestampFormat = "yyyyMMdd-HHmmss";

    private readonly IScreenCaptureService _screen;
    private readonly ICaptureDirectoryProvider _directory;
    private readonly ICaptureNotifier _notifier;
    private readonly TimeProvider _clock;

    public CaptureService(
        IScreenCaptureService screen,
        ICaptureDirectoryProvider directory,
        ICaptureNotifier notifier,
        TimeProvider clock)
    {
        _screen = screen;
        _directory = directory;
        _notifier = notifier;
        _clock = clock;
    }

    public async Task<CaptureResult> CaptureAsync()
    {
        return await Task.Run(Capture).ConfigureAwait(false);
    }

    private CaptureResult Capture()
    {
        string path = BuildPath();
        _screen.CaptureToPng(path);
        _notifier.NotifyCaptured(path);
        return new CaptureResult(path);
    }

    // Imágenes\Acetato\acetato-AAAAMMDD-HHmmss.png; el sello evita sobrescrituras.
    private string BuildPath()
    {
        string folder = _directory.GetCaptureDirectory();
        Directory.CreateDirectory(folder);
        string stamp = _clock.GetLocalNow().ToString(TimestampFormat, CultureInfo.InvariantCulture);
        return Path.Combine(folder, $"{FilePrefix}{stamp}.png");
    }
}
