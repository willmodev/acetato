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
    internal const uint WmApp = 0x8000; // Aviso de petición encolada al hilo de atajos (HU-21)
    internal const uint WmNcLButtonDown = 0x00A1; // Clic en área no cliente (arrastre)
    internal const uint ModAlt = 0x0001;
    internal const uint ModControl = 0x0002;
    internal const uint ModShift = 0x0004;
    internal const uint ModWin = 0x0008;
    internal const uint ModNoRepeat = 0x4000;

    // PeekMessage sin retirar: fuerza a crear la cola de mensajes del hilo.
    internal const uint PmNoRemove = 0x0000;

    // Hit-test para arrastrar la barra como si pulsaran su barra de título.
    internal const int HtCaption = 2;

    // Virtual-key codes de las teclas de atajo (HU-21). Las series A–Z, 0–9
    // (fila superior), F1–F12 y numpad son contiguas: basta con su inicio.
    internal const uint VkA = 0x41;
    internal const uint Vk0 = 0x30;
    internal const uint VkF1 = 0x70;
    internal const uint VkNumPad0 = 0x60;
    internal const uint VkBack = 0x08; // Retroceso
    internal const uint VkSpace = 0x20; // Espacio
    internal const uint VkPrior = 0x21; // RePág
    internal const uint VkNext = 0x22; // AvPág
    internal const uint VkEnd = 0x23;
    internal const uint VkHome = 0x24;
    internal const uint VkLeft = 0x25;
    internal const uint VkUp = 0x26;
    internal const uint VkRight = 0x27;
    internal const uint VkDown = 0x28;
    internal const uint VkInsert = 0x2D;
    internal const uint VkDelete = 0x2E; // Supr

    // Estilos extendidos de ventana del overlay y de la barra.
    internal const int GwlExStyle = -20;
    internal const long WsExTransparent = 0x00000020;
    internal const long WsExToolWindow = 0x00000080;
    internal const long WsExLayered = 0x00080000;
    internal const long WsExNoActivate = 0x08000000; // La barra no roba el foco

    // Métricas de pantalla del monitor primario (HU-12).
    internal const int SmCxScreen = 0;
    internal const int SmCyScreen = 1;

    // Comando de presentación para SetWindowPlacement: maximizar (HU-09).
    internal const uint SwShowMaximized = 3;

    // Afinidad de presentación: la barra queda visible pero fuera de toda captura (HU-12).
    internal const uint WdaExcludeFromCapture = 0x00000011;

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnregisterHotKey(nint hWnd, int id);

    [LibraryImport("user32.dll", EntryPoint = "GetMessageW", SetLastError = true)]
    internal static partial int GetMessage(out NativeMessage lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [LibraryImport("user32.dll", EntryPoint = "PeekMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool PeekMessage(out NativeMessage lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [LibraryImport("user32.dll", EntryPoint = "PostThreadMessageW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool PostThreadMessage(uint idThread, uint msg, nint wParam, nint lParam);

    [LibraryImport("kernel32.dll")]
    internal static partial uint GetCurrentThreadId();

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    internal static partial nint GetWindowLongPtr(nint hWnd, int nIndex);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    internal static partial nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ReleaseCapture();

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    internal static partial nint SendMessage(nint hWnd, uint msg, nint wParam, nint lParam);

    [LibraryImport("user32.dll")]
    internal static partial int GetSystemMetrics(int nIndex);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetWindowDisplayAffinity(nint hWnd, uint dwAffinity);

    // Posición del cursor (HU-09) para elegir el monitor activo (barra/deshacer).
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetCursorPos(out NativePoint lpPoint);

    // Posiciona/maximiza una ventana en píxeles físicos (HU-09): rcNormalPosition
    // en coordenadas del monitor objetivo + SW_SHOWMAXIMIZED → Windows maximiza en
    // ese monitor y WPF adopta su DPI. Solo 2 parámetros (a diferencia de
    // MoveWindow/SetWindowPos, que romperían S107).
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetWindowPlacement(nint hWnd, ref WindowPlacementNative lpwndpl);
}
