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

    private OverlayController CreateController() => new(_surface, _styler);

    [Fact]
    public void Toggle_when_hidden_shows_and_applies_styles()
    {
        _surface.IsVisible.Returns(false);
        _surface.Handle.Returns(new nint(42));
        var controller = CreateController();

        controller.Toggle();

        _surface.Received(1).Show();
        _surface.DidNotReceive().Hide();
        _styler.Received(1).ApplyOverlayStyles(new nint(42));
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
    public void Styles_are_not_applied_when_handle_is_not_ready()
    {
        _surface.IsVisible.Returns(false);
        _surface.Handle.Returns(nint.Zero);
        var controller = CreateController();

        controller.Toggle();

        _surface.Received(1).Show();
        _styler.DidNotReceive().ApplyOverlayStyles(Arg.Any<nint>());
    }
}
