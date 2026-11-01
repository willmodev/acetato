namespace Acetato.Domain;

/// <summary>
/// Restricciones del arrastre de una forma. Con Mayús, la Elipse dibuja un círculo
/// (HU-16): el recuadro pasa a ser cuadrado. Tipo puro (sin WPF), testeable.
/// </summary>
public static class ShapeConstraint
{
    /// <summary>
    /// Ajusta <paramref name="end"/> para que el recuadro con <paramref name="start"/>
    /// sea cuadrado: toma el lado más largo y conserva la dirección del arrastre.
    /// </summary>
    public static StrokePoint Square(StrokePoint start, StrokePoint end)
    {
        double dx = end.X - start.X;
        double dy = end.Y - start.Y;
        double side = Math.Max(Math.Abs(dx), Math.Abs(dy));
        return new StrokePoint(start.X + (Direction(dx) * side), start.Y + (Direction(dy) * side));
    }

    // Un eje sin desplazamiento crece hacia la derecha o hacia abajo.
    private static double Direction(double delta) => delta < 0d ? -1d : 1d;
}
