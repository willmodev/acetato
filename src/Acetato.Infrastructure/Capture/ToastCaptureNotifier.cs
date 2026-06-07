using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Acetato.Application.Abstractions;
using CommunityToolkit.WinUI.Notifications;

namespace Acetato.Infrastructure.Capture;

/// <summary>
/// Muestra una notificación de Windows al guardar una captura (HU-12). Al hacer
/// clic abre el PNG con el visor predeterminado, como el recorte de Windows. Usa
/// CommunityToolkit, que registra la identidad de la app (sin acceso directo en
/// Inicio) para que el sistema no descarte el toast. El interop vive en
/// Infrastructure.
/// </summary>
public sealed class ToastCaptureNotifier : ICaptureNotifier, IDisposable
{
    private const string FileArgument = "file";

    public ToastCaptureNotifier() => ToastNotificationManagerCompat.OnActivated += OnToastActivated;

    public void NotifyCaptured(string filePath)
    {
        try
        {
            new ToastContentBuilder()
                .AddArgument(FileArgument, filePath)
                .AddText("Captura guardada")
                .AddText(Path.GetFileName(filePath))
                .Show();
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException)
        {
            // Best-effort: un aviso fallido no debe romper la captura ya guardada.
        }
    }

    // Clic en el toast: abrir la imagen indicada en sus argumentos.
    private static void OnToastActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        var arguments = ToastArguments.Parse(e.Argument);
        if (arguments.Contains(FileArgument))
        {
            OpenFile(arguments[FileArgument]);
        }
    }

    private static void OpenFile(string path)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or FileNotFoundException)
        {
            // El archivo pudo moverse o borrarse entre la captura y el clic; ignorar.
        }
    }

    public void Dispose() => ToastNotificationManagerCompat.OnActivated -= OnToastActivated;
}
