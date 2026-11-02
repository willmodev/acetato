namespace Acetato.Domain;

/// <summary>
/// Tirador de la selección que se jala para cambiar su tamaño (HU-28): las cuatro
/// esquinas y los cuatro lados.
/// </summary>
public enum ResizeHandle
{
    TopLeft,
    Top,
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left,
}
