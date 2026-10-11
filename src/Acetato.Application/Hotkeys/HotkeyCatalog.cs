using Acetato.Application.Abstractions;
using Acetato.Domain;

namespace Acetato.Application.Hotkeys;

/// <summary>
/// Las definiciones de los atajos (HU-21), una por <see cref="HotkeyAction"/>, en
/// el orden en que se muestran en Ajustes. Es la única fuente de los
/// predeterminados.
/// </summary>
public static class HotkeyCatalog
{
    private const HotkeyModifiers CtrlAlt = HotkeyModifiers.Ctrl | HotkeyModifiers.Alt;

    /// <summary>Las 15 definiciones, en orden de presentación.</summary>
    public static IReadOnlyList<HotkeyDefinition> All { get; } =
    [
        Always(HotkeyAction.Toggle, HotkeyKey.D),
        Always(HotkeyAction.DrawingModeToggle, HotkeyKey.E),
        Drawing(HotkeyAction.ColorRed, HotkeyKey.D1),
        Drawing(HotkeyAction.ColorBlue, HotkeyKey.D2),
        Drawing(HotkeyAction.ColorYellow, HotkeyKey.D3),
        Drawing(HotkeyAction.ColorGreen, HotkeyKey.D4),
        Drawing(HotkeyAction.ColorWhite, HotkeyKey.D5),
        Drawing(HotkeyAction.ColorBlack, HotkeyKey.D6),
        Drawing(HotkeyAction.ColorGradient, HotkeyKey.D7),
        Drawing(HotkeyAction.Undo, HotkeyKey.Z),
        Drawing(HotkeyAction.Clear, HotkeyKey.Back),
        Drawing(HotkeyAction.CycleTool, HotkeyKey.Space),
        Drawing(HotkeyAction.SelectTextTool, HotkeyKey.T),
        Drawing(HotkeyAction.SelectLaserTool, HotkeyKey.L),
        Drawing(HotkeyAction.Capture, HotkeyKey.S),
    ];

    private static readonly Dictionary<HotkeyAction, HotkeyDefinition> ByAction =
        All.ToDictionary(definition => definition.Action);

    /// <summary>Definición de <paramref name="action"/>.</summary>
    public static HotkeyDefinition Of(HotkeyAction action) => ByAction[action];

    private static HotkeyDefinition Always(HotkeyAction action, HotkeyKey key) =>
        new(action, HotkeyScope.Always, new KeyChord(CtrlAlt, key));

    private static HotkeyDefinition Drawing(HotkeyAction action, HotkeyKey key) =>
        new(action, HotkeyScope.DrawingMode, new KeyChord(CtrlAlt, key));
}
