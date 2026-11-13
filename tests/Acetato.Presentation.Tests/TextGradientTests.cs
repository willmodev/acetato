using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Acetato.Application.Drawing;
using Acetato.Domain;
using Acetato.Presentation.Behaviors;
using Acetato.Presentation.Resources;
using Acetato.Presentation.ViewModels;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Texto con degradado (spec 15): con el modo Degradado o con Tintas degradadas, el texto toma el
/// par del próximo trazo en diagonal sobre su recuadro; sin degradado sigue en tinta sólida. Al
/// fijar un texto con degradado se sortea el par siguiente; uno vacío no consume nada.
/// </summary>
public sealed class TextGradientTests
{
    private static readonly GradientPair Neon = GradientPair.FromNeon(TintaGradiente.BlueCyan);

    [Fact]
    public void The_brush_runs_diagonally_from_the_start_to_the_end_color()
    {
        var brush = WpfTestHost.Run(() => DiagonalGradientBrush.Create(Neon));
        var (start, end) = WpfTestHost.Run(() => GradientPairMap.Resolve(Neon));

        brush.IsFrozen.Should().BeTrue();
        brush.StartPoint.Should().Be(new Point(0, 0));
        brush.EndPoint.Should().Be(new Point(1, 1));
        brush.GradientStops.Select(stop => stop.Color).Should().Equal(start, end);
    }

    [Fact]
    public void Text_typed_with_a_pair_is_painted_with_that_gradient()
    {
        var foreground = WpfTestHost.Run(() => Type(Neon, "Hola").LabelForeground);

        foreground.Should().BeOfType<LinearGradientBrush>();
    }

    [Fact]
    public void The_editor_already_shows_the_gradient_while_typing()
    {
        var foreground = WpfTestHost.Run(() => Type(Neon, text: null).EditorForeground);

        foreground.Should().BeOfType<LinearGradientBrush>();
    }

    [Fact]
    public void With_ink_pairs_the_text_takes_the_pair_of_its_ink()
    {
        var stops = WpfTestHost.Run(() =>
        {
            var pair = GradientPair.FromInk(TintaColor.Red);
            var brush = (LinearGradientBrush)Type(pair, "Hola").LabelForeground;
            return (Stops: brush.GradientStops.Select(stop => stop.Color).ToList(), Expected: GradientPairMap.Resolve(pair));
        });

        stops.Stops.Should().Equal(stops.Expected.Start, stops.Expected.End);
    }

    [Fact]
    public void Fixing_a_text_with_gradient_draws_the_next_pair_once()
    {
        var used = WpfTestHost.Run(() => Type(Neon, "Hola").UsedCount);

        used.Should().Be(1);
    }

    [Fact]
    public void An_empty_text_is_discarded_and_does_not_consume_the_pair()
    {
        var typed = WpfTestHost.Run(() =>
        {
            var result = Type(Neon, text: string.Empty);
            return (result.UsedCount, result.LabelCount);
        });

        typed.Should().Be((0, 0));
    }

    [Fact]
    public void Without_gradient_the_text_keeps_the_solid_ink_and_consumes_nothing()
    {
        var result = WpfTestHost.Run(() =>
        {
            var typed = Type(pair: null, "Hola");
            return (typed.LabelForeground, typed.UsedCount);
        });

        result.LabelForeground.Should().BeOfType<SolidColorBrush>();
        result.UsedCount.Should().Be(0);
    }

    // Escribe un texto en un lienzo con la herramienta Texto: clic, escribir, fijar. Con text null
    // lo deja en edición.
    private static Typed Type(GradientPair? pair, string? text)
    {
        using var viewModel = new OverlayViewModel(new DrawingSettings());
        var canvas = new InkCanvas { DataContext = viewModel };
        int used = 0;
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object?>()).Returns(true);
        command.When(c => c.Execute(Arg.Any<object?>())).Do(_ => used++);
        InkGradient.SetPair(canvas, pair);
        InkGradient.SetUsedCommand(canvas, command);
        TextAnnotationBehavior.SetTool(canvas, ToolKind.Text);
        canvas.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
        {
            RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent,
            Source = canvas,
        });
        var editor = canvas.Children.OfType<TextBox>().Single();
        if (text is null)
        {
            return new Typed(editor.Foreground, null, used, 0);
        }

        editor.Text = text;
        TextAnnotationBehavior.TryCommit(canvas);
        var labels = canvas.Children.OfType<TextBlock>().Where(label => label.Text.Length > 0).ToList();
        return new Typed(editor.Foreground, labels.FirstOrDefault()?.Foreground, used, labels.Count);
    }

    private sealed record Typed(Brush EditorForeground, Brush? LabelForegroundOrNull, int UsedCount, int LabelCount)
    {
        public Brush LabelForeground => LabelForegroundOrNull!;
    }
}
