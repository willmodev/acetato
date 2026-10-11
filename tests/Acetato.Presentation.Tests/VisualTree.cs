using System.Windows;
using System.Windows.Media;

namespace Acetato.Presentation.Tests;

/// <summary>Recorrido del árbol visual para los tests de humo.</summary>
internal static class VisualTree
{
    /// <summary>Todos los descendientes de tipo <typeparamref name="T"/>, incluida la raíz.</summary>
    public static IEnumerable<T> Descendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        if (root is T match)
        {
            yield return match;
        }

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            foreach (var descendant in Descendants<T>(VisualTreeHelper.GetChild(root, i)))
            {
                yield return descendant;
            }
        }
    }
}
