using Acetato.Domain;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// Las dos hojas de un monitor (HU-35): la de la pantalla, con lo dibujado sobre lo que
/// hay debajo, y la de la pizarra. Cada una tiene su propio <see cref="OverlayViewModel"/>
/// (trazos e historial), así Deshacer, Borrador, Mover y el botón de limpiar actúan solo sobre
/// la hoja que se ve. <see cref="Mode"/> dice cuál se ve: con la pizarra apagada, la de
/// la pantalla; blanca o negra, la de la pizarra. La ventana del overlay la usa como
/// contexto de datos.
/// </summary>
public sealed partial class OverlaySheets : ObservableObject, IDisposable
{
    public OverlaySheets(OverlayViewModel screen, OverlayViewModel board)
    {
        Screen = screen;
        Board = board;
    }

    /// <summary>Hoja de la pantalla: lo dibujado sobre lo que hay debajo.</summary>
    public OverlayViewModel Screen { get; }

    /// <summary>Hoja de la pizarra: la comparten la blanca y la negra.</summary>
    public OverlayViewModel Board { get; }

    /// <summary>Estado de la pizarra de este monitor; apagada = se ve la hoja de la pantalla.</summary>
    [ObservableProperty]
    private WhiteboardMode _mode = WhiteboardMode.Off;

    /// <summary>La hoja que se ve ahora, la que reciben las acciones del usuario.</summary>
    public OverlayViewModel Visible => Mode == WhiteboardMode.Off ? Screen : Board;

    partial void OnModeChanged(WhiteboardMode value) => OnPropertyChanged(nameof(Visible));

    public void Dispose()
    {
        Screen.Dispose();
        Board.Dispose();
    }
}
