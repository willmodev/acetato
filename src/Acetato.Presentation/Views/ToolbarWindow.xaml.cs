using System.Windows;

namespace Acetato.Presentation.Views;

/// <summary>
/// Barra de herramientas flotante (HU-10). Ventana sin bordes, glass y siempre
/// encima; los estilos nativos (no robar foco, fuera del Alt-Tab) los aplica el
/// styler de Infrastructure. El code-behind solo llama a InitializeComponent
/// (MVVM estricto).
/// </summary>
public partial class ToolbarWindow : Window
{
    public ToolbarWindow()
    {
        InitializeComponent();
    }
}
