using System.IO;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Acetato.Application.Abstractions;
using Acetato.Application.Hotkeys;
using Acetato.Domain;
using Acetato.Presentation.ViewModels;
using Acetato.Presentation.Views;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Humo de la ventana Ajustes (HU-21): carga el XAML con los recursos reales de la
/// app y hace el layout SIN mostrarla. Detecta claves de recurso, estilos o
/// plantillas rotas que el build no ve. No abre nada en pantalla.
/// </summary>
public sealed class SettingsWindowSmokeTests
{
    private static readonly string[] Dictionaries =
    [
        "Resources/Tokens.xaml",
        "Resources/Icons.xaml",
        "Resources/ToolbarStyles.xaml",
        "Resources/SettingsStyles.xaml",
    ];

    [Fact]
    public void Settings_window_loads_and_lays_out_the_fifteen_rows()
    {
        int texts = RunOnSta(LoadAndCountTexts);

        // 15 nombres + 15 combinaciones como mínimo.
        texts.Should().BeGreaterThanOrEqualTo(30);
    }

    private static int LoadAndCountTexts()
    {
        EnsureApplicationResources();
        using var coordinator = CreateCoordinator();
        using var settings = new SettingsViewModel(coordinator, Substitute.For<IWindowDragHandler>());
        var window = new SettingsWindow();

        // Una ventana sin mostrar no hace layout: se mide su contenido suelto,
        // con el mismo DataContext y los recursos de la app.
        var content = (FrameworkElement)window.Content;
        window.Content = null;
        content.DataContext = settings;
        content.Measure(new Size(480, 600));
        content.Arrange(new Rect(0, 0, 480, 600));
        content.UpdateLayout();
        return CountTextBlocks(content);
    }

    private static HotkeyCoordinator CreateCoordinator()
    {
        var hotkeys = Substitute.For<IGlobalHotkeyService>();
        hotkeys.Apply(Arg.Any<IReadOnlyList<HotkeyAssignment>>()).Returns([]);
        hotkeys.IsAvailable(Arg.Any<KeyChord>()).Returns(true);
        var store = Substitute.For<IHotkeySettingsStore>();
        store.Load().Returns(new HotkeySettingsLoad(HotkeyMap.Defaults, WasCorrupt: false));
        var coordinator = new HotkeyCoordinator(hotkeys, Substitute.For<IOverlayController>(), store, Substitute.For<IHotkeyNotifier>());
        coordinator.Start();
        return coordinator;
    }

    // Recursos de App.xaml sin crear App (que arrancaría la app entera).
    private static void EnsureApplicationResources()
    {
        if (System.Windows.Application.Current is not null)
        {
            return;
        }

        var application = new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        foreach (string path in Dictionaries)
        {
            application.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Acetato;component/{path}", UriKind.Absolute),
            });
        }
    }

    private static int CountTextBlocks(DependencyObject root)
    {
        int count = root is System.Windows.Controls.TextBlock ? 1 : 0;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            count += CountTextBlocks(VisualTreeHelper.GetChild(root, i));
        }

        return count;
    }

    private static T RunOnSta<T>(Func<T> work)
    {
        T result = default!;
        ExceptionDispatchInfo? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                result = work();
            }
            catch (Exception ex) when (ex is XamlParseException or InvalidOperationException or IOException)
            {
                failure = ExceptionDispatchInfo.Capture(ex);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        failure?.Throw();
        return result;
    }
}
