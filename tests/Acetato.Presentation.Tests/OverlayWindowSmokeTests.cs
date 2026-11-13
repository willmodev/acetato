using System.Windows;
using System.Windows.Markup;
using Acetato.Application.Drawing;
using Acetato.Domain;
using Acetato.Presentation.Behaviors;
using Acetato.Presentation.ViewModels;
using Acetato.Presentation.Views;
using FluentAssertions;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Humo de la ventana del overlay con dos hojas (HU-35): carga con los recursos reales, cada
/// hoja toma su propio ViewModel (trazos independientes) y las dos reciben la herramienta
/// activa y las capas del trazo en vivo y del láser. No abre nada en pantalla.
/// </summary>
public sealed class OverlayWindowSmokeTests
{
    [Fact]
    public void Each_sheet_takes_its_own_view_model_and_strokes()
    {
        var result = WpfTestHost.Run(() =>
        {
            using var loaded = new Loaded();
            var (window, sheets) = (loaded.Window, loaded.Sheets);
            return (
                ScreenOwn: ReferenceEquals(window.InkSurface.DataContext, sheets.Screen),
                BoardOwn: ReferenceEquals(window.BoardSurface.DataContext, sheets.Board),
                ScreenStrokes: ReferenceEquals(window.InkSurface.Strokes, sheets.Screen.Strokes),
                BoardStrokes: ReferenceEquals(window.BoardSurface.Strokes, sheets.Board.Strokes));
        });

        result.Should().Be((true, true, true, true));
    }

    [Fact]
    public void Drawing_on_the_board_does_not_touch_the_strokes_of_the_screen()
    {
        var counts = WpfTestHost.Run(() =>
        {
            using var loaded = new Loaded();
            var window = loaded.Window;
            window.BoardSurface.Strokes.Add(new System.Windows.Ink.Stroke(
                [new System.Windows.Input.StylusPoint(1, 1), new System.Windows.Input.StylusPoint(9, 9)]));
            return (Screen: window.InkSurface.Strokes.Count, Board: window.BoardSurface.Strokes.Count);
        });

        counts.Should().Be((0, 1));
    }

    [Fact]
    public void Without_the_whiteboard_the_screen_sheet_shows_and_the_board_is_hidden()
    {
        var visibility = WpfTestHost.Run(() =>
        {
            using var loaded = new Loaded();
            var window = loaded.Window;
            return (Screen: window.InkSurface.Visibility, Board: window.BoardSurface.Visibility);
        });

        visibility.Should().Be((Visibility.Visible, Visibility.Collapsed));
    }

    [Fact]
    public void Both_sheets_follow_the_active_tool_and_share_the_live_layers()
    {
        var result = WpfTestHost.Run(() =>
        {
            using var loaded = new Loaded();
            var window = loaded.Window;
            loaded.Settings.SelectTool(ToolKind.Highlighter);
            return (
                ScreenTool: FreehandStrokeBehavior.GetTool(window.InkSurface),
                BoardTool: FreehandStrokeBehavior.GetTool(window.BoardSurface),
                ScreenLayer: FreehandStrokeBehavior.GetLayer(window.InkSurface) is not null,
                BoardLayer: FreehandStrokeBehavior.GetLayer(window.BoardSurface) is not null,
                SameLayer: ReferenceEquals(
                    FreehandStrokeBehavior.GetLayer(window.InkSurface),
                    FreehandStrokeBehavior.GetLayer(window.BoardSurface)));
        });

        result.Should().Be((ToolKind.Highlighter, ToolKind.Highlighter, true, true, true));
    }

    // La ventana con sus dos hojas; al terminar libera los ViewModels (se suscriben a los ajustes).
    private sealed class Loaded : IDisposable
    {
        private readonly OverlayViewModel _screen;
        private readonly OverlayViewModel _board;

        public Loaded()
        {
            Settings = new DrawingSettings();
            _screen = new OverlayViewModel(Settings);
            _board = new OverlayViewModel(Settings);
            Sheets = new OverlaySheets(_screen, _board);
            Window = new OverlayWindow();
            // Sin mostrar la ventana: se suelta su contenido, que conserva los nombres de la ventana
            // (los enlaces por nombre de las capas en vivo), y se le da el contexto, como en los demás humos.
            var content = (FrameworkElement)Window.Content;
            Window.Content = null;
            NameScope.SetNameScope(content, NameScope.GetNameScope(Window));
            content.DataContext = Sheets;
            content.Measure(new Size(400, 300));
            content.Arrange(new Rect(0, 0, 400, 300));
            content.UpdateLayout();
        }

        public DrawingSettings Settings { get; }

        public OverlaySheets Sheets { get; }

        public OverlayWindow Window { get; }

        public void Dispose()
        {
            _screen.Dispose();
            _board.Dispose();
        }
    }
}
