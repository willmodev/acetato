namespace Acetato.Application.Abstractions;

/// <summary>
/// Estado de las teclas modificadoras en este instante (SPEC 11). Mantener Ctrl y
/// arrastrar mueve un dibujo desde cualquier herramienta, y la capa puede no tener
/// el foco del teclado (la barra nunca la activa), así que se lee el estado real del
/// sistema. El adaptador (P/Invoke) vive en Infrastructure.
/// </summary>
public interface IModifierKeyState
{
    /// <summary>Ctrl (izquierda o derecha) está pulsada ahora mismo.</summary>
    public bool IsCtrlDown { get; }
}
