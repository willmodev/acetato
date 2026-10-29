using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Overlay;
using Acetato.Domain;
using Acetato.Presentation.Settings;
using Acetato.Presentation.ViewModels;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Botón Puntero de la barra (HU-24): sale del modo dibujo sin ocultar nada,
/// elegir una herramienta vuelve a él y la tinta o el grosor no cambian el modo.
/// Usa el controlador real con superficies falsas.
/// </summary>
public sealed class ToolbarViewModelTests
{
    [Fact]
    public void Pointer_leaves_drawing_mode_and_keeps_the_overlay_visible()
    {
        var (controller, pointer, selected) = WpfTestHost.Run(() => WithToolbar((controller, viewModel) =>
        {
            viewModel.EnterPointerModeCommand.Execute(null);
            return (controller, viewModel.IsPointerMode, SelectedTools(viewModel));
        }));

        controller.IsDrawingMode.Should().BeFalse();
        controller.IsVisible.Should().BeTrue();
        pointer.Should().BeTrue();
        selected.Should().BeEmpty();
    }

    [Fact]
    public void Picking_a_tool_in_pointer_mode_returns_to_drawing_with_that_tool()
    {
        var (drawing, pointer, selected) = WpfTestHost.Run(() => WithToolbar((controller, viewModel) =>
        {
            viewModel.EnterPointerModeCommand.Execute(null);
            viewModel.PickToolCommand.Execute(ToolKind.Arrow);
            return (controller.IsDrawingMode, viewModel.IsPointerMode, SelectedTools(viewModel));
        }));

        drawing.Should().BeTrue();
        pointer.Should().BeFalse();
        selected.Should().Equal(ToolKind.Arrow);
    }

    [Fact]
    public void Ink_and_thickness_do_not_change_the_mode()
    {
        var drawing = WpfTestHost.Run(() => WithToolbar((controller, viewModel) =>
        {
            viewModel.EnterPointerModeCommand.Execute(null);
            viewModel.PickSwatchCommand.Execute(viewModel.Swatches[1]);
            viewModel.PickThicknessCommand.Execute(2);
            return controller.IsDrawingMode;
        }));

        drawing.Should().BeFalse();
    }

    [Fact]
    public void Entering_pointer_mode_closes_the_popovers()
    {
        var (ink, thickness) = WpfTestHost.Run(() => WithToolbar((_, viewModel) =>
        {
            viewModel.ToggleInkPopoverCommand.Execute(null);
            viewModel.EnterPointerModeCommand.Execute(null);
            var inkOpen = viewModel.IsInkPopoverOpen;
            viewModel.PickToolCommand.Execute(ToolKind.Pencil);
            viewModel.ToggleThicknessPopoverCommand.Execute(null);
            viewModel.EnterPointerModeCommand.Execute(null);
            return (inkOpen, viewModel.IsThicknessPopoverOpen);
        }));

        ink.Should().BeFalse();
        thickness.Should().BeFalse();
    }

    [Fact]
    public void In_drawing_mode_the_active_tool_is_highlighted_and_the_pointer_is_not()
    {
        var (pointer, selected) = WpfTestHost.Run(() => WithToolbar((_, viewModel) =>
            (viewModel.IsPointerMode, SelectedTools(viewModel))));

        pointer.Should().BeFalse();
        selected.Should().Equal(ToolKind.Pencil);
    }

    [Fact]
    public void Pointer_in_pointer_mode_does_nothing()
    {
        var changes = WpfTestHost.Run(() => WithToolbar((controller, viewModel) =>
        {
            viewModel.EnterPointerModeCommand.Execute(null);
            var count = 0;
            controller.StateChanged += (_, _) => count++;
            viewModel.EnterPointerModeCommand.Execute(null);
            return count;
        }));

        changes.Should().Be(0);
    }

    [Fact]
    public void The_pointer_is_not_a_drawing_tool_of_the_bar()
    {
        var kinds = WpfTestHost.Run(() => WithToolbar((_, viewModel) =>
            viewModel.Tools.Select(tool => tool.Kind).ToList()));

        kinds.Should().NotContain(ToolKind.Select);
    }

    // Barra con el overlay ya visible en modo dibujo, como al arrancar la app.
    private static T WithToolbar<T>(Func<OverlayController, ToolbarViewModel, T> work)
    {
        using var coordinator = TestCoordinators.Started();
        using var shortcuts = new ToolbarShortcuts(coordinator);
        var settings = new DrawingSettings();
        var surface = Substitute.For<IOverlaySurface>();
        var controller = new OverlayController(
            surface,
            Substitute.For<IToolbarSurface>(),
            Substitute.For<IToolbarWindowStyler>());
        controller.Toggle(); // oculta → visible y en modo dibujo
        surface.IsVisible.Returns(true);
        var dragHandler = Substitute.For<IWindowDragHandler>();
        var broadcaster = new OverlayBroadcaster(
            Substitute.For<IOverlayCanvasProvider>(),
            Substitute.For<ICursorPositionProvider>(),
            settings,
            Substitute.For<IScreenClipLauncher>());
        var chrome = new ToolbarChrome(dragHandler, new SettingsWindowLauncher(controller, coordinator, dragHandler), shortcuts);
        using var viewModel = new ToolbarViewModel(broadcaster, settings, controller, chrome);
        return work(controller, viewModel);
    }

    private static List<ToolKind> SelectedTools(ToolbarViewModel viewModel) =>
        [.. viewModel.Tools.Where(tool => tool.IsSelected).Select(tool => tool.Kind)];
}
