namespace Acetato.Application.Abstractions;

/// <summary>
/// Lee y fija la posición de la barra en píxeles físicos (HU-29). En DIP, con monitores
/// de distinta escala, la barra caería en otro sitio (lección de HU-09). El interop vive
/// en Infrastructure.
/// </summary>
public interface IToolbarWindowPlacer
{
    /// <summary>Recuadro de la ventana en píxeles físicos; <c>null</c> si no se pudo leer.</summary>
    public (int X, int Y, int Width, int Height)? GetBounds(nint windowHandle);

    /// <summary>Lleva la esquina de la ventana a (<paramref name="x"/>, <paramref name="y"/>), sin activarla.</summary>
    public void MoveTo(nint windowHandle, int x, int y);
}
