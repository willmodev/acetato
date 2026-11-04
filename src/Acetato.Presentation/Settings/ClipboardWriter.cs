using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace Acetato.Presentation.Settings;

/// <summary>
/// Envuelve <see cref="Clipboard.SetText(string)"/> para poder probar Ajustes sin tocar el
/// portapapeles (HU-33). Si otra app lo tiene tomado, no copia y lo anota.
/// </summary>
public sealed partial class ClipboardWriter : IClipboardWriter
{
    private readonly ILogger _logger;

    public ClipboardWriter(ILogger<ClipboardWriter> logger) => _logger = logger;

    public void SetText(string text)
    {
        try
        {
            Clipboard.SetText(text);
        }
        catch (ExternalException ex)
        {
            LogCopyFailed(ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "No se pudo copiar al portapapeles")]
    private partial void LogCopyFailed(Exception exception);
}
