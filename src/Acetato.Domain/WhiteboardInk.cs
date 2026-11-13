namespace Acetato.Domain;

/// <summary>
/// Tinta contraria sobre la pizarra (HU-35): si la tinta activa es del mismo color que
/// el fondo, el primer trazo no se vería, así que pasa a la contraria. Solo cambian la
/// blanca sobre la pizarra blanca y la negra sobre la negra; el resto de tintas se
/// leen sobre los dos fondos. Tipo puro (sin WPF), testeable.
/// </summary>
public static class WhiteboardInk
{
    /// <summary>
    /// Tinta que debe usarse en lugar de <paramref name="ink"/> sobre la pizarra
    /// <paramref name="mode"/>, o <c>null</c> si la tinta actual ya se ve.
    /// </summary>
    public static TintaColor? Contrast(TintaColor ink, WhiteboardMode mode) => (ink, mode) switch
    {
        (TintaColor.White, WhiteboardMode.White) => TintaColor.Black,
        (TintaColor.Black, WhiteboardMode.Black) => TintaColor.White,
        _ => null,
    };
}
