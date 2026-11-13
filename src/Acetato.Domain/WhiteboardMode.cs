namespace Acetato.Domain;

/// <summary>
/// Estado de la pizarra (HU-35): apagada, blanca o negra. Los colores concretos del
/// fondo (<c>Board.White</c> y <c>Board.Black</c>) viven en el design system.
/// </summary>
public enum WhiteboardMode
{
    Off,
    White,
    Black,
}
