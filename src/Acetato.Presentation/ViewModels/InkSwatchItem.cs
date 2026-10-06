using System.Windows.Media;
using Acetato.Domain;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// Ítem del popover de tintas (HU-10): una tinta sólida seleccionable, o la muestra del
/// modo Degradado (HU-19). Lleva su pincel para pintar la muestra y si está activo
/// (para mostrar el anillo de acento).
/// </summary>
public sealed partial class InkSwatchItem : ObservableObject
{
    public InkSwatchItem(TintaColor color, Brush fill)
        : this((TintaColor?)color, fill)
    {
    }

    private InkSwatchItem(TintaColor? color, Brush fill)
    {
        Color = color;
        Fill = fill;
    }

    /// <summary>Tinta sólida que representa este ítem; <c>null</c> en la muestra de degradado.</summary>
    public TintaColor? Color { get; }

    /// <summary>Indica si es la muestra del modo Degradado (no una tinta sólida).</summary>
    public bool IsGradient => Color is null;

    /// <summary>Pincel con el color de la tinta, o con el degradado de muestra.</summary>
    public Brush Fill { get; }

    /// <summary>Indica si es la opción activa (la tinta elegida o el modo Degradado).</summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>Crea la muestra del modo Degradado con el pincel dado.</summary>
    public static InkSwatchItem CreateGradient(Brush fill) => new((TintaColor?)null, fill);
}
