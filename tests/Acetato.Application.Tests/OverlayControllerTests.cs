using Acetato.Application.Abstractions;
using Acetato.Application.Overlay;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Application.Tests;

public sealed class OverlayControllerTests
{
    private readonly IOverlaySurface _surface = Substitute.For<IOverlaySurface>();
    private readonly IOverlayWindowStyler _styler = Substitute.For<IOverlayWindowStyler>();
    private readonly IToolbarSurface _toolbar = Substitute.For<IToolbarSurface>();
    private readonly IToolbarWindowStyler _toolbarStyler = Substitute.For<IToolbarWindowStyler>();

    private OverlayController CreateController() => new(_surface, _styler, _toolbar, _toolbarStyler);

    [Fact]
    public void Toggle_when_hidden_shows_applies_styles_and_enters_drawing_mode()
    {
        _surface.IsVisible.Returns(false);
        _surface.Handle.Returns(new nint(42));
        var controller = CreateController();

        controller.Toggle();

        _surface.Received(1).Show();
        _surface.DidNotReceive().Hide();
        _styler.Received(1).ApplyOverlayStyles(new nint(42));
        controller.IsDrawingMode.Should().BeTrue();
        // Modo dibujo => la capa captura los clics => sin click-through.
        _styler.Received(1).SetClickThrough(new nint(42), false);
    }

    [Fact]
    public void Toggle_when_visible_hides_and_does_not_restyle()
    {
        _surface.IsVisible.Returns(true);
        var controller = CreateController();

        controller.Toggle();

        _surface.Received(1).Hide();
        _surface.DidNotReceive().Show();
        _styler.DidNotReceive().ApplyOverlayStyles(Arg.Any<nint>());
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
        _surface.Handle.Returns(new nint(7));
        var controller = CreateController();

        controller.SetDrawingMode(false);

        controller.IsDrawingMode.Should().BeFalse();
        _styler.Received(1).SetClickThrough(new nint(7), true);
    }

    [Fact]
    public void Set_drawing_mode_disables_click_through()
    {
        _surface.Handle.Returns(new nint(7));
        var controller = CreateController();

        controller.SetDrawingMode(true);

        controller.IsDrawingMode.Should().BeTrue();
        _styler.Received(1).SetClickThrough(new nint(7), false);
    }

    [Fact]
    public void Styles_are_applied_only_once_across_multiple_shows()
    {
        _surface.IsVisible.Returns(false);
        _surface.Handle.Returns(new nint(7));
        var controller = CreateController();

        controller.Toggle();
        controller.Toggle();

        _styler.Received(1).ApplyOverlayStyles(Arg.Any<nint>());
    }

    [Fact]
    public void Mode_is_not_applied_when_handle_is_not_ready()
    {
        _surface.Handle.Returns(nint.Zero);
        var controller = CreateController();

        controller.SetDrawingMode(true);

        controller.IsDrawingMode.Should().BeTrue();
        _styler.DidNotReceive().SetClickThrough(Arg.Any<nint>(), Arg.Any<bool>());
    }
}
