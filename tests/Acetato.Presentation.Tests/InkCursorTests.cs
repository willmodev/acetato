using System.Windows.Input;
using System.Windows.Media;
using Acetato.Application.Drawing;
using Acetato.Domain;
using Acetato.Presentation.Behaviors;
using Acetato.Presentation.Resources;
using Acetato.Presentation.ViewModels;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Cursor de tinta (HU-26): el círculo se pinta con la tinta y los aros, se
/// empaqueta como cursor de Windows, se reutiliza por combinación y solo
/// reemplaza a la cruz de las herramientas que dibujan.
/// </summary>
public sealed class InkCursorTests
{
    private static readonly Color Red = Color.FromRgb(0xFF, 0x3B, 0x30);
    private static readonly Color Light = Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF);
    private static readonly Color Dark = Color.FromArgb(0x99, 0x00, 0x00, 0x00);

    [Fact]
    public void The_center_has_the_ink_and_the_corners_are_transparent()
    {
        const int Diameter = 12;
        var pixels = WpfTestHost.Run(() => InkCursorRenderer.Render(new SolidColorBrush(Red), Diameter, Light, Dark));
        int size = InkCursorRenderer.SizeFor(Diameter);

        Pixel(pixels, size, size / 2, size / 2).Should().Equal(Red.B, Red.G, Red.R, (byte)0xFF);
        Pixel(pixels, size, 0, 0)[3].Should().Be(0);
    }

    [Fact]
    public void A_dark_ring_surrounds_the_ink()
    {
        const int Diameter = 12;
        var pixels = WpfTestHost.Run(() => InkCursorRenderer.Render(new SolidColorBrush(Red), Diameter, Light, Dark));
        int size = InkCursorRenderer.SizeFor(Diameter);

        // Justo fuera del relleno, sobre el eje horizontal: aro oscuro, opaco en parte.
        var ring = Pixel(pixels, size, (size / 2) + (Diameter / 2), size / 2);
        ring[2].Should().BeLessThan(0x40);
        ring[3].Should().BeGreaterThan(0);
    }

    [Fact]
    public void In_gradient_mode_the_circle_shows_the_pair_of_the_next_stroke()
    {
        var (solid, gradient, otherPair, start, end) = WpfTestHost.Run(() =>
        {
            var tip = new InkCursorTip(Red, TintaGradiente.BlueCyan, 18d);
            var colors = TintaGradienteMap.Resolve(TintaGradiente.BlueCyan);
            return (InkCursorFactory.For(tip with { Gradient = null }, 1d), InkCursorFactory.For(tip, 1d),
                InkCursorFactory.For(tip with { Gradient = TintaGradiente.PinkRed }, 1d), colors.Start, colors.End);
        });

        gradient.Should().NotBeSameAs(solid);
        otherPair.Should().NotBeSameAs(gradient);

        // Arriba a la izquierda domina el inicio del par y abajo a la derecha, el fin.
        const int Diameter = 18;
        var pixels = WpfTestHost.Run(() => InkCursorRenderer.Render(
            InkCursorFactory.FillOf(new InkCursorTip(Red, TintaGradiente.BlueCyan, Diameter)), Diameter, Light, Dark));
        int size = InkCursorRenderer.SizeFor(Diameter);
        var topLeft = Pixel(pixels, size, (size / 2) - 4, (size / 2) - 4);
        var bottomRight = Pixel(pixels, size, (size / 2) + 4, (size / 2) + 4);
        Distance(topLeft, start).Should().BeLessThan(Distance(topLeft, end));
        Distance(bottomRight, end).Should().BeLessThan(Distance(bottomRight, start));
    }

    [Fact]
    public void After_each_gradient_stroke_the_tip_moves_to_the_next_pair()
    {
        var (before, after, notified, thicknessTip) = WpfTestHost.Run(() =>
        {
            var settings = new DrawingSettings();
            using var overlay = new OverlayViewModel(settings);
            settings.SelectGradient();
            var first = overlay.CursorTip;
            var changes = 0;
            overlay.PropertyChanged += (_, e) => changes += string.Equals(e.PropertyName, nameof(OverlayViewModel.CursorTip), StringComparison.Ordinal) ? 1 : 0;
            overlay.GradientUsedCommand.Execute(null);
            var second = overlay.CursorTip;
            settings.IncreaseThickness();
            return (first, second, changes, overlay.CursorTip);
        });

        before.Gradient.Should().NotBeNull();
        after.Gradient.Should().NotBeNull().And.NotBe(before.Gradient);
        notified.Should().BeGreaterThan(0);
        thicknessTip.Thickness.Should().Be(ThicknessScale.Steps[1]);
    }

    [Fact]
    public void The_cursor_file_has_one_image_with_the_hotspot_in_the_center()
    {
        const int Size = 16;
        var bytes = CursorFile.Write(new byte[Size * Size * 4], Size, Size / 2);

        BitConverter.ToUInt16(bytes, 2).Should().Be(2);
        BitConverter.ToUInt16(bytes, 4).Should().Be(1);
        bytes[6].Should().Be(Size);
        BitConverter.ToUInt16(bytes, 10).Should().Be(Size / 2);
        BitConverter.ToUInt16(bytes, 12).Should().Be(Size / 2);
        bytes.Should().HaveCount(6 + 16 + 40 + (Size * Size * 4) + (4 * Size));
    }

    [Fact]
    public void The_same_tip_and_scale_reuse_the_same_cursor()
    {
        var (first, second, thicker, scaled) = WpfTestHost.Run(() =>
        {
            var tip = new InkCursorTip(Red, null, 8d);
            return (InkCursorFactory.For(tip, 1d), InkCursorFactory.For(tip with { }, 1d),
                InkCursorFactory.For(tip with { Thickness = 12d }, 1d), InkCursorFactory.For(tip, 1.5d));
        });

        second.Should().BeSameAs(first);
        thicker.Should().NotBeSameAs(first);
        scaled.Should().NotBeSameAs(first);
    }

    [Theory]
    [InlineData(ToolKind.Pencil)]
    [InlineData(ToolKind.Line)]
    [InlineData(ToolKind.Arrow)]
    [InlineData(ToolKind.Rectangle)]
    public void Drawing_tools_use_the_ink_circle_instead_of_the_cross(ToolKind tool)
    {
        var (cursor, expected) = WpfTestHost.Run(() =>
        {
            var tip = new InkCursorTip(Red, null, 5d);
            return (ToolCursorBehavior.CursorFor(tool, tip, 1d), InkCursorFactory.For(tip, 1d));
        });

        cursor.Should().BeSameAs(expected);
        cursor.Should().NotBeSameAs(Cursors.Cross);
    }

    [Fact]
    public void Text_laser_and_eraser_keep_their_cursor()
    {
        var (text, laser, eraser) = WpfTestHost.Run(() =>
        {
            var tip = new InkCursorTip(Red, null, 5d);
            return (ToolCursorBehavior.CursorFor(ToolKind.Text, tip, 1d),
                ToolCursorBehavior.CursorFor(ToolKind.Laser, tip, 1d),
                ToolCursorBehavior.CursorFor(ToolKind.Eraser, tip, 1d));
        });

        text.Should().BeSameAs(Cursors.IBeam);
        laser.Should().BeSameAs(Cursors.None);
        eraser.Should().BeNull();
    }

    private static int Distance(byte[] bgra, Color color) =>
        Math.Abs(bgra[0] - color.B) + Math.Abs(bgra[1] - color.G) + Math.Abs(bgra[2] - color.R);

    private static byte[] Pixel(byte[] pixels, int size, int x, int y) =>
        pixels.AsSpan(((y * size) + x) * 4, 4).ToArray();
}
