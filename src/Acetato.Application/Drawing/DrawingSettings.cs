using Acetato.Domain;

namespace Acetato.Application.Drawing;

/// <summary>
/// Implementación del estado de dibujo. El grosor se mueve por la
/// <see cref="ThicknessScale"/> con tope en ambos extremos. Solo dispara
/// <see cref="Changed"/> cuando el valor cambia de verdad (evita refrescos
/// inútiles al pulsar en el tope).
/// </summary>
public sealed class DrawingSettings : IDrawingSettings
{
    private int _thicknessIndex = ThicknessScale.DefaultIndex;

    public TintaColor Color { get; private set; } = TintaColor.Red;

    public double Thickness => ThicknessScale.At(_thicknessIndex);

    public int ThicknessIndex => _thicknessIndex;

    public event EventHandler? Changed;

    public void SelectColor(TintaColor color)
    {
        if (Color == color)
        {
            return;
        }

        Color = color;
        RaiseChanged();
    }

    public void IncreaseThickness() => MoveThicknessTo(ThicknessScale.Next(_thicknessIndex));

    public void DecreaseThickness() => MoveThicknessTo(ThicknessScale.Previous(_thicknessIndex));

    public void SelectThickness(int index) =>
        MoveThicknessTo(Math.Clamp(index, ThicknessScale.MinIndex, ThicknessScale.MaxIndex));

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
