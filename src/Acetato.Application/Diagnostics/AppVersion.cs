namespace Acetato.Application.Diagnostics;

/// <summary>
/// Versión de Acetato (HU-33). <see cref="Display"/> es la que ve el usuario (0.9.0);
/// <see cref="Full"/> lleva además el código del commit (0.9.0+0ba354c…) y va al
/// registro y al reporte para soporte.
/// </summary>
/// <param name="Display">Versión sin el código del commit.</param>
/// <param name="Full">Versión completa, tal como la declara el ensamblado.</param>
public sealed record AppVersion(string Display, string Full)
{
    private const char MetadataSeparator = '+';

    /// <summary>Versión cuando el ensamblado no declara ninguna.</summary>
    public static AppVersion Unknown { get; } = new("0.0.0", "0.0.0");

    /// <summary>
    /// Lee la versión informativa del ensamblado y corta en el <c>+</c> la que se
    /// muestra en pantalla. Vacía o en blanco vale <see cref="Unknown"/>.
    /// </summary>
    public static AppVersion Parse(string? informationalVersion)
    {
        string full = informationalVersion?.Trim() ?? string.Empty;
        int separator = full.IndexOf(MetadataSeparator, StringComparison.Ordinal);
        string display = separator < 0 ? full : full[..separator];
        return display.Length == 0 ? Unknown : new AppVersion(display, full);
    }
}
