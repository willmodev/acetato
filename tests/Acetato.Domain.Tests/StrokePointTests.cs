using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class StrokePointTests
{
    [Fact]
    public void Two_points_with_same_coordinates_are_equal()
    {
        var a = new StrokePoint(10, 20);
        var b = new StrokePoint(10, 20);

        a.Should().Be(b);
    }
}
