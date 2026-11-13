using System.Windows.Input;
using System.Windows.Threading;
using Acetato.Application.Abstractions;
using Acetato.Presentation.Lifetime;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>Una sola copia (HU-31): la segunda avisa, lo anota y se cierra; la primera activa el dibujo.</summary>
public sealed class SingleInstanceGateTests : IDisposable
{
    private readonly ISingleInstanceGuard _guard = Substitute.For<ISingleInstanceGuard>();
    private readonly RecordingLogger<SingleInstanceGate> _log = new();
    private readonly SingleInstanceGate _gate;

    public SingleInstanceGateTests() => _gate = new SingleInstanceGate(_guard, _log);

    [Fact]
    public void The_first_copy_enters_without_notifying()
    {
        _guard.TryClaim().Returns(true);

        _gate.TryEnter().Should().BeTrue();

        _guard.DidNotReceive().NotifyOwner();
        _log.Entries.Should().BeEmpty();
    }

    [Fact]
    public void A_second_copy_notifies_the_owner_logs_and_leaves()
    {
        _guard.TryClaim().Returns(false);

        _gate.TryEnter().Should().BeFalse();

        _guard.Received(1).NotifyOwner();
        _log.Entries.Should().ContainSingle()
            .Which.Message.Should().Be("Ya había otra copia abierta; esta se cierra");
    }

    [Fact]
    public void The_owner_runs_the_activation_on_the_interface_thread()
    {
        bool activated = WpfTestHost.Run(() =>
        {
            var command = Substitute.For<ICommand>();
            var dispatcher = Dispatcher.CurrentDispatcher;
            _gate.OnActivationRequested(dispatcher, command);

            _guard.ActivationRequested += Raise.Event();
            dispatcher.Invoke(static () => { }, DispatcherPriority.ContextIdle);

            command.Received(1).Execute(null);
            return true;
        });

        activated.Should().BeTrue();
    }

    public void Dispose()
    {
        _gate.Dispose();
        _guard.Dispose();
    }
}
