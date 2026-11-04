using Acetato.Infrastructure.Diagnostics;
using FluentAssertions;
using Xunit;

namespace Acetato.Infrastructure.Tests;

/// <summary>Versión leída del ensamblado (HU-33): la del proyecto más el código del commit.</summary>
public sealed class AssemblyAppInfoTests
{
    [Fact]
    public void Reads_the_project_version_from_the_assembly()
    {
        var version = new AssemblyAppInfo(typeof(AssemblyAppInfo).Assembly).Version;

        version.Display.Should().Be("0.9.0");
        version.Full.Should().StartWith("0.9.0");
    }

    [Fact]
    public void Without_assembly_the_version_is_unknown()
    {
        new AssemblyAppInfo(null).Version.Display.Should().Be("0.0.0");
    }
}
