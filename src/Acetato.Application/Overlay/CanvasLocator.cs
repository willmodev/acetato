using Acetato.Application.Abstractions;
using Acetato.Domain;

namespace Acetato.Application.Overlay;

/// <summary>
/// Busca el lienzo del monitor que contiene el cursor (HU-09). Lo comparten Deshacer y
/// borrar la selección (<see cref="OverlayBroadcaster"/>) y la pizarra (HU-35).
/// </summary>
internal static class CanvasLocator
{
    /// <summary>Lienzo bajo el cursor; <c>null</c> si no hay lienzos.</summary>
    public static IOverlayCanvas? AtCursor(IReadOnlyList<IOverlayCanvas> canvases, ICursorPositionProvider cursor)
    {
        if (canvases.Count == 0)
        {
            return null;
        }

        var (x, y) = cursor.GetCursorPosition();
        var monitors = new List<MonitorInfo>(canvases.Count);
        foreach (var canvas in canvases)
        {
            monitors.Add(canvas.Monitor);
        }

        return canvases[MonitorPicker.IndexAt(monitors, x, y)];
    }
}
