using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WorkspaceTableViewportPolicyTests
{
    [Theory]
    [InlineData(920, 640, 640)]
    [InlineData(640, 920, 640)]
    [InlineData(0, 640, 640)]
    [InlineData(920, 0, 920)]
    [InlineData(0, 0, 0)]
    public void MeasuredPageViewportBoundsHostHeight(double hostHeight, double measuredViewportHeight, double expected)
    {
        Assert.Equal(expected, WorkspaceTableViewportPolicy.ResolvePageViewportHeight(hostHeight, measuredViewportHeight));
    }
}
