using Acetato.Application.Diagnostics;

namespace Acetato.Application.Abstractions;

/// <summary>
/// Datos de la app que muestran Ajustes, el registro y el reporte (HU-33). El adaptador
/// lee el ensamblado en Infrastructure.
/// </summary>
public interface IAppInfo
{
    /// <summary>Versión de Acetato: la de pantalla y la completa.</summary>
    public AppVersion Version { get; }
}
