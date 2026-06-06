using Acetato.Domain;

namespace Acetato.Application.Drawing;

/// <summary>
/// Estado del trazo activo: color (HU-05) y grosor (HU-06). Agnóstico de
/// plataforma; la traducción a tipos de WPF vive en Presentation. Notifica con
/// <see cref="Changed"/> para que la vista refresque los atributos de dibujo.
/// </summary>
public interface IDrawingSettings
{
    /// <summary>Color activo del próximo trazo.</summary>
    public TintaColor Color { get; }

    /// <summary>Grosor activo (px) del próximo trazo.</summary>
    public double Thickness { get; }

    /// <summary>Índice del grosor activo en la escala (0 = más fino).</summary>
    public int ThicknessIndex { get; }

    /// <summary>Cambia el color activo (HU-05).</summary>
    public void SelectColor(TintaColor color);

    /// <summary>Sube un paso de grosor; en el máximo se mantiene (HU-06).</summary>
    public void IncreaseThickness();

    /// <summary>Baja un paso de grosor; en el mínimo se mantiene (HU-06).</summary>
    public void DecreaseThickness();

    /// <summary>Fija el grosor por índice de la escala; hace clamp al rango (HU-10).</summary>
    public void SelectThickness(int index);

    /// <summary>Se dispara cuando color o grosor cambian de verdad.</summary>
    public event EventHandler? Changed;
}
