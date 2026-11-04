using Acetato.Application.Abstractions;
using Acetato.Application.Diagnostics;
using Acetato.Presentation.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Errores inesperados (HU-32): en la interfaz Acetato sigue abierto y avisa una vez;
/// una racha o un error fuera de la interfaz lo cierran. Cada error queda anotado.
/// </summary>
public sealed class UnhandledErrorHandlerTests : IDisposable
{
    private readonly RecordingLogger<UnhandledErrorHandler> _log = new();
    private readonly IErrorNotifier _notifier = Substitute.For<IErrorNotifier>();
    private readonly IFatalExit _fatal = Substitute.For<IFatalExit>();
    private readonly ManualTimeProvider _time = new();
    private readonly UnhandledErrorHandler _handler;

    public UnhandledErrorHandlerTests() =>
        _handler = new UnhandledErrorHandler(_log, new ErrorPolicy(_time), _notifier, _fatal);

    [Fact]
    public void Interface_error_keeps_Acetato_open_notifies_once_and_logs_every_time()
    {
        _handler.HandleInterfaceError(new InvalidOperationException("uno")).Should().BeTrue();
        _time.Advance(TimeSpan.FromMinutes(1));
        _handler.HandleInterfaceError(new InvalidOperationException("dos")).Should().BeTrue();

        _notifier.Received(1).NotifyRecovered();
        _fatal.DidNotReceiveWithAnyArgs().Exit(default!);
        _log.Entries.Should().HaveCount(2).And.OnlyContain(entry => entry.Level == LogLevel.Error);
        _log.Entries[1].Exception!.Message.Should().Be("dos");
    }

    [Fact]
    public void Three_interface_errors_in_ten_seconds_close_with_the_last_detail()
    {
        var last = new InvalidOperationException("tres");

        _handler.HandleInterfaceError(new InvalidOperationException("uno"));
        _handler.HandleInterfaceError(new InvalidOperationException("dos"));
        _handler.HandleInterfaceError(last);

        _fatal.Received(1).Exit(last);
        _log.Entries[^1].Level.Should().Be(LogLevel.Critical);
        _log.Entries[^1].Exception.Should().BeSameAs(last);
    }

    [Fact]
    public void Errors_while_closing_do_not_close_twice()
    {
        _handler.HandleFatalError(new InvalidOperationException("fatal"));

        _handler.HandleInterfaceError(new InvalidOperationException("otro")).Should().BeTrue();
        _handler.HandleFatalError(new InvalidOperationException("otro fatal"));

        _fatal.ReceivedWithAnyArgs(1).Exit(default!);
        _notifier.DidNotReceive().NotifyRecovered();
    }

    [Fact]
    public void Fatal_error_is_logged_and_closes()
    {
        var error = new InvalidOperationException("fuera de la interfaz");

        _handler.HandleFatalError(error);

        _fatal.Received(1).Exit(error);
        _log.Entries.Should().ContainSingle().Which.Level.Should().Be(LogLevel.Critical);
    }

    [Fact]
    public void Startup_error_is_logged_and_closes()
    {
        var error = new InvalidOperationException("al arrancar");

        _handler.HandleStartupError(error);

        _fatal.Received(1).Exit(error);
        _log.Entries.Should().ContainSingle().Which.Message.Should().Contain("Error al arrancar");
    }

    [Fact]
    public void Background_error_is_only_a_warning()
    {
        _handler.HandleBackgroundError(new InvalidOperationException("de fondo"));

        _log.Entries.Should().ContainSingle().Which.Level.Should().Be(LogLevel.Warning);
        _notifier.DidNotReceive().NotifyRecovered();
        _fatal.DidNotReceiveWithAnyArgs().Exit(default!);
    }

    public void Dispose() => _handler.Dispose();
}
