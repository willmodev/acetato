using System.IO;

namespace Acetato.Infrastructure.Diagnostics;

/// <summary>
/// Carpeta del registro (HU-32): %LocalAppData%\Acetato\logs. Es la carpeta local del
/// usuario, la misma que usa PowerToys. A diferencia de %AppData%, donde vive
/// settings.json, no se copia entre equipos en redes de empresa.
/// </summary>
public static class LogDirectory
{
    /// <summary>Ruta de la carpeta; puede no existir aún (Serilog la crea al escribir).</summary>
    public static string Default =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Acetato", "logs");
}
