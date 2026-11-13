using Acetato.Infrastructure.Diagnostics;
using FluentAssertions;
using Xunit;

namespace Acetato.Infrastructure.Tests;

/// <summary>Una sola copia (HU-31), con un nombre único por test.</summary>
public sealed class NamedSingleInstanceGuardTests : IDisposable
{
    private static readonly TimeSpan SignalTimeout = TimeSpan.FromSeconds(5);

    private readonly string _name = "Acetato.Tests." + Guid.NewGuid().ToString("N");
    private readonly NamedSingleInstanceGuard _first;
    private readonly NamedSingleInstanceGuard _second;

    public NamedSingleInstanceGuardTests()
    {
        _first = new NamedSingleInstanceGuard(_name);
        _second = new NamedSingleInstanceGuard(_name);
    }

    [Fact]
    public void The_first_claims_and_the_second_does_not()
    {
        _first.TryClaim().Should().BeTrue();
        _second.TryClaim().Should().BeFalse();
    }

    [Fact]
    public void The_second_signal_reaches_the_first()
    {
        using var activated = new ManualResetEventSlim();
        _first.ActivationRequested += (_, _) => activated.Set();
        _first.TryClaim();

        _second.TryClaim();
        _second.NotifyOwner();

        activated.Wait(SignalTimeout).Should().BeTrue();
    }

    [Fact]
    public void Every_new_copy_rings_again()
    {
        int rings = 0;
        using var activated = new SemaphoreSlim(0);
        _first.ActivationRequested += (_, _) =>
        {
            Interlocked.Increment(ref rings);
            activated.Release();
        };
        _first.TryClaim();

        _second.NotifyOwner();
        activated.Wait(SignalTimeout).Should().BeTrue();
        _second.NotifyOwner();
        activated.Wait(SignalTimeout).Should().BeTrue();

        Volatile.Read(ref rings).Should().Be(2);
    }

    [Fact]
    public void After_the_first_closes_a_new_copy_can_claim()
    {
        _first.TryClaim();
        _first.Dispose();

        _second.TryClaim().Should().BeTrue();
    }

    public void Dispose()
    {
        _first.Dispose();
        _second.Dispose();
    }
}
