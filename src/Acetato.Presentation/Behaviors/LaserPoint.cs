using System.Runtime.InteropServices;
using System.Windows;

namespace Acetato.Presentation.Behaviors;

/// <summary>
/// Un punto de la estela del láser: su posición en DIP del InkCanvas, la marca
/// de tiempo (ticks de <see cref="System.Diagnostics.Stopwatch"/>) en que se creó y
/// si es el primer punto de un trazo nuevo (no debe conectarse con el anterior).
/// La edad derivada alimenta a <see cref="Acetato.Domain.LaserFade"/> para el
/// desvanecido. Efímero: no se persiste ni entra al historial.
/// </summary>
[StructLayout(LayoutKind.Auto)]
internal readonly record struct LaserPoint(Point Position, long CreatedAtTicks, bool StartsStroke);
