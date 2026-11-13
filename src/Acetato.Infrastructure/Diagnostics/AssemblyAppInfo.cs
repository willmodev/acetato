using System.Reflection;
using Acetato.Application.Abstractions;
using Acetato.Application.Diagnostics;

namespace Acetato.Infrastructure.Diagnostics;

/// <summary>
/// Versión de Acetato leída del ensamblado de entrada (HU-33): la versión informativa,
/// que el build arma con la <c>Version</c> del proyecto y el código del commit.
/// </summary>
public sealed class AssemblyAppInfo : IAppInfo
{
    /// <summary>Lee el ensamblado que arrancó el proceso (Acetato.exe).</summary>
    public AssemblyAppInfo()
        : this(Assembly.GetEntryAssembly())
    {
    }

    /// <summary>Lee <paramref name="assembly"/> (los tests pasan uno conocido).</summary>
    public AssemblyAppInfo(Assembly? assembly) =>
        Version = AppVersion.Parse(
            assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);

    public AppVersion Version { get; }
}
