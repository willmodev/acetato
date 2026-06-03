using System.Windows;
using Acetato.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Acetato.Presentation;

/// <summary>
/// Composition root de la aplicación. Construye el contenedor de DI y arranca
/// la ventana overlay. La lógica de las herramientas vive en los ViewModels;
/// aquí solo se compone el grafo de dependencias.
/// </summary>
public partial class App : System.Windows.Application
{
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _services = BuildServiceProvider();
        var overlay = _services.GetRequiredService<OverlayWindow>();
        overlay.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // Vistas / ViewModels
        services.AddSingleton<OverlayWindow>();

        // Adaptadores de Infrastructure se registran aquí a medida que se
        // implementen las HUs (hotkeys, overlay interop, captura, bandeja).

        return services.BuildServiceProvider();
    }
}
