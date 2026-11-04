using System.IO;
using System.Runtime.InteropServices;
using Acetato.Application.Abstractions;
using CommunityToolkit.WinUI.Notifications;
using Microsoft.Extensions.Logging;

namespace Acetato.Infrastructure.Diagnostics;

/// <summary>
/// Aviso de Windows tras un error de la interfaz del que Acetato se recuperó (HU-32). No
/// lleva argumentos ni botones, así que hacer clic en él no abre nada. Nunca lanza: si los
/// avisos de Windows están apagados o fallan, el error ya quedó anotado y no pasa nada más.
/// </summary>
public sealed partial class ToastErrorNotifier : IErrorNotifier
{
    private const string Title = "Algo falló, pero Acetato sigue abierto";
    private const string Hint = "Si se repite, usa Ajustes › Reportar problema";

    private readonly ILogger _logger;

    public ToastErrorNotifier(ILogger<ToastErrorNotifier> logger) => _logger = logger;

    public void NotifyRecovered()
    {
        try
        {
            new ToastContentBuilder().AddText(Title).AddText(Hint).Show();
        }
        catch (Exception ex) when (ex is ExternalException or InvalidOperationException or UnauthorizedAccessException or IOException)
        {
            LogToastFailed(ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "No salió el aviso de Windows del error")]
    private partial void LogToastFailed(Exception exception);
}
