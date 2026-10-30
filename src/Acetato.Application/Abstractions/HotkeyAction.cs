namespace Acetato.Application.Abstractions;

/// <summary>
/// Acciones disparables por atajos globales. El servicio de atajos
/// (Infrastructure) traduce cada combinación de teclas a una de estas
/// acciones; el enrutador (Presentation) la mapea a su comando. Añadir una
/// herramienta nueva = añadir un valor aquí y su definición en HotkeyCatalog.
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

    /// <summary>Modo Degradado: cada trazo con un par neón al azar (HU-19).</summary>
    ColorGradient,

    /// <summary>Deshacer el último trazo (HU-07).</summary>
    Undo,

    /// <summary>Cambiar a la siguiente herramienta del anillo (HU-11).</summary>
    CycleTool,

    /// <summary>
    /// Abrir el recorte de pantalla de Windows (HU-23). Conserva el nombre de la
    /// captura de HU-12 para que los <c>settings.json</c> existentes sigan valiendo.
    /// </summary>
    Capture,

    /// <summary>Seleccionar la herramienta Texto (HU-13).</summary>
    SelectTextTool,

    /// <summary>Seleccionar la herramienta Láser (HU-15).</summary>
    SelectLaserTool,

    /// <summary>Seleccionar la herramienta Lápiz (HU-22).</summary>
    SelectPencilTool,

    /// <summary>Seleccionar la herramienta Borrador (HU-22).</summary>
    SelectEraserTool,

    /// <summary>Seleccionar la herramienta Línea (HU-22).</summary>
    SelectLineTool,

    /// <summary>Seleccionar la herramienta Flecha (HU-22).</summary>
    SelectArrowTool,

    /// <summary>Seleccionar la herramienta Rectángulo (HU-22).</summary>
    SelectRectangleTool,

    /// <summary>Subir el grosor un paso, con tope en el más grueso (HU-22).</summary>
    ThicknessUp,

    /// <summary>Bajar el grosor un paso, con tope en el más fino (HU-22).</summary>
    ThicknessDown,

    /// <summary>
    /// Salir del modo dibujo sin ocultar nada (HU-24). Su atajo es fijo
    /// (<see cref="Hotkeys.FixedHotkeys"/>): no está en el catálogo ni en Ajustes.
    /// </summary>
    EnterPointerMode,
}
