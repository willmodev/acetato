using System.Windows.Media;
using Acetato.Domain;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// Ítem del popover de tintas (HU-10): una tinta seleccionable, su pincel para
/// pintar el swatch y si es la tinta activa (para mostrar el anillo de acento).
/// </summary>
public sealed partial class InkSwatchItem : ObservableObject
{
    public InkSwatchItem(TintaColor color, Brush fill)
    {
        Color = color;
        Fill = fill;
    }

    /// <summary>Tinta que representa este swatch.</summary>
    public TintaColor Color { get; }

    /// <summary>Pincel con el color de la tinta.</summary>
    public Brush Fill { get; }

    /// <summary>Indica si es la tinta activa.</summary>
    [ObservableProperty]
    private bool _isSelected;
}
