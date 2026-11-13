using System.Text.RegularExpressions;

namespace Acetato.Application.Diagnostics;

/// <summary>
/// Quita del reporte para soporte (HU-33) el nombre de usuario de Windows y el del equipo:
/// los cambia por <c>&lt;usuario&gt;</c> y <c>&lt;equipo&gt;</c>, como hace PowerToys. Solo
/// reemplaza el nombre completo, dentro de rutas (<c>\Users\ana\</c>) o como palabra suelta;
/// no toca palabras que solo lo contienen («banana», «Mariana»). No distingue mayúsculas.
/// </summary>
public static class ReportRedactor
{
    /// <summary>Lo que queda en lugar del nombre de usuario.</summary>
    public const string UserPlaceholder = "<usuario>";

    /// <summary>Lo que queda en lugar del nombre del equipo.</summary>
    public const string MachinePlaceholder = "<equipo>";

    private const string UserGroup = "user";

    // Un nombre cuenta solo si no lo rodean letras, dígitos ni guion bajo.
    private const string WordStart = @"(?<![\p{L}\p{N}_])";
    private const string WordEnd = @"(?![\p{L}\p{N}_])";

    private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(2);

    /// <summary>Texto con el usuario y el equipo cambiados; igual si los nombres están vacíos.</summary>
    public static string Redact(string text, string userName, string machineName)
    {
        ArgumentNullException.ThrowIfNull(text);
        var names = Alternatives(userName, machineName);
        if (names.Count == 0)
        {
            return text;
        }

        string pattern = WordStart + "(?:" + string.Join('|', names) + ")" + WordEnd;
        return Regex.Replace(
            text,
            pattern,
            match => match.Groups[UserGroup].Success ? UserPlaceholder : MachinePlaceholder,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            MatchTimeout);
    }

    // Una sola pasada, con el nombre más largo primero: así «ana-pc» (equipo) no queda
    // como «<usuario>-pc» cuando el usuario es «ana», y un reemplazo no se vuelve a tocar.
    private static List<string> Alternatives(string userName, string machineName)
    {
        var names = new List<(string Name, string Group)>(2);
        if (!string.IsNullOrWhiteSpace(userName))
        {
            names.Add((userName.Trim(), UserGroup));
        }

        if (!string.IsNullOrWhiteSpace(machineName))
        {
            names.Add((machineName.Trim(), "machine"));
        }

        return [.. names
            .OrderByDescending(name => name.Name.Length)
            .Select(name => $"(?<{name.Group}>{Regex.Escape(name.Name)})")];
    }
}
