using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32;

namespace Acetato.Infrastructure.Diagnostics;

/// <summary>
/// Datos del equipo que van a la línea «Inicio» del registro y a sistema.txt (HU-32,
/// HU-33): Windows con su versión y compilación, .NET e idioma. Nunca lanza: lo que no
/// se pueda leer queda en blanco.
/// </summary>
public static class SystemDescription
{
    private const string CurrentVersionKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    // Windows 11 conserva la versión 10.0; se distingue por la compilación.
    private const int FirstWindows11Build = 22000;

    /// <summary>Por ejemplo «Windows 11 25H2 (10.0.26300.1234)».</summary>
    public static string WindowsVersion()
    {
        var version = Environment.OSVersion.Version;
        string name = version.Build >= FirstWindows11Build ? "Windows 11" : "Windows 10";
        string release = ReadCurrentVersion("DisplayVersion") is string display ? $" {display}" : string.Empty;
        string revision = ReadCurrentVersion("UBR") is int ubr ? ubr.ToString(CultureInfo.InvariantCulture) : "0";
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{name}{release} ({version.Major}.{version.Minor}.{version.Build}.{revision})");
    }

    /// <summary>Por ejemplo «.NET 10.0.10».</summary>
    public static string RuntimeVersion => RuntimeInformation.FrameworkDescription;

    /// <summary>Idioma de la interfaz de Windows, por ejemplo «es-CO».</summary>
    public static string Language => CultureInfo.CurrentUICulture.Name;

    private static object? ReadCurrentVersion(string valueName)
    {
        try
        {
            return Registry.GetValue(CurrentVersionKey, valueName, defaultValue: null);
        }
        catch (Exception ex) when (ex is SecurityException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
