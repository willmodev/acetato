using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class ToolRingTests
{
    [Fact]
    public void Order_is_the_drawing_tool_cycle()
    {
        ToolRing.Order.Should().Equal(
            ToolKind.Pencil, ToolKind.Line, ToolKind.Arrow, ToolKind.Rectangle, ToolKind.Eraser);
    }

    [Fact]
    public void Next_advances_to_the_following_tool()
    {
        ToolRing.Next(ToolKind.Pencil).Should().Be(ToolKind.Line);
    }

    [Fact]
    public void Next_wraps_from_the_last_to_the_first()
    {
        ToolRing.Next(ToolKind.Eraser).Should().Be(ToolKind.Pencil);
    }

    [Fact]
    public void Next_from_a_tool_outside_the_ring_returns_the_first()
    {
        ToolRing.Next(ToolKind.Text).Should().Be(ToolKind.Pencil);
    }
}
