namespace Acetato.Presentation.Tests;

/// <summary>Reloj que solo avanza cuando el test lo pide.</summary>
internal sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _now = new(2026, 11, 3, 15, 30, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _now;

    /// <summary>Adelanta el reloj.</summary>
    public void Advance(TimeSpan delta) => _now += delta;
}
