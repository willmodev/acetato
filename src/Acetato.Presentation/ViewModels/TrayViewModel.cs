using Acetato.Application.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// ViewModel del icono de bandeja (HU-08). En modo dibujo la capa cubre la
/// pantalla y captura los clics, así que la bandeja solo es alcanzable cuando
/// NO se está dibujando: por eso el menú solo ofrece "Activar dibujo" (mostrar
/// el overlay en modo dibujo) y "Salir". Para salir del dibujo se usan los
/// atajos (Ctrl+Alt+D ocultar, Ctrl+Alt+E click-through).
/// </summary>
public sealed partial class TrayViewModel : ObservableObject
{
    private readonly IOverlayController _controller;

    public TrayViewModel(IOverlayController controller) => _controller = controller;

    /// <summary>Muestra el overlay en modo dibujo (lo crea si estaba oculto).</summary>
    [RelayCommand]
    private void ActivateDrawing()
    {
        if (_controller.IsVisible)
        {
            _controller.SetDrawingMode(true);
        }
        else
        {
            _controller.Toggle(); // muestra el overlay y entra en modo dibujo
        }
    }

    /// <summary>Cierra la aplicación; App.OnExit libera los atajos globales.</summary>
    [RelayCommand]
    private static void Exit() => System.Windows.Application.Current.Shutdown();
}
