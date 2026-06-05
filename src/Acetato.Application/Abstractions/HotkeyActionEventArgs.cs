namespace Acetato.Application.Abstractions;

/// <summary>
/// Datos del evento de atajo: qué <see cref="HotkeyAction"/> se solicitó.
/// </summary>
public sealed class HotkeyActionEventArgs(HotkeyAction action) : EventArgs
{
    /// <summary>Acción solicitada por el atajo pulsado.</summary>
    public HotkeyAction Action { get; } = action;
}
