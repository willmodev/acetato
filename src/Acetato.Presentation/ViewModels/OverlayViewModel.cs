using System.Windows.Ink;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// ViewModel del overlay. Posee los trazos del usuario (enlazados al InkCanvas)
/// y expone el comando para limpiarlos (HU-04). Toda la lógica vive aquí; el
/// code-behind solo llama a InitializeComponent.
/// </summary>
public sealed partial class OverlayViewModel : ObservableObject
{
    /// <summary>Trazos dibujados; enlazado a <c>InkCanvas.Strokes</c>.</summary>
    public StrokeCollection Strokes { get; } = [];

    /// <summary>Borra todos los trazos. No falla si la capa ya está vacía.</summary>
    [RelayCommand]
    private void Clear() => Strokes.Clear();
}
