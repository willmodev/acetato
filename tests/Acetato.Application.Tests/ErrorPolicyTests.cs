using Acetato.Application.Diagnostics;
using FluentAssertions;
using Xunit;

namespace Acetato.Application.Tests;

/// <summary>
/// Qué pasa ante un error de la interfaz (HU-32): aviso la primera vez, silencio
/// después y cierre ante una racha de 3 en menos de 10 segundos.
/// </summary>
public sealed class ErrorPolicyTests
{
    private readonly ManualTimeProvider _time = new();
    private readonly ErrorPolicy _policy;

    public ErrorPolicyTests() => _policy = new ErrorPolicy(_time);

    [Fact]
    public void First_error_of_the_session_notifies()
    {
        _policy.Register().Should().Be(ErrorDecision.Notify);
    }

    [Fact]
    public void Later_errors_are_only_logged()
    {
        _policy.Register();
        _time.Advance(TimeSpan.FromMinutes(1));

        _policy.Register().Should().Be(ErrorDecision.Silent);
        _time.Advance(TimeSpan.FromMinutes(1));
        _policy.Register().Should().Be(ErrorDecision.Silent);
    }

    [Fact]
    public void Three_errors_within_ten_seconds_close()
    {
        _policy.Register();
        _time.Advance(TimeSpan.FromSeconds(4));
        _policy.Register();
        _time.Advance(TimeSpan.FromSeconds(5));

        _policy.Register().Should().Be(ErrorDecision.Close);
    }

    [Fact]
    public void Three_errors_spread_over_ten_seconds_do_not_close()
    {
        _policy.Register();
        _time.Advance(TimeSpan.FromSeconds(5));
        _policy.Register();
        _time.Advance(TimeSpan.FromSeconds(5));

        // El primero ya salió de la ventana: quedan dos.
        _policy.Register().Should().Be(ErrorDecision.Silent);
    }

    [Fact]
    public void A_burst_later_in_the_session_still_closes()
    {
        _policy.Register();
        _time.Advance(TimeSpan.FromMinutes(5));
        _policy.Register();
        _policy.Register();

        _policy.Register().Should().Be(ErrorDecision.Close);
    }
}
