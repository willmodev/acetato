using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
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
        int texts = WpfTestHost.Run(() => WithContent(content => VisualTree.Descendants<TextBlock>(content).Count()));

        // 23 nombres + 23 combinaciones como mínimo.
        texts.Should().BeGreaterThanOrEqualTo(46);
    }

    [Fact]
    public void Settings_window_shows_the_drawing_block_with_its_switch_and_hint()
    {
        var (texts, switches) = WpfTestHost.Run(() => WithContent(content => (
            VisualTree.Descendants<TextBlock>(content).Select(text => text.Text).ToList(),
            VisualTree.Descendants<ToggleButton>(content).Count())));

        texts.Should().ContainInOrder("Dibujo", "Tintas degradadas", "Cada tinta pasa a un tono vecino", "Siempre activos");
        switches.Should().Be(1);
    }

    // Carga la ventana, hace el layout de su contenido y lee lo que pide el test.
    private static T WithContent<T>(Func<FrameworkElement, T> read)
    {
        using var coordinator = TestCoordinators.Started();
        using var settings = new SettingsViewModel(
            coordinator, Substitute.For<IWindowDragHandler>(), new DrawingSettings(), Substitute.For<IDrawingPreferencesStore>());
        var window = new SettingsWindow();

        // Una ventana sin mostrar no hace layout: se mide su contenido suelto,
        // con el mismo DataContext y los recursos de la app.
        var content = (FrameworkElement)window.Content;
        window.Content = null;
        content.DataContext = settings;
        content.Measure(new Size(480, 600));
        content.Arrange(new Rect(0, 0, 480, 600));
        content.UpdateLayout();
        return read(content);
    }
}
