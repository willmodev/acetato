using System.Runtime.InteropServices;

namespace Acetato.Domain;

/// <summary>
/// Recuadro de una selección en coordenadas lógicas (DIP), con el origen arriba a la
/// izquierda (HU-28). Tipo puro (sin WPF), testeable.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct SelectionBox(double X, double Y, double Width, double Height);
