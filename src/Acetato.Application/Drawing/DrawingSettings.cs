using Acetato.Domain;

namespace Acetato.Application.Drawing;

/// <summary>
/// Implementación del estado de dibujo. El grosor se mueve por la
/// <see cref="ThicknessScale"/> con tope en ambos extremos. Solo dispara
/// <see cref="Changed"/> cuando el valor cambia de verdad (evita refrescos
/// inútiles al pulsar en el tope). El sorteo del par de degradado vive aquí, en la
/// instancia compartida, para que no se repita aunque se dibuje en monitores distintos.
/// </summary>
public sealed class DrawingSettings : IDrawingSettings
{
    private readonly IRandomSource _random;
    private int _thicknessIndex = ThicknessScale.DefaultIndex;
    private ToolKind _selectedTool = ToolKind.Pencil;

    /// <param name="random">Fuente de azar del degradado; por defecto la segura de producción.</param>
    public DrawingSettings(IRandomSource? random = null)
    {
        _random = random ?? SecureRandomSource.Instance;
        NextGradient = GradientPicker.First(_random);
    }

    public TintaColor Color { get; private set; } = TintaColor.Red;

    public bool IsGradient { get; private set; }

    public TintaGradiente NextGradient { get; private set; }

    public double Thickness => ThicknessScale.At(_thicknessIndex);

    public int ThicknessIndex => _thicknessIndex;

    public ToolKind SelectedTool => _selectedTool;

    public event EventHandler? Changed;

    public void SelectColor(TintaColor color)
    {
        if (Color == color && !IsGradient)
        {
            return;
        }

        Color = color;
        IsGradient = false;
        RaiseChanged();
    }

    public void SelectGradient()
    {
        if (IsGradient)
        {
            return;
        }

        IsGradient = true;
        RaiseChanged();
    }

    public void AdvanceGradient()
    {
        NextGradient = GradientPicker.Next(NextGradient, _random);
        RaiseChanged();
    }

    public void IncreaseThickness() => MoveThicknessTo(ThicknessScale.Next(_thicknessIndex));

    public void DecreaseThickness() => MoveThicknessTo(ThicknessScale.Previous(_thicknessIndex));

    public void SelectThickness(int index) =>
        MoveThicknessTo(Math.Clamp(index, ThicknessScale.MinIndex, ThicknessScale.MaxIndex));

    public void SelectTool(ToolKind tool)
    {
        if (_selectedTool == tool)
        {
            return;
        }

        _selectedTool = tool;
        RaiseChanged();
    }

    public void CycleTool() => SelectTool(ToolRing.Next(_selectedTool));

    private void MoveThicknessTo(int newIndex)
    {
        if (newIndex == _thicknessIndex)
        {
            return;
        }

        _thicknessIndex = newIndex;
        RaiseChanged();
    }

    private void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
