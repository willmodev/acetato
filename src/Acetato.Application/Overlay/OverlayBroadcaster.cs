using System.Runtime.InteropServices;
using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Domain;

namespace Acetato.Application.Overlay;

/// <summary>
/// Reparte las acciones de dibujo entre los lienzos del overlay (uno por
/// monitor, HU-09). Las acciones globales (color, herramienta, grosor) van por el estado
/// compartido <see cref="IDrawingSettings"/>; "limpiar" afecta a cada lienzo;
/// "deshacer" solo al del monitor activo (bajo el cursor); el recorte abre la
/// herramienta del sistema, que cubre todos los monitores. Lo usan tanto los atajos como los botones de la
/// barra. Agnóstico de plataforma: depende solo de puertos.
/// </summary>
public sealed class OverlayBroadcaster
{
    private readonly IOverlayCanvasProvider _canvases;
    private readonly ICursorPositionProvider _cursor;
    private readonly IDrawingSettings _settings;
    private readonly IScreenClipLauncher _screenClip;

    public OverlayBroadcaster(
        IOverlayCanvasProvider canvases,
        ICursorPositionProvider cursor,
        IDrawingSettings settings,
        IScreenClipLauncher screenClip)
    {
        _canvases = canvases;
        _cursor = cursor;
        _settings = settings;
        _screenClip = screenClip;
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

    /// <summary>Activa el modo Degradado; afecta a todas las pantallas (HU-19).</summary>
    public void SelectGradient() => _settings.SelectGradient();

    /// <summary>Avanza a la siguiente herramienta del anillo (HU-11).</summary>
    public void CycleTool() => _settings.CycleTool();

    /// <summary>Selecciona una herramienta concreta; afecta a todas las pantallas (HU-13).</summary>
    public void SelectTool(ToolKind tool) => _settings.SelectTool(tool);

    /// <summary>Sube el grosor un paso, con tope en el más grueso (HU-22).</summary>
    public void IncreaseThickness() => _settings.IncreaseThickness();

    /// <summary>Baja el grosor un paso, con tope en el más fino (HU-22).</summary>
    public void DecreaseThickness() => _settings.DecreaseThickness();

    /// <summary>
    /// Abre el recorte de pantalla del sistema (HU-23). Si no se puede abrir
    /// (protocolo ausente o bloqueado por política) la falla se traga y la app
    /// sigue como estaba.
    /// </summary>
    public void LaunchScreenClip()
    {
        try
        {
            _screenClip.Launch();
        }
        catch (ExternalException)
        {
            // El sistema no pudo abrir el recorte: el atajo no hace nada.
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
