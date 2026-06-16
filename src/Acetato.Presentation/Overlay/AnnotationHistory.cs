using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Historial unificado y cronológico de UNA pane (HU-13): registra trazos Y
/// textos en el orden en que se crean y deshace "lo último, sea lo que sea".
/// Reemplaza a la antigua pila de solo-trazos. Los trazos se sincronizan con la
/// <see cref="StrokeCollection"/> (única fuente de verdad): así el ir y venir del
/// trazo de previsualización de las formas se autocancela y solo queda UNA
/// entrada por forma final, y el borrador también se refleja. El texto se
/// registra de forma explícita al fijarlo.
/// </summary>
internal sealed class AnnotationHistory : IDisposable
{
    private readonly StrokeCollection _strokes;
    private readonly List<IAnnotation> _timeline = [];
    private readonly Dictionary<Stroke, StrokeAnnotation> _strokeEntries = [];

    // Mientras quitamos por nuestra cuenta (deshacer/limpiar), ignoramos el eco
    // de StrokesChanged para no contabilizar dos veces la misma baja.
    private bool _suppressSync;

    public AnnotationHistory(StrokeCollection strokes)
    {
        _strokes = strokes;
        _strokes.StrokesChanged += OnStrokesChanged;
    }

    /// <summary>Registra un texto recién fijado al final de la línea de tiempo.</summary>
    public void RecordText(InkCanvas canvas, UIElement element) =>
        _timeline.Add(new TextAnnotation(canvas, element));

    /// <summary>Quita la última anotación, sea trazo o texto. Sin anotaciones no hace nada.</summary>
    public void UndoLast()
    {
        if (_timeline.Count == 0)
        {
            return;
        }

        Remove(_timeline[^1]);
    }

    /// <summary>Vacía trazos, textos e historial (HU-04).</summary>
    public void Clear()
    {
        // Copia: Remove muta _timeline durante el recorrido.
        foreach (var annotation in _timeline.ToArray())
        {
            Remove(annotation);
        }
    }

    public void Dispose() => _strokes.StrokesChanged -= OnStrokesChanged;

    // Quita una anotación del historial y de su lienzo, sin re-sincronizar.
    private void Remove(IAnnotation annotation)
    {
        _timeline.Remove(annotation);
        if (annotation is StrokeAnnotation strokeEntry)
        {
            _strokeEntries.Remove(strokeEntry.Stroke);
        }

        _suppressSync = true;
        try
        {
            annotation.Remove();
        }
        finally
        {
            _suppressSync = false;
        }
    }

    private void OnStrokesChanged(object? sender, StrokeCollectionChangedEventArgs e)
    {
        if (_suppressSync)
        {
            return;
        }

        foreach (var stroke in e.Removed)
        {
            ForgetStroke(stroke);
        }

        foreach (var stroke in e.Added)
        {
            RememberStroke(stroke);
        }
    }

    private void RememberStroke(Stroke stroke)
    {
        var entry = new StrokeAnnotation(_strokes, stroke);
        _strokeEntries[stroke] = entry;
        _timeline.Add(entry);
    }

    private void ForgetStroke(Stroke stroke)
    {
        if (_strokeEntries.Remove(stroke, out var entry))
        {
            _ = _timeline.Remove(entry);
        }
    }
}
