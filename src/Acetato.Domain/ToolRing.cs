namespace Acetato.Domain;

/// <summary>
/// Anillo de herramientas dibujables (HU-11) para ciclar con un solo atajo
/// (Ctrl+Alt+Espacio): lápiz → línea → flecha → rectángulo → texto → borrador → lápiz.
/// Tipo puro (sin WPF), testeable.
/// </summary>
public static class ToolRing
{
    /// <summary>Orden del ciclo de herramientas.</summary>
    public static IReadOnlyList<ToolKind> Order { get; } =
    [
        ToolKind.Pencil, ToolKind.Line, ToolKind.Arrow,
        ToolKind.Rectangle, ToolKind.Text, ToolKind.Eraser,
    ];

    /// <summary>
    /// Siguiente herramienta del anillo; envuelve al llegar al final. Si la
    /// herramienta actual no está en el anillo, devuelve la primera.
    /// </summary>
    public static ToolKind Next(ToolKind current)
    {
        int index = IndexOf(current);
        return Order[(index + 1) % Order.Count];
    }

    private static int IndexOf(ToolKind tool)
    {
        for (int i = 0; i < Order.Count; i++)
        {
            if (Order[i] == tool)
            {
                return i;
            }
        }

        return -1; // (-1 + 1) % n = 0 → primera del anillo
    }
}
