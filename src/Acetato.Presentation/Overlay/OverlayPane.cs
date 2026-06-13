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
/// por monitor. La ventana siembra su Left/Top en el monitor objetivo (para que WPF
/// no la reubique al primario) y luego maximiza en píxeles FÍSICOS sobre ese monitor
/// (vía <see cref="IOverlayWindowPlacer"/>), de modo que cubra el monitor correcto y
/// adopte su DPI sin desfase del trazo, en escalado igual o mixto.
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

    // Público para que la barra flotante se haga "owned" de esta pane (IOverlayCanvas).
    public nint Handle => new WindowInteropHelper(_window).Handle;

    public void Show(double primaryScale)
    {
        if (_shown)
        {
            _window.Show();
            return;
        }

        // Sembrar Left/Top en el monitor objetivo (DIP con la escala del primario)
        // ANTES de Show: en una ventana WPF layered (AllowsTransparency), si quedan en
        // su valor por defecto (sobre el primario), WPF reubica la ventana al primario
        // y pisa el SetWindowPlacement nativo → solo se dibuja en el principal. La barra
        // flotante (misma clase de ventana) ya usa este sembrado y cae en su monitor.
        _window.Left = DipConverter.ToDip(Monitor.X, primaryScale);
        _window.Top = DipConverter.ToDip(Monitor.Y, primaryScale);
        _window.Show();
        // Maximizar en píxeles FÍSICOS sobre su monitor (síncrono tras Show); corrige
        // el tamaño/DPI exactos tras el sembrado, también en DPI mixto.
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
