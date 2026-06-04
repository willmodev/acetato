using System.Runtime.InteropServices;

namespace Acetato.Domain;

/// <summary>
/// Punto de un trazo, en coordenadas lógicas (independientes de DPI).
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct StrokePoint(double X, double Y);
