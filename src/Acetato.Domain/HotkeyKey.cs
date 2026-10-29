namespace Acetato.Domain;

/// <summary>
/// Tecla final de un atajo (HU-21). El enum ES la lista blanca: solo teclas
/// que no cambian de sitio entre distribuciones de teclado. Las de símbolo
/// (<c>+</c>, <c>ñ</c>, <c>[</c>…) no se pueden ni representar. El nombre de
/// cada valor es el que se guarda en settings.json, así que no se renombra.
/// </summary>
public enum HotkeyKey
{
    A,
    B,
    C,
    D,
    E,
    F,
    G,
    H,
    I,
    J,
    K,
    L,
    M,
    N,
    O,
    P,
    Q,
    R,
    S,
    T,
    U,
    V,
    W,
    X,
    Y,
    Z,

    /// <summary>Dígitos de la fila superior (no el teclado numérico).</summary>
    D0,
    D1,
    D2,
    D3,
    D4,
    D5,
    D6,
    D7,
    D8,
    D9,

    F1,
    F2,
    F3,
    F4,
    F5,
    F6,
    F7,
    F8,
    F9,
    F10,
    F11,
    F12,

    Space,

    /// <summary>Retroceso.</summary>
    Back,

    /// <summary>Supr.</summary>
    Delete,
    Insert,
    Home,
    End,
    PageUp,
    PageDown,
    Left,
    Up,
    Right,
    Down,

    NumPad0,
    NumPad1,
    NumPad2,
    NumPad3,
    NumPad4,
    NumPad5,
    NumPad6,
    NumPad7,
    NumPad8,
    NumPad9,

    /// <summary>
    /// Esc. Solo para el atajo fijo del Puntero (HU-24): en Ajustes no se puede
    /// asignar (la captura la usa para cancelar).
    /// </summary>
    Escape,
}
