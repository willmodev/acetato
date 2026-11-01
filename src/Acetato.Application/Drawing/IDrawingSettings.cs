using Acetato.Domain;

namespace Acetato.Application.Drawing;

/// <summary>
/// Estado del trazo activo: color (HU-05), grosor (HU-06), modo Degradado (HU-19) y Tintas
/// degradadas (HU-27). Agnóstico de plataforma; la traducción a tipos de WPF vive en Presentation. Notifica con
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

    /// <summary>Herramienta activa (HU-11).</summary>
    public ToolKind SelectedTool { get; }

    /// <summary>Modo Degradado activo: cada trazo toma un par neón al azar (HU-19).</summary>
    public bool IsGradient { get; }

    /// <summary>Par neón que tomará el PRÓXIMO trazo en modo Degradado (HU-19).</summary>
    public TintaGradiente NextGradient { get; }

    /// <summary>Tintas degradadas: cada tinta sólida pinta hacia su vecina fija (HU-27).</summary>
    public bool InkPairsEnabled { get; }

    /// <summary>
    /// Par del PRÓXIMO trazo: el neón en modo Degradado; si no, la tinta hacia su vecina con
    /// Tintas degradadas encendido; <c>null</c> = tinta plana (HU-19, HU-27).
    /// </summary>
    public GradientPair? NextStrokePair { get; }

    /// <summary>Cambia el color activo y sale del modo Degradado (HU-05/HU-19).</summary>
    public void SelectColor(TintaColor color);

    /// <summary>Entra al modo Degradado; <see cref="Color"/> conserva la última tinta sólida (HU-19).</summary>
    public void SelectGradient();

    /// <summary>
    /// Tras fijar un trazo en modo Degradado, sortea el par neón del siguiente, distinto del
    /// actual (HU-19). Fuera del modo no hace nada (HU-27).
    /// </summary>
    public void AdvanceGradient();

    /// <summary>Enciende o apaga Tintas degradadas (HU-27).</summary>
    public void SetInkPairs(bool enabled);

    /// <summary>Sube un paso de grosor; en el máximo se mantiene (HU-06).</summary>
    public void IncreaseThickness();

    /// <summary>Baja un paso de grosor; en el mínimo se mantiene (HU-06).</summary>
    public void DecreaseThickness();

    /// <summary>Fija el grosor por índice de la escala; hace clamp al rango (HU-10).</summary>
    public void SelectThickness(int index);

    /// <summary>Selecciona la herramienta activa (HU-11).</summary>
    public void SelectTool(ToolKind tool);

    /// <summary>Avanza a la siguiente herramienta del anillo (HU-11).</summary>
    public void CycleTool();

    /// <summary>Se dispara cuando color, degradado, grosor o herramienta cambian de verdad.</summary>
    public event EventHandler? Changed;
}
