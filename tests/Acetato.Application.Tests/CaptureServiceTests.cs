using Acetato.Application.Abstractions;
using Acetato.Application.Capture;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Acetato.Application.Tests;

public sealed class CaptureServiceTests : IDisposable
{
    private static readonly DateTimeOffset FixedInstant = new(2026, 6, 7, 15, 30, 12, TimeSpan.Zero);

    private readonly IScreenCaptureService _screen = Substitute.For<IScreenCaptureService>();
    private readonly ICaptureDirectoryProvider _directory = Substitute.For<ICaptureDirectoryProvider>();
    private readonly ICaptureNotifier _notifier = Substitute.For<ICaptureNotifier>();
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), "AcetatoCaptureTests", Guid.NewGuid().ToString("N"));

    private CaptureService CreateService()
    {
        _directory.GetCaptureDirectory().Returns(_tempDir);
        return new CaptureService(_screen, _directory, _notifier, new FixedTimeProvider(FixedInstant));
    }

    [Fact]
    public async Task Capture_saves_png_with_timestamped_name_in_directory()
    {
        var service = CreateService();

        var result = await service.CaptureAsync();

        string expected = Path.Combine(_tempDir, "acetato-20260607-153012.png");
        result.FilePath.Should().Be(expected);
        _screen.Received(1).CaptureToPng(expected);
        _notifier.Received(1).NotifyCaptured(expected);
    }

    [Fact]
    public async Task Capture_creates_the_target_directory_when_missing()
    {
        var service = CreateService();

        await service.CaptureAsync();

        Directory.Exists(_tempDir).Should().BeTrue();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    // Reloj fijo en UTC para un sello de tiempo determinista, sin depender de la
    // zona horaria de la máquina.
    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;

        public FixedTimeProvider(DateTimeOffset now) => _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
