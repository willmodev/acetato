using System.Windows.Media;
using Acetato.Domain;

namespace Acetato.Presentation.ViewModels;

/// <summary>
/// Lo que pinta el cursor de tinta (HU-26): la tinta sólida activa, el par del
/// próximo trazo en degradado, neón o de tinta (<c>null</c> = tinta plana) y el grosor en DIP.
/// Es un valor: dos puntas iguales comparten el mismo cursor en caché.
/// </summary>
/// <param name="Solid">Última tinta sólida elegida.</param>
/// <param name="Gradient">Par del próximo trazo; <c>null</c> = tinta sólida.</param>
/// <param name="Thickness">Grosor activo, en DIP.</param>
public sealed record InkCursorTip(Color Solid, GradientPair? Gradient, double Thickness, bool Highlighter = false);
