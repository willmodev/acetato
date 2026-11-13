namespace Acetato.Domain;

/// <summary>
/// Ciclo de la pizarra (HU-35) con un solo botón o atajo: apagada → blanca → negra →
/// apagada. Tipo puro (sin WPF), testeable.
/// </summary>
public static class WhiteboardCycle
{
    /// <summary>Estado que sigue al actual; tras la negra vuelve a apagada.</summary>
    public static WhiteboardMode Next(WhiteboardMode current) => current switch
    {
        WhiteboardMode.Off => WhiteboardMode.White,
        WhiteboardMode.White => WhiteboardMode.Black,
        _ => WhiteboardMode.Off,
    };
}
