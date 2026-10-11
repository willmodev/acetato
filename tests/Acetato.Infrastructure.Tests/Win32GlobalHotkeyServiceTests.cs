using System.Diagnostics;
using Acetato.Application.Abstractions;
using Acetato.Domain;
using Acetato.Infrastructure.Hotkeys;
using FluentAssertions;
using Xunit;

namespace Acetato.Infrastructure.Tests;

/// <summary>
/// Hilo de atajos (HU-20) y registro dinámico (HU-21). Registra atajos globales reales: cerrar la app
/// antes de ejecutar los tests.
/// </summary>
public sealed class Win32GlobalHotkeyServiceTests
{
    // Menor que el tope de espera de Unregister (2 s): un cuelgue del hilo de atajos
    // hace fallar el test aunque el tope salve el cierre real de la app.
    private static readonly TimeSpan MaxShutdown = TimeSpan.FromSeconds(1);

    // Combinaciones raras para no chocar con atajos reales del equipo.
    private static readonly KeyChord First = new(
        HotkeyModifiers.Ctrl | HotkeyModifiers.Alt | HotkeyModifiers.Shift, HotkeyKey.F11);

    private static readonly KeyChord Second = new(
        HotkeyModifiers.Ctrl | HotkeyModifiers.Alt | HotkeyModifiers.Shift, HotkeyKey.F12);

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

    [Fact]
    public void Apply_registers_and_a_second_call_replaces_the_set()
    {
        using var owner = new Win32GlobalHotkeyService();
        using var probe = new Win32GlobalHotkeyService();
        owner.Register();
        probe.Register();

        owner.Apply([new HotkeyAssignment(HotkeyAction.Undo, First)]).Should().BeEmpty();
        probe.IsAvailable(First).Should().BeFalse();

        owner.Apply([new HotkeyAssignment(HotkeyAction.Undo, Second)]).Should().BeEmpty();
        probe.IsAvailable(First).Should().BeTrue();
        probe.IsAvailable(Second).Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_is_false_for_a_chord_already_registered()
    {
        using var service = new Win32GlobalHotkeyService();
        service.Register();
        service.IsAvailable(First).Should().BeTrue();

        _ = service.Apply([new HotkeyAssignment(HotkeyAction.Capture, First)]);

        service.IsAvailable(First).Should().BeFalse();
    }

    [Fact]
    public void Apply_reports_the_actions_whose_chord_is_taken()
    {
        using var owner = new Win32GlobalHotkeyService();
        using var other = new Win32GlobalHotkeyService();
        owner.Register();
        other.Register();
        _ = owner.Apply([new HotkeyAssignment(HotkeyAction.Undo, First)]);

        var failed = other.Apply(
        [
            new HotkeyAssignment(HotkeyAction.Clear, First),
            new HotkeyAssignment(HotkeyAction.Capture, Second),
        ]);

        failed.Should().Equal(HotkeyAction.Clear);
    }

    [Fact]
    public void Unregister_releases_the_applied_chords()
    {
        using var owner = new Win32GlobalHotkeyService();
        using var probe = new Win32GlobalHotkeyService();
        owner.Register();
        probe.Register();
        _ = owner.Apply([new HotkeyAssignment(HotkeyAction.Undo, First)]);

        owner.Unregister();

        probe.IsAvailable(First).Should().BeTrue();
    }

    [Fact]
    public void Apply_without_a_running_thread_fails_every_action()
    {
        using var service = new Win32GlobalHotkeyService();

        service.Apply([new HotkeyAssignment(HotkeyAction.Undo, First)]).Should().Equal(HotkeyAction.Undo);
        service.IsAvailable(First).Should().BeFalse();
    }
}
