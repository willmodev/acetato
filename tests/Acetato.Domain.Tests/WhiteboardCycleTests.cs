using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class WhiteboardCycleTests
{
    [Fact]
    public void Off_goes_to_white()
    {
        WhiteboardCycle.Next(WhiteboardMode.Off).Should().Be(WhiteboardMode.White);
    }

    [Fact]
    public void White_goes_to_black()
    {
        WhiteboardCycle.Next(WhiteboardMode.White).Should().Be(WhiteboardMode.Black);
    }

    [Fact]
    public void Black_goes_back_to_off()
    {
        WhiteboardCycle.Next(WhiteboardMode.Black).Should().Be(WhiteboardMode.Off);
    }
}
