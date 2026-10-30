using System.Windows;
using System.Windows.Controls;
using Acetato.Application.Abstractions;
using Acetato.Presentation.ViewModels;
using Acetato.Presentation.Views;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Humo de la ventana Ajustes (HU-21): carga el XAML con los recursos reales de la
/// app y hace el layout SIN mostrarla. Detecta claves de recurso, estilos o
/// plantillas rotas que el build no ve. No abre nada en pantalla.
/// </summary>
public sealed class SettingsWindowSmokeTests
{
    [Fact]
    public void Settings_window_loads_and_lays_out_the_twenty_three_rows()
    {
        int texts = WpfTestHost.Run(LoadAndCountTexts);

        // 23 nombres + 23 combinaciones como mínimo.
        texts.Should().BeGreaterThanOrEqualTo(46);
    }

    private static int LoadAndCountTexts()
    {
        using var coordinator = TestCoordinators.Started();
        using var settings = new SettingsViewModel(coordinator, Substitute.For<IWindowDragHandler>());
        var window = new SettingsWindow();

        // Una ventana sin mostrar no hace layout: se mide su contenido suelto,
        // con el mismo DataContext y los recursos de la app.
        var content = (FrameworkElement)window.Content;
        window.Content = null;
        content.DataContext = settings;
        content.Measure(new Size(480, 600));
        content.Arrange(new Rect(0, 0, 480, 600));
        content.UpdateLayout();
        return VisualTree.Descendants<TextBlock>(content).Count();
    }
}
