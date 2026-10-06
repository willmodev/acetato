using System.Runtime.InteropServices;

namespace Acetato.Domain;

/// <summary>
/// Punta abierta de la flecha libre: ala izquierda → vértice → ala derecha. "Izquierda"
/// es la del lado izquierdo del recorrido tal como se ve en pantalla (eje Y hacia abajo).
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct ArrowHead(StrokePoint Left, StrokePoint Tip, StrokePoint Right);
