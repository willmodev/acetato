using System.Globalization;
using System.Runtime.InteropServices;
using Acetato.Application.Abstractions;
using CommunityToolkit.WinUI.Notifications;

namespace Acetato.Infrastructure.Settings;

/// <summary>
/// Notificación de Windows con los problemas de atajos al arrancar (HU-21). Lleva
/// el argumento <c>action=settings</c> y solo atiende ese argumento en el evento
/// global de activación de CommunityToolkit.
/// </summary>
public sealed class ToastHotkeyNotifier : IHotkeyNotifier, IDisposable
{
    private const string ActionArgument = "action";
    private const string SettingsValue = "settings";

    public ToastHotkeyNotifier() => ToastNotificationManagerCompat.OnActivated += OnToastActivated;

    public event EventHandler? SettingsRequested;

    public void NotifyStartupProblems(int unavailableCount, bool settingsWereReset)
    {
        if (unavailableCount <= 0 && !settingsWereReset)
        {
            return;
        }

        try
        {
            var builder = new ToastContentBuilder().AddArgument(ActionArgument, SettingsValue);
            foreach (string line in Lines(unavailableCount, settingsWereReset))
            {
                builder.AddText(line);
            }

            builder.Show();
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException)
        {
            // Best-effort: la fila «No disponible» en Ajustes sigue avisando.
        }
    }

    public void Dispose() => ToastNotificationManagerCompat.OnActivated -= OnToastActivated;

    private static List<string> Lines(int unavailableCount, bool settingsWereReset)
    {
        var lines = new List<string>(2);
        if (unavailableCount == 1)
        {
            lines.Add("1 atajo no está disponible");
        }
        else if (unavailableCount > 1)
        {
            lines.Add(string.Create(CultureInfo.InvariantCulture, $"{unavailableCount} atajos no están disponibles"));
        }

        if (settingsWereReset)
        {
            lines.Add("Ajustes dañados: se usan los atajos predeterminados");
        }

        return lines;
    }

    // Clic en el toast: solo los avisos de atajos piden Ajustes.
    private void OnToastActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        var arguments = ToastArguments.Parse(e.Argument);
        if (arguments.Contains(ActionArgument, SettingsValue))
        {
            SettingsRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
