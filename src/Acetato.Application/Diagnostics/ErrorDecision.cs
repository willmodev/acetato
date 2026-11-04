namespace Acetato.Application.Diagnostics;

/// <summary>Qué hacer ante un error inesperado de la interfaz (HU-32).</summary>
public enum ErrorDecision
{
    /// <summary>Primera vez en la sesión: Acetato sigue abierto y sale un aviso de Windows.</summary>
    Notify,

    /// <summary>Ya se avisó en esta sesión: Acetato sigue abierto y solo se anota.</summary>
    Silent,

    /// <summary>Racha de errores: se trata como fatal, con mensaje y cierre.</summary>
    Close,
}
