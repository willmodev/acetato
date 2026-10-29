using System.Windows;
using System.Windows.Controls;
using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Hotkeys;
using Acetato.Application.Overlay;
using Acetato.Presentation.Settings;
using Acetato.Presentation.ViewModels;
using Acetato.Presentation.Views;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Humo de los tooltips de la barra (HU-22): la ventana carga con los recursos
/// reales y la plantilla pinta el nombre y el atajo en monoespaciada. No abre
/// nada en pantalla.
/// </summary>
public sealed class ToolbarWindowSmokeTests
{
    [Fact]
    public void Every_toolbar_button_has_a_hint_and_the_window_styles_its_tooltips()
    {
        var (labels, hasTooltipStyle) = WpfTestHost.Run(LoadToolbar);

        hasTooltipStyle.Should().BeTrue();
        labels.Should().Contain(["Puntero", "Lápiz", "Rectángulo", "Deshacer", "Limpiar todo", "Recortar", "Cerrar", "Mover"]);
    }

    [Fact]
    public void The_tooltip_shows_the_name_and_the_chord_in_monospace()
    {
        var texts = WpfTestHost.Run(() => RenderHint("Ctrl+Z"));

        texts.Should().ContainSingle(text => text.Text == "Deshacer" && !text.Mono && text.Visible);
        texts.Should().ContainSingle(text => text.Text == "Ctrl+Z" && text.Mono && text.Visible);
    }

    [Fact]
    public void Without_chord_the_tooltip_shows_only_the_name()
    {
        var texts = WpfTestHost.Run(() => RenderHint(null));

        texts.Where(text => text.Visible).Select(text => text.Text).Should().Equal("Deshacer");
    }

    private static (List<string> Labels, bool HasTooltipStyle) LoadToolbar()
    {
        using var coordinator = TestCoordinators.Started();
        using var shortcuts = new ToolbarShortcuts(coordinator);
        var settings = new DrawingSettings();
        var controller = Substitute.For<IOverlayController>();
        var dragHandler = Substitute.For<IWindowDragHandler>();
        var broadcaster = new OverlayBroadcaster(
            Substitute.For<IOverlayCanvasProvider>(),
            Substitute.For<ICursorPositionProvider>(),
            settings,
            Substitute.For<IScreenClipLauncher>());
        var chrome = new ToolbarChrome(dragHandler, new SettingsWindowLauncher(controller, coordinator, dragHandler), shortcuts);
        using var viewModel = new ToolbarViewModel(broadcaster, settings, controller, chrome);
        var window = new ToolbarWindow();
        var content = (FrameworkElement)window.Content;
        window.Content = null;
        content.DataContext = viewModel;
        content.Measure(new Size(1200, 200));
        content.Arrange(new Rect(0, 0, 1200, 200));
        content.UpdateLayout();

        var labels = VisualTree.Descendants<FrameworkElement>(content)
            .Select(element => element.ToolTip)
            .OfType<ShortcutHint>()
            .Select(hint => hint.Label)
            .ToList();
        return (labels, window.Resources[typeof(ToolTip)] is Style);
    }

    private static List<(string Text, bool Mono, bool Visible)> RenderHint(string? shortcut)
    {
        var hint = new ShortcutHint("Deshacer", HotkeyAction.Undo) { Shortcut = shortcut };
        var presenter = new ContentPresenter
        {
            Content = hint,
            ContentTemplate = (DataTemplate)System.Windows.Application.Current.FindResource("ShortcutHintTemplate"),
        };
        presenter.Measure(new Size(400, 100));
        presenter.Arrange(new Rect(0, 0, 400, 100));
        presenter.UpdateLayout();
        var mono = (System.Windows.Media.FontFamily)System.Windows.Application.Current.FindResource("Font.Mono");
        return
        [
            .. VisualTree.Descendants<TextBlock>(presenter)
                .Select(text => (text.Text, Equals(text.FontFamily, mono), IsShown(text, presenter))),
        ];
    }

    // Visible si ni el texto ni sus ancestros hasta la raíz están colapsados.
    private static bool IsShown(FrameworkElement element, FrameworkElement root)
    {
        for (var current = element; current is not null; current = System.Windows.Media.VisualTreeHelper.GetParent(current) as FrameworkElement)
        {
            if (current.Visibility != Visibility.Visible)
            {
                return false;
            }

            if (ReferenceEquals(current, root))
            {
                return true;
            }
        }

        return true;
    }
}
