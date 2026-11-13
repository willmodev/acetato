using System.Diagnostics.CodeAnalysis;

namespace Acetato.Presentation.Diagnostics;

/// <summary>Cierre ante un error del que Acetato no se puede recuperar (HU-32).</summary>
internal interface IFatalExit
{
    /// <summary>Vacía el registro, muestra el mensaje de cierre y termina el proceso.</summary>
    [DoesNotReturn]
    public void Exit(Exception exception);
}
