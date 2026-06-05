using Acetato.Application.Drawing;
using Acetato.Domain;
using FluentAssertions;
using Xunit;

namespace Acetato.Application.Tests;

public sealed class DrawingSettingsTests
{
    private static DrawingSettings CreateSettings() => new();

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
}
