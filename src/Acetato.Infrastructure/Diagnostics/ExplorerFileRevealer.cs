using System.Diagnostics;

namespace Acetato.Infrastructure.Diagnostics;

/// <summary>
/// Abre el Explorador de Windows con el reporte seleccionado (HU-33), con
/// <c>explorer.exe /select,"ruta"</c>.
/// </summary>
public sealed class ExplorerFileRevealer : IFileRevealer
{
    public void Reveal(string filePath)
    {
        var start = new ProcessStartInfo("explorer.exe", $"/select,\"{filePath}\"") { UseShellExecute = true };
        using var process = Process.Start(start);
    }
}
