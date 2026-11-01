using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class ShapeConstraintTests
{
    [Fact]
    public void Square_takes_the_longer_side()
    {
        var end = ShapeConstraint.Square(new StrokePoint(10, 10), new StrokePoint(110, 40));

        end.Should().Be(new StrokePoint(110, 110));
    }

    [Fact]
    public void Square_keeps_the_drag_direction()
    {
        var end = ShapeConstraint.Square(new StrokePoint(100, 100), new StrokePoint(80, 40));

        end.Should().Be(new StrokePoint(40, 40));
    }

    [Fact]
    public void Square_keeps_an_already_square_drag()
    {
        var end = ShapeConstraint.Square(new StrokePoint(0, 0), new StrokePoint(-50, 50));

        end.Should().Be(new StrokePoint(-50, 50));
    }

    [Fact]
    public void Square_grows_a_flat_drag_downwards()
    {
        var end = ShapeConstraint.Square(new StrokePoint(0, 0), new StrokePoint(30, 0));

        end.Should().Be(new StrokePoint(30, 30));
    }
}
