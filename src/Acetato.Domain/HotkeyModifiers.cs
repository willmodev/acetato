namespace Acetato.Domain;

/// <summary>
/// Teclas modificadoras de un atajo (HU-21). Combinables entre sí. Tipo puro,
/// sin WPF ni Win32: Infrastructure y Presentation traducen desde y hacia aquí.
/// </summary>
[Flags]
public enum HotkeyModifiers
{
    /// <summary>Sin modificador (solo válido con F1–F12).</summary>
    None = 0,

    /// <summary>Control.</summary>
    Ctrl = 1,

    /// <summary>Alt.</summary>
    Alt = 2,

    /// <summary>Mayús. Nunca basta sola como modificador.</summary>
    Shift = 4,

    /// <summary>Tecla de Windows.</summary>
    Win = 8,
}
