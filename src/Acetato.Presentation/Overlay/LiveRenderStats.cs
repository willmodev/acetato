using System.Diagnostics;
using System.Globalization;

namespace Acetato.Presentation.Overlay;

/// <summary>
/// Medición del pintado en vivo del trazo libre (HU-20), solo en compilación Debug:
/// acumula el tiempo de cada repintado y, al soltar, escribe en la salida de depuración
/// el típico (p50), el peor habitual (p95) y el máximo. En Release los métodos
/// desaparecen (<see cref="ConditionalAttribute"/>): cero costo.
/// </summary>
internal sealed class LiveRenderStats
{
    private readonly List<double> _samplesMs = [];

    [Conditional("DEBUG")]
    public void Record(TimeSpan elapsed) => _samplesMs.Add(elapsed.TotalMilliseconds);

    /// <summary>Escribe el resumen del trazo terminado y vacía las muestras.</summary>
    [Conditional("DEBUG")]
    public void Report(int pointCount)
    {
        if (_samplesMs.Count == 0)
        {
            return;
        }

        _samplesMs.Sort();
        Debug.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"Acetato.LiveStroke: repintados={_samplesMs.Count} p50={Percentile(0.5):F2}ms p95={Percentile(0.95):F2}ms max={_samplesMs[^1]:F2}ms puntos={pointCount}"));
        _samplesMs.Clear();
    }

    // Percentil por rango más cercano sobre las muestras ya ordenadas.
    private double Percentile(double fraction)
    {
        int index = (int)Math.Ceiling(fraction * _samplesMs.Count) - 1;
        return _samplesMs[Math.Clamp(index, 0, _samplesMs.Count - 1)];
    }
}
