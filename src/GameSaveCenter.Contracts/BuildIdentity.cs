using System.Reflection;

namespace GameSaveCenter.Contracts;

/// <summary>
/// Resolves the build identity embedded by MSBuild into an assembly. The identity
/// is intentionally separate from the semantic extension version so two builds of
/// the same version can still be distinguished during Worker/plugin diagnostics.
/// </summary>
public static class BuildIdentity
{
    public const string Unknown = "unknown";

    public static string ForAssembly(Assembly assembly)
    {
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (informational != null && informational.Trim().Length > 0)
            return informational.Trim();

        var assemblyName = assembly.GetName();
        return assemblyName?.Version?.ToString() ?? Unknown;
    }
}
