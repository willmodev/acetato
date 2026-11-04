namespace Acetato.Application.Abstractions;

/// <summary>
/// «Reportar problema» de Ajustes (HU-33): arma en el escritorio un .zip con los
/// registros, settings.json e información del sistema, sin el nombre de usuario ni el del
/// equipo, para enviarlo a soporte. El adaptador vive en Infrastructure.
/// </summary>
public interface IProblemReportWriter
{
    /// <summary>Crea el reporte fuera del hilo de la interfaz. Nunca lanza: si falla, lo anota y lo dice.</summary>
    public Task<ProblemReportResult> CreateAsync(CancellationToken cancellationToken);
}
