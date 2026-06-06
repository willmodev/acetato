using System.Windows.Interop;
using Acetato.Application.Abstractions;
using Acetato.Presentation.Views;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Adaptador WPF de <see cref="IToolbarSurface"/>: envuelve la
/// <see cref="ToolbarWindow"/> para exponer visibilidad y manejador a la capa de
/// aplicación sin filtrar WPF hacia adentro.
/// </summary>
public sealed class ToolbarWindowSurface : IToolbarSurface
{
    private readonly ToolbarWindow _window;

    public ToolbarWindowSurface(ToolbarWindow window)
    {
        _window = window;
    }

    public bool IsVisible => _window.IsVisible;

    public nint Handle => new WindowInteropHelper(_window).Handle;

    public void Show() => _window.Show();

    public void Hide() => _window.Hide();
}
