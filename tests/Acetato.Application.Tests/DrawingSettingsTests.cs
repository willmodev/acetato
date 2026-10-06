using Acetato.Application.Drawing;
using Acetato.Domain;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Application.Tests;

public sealed class DrawingSettingsTests
{
    private static DrawingSettings CreateSettings() => new();

    // Fuente de azar de prueba: devuelve los índices dados, en orden (y repite el último).
    private static DrawingSettings CreateSettings(params int[] draws)
    {
        var random = Substitute.For<IRandomSource>();
        random.NextInt(Arg.Any<int>()).Returns(draws[0], draws[1..]);
        return new DrawingSettings(random);
    }

    [Fact]
    public void Default_color_is_red()
    {
        CreateSettings().Color.Should().Be(TintaColor.Red);
    }

    [Fact]
    public void Default_thickness_is_the_minimum_step()
    {
        CreateSettings().Thickness.Should().Be(ThicknessScale.Steps[0]);
    }

    [Fact]
    public void Select_color_updates_the_active_color()
    {
        var settings = CreateSettings();

        settings.SelectColor(TintaColor.Blue);

        settings.Color.Should().Be(TintaColor.Blue);
    }

    [Fact]
    public void Select_color_raises_changed()
    {
        var settings = CreateSettings();
        var raised = 0;
        settings.Changed += (_, _) => raised++;

        settings.SelectColor(TintaColor.Green);

        raised.Should().Be(1);
    }

    [Fact]
    public void Select_same_color_does_not_raise_changed()
    {
        var settings = CreateSettings();
        var raised = 0;
        settings.Changed += (_, _) => raised++;

        settings.SelectColor(TintaColor.Red); // ya es Red por defecto

        raised.Should().Be(0);
    }

    [Fact]
    public void Increase_thickness_advances_to_next_step()
    {
        var settings = CreateSettings();

        settings.IncreaseThickness();

        settings.Thickness.Should().Be(ThicknessScale.Steps[1]);
    }

    [Fact]
    public void Increase_thickness_at_max_keeps_value()
    {
        var settings = CreateSettings();
        for (var i = 0; i < ThicknessScale.Steps.Count + 2; i++)
        {
            settings.IncreaseThickness();
        }

        settings.Thickness.Should().Be(ThicknessScale.Steps[^1]);
    }

    [Fact]
    public void Decrease_thickness_at_min_keeps_value()
    {
        var settings = CreateSettings();

        settings.DecreaseThickness();

        settings.Thickness.Should().Be(ThicknessScale.Steps[0]);
    }

    [Fact]
    public void Decrease_at_min_does_not_raise_changed()
    {
        var settings = CreateSettings();
        var raised = 0;
        settings.Changed += (_, _) => raised++;

        settings.DecreaseThickness(); // ya está en el mínimo

        raised.Should().Be(0);
    }

    [Fact]
    public void Select_thickness_sets_the_step_by_index()
    {
        var settings = CreateSettings();

        settings.SelectThickness(3);

        settings.Thickness.Should().Be(ThicknessScale.Steps[3]);
        settings.ThicknessIndex.Should().Be(3);
    }

    [Fact]
    public void Select_thickness_clamps_an_out_of_range_index()
    {
        var settings = CreateSettings();

        settings.SelectThickness(99);

        settings.ThicknessIndex.Should().Be(ThicknessScale.MaxIndex);
    }

    [Fact]
    public void Select_same_thickness_does_not_raise_changed()
    {
        var settings = CreateSettings();
        var raised = 0;
        settings.Changed += (_, _) => raised++;

        settings.SelectThickness(ThicknessScale.DefaultIndex); // ya está ahí

        raised.Should().Be(0);
    }

    [Fact]
    public void Default_tool_is_pencil()
    {
        CreateSettings().SelectedTool.Should().Be(ToolKind.Pencil);
    }

    [Fact]
    public void Select_tool_updates_and_raises_changed()
    {
        var settings = CreateSettings();
        var raised = 0;
        settings.Changed += (_, _) => raised++;

        settings.SelectTool(ToolKind.Rectangle);

        settings.SelectedTool.Should().Be(ToolKind.Rectangle);
        raised.Should().Be(1);
    }

    [Fact]
    public void Select_same_tool_does_not_raise_changed()
    {
        var settings = CreateSettings();
        var raised = 0;
        settings.Changed += (_, _) => raised++;

        settings.SelectTool(ToolKind.Pencil); // ya es Pencil por defecto

        raised.Should().Be(0);
    }

    [Fact]
    public void Cycle_tool_advances_through_the_ring()
    {
        var settings = CreateSettings(); // Pencil

        settings.CycleTool();

        settings.SelectedTool.Should().Be(ToolKind.Laser);
    }

    [Fact]
    public void Cycle_tool_wraps_around()
    {
        var settings = CreateSettings();
        settings.SelectTool(ToolKind.Eraser); // última del anillo

        settings.CycleTool();

        settings.SelectedTool.Should().Be(ToolKind.Pencil);
    }

    [Fact]
    public void Gradient_mode_is_off_by_default()
    {
        var settings = CreateSettings();

        settings.IsGradient.Should().BeFalse();
        Enum.IsDefined(settings.NextGradient).Should().BeTrue();
    }

    [Fact]
    public void First_gradient_pair_comes_from_the_random_source()
    {
        CreateSettings(3).NextGradient.Should().Be(TintaGradiente.VioletPink);
    }

    [Fact]
    public void Select_gradient_enters_the_mode_and_raises_changed()
    {
        var settings = CreateSettings();
        var raised = 0;
        settings.Changed += (_, _) => raised++;

        settings.SelectGradient();

        settings.IsGradient.Should().BeTrue();
        raised.Should().Be(1);
    }

    [Fact]
    public void Select_gradient_when_already_active_does_not_raise_changed()
    {
        var settings = CreateSettings();
        settings.SelectGradient();
        var raised = 0;
        settings.Changed += (_, _) => raised++;

        settings.SelectGradient();

        raised.Should().Be(0);
    }

    [Fact]
    public void Select_gradient_keeps_the_last_solid_color()
    {
        var settings = CreateSettings();
        settings.SelectColor(TintaColor.Blue);

        settings.SelectGradient();

        settings.Color.Should().Be(TintaColor.Blue);
    }

    [Fact]
    public void Select_color_leaves_the_gradient_mode()
    {
        var settings = CreateSettings();
        settings.SelectGradient();

        settings.SelectColor(TintaColor.Green);

        settings.IsGradient.Should().BeFalse();
        settings.Color.Should().Be(TintaColor.Green);
    }

    [Fact]
    public void Select_the_same_solid_color_while_in_gradient_mode_leaves_the_mode_and_raises_changed()
    {
        var settings = CreateSettings();
        settings.SelectGradient(); // el color sólido sigue siendo Red
        var raised = 0;
        settings.Changed += (_, _) => raised++;

        settings.SelectColor(TintaColor.Red);

        settings.IsGradient.Should().BeFalse();
        raised.Should().Be(1);
    }

    [Fact]
    public void Advance_gradient_raises_changed()
    {
        var settings = CreateSettings();
        var raised = 0;
        settings.Changed += (_, _) => raised++;

        settings.AdvanceGradient();

        raised.Should().Be(1);
    }

    [Fact]
    public void Advance_gradient_never_repeats_the_previous_pair()
    {
        var settings = CreateSettings(3, 3, 0, 4, 2);
        var previous = settings.NextGradient;

        for (var i = 0; i < 20; i++)
        {
            settings.AdvanceGradient();

            settings.NextGradient.Should().NotBe(previous);
            previous = settings.NextGradient;
        }
    }

    [Fact]
    public void Advance_gradient_never_repeats_with_the_production_source()
    {
        var settings = CreateSettings();
        var previous = settings.NextGradient;

        for (var i = 0; i < 200; i++)
        {
            settings.AdvanceGradient();

            settings.NextGradient.Should().NotBe(previous);
            previous = settings.NextGradient;
        }
    }
}
