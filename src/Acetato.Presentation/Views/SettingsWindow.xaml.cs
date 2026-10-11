using System.Windows;

namespace Acetato.Presentation.Views;

/// <summary>
/// Ventana Ajustes (HU-21). La abre <c>SettingsWindowLauncher</c> con su
/// ViewModel. El code-behind solo llama a InitializeComponent (MVVM estricto).
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }
}
