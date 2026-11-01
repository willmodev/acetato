using System.Windows;
using System.Windows.Media;
using Acetato.Domain;
using Acetato.Presentation.Resources;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// Construye las muestras del popover de tintas: las 6 tintas sólidas y, a continuación,
/// la del modo Degradado (HU-19). Da también el relleno de cada tinta, plano o con su par
/// de Tintas degradadas (HU-27). Mantiene fuera del ViewModel de la barra la resolución
/// de colores desde los tokens.
/// </summary>
internal static class InkSwatchFactory
{
    private static readonly TintaColor[] SolidColors =
    [
        TintaColor.Red, TintaColor.Blue, TintaColor.Yellow,
        TintaColor.Green, TintaColor.White, TintaColor.Black,
    ];

    // Rellenos de cada tinta, planos o con su par: congelados y creados una sola vez.
    private static readonly Dictionary<(TintaColor Color, bool InkPairs), Brush> Fills = [];

    private static Brush? _gradientSample;

    /// <summary>
    /// Pincel de muestra del modo Degradado: un barrido diagonal con colores de varios
    /// pares neón. Representa el modo (no un par concreto) en la muestra y en el botón Color.
    /// </summary>
    public static Brush GradientSample => _gradientSample ??= BuildGradientSample();

    /// <summary>
    /// Relleno de la muestra de <paramref name="color"/>: la tinta plana o, con Tintas
    /// degradadas, su par en diagonal (135°, inicio arriba a la izquierda), como el botón
    /// Color y el cursor (HU-27).
    /// </summary>
    public static Brush FillFor(TintaColor color, bool inkPairs)
    {
        if (!Fills.TryGetValue((color, inkPairs), out var fill))
        {
            fill = inkPairs ? BuildPairFill(color) : new SolidColorBrush(TintaColorMap.Resolve(color));
            fill.Freeze();
            Fills[(color, inkPairs)] = fill;
        }

        return fill;
    }

    /// <summary>Las 6 tintas sólidas y, al final, la muestra de degradado.</summary>
    public static List<InkSwatchItem> Create()
    {
        var items = new List<InkSwatchItem>(SolidColors.Length + 1);
        foreach (var color in SolidColors)
        {
            items.Add(new InkSwatchItem(color, FillFor(color, inkPairs: false)));
        }

        items.Add(InkSwatchItem.CreateGradient(GradientSample));
        return items;
    }

    private static LinearGradientBrush BuildPairFill(TintaColor color)
    {
        var (start, end) = GradientPairMap.Resolve(GradientPair.FromInk(color));
        return new LinearGradientBrush(start, end, new Point(0d, 0d), new Point(1d, 1d));
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
