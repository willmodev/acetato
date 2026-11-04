namespace Acetato.Application.Abstractions;

/// <summary>Resultado de armar el reporte para soporte (HU-33).</summary>
/// <param name="Succeeded">Se creó el .zip.</param>
/// <param name="FilePath">Ruta del .zip; <c>null</c> si no se pudo crear.</param>
public sealed record ProblemReportResult(bool Succeeded, string? FilePath)
{
    /// <summary>No se pudo crear el reporte (el motivo queda en el registro).</summary>
    public static ProblemReportResult Failed { get; } = new(Succeeded: false, FilePath: null);
}
