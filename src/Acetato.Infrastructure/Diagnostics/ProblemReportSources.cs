using Acetato.Infrastructure.Settings;

namespace Acetato.Infrastructure.Diagnostics;

/// <summary>De dónde sale y a dónde va el reporte para soporte (HU-33).</summary>
/// <param name="LogsDirectory">Carpeta del registro.</param>
/// <param name="SettingsDirectory">Carpeta de settings.json.</param>
/// <param name="DesktopDirectory">Carpeta donde se guarda el .zip.</param>
public sealed record ProblemReportSources(string LogsDirectory, string SettingsDirectory, string DesktopDirectory)
{
    /// <summary>
    /// Las carpetas de la app y el escritorio que reporta Windows (si está redirigido a
    /// OneDrive, el de OneDrive).
    /// </summary>
    public static ProblemReportSources Default => new(
        LogDirectory.Default,
        SettingsFile.DefaultDirectory,
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
}
