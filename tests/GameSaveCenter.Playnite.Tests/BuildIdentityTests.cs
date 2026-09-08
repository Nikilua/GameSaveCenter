using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class BuildIdentityTests
{
    [Fact]
    public void AssemblyBuildIdentityIsStableAndNonEmpty()
    {
        var assembly = typeof(BuildIdentityTests).Assembly;
        var identity = BuildIdentity.ForAssembly(assembly);

        Assert.False(string.IsNullOrWhiteSpace(identity));
        var semanticVersion = assembly.GetName().Version?.ToString(3) ?? BuildIdentity.Unknown;
        Assert.StartsWith(semanticVersion, identity, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SameVersionDifferentBuildIsIncompatibleWhenBothIdentitiesAreKnown()
    {
        Assert.False(WorkerLauncher.IsBuildIdentityCompatible("0.6.73+new-build", "0.6.73+old-build"));
        Assert.False(WorkerLauncher.IsBuildIdentityCompatible("", "0.6.73+old-build"));
        Assert.True(WorkerLauncher.IsBuildIdentityCompatible("0.6.73+same-build", "0.6.73+same-build"));
    }

    [Fact]
    public void UnknownBuildIdentityIsExplicitlyRecognized()
    {
        Assert.True(BuildIdentity.IsUnknown("unknown"));
        Assert.True(BuildIdentity.IsUnknown("0.6.73+unknown"));
        Assert.True(BuildIdentity.IsUnknown(""));
        Assert.False(BuildIdentity.IsUnknown("0.6.73+commit-sha"));
    }
}
