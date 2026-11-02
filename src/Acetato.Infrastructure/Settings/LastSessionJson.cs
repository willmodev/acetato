using System.Text.Json.Nodes;
using Acetato.Application.Abstractions;
using Acetato.Domain;

namespace Acetato.Infrastructure.Settings;

/// <summary>
/// Sección de la última sesión de settings.json (HU-29):
/// <c>"session": { "ink": "Blue", "gradient": false, "thickness": 3, "tool": "Arrow",
/// "toolbar": { "collapsed": false, "placement": { "monitor": { … }, "x": 812, "y": 64 } } }</c>.
/// Tinta y herramienta van por nombre. Cada campo ausente, desconocido o fuera de rango
/// vale lo de fábrica por su cuenta: una sesión a medias se aprovecha en lo que sirve.
/// </summary>
internal static class LastSessionJson
{
    /// <summary>Nombre de la sección de la última sesión en settings.json.</summary>
    public const string SectionKey = "session";

    private const string InkKey = "ink";
    private const string GradientKey = "gradient";
    private const string ThicknessKey = "thickness";
    private const string ToolKey = "tool";
    private const string ToolbarKey = "toolbar";
    private const string CollapsedKey = "collapsed";
    private const string PlacementKey = "placement";
    private const string MonitorKey = "monitor";
    private const string XKey = "x";
    private const string YKey = "y";
    private const string WidthKey = "width";
    private const string HeightKey = "height";

    /// <summary>Sección <c>session</c> con la sesión dada.</summary>
    public static JsonObject ToSection(LastSession session)
    {
        var toolbar = new JsonObject { [CollapsedKey] = session.ToolbarCollapsed };
        if (session.ToolbarPlacement is { } placement)
        {
            toolbar[PlacementKey] = PlacementSection(placement);
        }

        return new JsonObject
        {
            [InkKey] = session.Ink.ToString(),
            [GradientKey] = session.Gradient,
            [ThicknessKey] = session.ThicknessIndex,
            [ToolKey] = session.Tool.ToString(),
            [ToolbarKey] = toolbar,
        };
    }

    /// <summary>Sesión leída del objeto raíz del archivo.</summary>
    public static LastSession Read(JsonObject root)
    {
        if (root[SectionKey] is not JsonObject section)
        {
            return LastSession.Defaults;
        }

        var defaults = LastSession.Defaults;
        var toolbar = section[ToolbarKey] as JsonObject;
        return new LastSession(
            Name(section, InkKey, defaults.Ink),
            Flag(section, GradientKey, defaults.Gradient),
            Thickness(section, defaults.ThicknessIndex),
            Name(section, ToolKey, defaults.Tool),
            toolbar is not null && Flag(toolbar, CollapsedKey, defaults.ToolbarCollapsed),
            toolbar?[PlacementKey] is JsonObject placement ? ReadPlacement(placement) : null);
    }

    private static JsonObject PlacementSection(ToolbarPlacement placement) => new()
    {
        [MonitorKey] = new JsonObject
        {
            [XKey] = placement.Monitor.X,
            [YKey] = placement.Monitor.Y,
            [WidthKey] = placement.Monitor.Width,
            [HeightKey] = placement.Monitor.Height,
        },
        [XKey] = placement.X,
        [YKey] = placement.Y,
    };

    // Posición de la barra; null si le falta algún número o su monitor no sirve.
    private static ToolbarPlacement? ReadPlacement(JsonObject placement)
    {
        if (placement[MonitorKey] is not JsonObject monitor || ReadMonitor(monitor) is not { } home)
        {
            return null;
        }

        return Integer(placement, XKey) is { } x && Integer(placement, YKey) is { } y
            ? new ToolbarPlacement(home, x, y)
            : null;
    }

    // Monitor de la barra; null si le falta algún número o no tiene tamaño.
    private static MonitorInfo? ReadMonitor(JsonObject monitor)
    {
        if (Integer(monitor, XKey) is not { } x || Integer(monitor, YKey) is not { } y)
        {
            return null;
        }

        return Integer(monitor, WidthKey) is int width and > 0 && Integer(monitor, HeightKey) is int height and > 0
            ? new MonitorInfo(x, y, width, height, IsPrimary: false)
            : null;
    }

    // Grosor guardado; fuera de la escala vale el de fábrica.
    private static int Thickness(JsonObject section, int fallback) =>
        Integer(section, ThicknessKey) is { } index && index >= ThicknessScale.MinIndex && index <= ThicknessScale.MaxIndex
            ? index
            : fallback;

    // Valor de un enum por su nombre exacto; un nombre desconocido o un número valen el de fábrica.
    private static TEnum Name<TEnum>(JsonObject section, string key, TEnum fallback)
        where TEnum : struct, Enum =>
        section[key] is JsonValue value
        && value.TryGetValue(out string? text)
        && Enum.GetNames<TEnum>().Contains(text, StringComparer.Ordinal)
            ? Enum.Parse<TEnum>(text!)
            : fallback;

    private static bool Flag(JsonObject section, string key, bool fallback) =>
        section[key] is JsonValue value && value.TryGetValue(out bool flag) ? flag : fallback;

    private static int? Integer(JsonObject section, string key) =>
        section[key] is JsonValue value && value.TryGetValue(out int number) ? number : null;
}
