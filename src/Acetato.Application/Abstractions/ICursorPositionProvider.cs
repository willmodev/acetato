namespace Acetato.Application.Abstractions;

/// <summary>
/// Provee la posición del cursor (HU-09) para decidir el monitor "activo": dónde
/// colocar la barra o a qué lienzo dirigir una acción dirigida (p.ej. deshacer).
/// El adaptador (P/Invoke) vive en Infrastructure.
/// </summary>
public interface ICursorPositionProvider
{
    /// <summary>
    /// Posición actual del cursor en píxeles físicos (coordenadas del escritorio
    /// virtual; pueden ser negativas con monitores a la izquierda o arriba).
    /// </summary>
    public (int X, int Y) GetCursorPosition();
}
