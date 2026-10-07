using System.Diagnostics;
using Acetato.Infrastructure.Hotkeys;
using FluentAssertions;
using Xunit;

namespace Acetato.Infrastructure.Tests;

/// <summary>
/// Cierre del hilo de atajos (HU-20). Registra atajos globales reales: cerrar la app
/// antes de ejecutar los tests (el registro es best-effort; aquí se mide el cierre).
/// </summary>
public sealed class Win32GlobalHotkeyServiceTests
{
    // Menor que el tope de espera de Unregister (2 s): un cuelgue del hilo de atajos
    // hace fallar el test aunque el tope salve el cierre real de la app.
    private static readonly TimeSpan MaxShutdown = TimeSpan.FromSeconds(1);

    [Fact]
    public void Unregister_ends_the_pump_thread_quickly()
    {
        using var service = new Win32GlobalHotkeyService();
        service.Register();

        var watch = Stopwatch.StartNew();
        service.Unregister();
        watch.Stop();

        watch.Elapsed.Should().BeLessThan(MaxShutdown);
    }

    [Fact]
    public void Register_after_Unregister_starts_again_and_ends_quickly()
    {
        using var service = new Win32GlobalHotkeyService();
        service.Register();
        service.Unregister();
        service.Register();

        var watch = Stopwatch.StartNew();
        service.Unregister();
        watch.Stop();

        watch.Elapsed.Should().BeLessThan(MaxShutdown);
    }
}
