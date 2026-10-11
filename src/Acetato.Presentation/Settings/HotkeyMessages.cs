using Acetato.Application.Abstractions;
using Acetato.Domain;

namespace Acetato.Presentation.Settings;

/// <summary>
/// Textos en español de Ajustes (HU-21): estados de la combinación y motivos
/// de rechazo que se muestran en línea bajo la fila.
/// </summary>
internal static class HotkeyMessages
{
    /// <summary>Acción sin combinación.</summary>
    public const string NoChord = "Sin atajo";

    /// <summary>Fila esperando la combinación.</summary>
    public const string Capturing = "Pulsa la combinación…";

    /// <summary>Tecla fuera de la lista blanca.</summary>
    public const string UnsupportedKey = "Tecla no admitida";

    /// <summary>La combinación la tiene otra app o Windows.</summary>
    public const string TakenByOtherApp = "En uso por otra aplicación";

    /// <summary>Motivo por el que la combinación no cumple las reglas.</summary>
    public static string Of(KeyChordError error) => error switch
    {
        KeyChordError.MissingModifier => "Usa Ctrl, Alt o Win",
        KeyChordError.ShiftOnly => "Mayús necesita otro modificador",
        _ => "Reservado por Windows",
    };

    /// <summary>Conflicto con otra acción de Acetato.</summary>
    public static string TakenBy(HotkeyAction owner) => $"Ya la usa {HotkeyActionLabels.Of(owner)}";
}
