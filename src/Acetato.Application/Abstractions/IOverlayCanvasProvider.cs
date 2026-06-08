namespace Acetato.Application.Abstractions;

/// <summary>
/// Expone los lienzos del overlay activos (uno por monitor, HU-09) para que el
/// broadcaster reparta las acciones. La superficie agregadora lo implementa.
/// </summary>
public interface IOverlayCanvasProvider
{
    /// <summary>Lienzos activos; vacío si el overlay no se ha mostrado.</summary>
    public IReadOnlyList<IOverlayCanvas> Canvases { get; }
}
