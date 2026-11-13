using Microsoft.Extensions.Logging;

namespace Acetato.Infrastructure.Tests;

/// <summary>Registro en memoria para comprobar qué se anota (HU-32).</summary>
/// <typeparam name="T">Categoría del registro, como en la app.</typeparam>
internal sealed class RecordingLogger<T> : ILogger<T>
{
    /// <summary>Lo anotado, en orden: nivel, mensaje y detalle.</summary>
    public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        Entries.Add((logLevel, formatter(state, exception), exception));
    }
}
