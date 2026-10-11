namespace Acetato.Application.Hotkeys;

/// <summary>
/// Grupo de activación de un atajo (HU-21). Es fijo por acción: el usuario no
/// lo elige.
/// </summary>
public enum HotkeyScope
{
    /// <summary>Registrado mientras la app corre (mostrar/ocultar, modo dibujo).</summary>
    Always,

    /// <summary>Registrado solo con el overlay visible y en modo dibujo.</summary>
    DrawingMode,
}
