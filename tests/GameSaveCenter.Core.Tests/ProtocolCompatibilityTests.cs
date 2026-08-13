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
}
