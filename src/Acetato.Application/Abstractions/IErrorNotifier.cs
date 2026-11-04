namespace Acetato.Application.Abstractions;

/// <summary>
/// Aviso al usuario de que algo falló pero Acetato sigue abierto (HU-32). El adaptador
/// muestra un aviso de Windows en Infrastructure.
/// </summary>
public interface IErrorNotifier
{
    /// <summary>
    /// Muestra el aviso, sin botones y sin acción al hacer clic. Nunca lanza: si los
    /// avisos están apagados, no pasa nada más.
    /// </summary>
    public void NotifyRecovered();
}
