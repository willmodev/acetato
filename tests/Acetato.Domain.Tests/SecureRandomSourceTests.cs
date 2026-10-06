using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Domain.Tests;

public sealed class SecureRandomSourceTests
{
    [Fact]
    public void NextInt_stays_within_the_requested_range()
    {
        for (var i = 0; i < 500; i++)
        {
            SecureRandomSource.Instance.NextInt(5).Should().BeInRange(0, 4);
        }
    }

    [Fact]
    public void NextInt_throws_when_the_upper_bound_is_not_positive()
    {
        var act = () => SecureRandomSource.Instance.NextInt(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
