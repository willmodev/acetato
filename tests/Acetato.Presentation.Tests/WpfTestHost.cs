using System.IO;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Acetato.Presentation.Tests;

/// <summary>
/// Hilo STA único para los tests de humo de WPF, con los recursos de App.xaml
/// cargados una vez. <c>Application.Current</c> es único por proceso y queda
/// atado al hilo que lo crea: si cada test creara su hilo, el segundo no podría
/// usar los recursos del primero.
/// </summary>
internal static class WpfTestHost
{
    private static readonly string[] Dictionaries =
    [
        "Resources/Tokens.xaml",
        "Resources/Icons.xaml",
        "Resources/ToolbarStyles.xaml",
        "Resources/SettingsStyles.xaml",
    ];

    private static readonly Lazy<Dispatcher> Host = new(StartHost);

    /// <summary>Ejecuta <paramref name="work"/> en el hilo STA con los recursos de la app.</summary>
    public static T Run<T>(Func<T> work) => Host.Value.Invoke(work);

    private static Dispatcher StartHost()
    {
        Dispatcher? dispatcher = null;
        ExceptionDispatchInfo? failure = null;
        using var ready = new ManualResetEventSlim();
        var thread = new Thread(() =>
        {
            try
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                LoadApplicationResources();
            }
            catch (Exception ex) when (ex is XamlParseException or InvalidOperationException or IOException)
            {
                failure = ExceptionDispatchInfo.Capture(ex);
            }
            finally
            {
                ready.Set();
            }

            Dispatcher.Run();
        })
        {
            IsBackground = true,
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        ready.Wait();
        failure?.Throw();
        return dispatcher!;
    }

    // Recursos de App.xaml sin crear App (que arrancaría la app entera).
    private static void LoadApplicationResources()
    {
        var application = System.Windows.Application.Current
            ?? new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        foreach (string path in Dictionaries)
        {
            application.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Acetato;component/{path}", UriKind.Absolute),
            });
        }
    }
}
