using System.Windows;
using Acetato.Application.Abstractions;
using Acetato.Application.Overlay;
using Acetato.Infrastructure.Hotkeys;
using Acetato.Infrastructure.Overlay;
using Acetato.Presentation.Overlay;
using Acetato.Presentation.ViewModels;
using Acetato.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Acetato.Presentation;

/// <summary>
/// Composition root de la aplicación. Construye el contenedor de DI y registra
/// el atajo global (HU-01). La app arranca en segundo plano (sin ventana
/// visible); el overlay aparece al pulsar el atajo.
/// </summary>
public partial class App : System.Windows.Application
{
    private ServiceProvider? _services;
    private IGlobalHotkeyService? _hotkeys;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _services = BuildServiceProvider();
        var controller = _services.GetRequiredService<IOverlayController>();
        var overlayViewModel = _services.GetRequiredService<OverlayViewModel>();

        _hotkeys = _services.GetRequiredService<IGlobalHotkeyService>();
        // Los eventos llegan en el hilo del bombeo; marshalamos al hilo de UI.
        _hotkeys.ToggleRequested += (_, _) => _ = Dispatcher.BeginInvoke(controller.Toggle);
        _hotkeys.DrawingModeToggleRequested += (_, _) =>
            _ = Dispatcher.BeginInvoke(() => controller.SetDrawingMode(!controller.IsDrawingMode));
        _hotkeys.ClearRequested += (_, _) =>
            _ = Dispatcher.BeginInvoke(() => overlayViewModel.ClearCommand.Execute(null));
        _hotkeys.Register();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeys?.Unregister();
        _services?.Dispose();
        base.OnExit(e);
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // Vistas, su ViewModel y el adaptador de superficie.
        services.AddSingleton<OverlayViewModel>();
        services.AddSingleton<OverlayWindow>(sp => new OverlayWindow
        {
            DataContext = sp.GetRequiredService<OverlayViewModel>(),
        });
        services.AddSingleton<IOverlaySurface, OverlayWindowSurface>();

        // Adaptadores de Infrastructure (interop Win32) — solo en el root.
        services.AddSingleton<IOverlayWindowStyler, Win32OverlayWindowStyler>();
        services.AddSingleton<IGlobalHotkeyService, Win32GlobalHotkeyService>();

        // Casos de uso / orquestación (Application).
        services.AddSingleton<IOverlayController, OverlayController>();

        return services.BuildServiceProvider();
    }
}
