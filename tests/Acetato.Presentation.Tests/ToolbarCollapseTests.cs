using System.Windows;
using System.Windows.Media;
using Acetato.Domain;
using Acetato.Presentation.Views;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Ojo de la barra (HU-25): pliega y despliega la barra, el icono muestra el
/// estado y el tooltip nombra la acción. Plegar cierra los popovers y no toca
/// el modo, la herramienta, la tinta ni el grosor.
/// </summary>
public sealed class ToolbarCollapseTests
{
    [Fact]
    public void The_bar_starts_expanded_with_the_open_eye()
    {
        var (collapsed, hint, eye) = WpfTestHost.Run(() => ToolbarHost.With(toolbar =>
            (toolbar.ViewModel.IsCollapsed, toolbar.ViewModel.CollapseHint.ToString(), toolbar.ViewModel.EyeGlyph)));

        collapsed.Should().BeFalse();
        hint.Should().Be("Ocultar barra  Ctrl+Alt+H");
        eye.Should().BeSameAs(WpfTestHost.Run(() => Glyph("Icon.Eye")));
    }

    [Fact]
    public void The_eye_collapses_the_bar_and_switches_icon_and_tooltip()
    {
        var (collapsed, shared, label, eye) = WpfTestHost.Run(() => ToolbarHost.With(toolbar =>
        {
            toolbar.ViewModel.ToggleCollapsedCommand.Execute(null);
            return (toolbar.ViewModel.IsCollapsed, toolbar.Layout.IsCollapsed,
                toolbar.ViewModel.CollapseHint.Label, toolbar.ViewModel.EyeGlyph);
        }));

        collapsed.Should().BeTrue();
        shared.Should().BeTrue();
        label.Should().Be("Mostrar barra");
        eye.Should().BeSameAs(WpfTestHost.Run(() => Glyph("Icon.EyeOff")));
    }

    [Fact]
    public void A_second_click_expands_the_bar_again()
    {
        var (collapsed, label) = WpfTestHost.Run(() => ToolbarHost.With(toolbar =>
        {
            toolbar.ViewModel.ToggleCollapsedCommand.Execute(null);
            toolbar.ViewModel.ToggleCollapsedCommand.Execute(null);
            return (toolbar.ViewModel.IsCollapsed, toolbar.ViewModel.CollapseHint.Label);
        }));

        collapsed.Should().BeFalse();
        label.Should().Be("Ocultar barra");
    }

    [Fact]
    public void The_bar_follows_the_shared_state_when_another_source_collapses_it()
    {
        var collapsed = WpfTestHost.Run(() => ToolbarHost.With(toolbar =>
        {
            toolbar.Layout.ToggleCollapsed();
            return toolbar.ViewModel.IsCollapsed;
        }));

        collapsed.Should().BeTrue();
    }

    [Fact]
    public void Collapsing_closes_the_popovers()
    {
        var (ink, thickness) = WpfTestHost.Run(() => ToolbarHost.With(toolbar =>
        {
            var viewModel = toolbar.ViewModel;
            viewModel.ToggleInkPopoverCommand.Execute(null);
            viewModel.ToggleCollapsedCommand.Execute(null);
            var inkOpen = viewModel.IsInkPopoverOpen;
            viewModel.ToggleCollapsedCommand.Execute(null);
            viewModel.ToggleThicknessPopoverCommand.Execute(null);
            viewModel.ToggleCollapsedCommand.Execute(null);
            return (inkOpen, viewModel.IsThicknessPopoverOpen);
        }));

        ink.Should().BeFalse();
        thickness.Should().BeFalse();
    }

    [Fact]
    public void Collapsing_keeps_mode_tool_ink_and_thickness()
    {
        var (drawing, tool, color, thickness) = WpfTestHost.Run(() => ToolbarHost.With(toolbar =>
        {
            toolbar.Settings.SelectTool(ToolKind.Arrow);
            toolbar.Settings.SelectColor(TintaColor.Blue);
            toolbar.Settings.SelectThickness(3);
            toolbar.ViewModel.ToggleCollapsedCommand.Execute(null);
            return (toolbar.Controller.IsDrawingMode, toolbar.Settings.SelectedTool,
                toolbar.Settings.Color, toolbar.Settings.ThicknessIndex);
        }));

        drawing.Should().BeTrue();
        tool.Should().Be(ToolKind.Arrow);
        color.Should().Be(TintaColor.Blue);
        thickness.Should().Be(3);
    }

    [Fact]
    public void Collapsed_the_window_keeps_only_grip_divider_and_eye()
    {
        var (expanded, collapsed, buttons) = WpfTestHost.Run(() => ToolbarHost.With(toolbar =>
        {
            var content = LoadContent(toolbar);
            var wide = content.DesiredSize.Width;
            toolbar.ViewModel.ToggleCollapsedCommand.Execute(null);
            Measure(content);
            var visible = VisualTree.Descendants<System.Windows.Controls.Button>(content).Count(IsShown);
            return (wide, content.DesiredSize.Width, visible);
        }));

        collapsed.Should().BeLessThan(expanded / 4);
        buttons.Should().Be(1);
    }

    // Contenido de la ventana real, medido suelto (una ventana sin Show no hace layout).
    private static FrameworkElement LoadContent(ToolbarContext toolbar)
    {
        var window = new ToolbarWindow();
        var content = (FrameworkElement)window.Content;
        window.Content = null;
        content.DataContext = toolbar.ViewModel;
        Measure(content);
        return content;
    }

    private static void Measure(FrameworkElement content)
    {
        content.Measure(new Size(double.PositiveInfinity, 200));
        content.Arrange(new Rect(content.DesiredSize));
        content.UpdateLayout();
    }

    // Visible de verdad: ni él ni ningún ancestro está oculto (IsVisible no sirve
    // sin una ventana en pantalla).
    private static bool IsShown(DependencyObject element)
    {
        for (var node = element; node is not null; node = VisualTreeHelper.GetParent(node))
        {
            if (node is UIElement { Visibility: not Visibility.Visible })
            {
                return false;
            }
        }

        return true;
    }

    private static Geometry Glyph(string key) => (Geometry)System.Windows.Application.Current.Resources[key];
}
