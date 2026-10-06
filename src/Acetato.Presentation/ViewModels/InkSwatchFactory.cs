using System.Windows;
using System.Windows.Media;
using Acetato.Domain;
using Acetato.Presentation.Resources;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// Construye las muestras del popover de tintas: las 6 tintas sólidas y, a continuación,
/// la del modo Degradado (HU-19). Mantiene fuera del ViewModel de la barra la resolución
/// de colores desde los tokens.
/// </summary>
internal static class InkSwatchFactory
{
    private static readonly TintaColor[] SolidColors =
    [
        TintaColor.Red, TintaColor.Blue, TintaColor.Yellow,
        TintaColor.Green, TintaColor.White, TintaColor.Black,
    ];

    private static Brush? _gradientSample;

    /// <summary>
    /// Pincel de muestra del modo Degradado: un barrido diagonal con colores de varios
    /// pares neón. Representa el modo (no un par concreto) en la muestra y en el botón Color.
    /// </summary>
    public static Brush GradientSample => _gradientSample ??= BuildGradientSample();

    /// <summary>Las 6 tintas sólidas y, al final, la muestra de degradado.</summary>
    public static List<InkSwatchItem> Create()
    {
        var items = new List<InkSwatchItem>(SolidColors.Length + 1);
        foreach (var color in SolidColors)
        {
            items.Add(new InkSwatchItem(color, new SolidColorBrush(TintaColorMap.Resolve(color))));
        }

        items.Add(InkSwatchItem.CreateGradient(GradientSample));
        return items;
    }

    private static LinearGradientBrush BuildGradientSample()
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0d, 0d),
            EndPoint = new Point(1d, 1d),
        };
        brush.GradientStops.Add(new GradientStop(TintaGradienteMap.Resolve(TintaGradiente.BlueCyan).Start, 0d));
        brush.GradientStops.Add(new GradientStop(TintaGradienteMap.Resolve(TintaGradiente.BlueCyan).End, 0.35d));
        brush.GradientStops.Add(new GradientStop(TintaGradienteMap.Resolve(TintaGradiente.YellowLime).End, 0.7d));
        brush.GradientStops.Add(new GradientStop(TintaGradienteMap.Resolve(TintaGradiente.PinkRed).Start, 1d));
        brush.Freeze();
        return brush;
    }
}
