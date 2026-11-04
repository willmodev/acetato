using Acetato.Application.Abstractions;
using Acetato.Application.Diagnostics;
using Acetato.Presentation.Settings;
using NSubstitute;

namespace Acetato.Presentation.Tests;

/// <summary>Bloque «Acerca de» de Ajustes (HU-33) con dobles y la versión 0.9.0.</summary>
internal static class TestAbout
{
    /// <summary>Versión, portapapeles y reporte falsos.</summary>
    public static SettingsAboutServices Services(
        IClipboardWriter? clipboard = null, IProblemReportWriter? reports = null)
    {
        var appInfo = Substitute.For<IAppInfo>();
        appInfo.Version.Returns(AppVersion.Parse("0.9.0+abc123"));
        return new SettingsAboutServices(
            appInfo, clipboard ?? Substitute.For<IClipboardWriter>(), reports ?? Substitute.For<IProblemReportWriter>());
    }
}
