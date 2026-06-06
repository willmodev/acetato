using CommunityToolkit.Mvvm.ComponentModel;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// Ítem del popover de grosor (HU-10): un paso de la escala, el diámetro del
/// punto que lo representa y si es el grosor activo (para resaltarlo).
/// </summary>
public sealed partial class ThicknessItem : ObservableObject
{
    public ThicknessItem(int index, double diameter)
    {
        Index = index;
        Diameter = diameter;
    }

    /// <summary>Índice del paso en la escala de grosores.</summary>
    public int Index { get; }

    /// <summary>Diámetro (px) del punto que representa este grosor.</summary>
    public double Diameter { get; }

    /// <summary>Indica si es el grosor activo.</summary>
    [ObservableProperty]
    private bool _isSelected;
}
