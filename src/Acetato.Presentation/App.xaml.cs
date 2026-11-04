using System.Windows;
using System.Windows.Controls;
using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Hotkeys;
using Acetato.Application.Overlay;
using Acetato.Application.Session;
using Acetato.Domain;
using Acetato.Infrastructure.Capture;
using Acetato.Infrastructure.Hotkeys;
using Acetato.Infrastructure.Input;
using Acetato.Infrastructure.Monitors;
using Acetato.Infrastructure.Overlay;
using Acetato.Infrastructure.Settings;
using Acetato.Presentation.Diagnostics;
using Acetato.Presentation.Overlay;
using Acetato.Presentation.Settings;
using Acetato.Presentation.ViewModels;
using Acetato.Presentation.Views;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Acetato.Presentation;

/// <summary>
/// Composition root de la aplicación. Construye el contenedor de DI y registra
/// el atajo global (HU-01). La app arranca con el overlay visible y en modo
/// dibujo (HU-24); el icono de bandeja sigue disponible.
/// </summary>
public partial class App : System.Windows.Application
{
    private AppDiagnostics? _diagnostics;
    private ServiceProvider? _services;
    private IGlobalHotkeyService? _hotkeys;
    private HotkeyActionRouter? _router;
    private TaskbarIcon? _trayIcon;

    // Orden (HU-32): registro → manejador de errores → arranque de siempre → «Inicio».
    // El arranque entero va envuelto: un error aquí, antes de que exista la barra, es fatal.
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            _diagnostics = AppDiagnostics.Start(this);
            StartApplication(_diagnostics);
        }
        catch (Exception ex)
        {
            // Anota, muestra el mensaje y cierra. Si otro cierre ya está en curso, el
            // error sigue su camino y termina el proceso igual.
            FailStartup(ex);
            throw;
        }
    }

    private void StartApplication(AppDiagnostics diagnostics)
    {
        _services = BuildServiceProvider(diagnostics);
        // Como se cerró (HU-29): antes de la barra y de los atajos, que siguen la herramienta.
        _services.GetRequiredService<SessionMemory>().Start();
        InitializeToolbar();
        WireHotkeys();
        InitializeTrayIcon();
        ShowOverlay();
        diagnostics.LogStartup(_services);
    }

    // Sin registro abierto todavía, solo queda el mensaje y el cierre.
    private void FailStartup(Exception exception)
    {
        if (_diagnostics is null)
        {
            new FatalExit(log: null).Exit(exception);
        }

        _diagnostics.FailStartup(exception);
    }

    // Arranque listo para dibujar (HU-24). Va al final: el coordinador ya sigue
    // el estado del overlay y registra los atajos de dibujo al mostrarse.
    private void ShowOverlay()
    {
        var controller = _services!.GetRequiredService<IOverlayController>();
        if (!controller.IsVisible)
        {
            controller.Toggle(); // muestra el overlay y entra en modo dibujo
        }
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
    // al hilo de UI y la reparte vía el broadcaster (a las panes de cada monitor).
    // El coordinador (HU-21) decide qué combinaciones se registran en cada momento.
    private void WireHotkeys()
    {
        var services = _services!;
        var controller = services.GetRequiredService<IOverlayController>();
        var broadcaster = services.GetRequiredService<OverlayBroadcaster>();
        var toolbarLayout = services.GetRequiredService<IToolbarLayout>();
        _router = new HotkeyActionRouter(controller, broadcaster, toolbarLayout, Dispatcher);

        _hotkeys = services.GetRequiredService<IGlobalHotkeyService>();
        _hotkeys.ActionRequested += _router.Handle;
        _hotkeys.Register();
        WireSettingsRequests(services);
        services.GetRequiredService<HotkeyCoordinator>().Start();
    }

    // Clic en el aviso de atajos (HU-21): llega en otro hilo, se abre Ajustes en el de UI.
    private void WireSettingsRequests(IServiceProvider services)
    {
        var launcher = services.GetRequiredService<SettingsWindowLauncher>();
        services.GetRequiredService<IHotkeyNotifier>().SettingsRequested +=
            (_, _) => _ = Dispatcher.BeginInvoke(launcher.Open);
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
                "settings" => viewModel.OpenSettingsCommand,
                "exit" => viewModel.ExitCommand,
                _ => item.Command,
            };
        }
    }

    // Orden (HU-20): sesión → atajos → servicios → icono de bandeja → registro. La sesión
    // se guarda primero, con el sitio de la barra, que solo se escribe aquí (HU-29). El
    // icono se quita casi al final: mientras se vea, la app sigue cerrándose (antes se
    // quitaba primero y ocultaba que el proceso quedaba vivo). El registro se vacía al
    // último, para anotar lo que falle al cerrar (HU-32).
    protected override void OnExit(ExitEventArgs e)
    {
        _services?.GetService<SessionMemory>()?.Save();
        _hotkeys?.Unregister();
        _services?.Dispose();
        _trayIcon?.Dispose();
        _diagnostics?.LogNormalExit();
        _diagnostics?.Dispose();
        base.OnExit(e);
    }

    private static ServiceProvider BuildServiceProvider(AppDiagnostics diagnostics)
    {
        var services = new ServiceCollection();
        diagnostics.Register(services);
        RegisterDrawingAndScreenClip(services);
        RegisterInfrastructure(services);
        RegisterOverlay(services);
        services.AddSingleton<TrayViewModel>();
        return services.BuildServiceProvider();
    }

    // Estado del trazo activo (HU-05/06) y recorte de pantalla de Windows (HU-23).
    private static void RegisterDrawingAndScreenClip(IServiceCollection services)
    {
        services.AddSingleton<IRandomSource>(SecureRandomSource.Instance);
        services.AddSingleton<IDrawingSettings>(CreateDrawingSettings);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IScreenClipLauncher, WindowsScreenClipLauncher>();
        services.AddSingleton<ScreenClipServices>();
    }

    // Estado del trazo con las opciones de dibujo guardadas: Tintas degradadas (HU-27) y
    // Transformación libre (HU-28).
    private static DrawingSettings CreateDrawingSettings(IServiceProvider services)
    {
        var settings = new DrawingSettings(services.GetRequiredService<IRandomSource>());
        var preferences = services.GetRequiredService<IDrawingPreferencesStore>().Load();
        settings.SetInkPairs(preferences.InkPairs);
        settings.SetFreeResize(preferences.FreeResize);
        return settings;
    }

    // Adaptadores de Infrastructure (interop Win32) — solo en el root.
    private static void RegisterInfrastructure(IServiceCollection services)
    {
        services.AddSingleton<IOverlayWindowStyler, Win32OverlayWindowStyler>();
        services.AddSingleton<IOverlayWindowPlacer, Win32OverlayWindowPlacer>();
        services.AddSingleton<IToolbarWindowStyler, Win32ToolbarWindowStyler>();
        services.AddSingleton<IWindowDragHandler, Win32WindowDragHandler>();
        services.AddSingleton<IGlobalHotkeyService, Win32GlobalHotkeyService>();
        services.AddSingleton<IHotkeySettingsStore>(sp => new JsonHotkeySettingsStore(Logger<JsonHotkeySettingsStore>(sp)));
        services.AddSingleton<IDrawingPreferencesStore>(sp => new JsonDrawingPreferencesStore(Logger<JsonDrawingPreferencesStore>(sp)));
        services.AddSingleton<ILastSessionStore>(sp => new JsonLastSessionStore(Logger<JsonLastSessionStore>(sp)));
        services.AddSingleton<IToolbarWindowPlacer, Win32ToolbarWindowPlacer>();
        services.AddSingleton<IHotkeyNotifier, ToastHotkeyNotifier>();
        services.AddSingleton<IMonitorService, Win32MonitorService>();
        services.AddSingleton<ICursorPositionProvider, Win32CursorPositionProvider>();
        services.AddSingleton<IModifierKeyState, Win32ModifierKeyState>();
    }

    private static ILogger<T> Logger<T>(IServiceProvider services) => services.GetRequiredService<ILogger<T>>();

    // Overlay multi-monitor (HU-09), barra (HU-10) y orquestación.
    private static void RegisterOverlay(IServiceCollection services)
    {
        // Una pane por monitor; el agregador es a la vez superficie y proveedor
        // de lienzos para el broadcaster (misma instancia singleton).
        services.AddSingleton<IOverlayPaneFactory, OverlayPaneFactory>();
        services.AddSingleton<MultiMonitorOverlaySurface>();
        services.AddSingleton<IOverlaySurface>(sp => sp.GetRequiredService<MultiMonitorOverlaySurface>());
        services.AddSingleton<IOverlayCanvasProvider>(sp => sp.GetRequiredService<MultiMonitorOverlaySurface>());
        services.AddSingleton<OverlayBroadcaster>();
        services.AddSingleton<IOverlayController, OverlayController>();
        services.AddSingleton<HotkeyCoordinator>();
        services.AddSingleton<SettingsWindowLauncher>();

        // Barra flotante (HU-10): VM, ventana (DataContext se asigna fuera del
        // contenedor para evitar el ciclo) y su superficie. El estado plegado
        // (HU-25) es único: lo comparten la barra y el atajo.
        services.AddSingleton<IToolbarLayout, ToolbarLayout>();
        services.AddSingleton<ToolbarPlacementServices>();
        services.AddSingleton<SessionMemory>();
        services.AddSingleton<ToolbarShortcuts>();
        services.AddSingleton<ToolbarChrome>();
        services.AddSingleton<ToolbarViewModel>();
        services.AddSingleton<ToolbarWindow>();
        services.AddSingleton<IToolbarSurface, ToolbarWindowSurface>();
    }
}
