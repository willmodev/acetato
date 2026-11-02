using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Application.Overlay;
using Acetato.Application.Session;
using Acetato.Domain;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Application.Tests;

/// <summary>
/// Arrancar como se cerró (HU-29): aplicar la última sesión y guardar solo lo que cambia
/// de verdad; el sitio de la barra, al salir.
/// </summary>
public sealed class SessionMemoryTests : IDisposable
{
    private static readonly ToolbarPlacement Placement = new(new MonitorInfo(1920, 0, 2560, 1440, IsPrimary: false), 2500, 80);

    private readonly DrawingSettings _drawing = new(Substitute.For<IRandomSource>());
    private readonly ToolbarLayout _layout = new();
    private readonly ILastSessionStore _store = Substitute.For<ILastSessionStore>();
    private readonly SessionMemory _memory;

    public SessionMemoryTests()
    {
        _store.Load().Returns(LastSession.Defaults);
        _memory = new SessionMemory(_drawing, _layout, _store);
    }

    public void Dispose() => _memory.Dispose();

    [Fact]
    public void Start_applies_the_last_session()
    {
        _store.Load().Returns(new LastSession(TintaColor.Blue, Gradient: true, 4, ToolKind.Arrow, ToolbarCollapsed: true, Placement));

        _memory.Start();

        _drawing.Color.Should().Be(TintaColor.Blue);
        _drawing.IsGradient.Should().BeTrue();
        _drawing.ThicknessIndex.Should().Be(4);
        _drawing.SelectedTool.Should().Be(ToolKind.Arrow);
        _layout.IsCollapsed.Should().BeTrue();
        _layout.Placement.Should().Be(Placement);
    }

    [Fact]
    public void Starting_does_not_write_the_file()
    {
        _memory.Start();

        _store.DidNotReceive().Save(Arg.Any<LastSession>());
    }

    [Fact]
    public void Choosing_an_ink_saves_it()
    {
        _memory.Start();

        _drawing.SelectColor(TintaColor.Green);

        _store.Received(1).Save(LastSession.Defaults with { Ink = TintaColor.Green });
    }

    [Fact]
    public void Thickness_tool_and_collapse_are_saved_when_they_change()
    {
        _memory.Start();

        _drawing.SelectThickness(2);
        _drawing.SelectTool(ToolKind.Ellipse);
        _layout.ToggleCollapsed();

        _store.Received(1).Save(new LastSession(TintaColor.Red, false, 2, ToolKind.Ellipse, ToolbarCollapsed: true, null));
    }

    [Fact]
    public void Drawing_another_neon_stroke_does_not_write_the_file()
    {
        _memory.Start();
        _drawing.SelectGradient();
        _store.ClearReceivedCalls();

        _drawing.AdvanceGradient();

        _store.DidNotReceive().Save(Arg.Any<LastSession>());
    }

    [Fact]
    public void Dragging_the_bar_is_saved_on_exit_not_on_each_step()
    {
        _memory.Start();

        _layout.Remember(Placement);
        _store.DidNotReceive().Save(Arg.Any<LastSession>());
        _memory.Save();

        _store.Received(1).Save(LastSession.Defaults with { ToolbarPlacement = Placement });
    }

    [Fact]
    public void Saving_without_starting_does_nothing()
    {
        _memory.Save();

        _store.DidNotReceive().Save(Arg.Any<LastSession>());
    }
}
