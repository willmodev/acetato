using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;

namespace Acetato.Application.Session;

/// <summary>
/// Hace que Acetato arranque como se cerró (HU-29). Al arrancar aplica la última sesión
/// guardada al estado de dibujo y a la barra. Después guarda en cada cambio de tinta,
/// Degradado, grosor, herramienta o plegado, solo si algo de eso cambió de verdad (sortear
/// el siguiente par neón no escribe el archivo). El sitio de la barra se guarda con
/// <see cref="Save"/> al salir, porque el arrastre lo cambia decenas de veces por segundo.
/// </summary>
public sealed class SessionMemory : IDisposable
{
    private readonly IDrawingSettings _drawing;
    private readonly IToolbarLayout _layout;
    private readonly ILastSessionStore _store;
    private LastSession? _saved;

    public SessionMemory(IDrawingSettings drawing, IToolbarLayout layout, ILastSessionStore store)
    {
        _drawing = drawing;
        _layout = layout;
        _store = store;
    }

    /// <summary>
    /// Aplica la última sesión y empieza a guardar los cambios. Va antes de arrancar los
    /// atajos, que dependen de la herramienta activa.
    /// </summary>
    public void Start()
    {
        if (_saved is not null)
        {
            return;
        }

        _saved = _store.Load();
        Restore(_saved);
        _drawing.Changed += OnChanged;
        _layout.Changed += OnChanged;
    }

    /// <summary>Guarda la sesión tal como está, si cambió desde lo último guardado.</summary>
    public void Save()
    {
        if (_saved is null)
        {
            return;
        }

        var current = Snapshot();
        if (current == _saved)
        {
            return;
        }

        _store.Save(current);
        _saved = current;
    }

    public void Dispose()
    {
        _drawing.Changed -= OnChanged;
        _layout.Changed -= OnChanged;
    }

    private void OnChanged(object? sender, EventArgs e) => Save();

    private void Restore(LastSession session)
    {
        _drawing.SelectColor(session.Ink);
        if (session.Gradient)
        {
            _drawing.SelectGradient();
        }

        _drawing.SelectThickness(session.ThicknessIndex);
        _drawing.SelectTool(session.Tool);
        if (session.ToolbarCollapsed != _layout.IsCollapsed)
        {
            _layout.ToggleCollapsed();
        }

        if (session.ToolbarPlacement is { } placement)
        {
            _layout.Remember(placement);
        }
    }

    private LastSession Snapshot() => new(
        _drawing.Color,
        _drawing.IsGradient,
        _drawing.ThicknessIndex,
        _drawing.SelectedTool,
        _layout.IsCollapsed,
        _layout.Placement);
}
