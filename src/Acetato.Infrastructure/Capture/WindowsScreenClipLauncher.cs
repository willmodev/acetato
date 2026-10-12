using System.Diagnostics;
using Acetato.Application.Abstractions;

namespace Acetato.Infrastructure.Capture;

/// <summary>
/// Abre la herramienta Recortes de Windows con el protocolo <c>ms-screenclip:</c>
/// (HU-23). El shell resuelve el protocolo, así que hace falta
/// <c>UseShellExecute</c>. Si el protocolo no existe o una política lo bloquea,
/// la excepción sube al caso de uso, que la traga.
/// </summary>
public sealed class WindowsScreenClipLauncher : IScreenClipLauncher
{
    private const string ScreenClipUri = "ms-screenclip:";

    public void Launch()
    {
        using var process = Process.Start(new ProcessStartInfo(ScreenClipUri) { UseShellExecute = true });
    }
}
