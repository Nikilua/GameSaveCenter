using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Core.Tests;

public sealed class ProtocolCompatibilityTests
{
    [Fact]
    public void MatchingProtocolWithinMinimumIsCompatible()
    {
        Assert.True(ProtocolCompatibility.IsCompatible(1, 1, 1));
        Assert.True(ProtocolCompatibility.IsCompatible(2, 2, 1));
    }

    [Fact]
    public void MismatchedOrBelowMinimumIsRejected()
    {
        Assert.False(ProtocolCompatibility.IsCompatible(1, 2, 1));
        Assert.False(ProtocolCompatibility.IsCompatible(1, 1, 2));
        Assert.False(ProtocolCompatibility.IsCompatible(2, 1, 1));
    }

    [Fact]
    public void WorkerCapabilitiesExposeHardeningFeatures()
    {
        Assert.Contains("RestoreReadiness", WorkerCapabilities.Current);
        Assert.Contains("MetadataBackup", WorkerCapabilities.Current);
        Assert.Contains("RepositoryRebuild", WorkerCapabilities.Current);
        Assert.Contains("PathRemap", WorkerCapabilities.Current);
        Assert.Contains("TaskReconcile", WorkerCapabilities.Current);
        Assert.Contains("GameOperationLock", WorkerCapabilities.Current);
        Assert.Contains("AtomicIo", WorkerCapabilities.Current);
        Assert.Contains("StorageAnalysis", WorkerCapabilities.Current);
        Assert.Contains("RetentionSimulation", WorkerCapabilities.Current);
        Assert.Contains("LocalMirror", WorkerCapabilities.Current);
        Assert.Contains("MaintenanceReport", WorkerCapabilities.Current);
    }
}
