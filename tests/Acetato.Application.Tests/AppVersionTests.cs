using Acetato.Application.Diagnostics;
using FluentAssertions;
using Xunit;

namespace Acetato.Application.Tests;

/// <summary>Versión de Acetato (HU-33): en pantalla sin el commit; completa en el registro.</summary>
public sealed class AppVersionTests
{
    [Fact]
    public void Cuts_the_commit_code_for_the_screen_and_keeps_it_in_full()
    {
        var version = AppVersion.Parse("0.9.0+0ba354c1d2e3");

        version.Display.Should().Be("0.9.0");
        version.Full.Should().Be("0.9.0+0ba354c1d2e3");
    }

    [Fact]
    public void Without_commit_code_both_are_the_same()
    {
        var version = AppVersion.Parse("0.9.0");

        version.Display.Should().Be("0.9.0");
        version.Full.Should().Be("0.9.0");
    }

    [Fact]
    public void Keeps_prerelease_labels_on_screen()
    {
        AppVersion.Parse("1.0.0-beta.2+abc").Display.Should().Be("1.0.0-beta.2");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("+abc")]
    public void Missing_version_is_unknown(string? informationalVersion)
    {
        AppVersion.Parse(informationalVersion).Should().Be(AppVersion.Unknown);
    }
}
