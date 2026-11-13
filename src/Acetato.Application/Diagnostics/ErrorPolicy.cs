namespace Acetato.Application.Diagnostics;

/// <summary>
/// Decide qué pasa ante cada error inesperado de la interfaz (HU-32). Acetato sigue
/// abierto: la primera vez en la sesión avisa (<see cref="ErrorDecision.Notify"/>) y las
/// siguientes solo anota (<see cref="ErrorDecision.Silent"/>). Si llegan
/// <see cref="BurstCount"/> errores en menos de <see cref="BurstWindow"/>, lo trata como
/// fatal (<see cref="ErrorDecision.Close"/>): un error que se repite sin parar suele ser un
/// bucle, y seguir abierto así es peor que cerrar.
/// </summary>
public sealed class ErrorPolicy
{
    /// <summary>Errores seguidos que cierran Acetato.</summary>
    public const int BurstCount = 3;

    /// <summary>Ventana de la racha: los errores deben caer en menos de este tiempo.</summary>
    public static readonly TimeSpan BurstWindow = TimeSpan.FromSeconds(10);

    private readonly TimeProvider _time;
    private readonly Queue<DateTimeOffset> _recent = new();
    private readonly Lock _gate = new();
    private bool _notified;

    public ErrorPolicy(TimeProvider time) => _time = time;

    /// <summary>Registra un error de la interfaz y dice qué hacer con él.</summary>
    public ErrorDecision Register()
    {
        lock (_gate)
        {
            var now = _time.GetUtcNow();
            _recent.Enqueue(now);
            while (now - _recent.Peek() >= BurstWindow)
            {
                _ = _recent.Dequeue();
            }

            if (_recent.Count >= BurstCount)
            {
                return ErrorDecision.Close;
            }

            if (_notified)
            {
                return ErrorDecision.Silent;
            }

            _notified = true;
            return ErrorDecision.Notify;
        }
    }
}
