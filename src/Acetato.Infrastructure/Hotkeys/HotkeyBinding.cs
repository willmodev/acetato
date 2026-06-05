using Acetato.Application.Abstractions;

namespace Acetato.Infrastructure.Hotkeys;

/// <summary>
/// Asociación inmutable entre el id de un atajo (el wParam de WM_HOTKEY), la
/// acción que dispara y la combinación de teclas que lo activa.
/// </summary>
internal sealed record HotkeyBinding(int Id, HotkeyAction Action, uint Modifiers, uint VirtualKey);
