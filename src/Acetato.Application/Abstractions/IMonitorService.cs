using Acetato.Domain;

namespace Acetato.Application.Abstractions;

/// <summary>
/// Enumera los monitores conectados (HU-09) para que el overlay cubra todas las
/// pantallas y la barra se ubique en la activa. El adaptador (P/Invoke) vive en
/// Infrastructure; aquí solo el contrato.
/// </summary>
public interface IMonitorService
{
    /// <summary>
    /// Monitores conectados, en píxeles físicos. En un sistema con pantalla nunca
    /// está vacío; el primario está marcado con <see cref="MonitorInfo.IsPrimary"/>.
    /// </summary>
    public IReadOnlyList<MonitorInfo> GetMonitors();
}
