using System.Windows;
using System.Windows.Controls;
using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Overlay;
using Acetato.Infrastructure.Hotkeys;
using Acetato.Infrastructure.Overlay;
using Acetato.Presentation.Overlay;
using Acetato.Presentation.ViewModels;
using Acetato.Presentation.Views;
using H.NotifyIcon;
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
    private HotkeyActionRouter? _router;
    private TaskbarIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _services = BuildServiceProvider();
        InitializeToolbar();
        WireHotkeys();
        InitializeTrayIcon();
    }

    // Asigna el DataContext de la barra (HU-10) fuera del contenedor para romper
    // el ciclo OverlayController → IToolbarSurface → ToolbarWindow → ToolbarViewModel
    // → IOverlayController. La ventana se construye sin VM; aquí se enlaza.
    private void InitializeToolbar()
    {
        var services = _services!;
        var window = services.GetRequiredService<ToolbarWindow>();
        window.DataContext = services.GetRequiredService<ToolbarViewModel>();
    }

    // Conecta el servicio de atajos con el enrutador, que marshala cada acción
    // al hilo de UI y la traduce al comando o método correspondiente.
    private void WireHotkeys()
    {
        var services = _services!;
        var controller = services.GetRequiredService<IOverlayController>();
        var viewModel = services.GetRequiredService<OverlayViewModel>();
        _router = new HotkeyActionRouter(controller, viewModel, Dispatcher);

        _hotkeys = services.GetRequiredService<IGlobalHotkeyService>();
        _hotkeys.ActionRequested += _router.Handle;
        _hotkeys.Register();
    }

    // Crea el icono de bandeja (HU-08) y cablea sus comandos en código. No se
    // enlazan por XAML porque H.NotifyIcon deja el DataContext del menú
    // inestable (apunta al TaskbarIcon al arrancar y ensucia el binding).
    private void InitializeTrayIcon()
    {
        var trayViewModel = _services!.GetRequiredService<TrayViewModel>();
        _trayIcon = (TaskbarIcon)Resources["TrayIcon"];
        WireTrayCommands(_trayIcon.ContextMenu, trayViewModel);
        _trayIcon.ForceCreate();
    }

    private static void WireTrayCommands(ContextMenu? menu, TrayViewModel viewModel)
    {
        if (menu is null)
        {
            return;
        }

        foreach (var item in menu.Items.OfType<MenuItem>())
        {
            item.Command = (item.Tag as string) switch
            {
                "activate" => viewModel.ActivateDrawingCommand,
                "exit" => viewModel.ExitCommand,
                _ => item.Command,
            };
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _hotkeys?.Unregister();
        _services?.Dispose();
        base.OnExit(e);
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // Estado del trazo activo (color/grosor) compartido por la sesión.
        services.AddSingleton<IDrawingSettings, DrawingSettings>();

        // Vistas, su ViewModel y el adaptador de superficie.
        services.AddSingleton<OverlayViewModel>();
        services.AddSingleton<OverlayWindow>(sp => new OverlayWindow
        {
            DataContext = sp.GetRequiredService<OverlayViewModel>(),
        });
        services.AddSingleton<IOverlaySurface, OverlayWindowSurface>();

        // Barra flotante (HU-10): VM, ventana (DataContext se asigna fuera del
        // contenedor para evitar el ciclo) y su superficie.
        services.AddSingleton<ToolbarViewModel>();
        services.AddSingleton<ToolbarWindow>();
        services.AddSingleton<IToolbarSurface, ToolbarWindowSurface>();

        // Adaptadores de Infrastructure (interop Win32) — solo en el root.
        services.AddSingleton<IOverlayWindowStyler, Win32OverlayWindowStyler>();
        services.AddSingleton<IToolbarWindowStyler, Win32ToolbarWindowStyler>();
        services.AddSingleton<IWindowDragHandler, Win32WindowDragHandler>();
        services.AddSingleton<IGlobalHotkeyService, Win32GlobalHotkeyService>();

        // Casos de uso / orquestación (Application).
        services.AddSingleton<IOverlayController, OverlayController>();

        // ViewModel del icono de bandeja (HU-08).
        services.AddSingleton<TrayViewModel>();

        return services.BuildServiceProvider();
    }
}
