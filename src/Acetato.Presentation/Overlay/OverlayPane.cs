using System.Windows.Interop;
using Acetato.Application.Abstractions;
using Acetato.Domain;
using Acetato.Presentation.ViewModels;
using Acetato.Presentation.Views;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Una ventana de overlay sobre un solo monitor (HU-09) con su propio
/// <see cref="OverlayViewModel"/> (trazos/undo independientes). Implementa
/// <see cref="IOverlayCanvas"/> para que el broadcaster dirija limpiar/deshacer
/// por monitor. La ventana se posiciona y maximiza en píxeles FÍSICOS sobre su
/// monitor (vía <see cref="IOverlayWindowPlacer"/>): en DIP, WPF la descolocaría
/// en monitores con DPI distinto.
/// </summary>
public sealed class OverlayPane : IOverlayCanvas, IDisposable
{
    private readonly OverlayWindow _window;
    private readonly OverlayViewModel _viewModel;
    private readonly IOverlayWindowStyler _styler;
    private readonly IOverlayWindowPlacer _placer;
    private bool _stylesApplied;
    private bool _shown;

    public OverlayPane(
        OverlayWindow window,
        OverlayViewModel viewModel,
        IOverlayWindowStyler styler,
        IOverlayWindowPlacer placer,
        MonitorInfo monitor)
    {
        _window = window;
        _viewModel = viewModel;
        _styler = styler;
        _placer = placer;
        Monitor = monitor;
    }

    public MonitorInfo Monitor { get; }

    public bool IsVisible => _window.IsVisible;

    private nint Handle => new WindowInteropHelper(_window).Handle;

    public void Show()
    {
        if (_shown)
        {
            _window.Show();
            return;
        }

        _window.Show();
        // Posicionar + maximizar en píxeles físicos sobre su monitor (síncrono tras
        // Show → sin parpadeo perceptible; la capa es casi transparente).
        _placer.MaximizeOnMonitor(Handle, Monitor);
        _shown = true;
    }

    public void Hide() => _window.Hide();

    public void ApplyStyles()
    {
        nint handle = Handle;
        if (_stylesApplied || handle == nint.Zero)
        {
            return;
        }

        _styler.ApplyOverlayStyles(handle);
        _stylesApplied = true;
    }

    public void SetClickThrough(bool clickThrough)
    {
        nint handle = Handle;
        if (handle == nint.Zero)
        {
            return;
        }

        _styler.SetClickThrough(handle, clickThrough);
    }

    public void Clear() => _viewModel.ClearCommand.Execute(null);

    public void Undo() => _viewModel.UndoCommand.Execute(null);

    public void Dispose()
    {
        _viewModel.Dispose();
        _window.Close();
    }
}
