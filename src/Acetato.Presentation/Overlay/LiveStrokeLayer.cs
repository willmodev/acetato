using System.Diagnostics;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Capa de pintado del trazo libre en curso (HU-20), encima del InkCanvas y sin recibir
/// el mouse. No guarda nada en el historial: el trazo entra a la colección del lienzo una
/// sola vez, al soltar. El trazo a mostrar se pide a una fuente al repintar, así la
/// medición (<see cref="LiveRenderStats"/>) cubre construirlo y pintarlo.
/// </summary>
public sealed class LiveStrokeLayer : FrameworkElement
{
    private Func<Stroke?>? _source;

    internal LiveRenderStats Stats { get; } = new();

    /// <summary>
    /// Muestra el trazo que entregue <paramref name="source"/> y pide un repintado. Se
    /// llama como mucho una vez por fotograma (lo limita quien la usa).
    /// </summary>
    public void Show(Func<Stroke?> source)
    {
        _source = source;
        InvalidateVisual();
    }

    /// <summary>Deja la capa vacía.</summary>
    public void Clear()
    {
        _source = null;
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
}
