using System.Runtime.InteropServices;

// Carga el interop solo desde System32 (mitiga el secuestro de DLL — CA5392).
[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

namespace Acetato.Infrastructure.Interop;

/// <summary>
/// Declaraciones P/Invoke a la API Win32. El interop vive EXCLUSIVAMENTE en
/// Infrastructure (regla de capas) y no se expone fuera del ensamblado.
/// </summary>
internal static partial class NativeMethods
{
    // Mensajes de ventana y modificadores de RegisterHotKey.
    internal const uint WmHotkey = 0x0312;
    internal const uint WmQuit = 0x0012;
    internal const uint ModAlt = 0x0001;
    internal const uint ModControl = 0x0002;
    internal const uint ModNoRepeat = 0x4000;

    // Estilos extendidos de ventana del overlay.
    internal const int GwlExStyle = -20;
    internal const long WsExTransparent = 0x00000020;
    internal const long WsExToolWindow = 0x00000080;
    internal const long WsExLayered = 0x00080000;

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnregisterHotKey(nint hWnd, int id);

    [LibraryImport("user32.dll", EntryPoint = "GetMessageW", SetLastError = true)]
    internal static partial int GetMessage(out NativeMessage lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [LibraryImport("user32.dll", EntryPoint = "PostThreadMessageW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool PostThreadMessage(uint idThread, uint msg, nint wParam, nint lParam);

    [LibraryImport("kernel32.dll")]
    internal static partial uint GetCurrentThreadId();

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    internal static partial nint GetWindowLongPtr(nint hWnd, int nIndex);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    internal static partial nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);
}
