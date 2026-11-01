using System.Windows.Input;
using Acetato.Application.Abstractions;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Estado de Ctrl según WPF (SPEC 11). Respaldo cuando no se inyecta el adaptador de
/// Win32 (p. ej. en tests): solo es fiable con la ventana en primer plano.
/// </summary>
internal sealed class WpfModifierKeyState : IModifierKeyState
{
    public static WpfModifierKeyState Instance { get; } = new();

    public bool IsCtrlDown => Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
}
