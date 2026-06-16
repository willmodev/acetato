using System.Runtime.InteropServices;
using Acetato.Application.Abstractions;
using Acetato.Application.Capture;
using Acetato.Application.Drawing;
using Acetato.Domain;

namespace Acetato.Application.Overlay;

/// <summary>
/// Reparte las acciones de dibujo entre los lienzos del overlay (uno por
/// monitor, HU-09). Las acciones globales (color, herramienta) van por el estado
/// compartido <see cref="IDrawingSettings"/>; "limpiar" afecta a cada lienzo;
/// "deshacer" solo al del monitor activo (bajo el cursor); la captura abarca el
/// escritorio virtual completo. Lo usan tanto los atajos como los botones de la
/// barra. Agnóstico de plataforma: depende solo de puertos.
/// </summary>
public sealed class OverlayBroadcaster
{
    private readonly IOverlayCanvasProvider _canvases;
    private readonly ICursorPositionProvider _cursor;
    private readonly IDrawingSettings _settings;
    private readonly ICaptureService _capture;

    public OverlayBroadcaster(
        IOverlayCanvasProvider canvases,
        ICursorPositionProvider cursor,
        IDrawingSettings settings,
        ICaptureService capture)
    {
        _canvases = canvases;
        _cursor = cursor;
        _settings = settings;
        _capture = capture;
    }

    /// <summary>Borra los trazos de todas las pantallas (HU-04/HU-09).</summary>
    public void Clear()
    {
        foreach (var canvas in _canvases.Canvases)
        {
            canvas.Clear();
        }
    }

    /// <summary>Deshace el último trazo de la pantalla bajo el cursor (HU-07/HU-09).</summary>
    public void Undo() => ActiveCanvas()?.Undo();

    /// <summary>Cambia la tinta activa; afecta a todas las pantallas (HU-05).</summary>
    public void SelectColor(TintaColor color) => _settings.SelectColor(color);

    /// <summary>Avanza a la siguiente herramienta del anillo (HU-11).</summary>
    public void CycleTool() => _settings.CycleTool();

    /// <summary>Selecciona una herramienta concreta; afecta a todas las pantallas (HU-13).</summary>
    public void SelectTool(ToolKind tool) => _settings.SelectTool(tool);

    /// <summary>
    /// Guarda el escritorio virtual anotado como PNG (HU-12). Se tragan fallas
    /// raras de E/S o GDI para no dejar la tarea sin observar.
    /// </summary>
    public async Task CaptureAsync()
    {
        try
        {
            await _capture.CaptureAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ExternalException)
        {
            // Falla rara de E/S o GDI: la captura no se realiza.
        }
    }

    // Lienzo del monitor que contiene el cursor; null si no hay lienzos.
    private IOverlayCanvas? ActiveCanvas()
    {
        var canvases = _canvases.Canvases;
        if (canvases.Count == 0)
        {
            return null;
        }

        var (x, y) = _cursor.GetCursorPosition();
        var monitors = new List<MonitorInfo>(canvases.Count);
        foreach (var canvas in canvases)
        {
            monitors.Add(canvas.Monitor);
        }

        return canvases[MonitorPicker.IndexAt(monitors, x, y)];
    }
}
