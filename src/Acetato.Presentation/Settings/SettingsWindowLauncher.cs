using System.Windows;
using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Presentation.ViewModels;
using Acetato.Presentation.Views;

namespace Acetato.Presentation.Settings;

/// <summary>
/// Abre la ventana Ajustes (HU-21) con una sola instancia: si ya está abierta,
/// la trae al frente. Al abrir pasa el overlay a click-through para que la
/// ventana sea usable sin ocultar los trazos. Cada apertura crea su ViewModel y
/// lo desecha al cerrar (sin suscripciones colgadas). Público porque lo usan los
/// ViewModels públicos de la barra y la bandeja.
/// </summary>
public sealed class SettingsWindowLauncher
{
    private readonly IOverlayController _overlay;
    private readonly HotkeyCoordinator _coordinator;
    private readonly IWindowDragHandler _dragHandler;
    private SettingsWindow? _window;

    public SettingsWindowLauncher(IOverlayController overlay, HotkeyCoordinator coordinator, IWindowDragHandler dragHandler)
    {
        _overlay = overlay;
        _coordinator = coordinator;
        _dragHandler = dragHandler;
    }

    /// <summary>Abre la ventana o trae al frente la que ya existe.</summary>
    public void Open()
    {
        if (_overlay.IsVisible && _overlay.IsDrawingMode)
        {
            _overlay.SetDrawingMode(false);
        }

        _window ??= CreateWindow();
        _window.Show();
        if (_window.WindowState == WindowState.Minimized)
        {
            _window.WindowState = WindowState.Normal;
        }

        _ = _window.Activate();
    }

    private SettingsWindow CreateWindow()
    {
        var viewModel = new SettingsViewModel(_coordinator, _dragHandler);
        var window = new SettingsWindow { DataContext = viewModel };
        viewModel.CloseRequested += (_, _) => window.Close();
        window.Closed += (_, _) =>
        {
            viewModel.Dispose();
            _window = null;
        };
        return window;
    }
}
