using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Domain;

namespace Acetato.Application.Overlay;

/// <summary>
/// Caso de uso de la pizarra (HU-35). Recorre apagada → blanca → negra → apagada con un
/// solo botón o atajo, la prende solo en el monitor del cursor y mantiene el botón y el
/// atajo sobre ESA pizarra aunque el cursor se vaya a otro monitor. Al cambiar de color
/// pasa la tinta a la contraria si se confundiría con el fondo (salvo en modo Degradado).
/// La apaga al salir del modo dibujo y al ocultar la capa. Vive aparte de
/// <see cref="OverlayBroadcaster"/> para que cada clase tenga una sola responsabilidad.
/// Agnóstico de plataforma: depende solo de puertos.
/// </summary>
public sealed class WhiteboardController : IWhiteboard, IDisposable
{
    private readonly IOverlayCanvasProvider _canvases;
    private readonly ICursorPositionProvider _cursor;
    private readonly IDrawingSettings _settings;
    private readonly IOverlayController _overlay;
    private IOverlayCanvas? _active;
    private bool _keepThroughPointerMode;

    public WhiteboardController(
        IOverlayCanvasProvider canvases,
        ICursorPositionProvider cursor,
        IDrawingSettings settings,
        IOverlayController overlay)
    {
        _canvases = canvases;
        _cursor = cursor;
        _settings = settings;
        _overlay = overlay;
        _overlay.StateChanged += OnOverlayStateChanged;
    }

    public WhiteboardMode Mode { get; private set; } = WhiteboardMode.Off;

    public event EventHandler? Changed;

    public void Cycle()
    {
        var next = WhiteboardCycle.Next(Mode);
        if (Mode == WhiteboardMode.Off)
        {
            // Se prende en el monitor del cursor y ahí se queda hasta apagarla.
            _active = CanvasLocator.AtCursor(_canvases.Canvases, _cursor);
        }

        if (_active is null)
        {
            return;
        }

        _active.ShowWhiteboard(next);
        if (next == WhiteboardMode.Off)
        {
            _active = null;
        }
        else
        {
            UseContrastInk(next);
        }

        SetMode(next);
    }

    public void KeepThroughNextPointerMode() => _keepThroughPointerMode = true;

    public void Dispose() => _overlay.StateChanged -= OnOverlayStateChanged;

    // Si la tinta activa es del color del fondo, el primer trazo no se vería: pasa a la
    // contraria. Con el modo Degradado no se toca nada: el neón se ve sobre los dos fondos.
    private void UseContrastInk(WhiteboardMode mode)
    {
        if (!_settings.IsGradient && WhiteboardInk.Contrast(_settings.Color, mode) is { } ink)
        {
            _settings.SelectColor(ink);
        }
    }

    // Ocultar la capa o salir del modo dibujo apagan la pizarra; su hoja queda guardada.
    private void OnOverlayStateChanged(object? sender, EventArgs e)
    {
        if (Mode == WhiteboardMode.Off)
        {
            _keepThroughPointerMode = false;
            return;
        }

        if (!_overlay.IsVisible || (!_overlay.IsDrawingMode && !ConsumeKeep()))
        {
            TurnOff();
        }
        else if (_overlay.IsDrawingMode)
        {
            _keepThroughPointerMode = false;
        }
    }

    // El recorte pasa una vez a modo puntero sin apagar la pizarra.
    private bool ConsumeKeep()
    {
        bool keep = _keepThroughPointerMode;
        _keepThroughPointerMode = false;
        return keep;
    }

    private void TurnOff()
    {
        _active?.ShowWhiteboard(WhiteboardMode.Off);
        _active = null;
        SetMode(WhiteboardMode.Off);
    }

    private void SetMode(WhiteboardMode mode)
    {
        Mode = mode;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
