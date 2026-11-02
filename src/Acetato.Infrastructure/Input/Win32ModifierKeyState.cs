using Acetato.Application.Abstractions;
using Acetato.Infrastructure.Interop;

namespace Acetato.Infrastructure.Input;

/// <summary>
/// Lee si Ctrl está pulsada vía Win32 (SPEC 11). <c>GetAsyncKeyState</c> da el
/// estado físico de la tecla aunque la ventana no tenga el foco. El interop vive
/// en Infrastructure.
/// </summary>
public sealed class Win32ModifierKeyState : IModifierKeyState
{
    // Bit alto del resultado: la tecla está abajo ahora mismo.
    private const short KeyDownMask = unchecked((short)0x8000);

    public bool IsCtrlDown => (NativeMethods.GetAsyncKeyState(NativeMethods.VkControl) & KeyDownMask) != 0;
}
