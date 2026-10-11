namespace Acetato.Application.Hotkeys;

/// <summary>
/// Resultado de intentar asignar una combinación a una acción (HU-21). Jerarquía
/// cerrada a este ensamblado: <see cref="ChordAssigned"/>, <see cref="ChordInvalid"/>,
/// <see cref="ChordTakenByAction"/> y <see cref="ChordTakenByOtherApp"/>. Son tipos
/// de primer nivel porque CA1034 prohíbe anidar tipos públicos.
/// </summary>
public abstract record AssignResult
{
    private protected AssignResult()
    {
    }
}
