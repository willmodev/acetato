using System.Windows;
using Acetato.Presentation.Behaviors;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Lectura del punto de pantalla que trae WM_NCHITTEST: con un monitor a la
/// izquierda del principal las coordenadas son negativas y deben conservar el signo.
/// </summary>
public sealed class HitAreaBehaviorTests
{
    [Theory]
    [InlineData(640, 360)]
    [InlineData(-1200, 64)]
    [InlineData(-5, -30)]
    public void Screen_point_keeps_the_sign_of_each_coordinate(int x, int y)
    {
        nint lParam = (y << 16) | (x & 0xFFFF);

        HitAreaBehavior.ScreenPoint(lParam).Should().Be(new Point(x, y));
    }
}
