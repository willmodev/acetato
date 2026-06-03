namespace Acetato.Domain;

/// <summary>
/// Punto de un trazo, en coordenadas lógicas (independientes de DPI).
/// </summary>
public readonly record struct StrokePoint(double X, double Y);
