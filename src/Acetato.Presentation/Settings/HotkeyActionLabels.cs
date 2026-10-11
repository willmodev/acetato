using Acetato.Application.Abstractions;

namespace Acetato.Presentation.Settings;

/// <summary>
/// Nombre en español de cada acción con atajo (HU-21), tal como se lista en
/// Ajustes. Sentence case, sin punto final.
/// </summary>
internal static class HotkeyActionLabels
{
    private static readonly Dictionary<HotkeyAction, string> Labels = new()
    {
        [HotkeyAction.Toggle] = "Mostrar/ocultar",
        [HotkeyAction.DrawingModeToggle] = "Modo dibujo",
        [HotkeyAction.ColorRed] = "Tinta roja",
        [HotkeyAction.ColorBlue] = "Tinta azul",
        [HotkeyAction.ColorYellow] = "Tinta amarilla",
        [HotkeyAction.ColorGreen] = "Tinta verde",
        [HotkeyAction.ColorWhite] = "Tinta blanca",
        [HotkeyAction.ColorBlack] = "Tinta negra",
        [HotkeyAction.ColorGradient] = "Degradado",
        [HotkeyAction.Undo] = "Deshacer",
        [HotkeyAction.Clear] = "Limpiar todo",
        [HotkeyAction.CycleTool] = "Siguiente herramienta",
        [HotkeyAction.SelectTextTool] = "Texto",
        [HotkeyAction.SelectLaserTool] = "Láser",
        [HotkeyAction.Capture] = "Captura",
    };

    /// <summary>Nombre de la acción.</summary>
    public static string Of(HotkeyAction action) => Labels[action];
}
