using System.Runtime.InteropServices;
using System.Windows.Ink;

namespace Acetato.Presentation.Overlay;

/// <summary>Trazo borrado y el índice que ocupaba en su colección (SPEC 11).</summary>
[StructLayout(LayoutKind.Auto)]
internal readonly record struct ErasedStroke(Stroke Stroke, int Index);
