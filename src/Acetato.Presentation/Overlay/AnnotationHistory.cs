using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Historial cronológico de acciones de UNA pane (HU-13, SPEC 11): deshace «la
/// última acción, sea cual sea» —dibujar un trazo o un texto, o borrar con el
/// Borrador—. Los trazos se sincronizan con la <see cref="StrokeCollection"/>
/// (única fuente de verdad). Las bajas cuentan como borrado deshacible solo
/// mientras <see cref="RecordsErasures"/> está activo (herramienta Borrador); con
/// cualquier otra herramienta se «olvidan» en silencio, así el ir y venir del trazo
/// de previsualización de las formas se autocancela y deja UNA entrada por forma.
/// El texto se registra de forma explícita al fijarlo.
/// </summary>
internal sealed class AnnotationHistory : IDisposable
{
    private readonly StrokeCollection _strokes;
    private readonly List<IHistoryEntry> _timeline = [];
    private readonly Dictionary<Stroke, StrokeAdded> _strokeEntries = [];

    // Orden de la colección antes del próximo cambio: el evento llega con el trazo ya
    // quitado, y para devolverlo a su capa hace falta el índice que tenía.
    private List<Stroke> _order;

    // Mientras revertimos por nuestra cuenta (deshacer/limpiar), ignoramos el eco
    // de StrokesChanged para no contabilizar dos veces el mismo cambio.
    private bool _suppressSync;

    public AnnotationHistory(StrokeCollection strokes)
    {
        _strokes = strokes;
        _order = [.. strokes];
        _strokes.StrokesChanged += OnStrokesChanged;
    }

    /// <summary>
    /// Las bajas de trazos se registran como borrado deshacible (herramienta
    /// Borrador). Apagado, una baja solo olvida la entrada del trazo.
    /// </summary>
    public bool RecordsErasures { get; set; }

    /// <summary>Registra un texto recién fijado al final de la línea de tiempo.</summary>
    public void RecordText(InkCanvas canvas, UIElement element) =>
        _timeline.Add(new TextAdded(canvas, element));

    /// <summary>Revierte la última acción. Sin acciones no hace nada.</summary>
    public void UndoLast()
    {
        if (_timeline.Count == 0)
        {
            return;
        }

        var entry = _timeline[^1];
        _timeline.RemoveAt(_timeline.Count - 1);
        if (entry is StrokeAdded added)
        {
            _ = _strokeEntries.Remove(added.Stroke);
        }

        Silently(entry.Undo);
    }

    /// <summary>Vacía trazos, textos e historial (HU-04). No se puede deshacer.</summary>
    public void Clear()
    {
        Silently(() =>
        {
            _strokes.Clear();
            foreach (var text in _timeline.OfType<TextAdded>())
            {
                text.Undo();
            }
        });
        _timeline.Clear();
        _strokeEntries.Clear();
    }

    public void Dispose() => _strokes.StrokesChanged -= OnStrokesChanged;

    // Aplica un cambio propio sin que el eco de StrokesChanged lo registre.
    private void Silently(Action change)
    {
        _suppressSync = true;
        try
        {
            change();
        }
        finally
        {
            _suppressSync = false;
        }
    }

    private void OnStrokesChanged(object? sender, StrokeCollectionChangedEventArgs e)
    {
        if (!_suppressSync)
        {
            Sync(e);
        }

        _order = [.. _strokes];
    }

    private void Sync(StrokeCollectionChangedEventArgs e)
    {
        if (RecordsErasures)
        {
            RecordErasure(e.Removed);
        }
        else
        {
            foreach (var stroke in e.Removed)
            {
                ForgetStroke(stroke);
            }
        }

        foreach (var stroke in e.Added)
        {
            RememberStroke(stroke);
        }
    }

    private void RecordErasure(StrokeCollection removed)
    {
        var erased = removed
            .Select(stroke => new ErasedStroke(stroke, _order.IndexOf(stroke)))
            .Where(entry => entry.Index >= 0)
            .ToList();
        if (erased.Count > 0)
        {
            _timeline.Add(new StrokesErased(_strokes, erased));
        }
    }

    private void RememberStroke(Stroke stroke)
    {
        var entry = new StrokeAdded(_strokes, stroke);
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
