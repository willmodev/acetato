using System.Diagnostics;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Capa de pintado del trazo libre en curso (HU-20), encima del InkCanvas y sin recibir
/// el mouse. No guarda nada en el historial: el trazo entra a la colección del lienzo una
/// sola vez, al soltar. El trazo a mostrar se pide a una fuente al repintar, así la
/// medición (<see cref="LiveRenderStats"/>) cubre construirlo y pintarlo. Repinta como
/// mucho una vez por fotograma (<see cref="CompositionTarget.Rendering"/>): el mouse puede
/// dar más eventos que fotogramas y pintar los sobrantes sería trabajo perdido.
/// </summary>
public sealed class LiveStrokeLayer : FrameworkElement
{
    private Func<Stroke?>? _source;
    private bool _dirty;

    internal LiveRenderStats Stats { get; } = new();

    /// <summary>Empieza a mostrar el trazo que entregue <paramref name="source"/>.</summary>
    public void Show(Func<Stroke?> source)
    {
        if (_source is null)
        {
            CompositionTarget.Rendering += OnRendering;
        }

        _source = source;
        _dirty = true;
    }

    /// <summary>El trazo cambió: se repinta en el próximo fotograma.</summary>
    public void Refresh() => _dirty = true;

    /// <summary>Deja la capa vacía y deja de escuchar los fotogramas.</summary>
    public void Clear()
    {
        if (_source is not null)
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        _source = null;
        _dirty = false;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (_source is null)
        {
            return;
        }

        long start = Stopwatch.GetTimestamp();
        _source()?.Draw(drawingContext);
        Stats.Record(Stopwatch.GetElapsedTime(start));
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (_dirty)
        {
            _dirty = false;
            InvalidateVisual();
        }
    }
}
