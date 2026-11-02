using System.Runtime.InteropServices;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Qué hace Mover con un clic (SPEC 11): dejarlo pasar a la herramienta, quedárselo
/// sin hacer nada (Ctrl en un sitio vacío) o arrastrar <see cref="Targets"/>.
/// </summary>
/// <param name="Handles">El clic no llega a la herramienta activa.</param>
/// <param name="Targets">Lo que se arrastra; vacío si no hay nada debajo.</param>
/// <param name="KeepSelected">Al soltar queda seleccionado (la mano sí, Ctrl no).</param>
[StructLayout(LayoutKind.Auto)]
internal readonly record struct GrabDecision(bool Handles, GrabTargets Targets, bool KeepSelected)
{
    /// <summary>El clic sigue su camino: lazo de la mano o dibujo de la herramienta.</summary>
    public static GrabDecision Pass { get; } = new(false, GrabTargets.None, false);
}
