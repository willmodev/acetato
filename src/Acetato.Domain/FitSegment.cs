using System.Runtime.InteropServices;

namespace Acetato.Domain;

/// <summary>
/// Tramo del recorrido que el ajuste Bézier intenta cubrir con UNA curva (HU-20):
/// índices de su primer y último punto y las tangentes unitarias en cada extremo.
/// <see cref="TangentEnd"/> apunta hacia atrás (del final hacia el interior del tramo),
/// como en el algoritmo de Schneider.
/// </summary>
[StructLayout(LayoutKind.Auto)]
internal readonly record struct FitSegment(int First, int Last, StrokePoint TangentStart, StrokePoint TangentEnd)
{
    public int Count => Last - First + 1;
}
