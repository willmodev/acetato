using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Acetato.Presentation.Diagnostics;

/// <summary>
/// Cierre fatal (HU-32): vacía el registro, avisa con un mensaje y termina con
/// <see cref="Environment.FailFast(string, Exception)"/>. Así Windows lo cuenta como cierre
/// y llegará al Health report de la Store. No pasa por <c>OnExit</c>: los atajos los libera
/// el sistema al morir el proceso.
/// </summary>
internal sealed class FatalExit : IFatalExit
{
    /// <summary>Mensaje que ve el usuario antes del cierre.</summary>
    public const string Message = "Acetato tuvo un problema y se va a cerrar. Se guardó un registro.";

    private const string Caption = "Acetato";

    private readonly IDisposable? _log;

    /// <param name="log">Registro a vaciar antes de cerrar; <c>null</c> si no se llegó a abrir.</param>
    public FatalExit(IDisposable? log) => _log = log;

    [DoesNotReturn]
    public void Exit(Exception exception)
    {
        _log?.Dispose();
        _ = MessageBox.Show(Message, Caption, MessageBoxButton.OK, MessageBoxImage.Error);
        Environment.FailFast(Message, exception);
    }
}
