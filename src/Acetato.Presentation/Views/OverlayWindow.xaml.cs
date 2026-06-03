using System.Windows;

namespace Acetato.Presentation.Views;

/// <summary>
/// Capa transparente a pantalla completa. Sin lógica de negocio: el
/// comportamiento (click-through, dibujo, toolbar) se inyecta vía ViewModel
/// al implementar las HUs.
/// </summary>
public partial class OverlayWindow : Window
{
    public OverlayWindow()
    {
        InitializeComponent();
    }
}
