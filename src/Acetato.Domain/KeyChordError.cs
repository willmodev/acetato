namespace Acetato.Domain;

/// <summary>Motivo por el que una combinación no se acepta como atajo (HU-21).</summary>
public enum KeyChordError
{
    /// <summary>Falta Ctrl, Alt o Win (y la tecla no es F1–F12).</summary>
    MissingModifier,

    /// <summary>Mayús sin otro modificador que la acompañe.</summary>
    ShiftOnly,

    /// <summary>Combinación reservada por Windows que el registro no bloquea.</summary>
    SystemShortcut,
}
