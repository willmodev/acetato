namespace Acetato.Application.Abstractions;

/// <summary>
/// Acciones disparables por atajos globales. El servicio de atajos
/// (Infrastructure) traduce cada combinación de teclas a una de estas
/// acciones; el enrutador (Presentation) la mapea a su comando. Añadir una
/// herramienta nueva = añadir un valor aquí y su fila en la tabla de bindings.
/// </summary>
public enum HotkeyAction
{
    /// <summary>Mostrar/ocultar el overlay (HU-01).</summary>
    Toggle,

    /// <summary>Alternar el modo dibujo/normal (HU-02).</summary>
    DrawingModeToggle,

    /// <summary>Limpiar todos los trazos (HU-04).</summary>
    Clear,

    /// <summary>Tinta roja (HU-05).</summary>
    ColorRed,

    /// <summary>Tinta azul (HU-05).</summary>
    ColorBlue,

    /// <summary>Tinta amarilla (HU-05).</summary>
    ColorYellow,

    /// <summary>Tinta verde (HU-05).</summary>
    ColorGreen,

    /// <summary>Tinta blanca (HU-05).</summary>
    ColorWhite,

    /// <summary>Tinta negra (HU-05).</summary>
    ColorBlack,

    /// <summary>Deshacer el último trazo (HU-07).</summary>
    Undo,

    /// <summary>Cambiar a la siguiente herramienta del anillo (HU-11).</summary>
    CycleTool,

    /// <summary>Guardar la pantalla anotada como PNG (HU-12).</summary>
    Capture,

    /// <summary>Seleccionar la herramienta Texto (HU-13).</summary>
    SelectTextTool,

    /// <summary>Seleccionar la herramienta Láser (HU-15).</summary>
    SelectLaserTool,
}
