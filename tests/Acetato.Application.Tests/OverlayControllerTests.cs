using Acetato.Application.Abstractions;
using Acetato.Application.Overlay;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Application.Tests;

public sealed class OverlayControllerTests
{
    private readonly IOverlaySurface _surface = Substitute.For<IOverlaySurface>();
    private readonly IToolbarSurface _toolbar = Substitute.For<IToolbarSurface>();
    private readonly IToolbarWindowStyler _toolbarStyler = Substitute.For<IToolbarWindowStyler>();

    private OverlayController CreateController() => new(_surface, _toolbar, _toolbarStyler);

    [Fact]
    public void Toggle_when_hidden_shows_applies_styles_and_enters_drawing_mode()
    {
        _surface.IsVisible.Returns(false);
        var controller = CreateController();

        controller.Toggle();

        _surface.Received(1).Show();
        _surface.DidNotReceive().Hide();
        _surface.Received(1).ApplyStyles();
        controller.IsDrawingMode.Should().BeTrue();
        // Modo dibujo => la capa captura los clics => sin click-through.
        _surface.Received(1).SetClickThrough(false);
    }

    [Fact]
    public void Toggle_when_visible_hides_and_does_not_restyle()
    {
        _surface.IsVisible.Returns(true);
        var controller = CreateController();

        controller.Toggle();

        _surface.Received(1).Hide();
        _surface.DidNotReceive().Show();
        _surface.DidNotReceive().ApplyStyles();
    }

    [Fact]
    public void Toggle_when_hidden_shows_both_surfaces()
    {
        _surface.IsVisible.Returns(false);
        var controller = CreateController();

        controller.Toggle();

        _surface.Received(1).Show();
        _toolbar.Received(1).Show();
    }

    [Fact]
    public void Toggle_when_visible_hides_both_surfaces()
    {
        _surface.IsVisible.Returns(true);
        var controller = CreateController();

        controller.Toggle();

        _surface.Received(1).Hide();
        _toolbar.Received(1).Hide();
    }

    [Fact]
    public void Toggle_when_hidden_applies_toolbar_styles()
    {
        _surface.IsVisible.Returns(false);
        _toolbar.Handle.Returns(new nint(99));
        var controller = CreateController();

        controller.Toggle();

        _toolbarStyler.Received(1).ApplyToolbarStyles(new nint(99));
    }

    [Fact]
    public void Toolbar_styles_are_applied_only_once_across_multiple_shows()
    {
        _surface.IsVisible.Returns(false);
        _toolbar.Handle.Returns(new nint(99));
        var controller = CreateController();

        controller.Toggle();
        controller.Toggle();

        _toolbarStyler.Received(1).ApplyToolbarStyles(Arg.Any<nint>());
    }

    [Fact]
    public void Toolbar_styles_are_not_applied_when_handle_is_not_ready()
    {
        _surface.IsVisible.Returns(false);
        _toolbar.Handle.Returns(nint.Zero);
        var controller = CreateController();

        controller.Toggle();

        _toolbarStyler.DidNotReceive().ApplyToolbarStyles(Arg.Any<nint>());
    }

    [Fact]
    public void Set_normal_mode_enables_click_through()
    {
        var controller = CreateController();

        controller.SetDrawingMode(false);

        controller.IsDrawingMode.Should().BeFalse();
        _surface.Received(1).SetClickThrough(true);
    }

    [Fact]
    public void Set_drawing_mode_disables_click_through()
    {
        var controller = CreateController();

        controller.SetDrawingMode(true);

        controller.IsDrawingMode.Should().BeTrue();
        _surface.Received(1).SetClickThrough(false);
    }

    [Fact]
    public void Overlay_styles_are_applied_each_time_it_is_shown()
    {
        _surface.IsVisible.Returns(false);
        var controller = CreateController();

        controller.Toggle();
        controller.Toggle();

        // La idempotencia vive ahora en la superficie; el controlador la invoca al mostrar.
        _surface.Received(2).ApplyStyles();
    }
}
