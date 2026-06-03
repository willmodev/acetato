using Acetato.Application.Abstractions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Application.Tests;

public sealed class AbstractionsTests
{
    [Fact]
    public void Overlay_controller_toggle_can_be_mocked()
    {
        var controller = Substitute.For<IOverlayController>();
        controller.IsVisible.Returns(true);

        controller.Toggle();

        controller.Received(1).Toggle();
        controller.IsVisible.Should().BeTrue();
    }
}
