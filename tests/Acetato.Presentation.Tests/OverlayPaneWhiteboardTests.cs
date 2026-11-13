using System.Windows.Ink;
using System.Windows.Input;
using Acetato.Application.Abstractions;
using Acetato.Application.Drawing;
using Acetato.Domain;
using Acetato.Presentation.Overlay;
using Acetato.Presentation.ViewModels;
using Acetato.Presentation.Views;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// La pizarra en un monitor (HU-35): Deshacer, el botón de limpiar y borrar actúan solo sobre la hoja
/// visible, y cada hoja conserva lo suyo al pasar de una a otra.
/// </summary>
public sealed class OverlayPaneWhiteboardTests
{
    [Fact]
    public void With_the_whiteboard_on_undo_only_touches_the_board_sheet()
    {
        var counts = WpfTestHost.Run(() =>
        {
            using var pane = new PaneUnderTest();
            PaneUnderTest.AddStroke(pane.Sheets.Screen);
            PaneUnderTest.AddStroke(pane.Sheets.Board);
            pane.Pane.ShowWhiteboard(WhiteboardMode.White);

            pane.Pane.Undo();

            return pane.Counts();
        });

        counts.Should().Be((1, 0));
    }

    [Fact]
    public void With_the_whiteboard_on_clear_only_empties_the_board_sheet()
    {
        var counts = WpfTestHost.Run(() =>
        {
            using var pane = new PaneUnderTest();
            PaneUnderTest.AddStroke(pane.Sheets.Screen);
            PaneUnderTest.AddStroke(pane.Sheets.Board);
            pane.Pane.ShowWhiteboard(WhiteboardMode.Black);

            pane.Pane.Clear();

            return pane.Counts();
        });

        counts.Should().Be((1, 0));
    }

    [Fact]
    public void With_the_whiteboard_off_clear_only_empties_the_screen_sheet()
    {
        var counts = WpfTestHost.Run(() =>
        {
            using var pane = new PaneUnderTest();
            PaneUnderTest.AddStroke(pane.Sheets.Screen);
            PaneUnderTest.AddStroke(pane.Sheets.Board);

            pane.Pane.Clear();

            return pane.Counts();
        });

        counts.Should().Be((0, 1));
    }

    [Fact]
    public void Each_sheet_keeps_its_own_strokes_when_the_whiteboard_goes_on_and_off()
    {
        var counts = WpfTestHost.Run(() =>
        {
            using var pane = new PaneUnderTest();
            PaneUnderTest.AddStroke(pane.Sheets.Screen);
            pane.Pane.ShowWhiteboard(WhiteboardMode.White);
            PaneUnderTest.AddStroke(pane.Sheets.Board);
            PaneUnderTest.AddStroke(pane.Sheets.Board);
            pane.Pane.ShowWhiteboard(WhiteboardMode.Black);
            pane.Pane.ShowWhiteboard(WhiteboardMode.Off);
            pane.Pane.ShowWhiteboard(WhiteboardMode.White);

            return pane.Counts();
        });

        counts.Should().Be((1, 2));
    }

    [Fact]
    public void The_mode_reaches_the_sheets_and_a_repeated_mode_changes_nothing()
    {
        var modes = WpfTestHost.Run(() =>
        {
            using var pane = new PaneUnderTest();
            pane.Pane.ShowWhiteboard(WhiteboardMode.Black);
            pane.Pane.ShowWhiteboard(WhiteboardMode.Black);
            return pane.Sheets.Mode;
        });

        modes.Should().Be(WhiteboardMode.Black);
    }

    // Una pane con sus dos hojas y estilizadores falsos; libera los ViewModels al terminar.
    private sealed class PaneUnderTest : IDisposable
    {
        private readonly OverlayViewModel _screen;
        private readonly OverlayViewModel _board;

        public PaneUnderTest()
        {
            var settings = new DrawingSettings();
            _screen = new OverlayViewModel(settings);
            _board = new OverlayViewModel(settings);
            Sheets = new OverlaySheets(_screen, _board);
            Pane = new OverlayPane(
                new OverlayWindow(),
                Sheets,
                Substitute.For<IOverlayWindowStyler>(),
                Substitute.For<IOverlayWindowPlacer>(),
                new MonitorInfo(0, 0, 1920, 1080, IsPrimary: true));
        }

        public OverlaySheets Sheets { get; }

        public OverlayPane Pane { get; }

        public (int Screen, int Board) Counts() => (_screen.Strokes.Count, _board.Strokes.Count);

        public static void AddStroke(OverlayViewModel sheet) =>
            sheet.Strokes.Add(new Stroke([new StylusPoint(1, 1), new StylusPoint(9, 9)]));

        public void Dispose()
        {
            _screen.Dispose();
            _board.Dispose();
        }
    }
}
