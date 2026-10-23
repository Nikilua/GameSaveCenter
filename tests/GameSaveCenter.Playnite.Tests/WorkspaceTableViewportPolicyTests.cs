using GameSaveCenter.Playnite.Infrastructure;
using System.Windows;
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

    [Theory]
    [InlineData(1600, 14, 793)]
    [InlineData(1280, 14, 633)]
    public void EqualStarInspectorUsesHalfOfRemainingDesktopWidth(double availableWidth, double gutterWidth, double expected)
    {
        Assert.Equal(expected, WorkspaceInspectorWidthPolicy.ResolveWidth(
            availableWidth,
            new GridLength(1, GridUnitType.Star),
            gutterWidth), 3);
    }

    [Fact]
    public void PixelInspectorRetainsItsConfiguredWidth()
    {
        Assert.Equal(480, WorkspaceInspectorWidthPolicy.ResolveWidth(1600, new GridLength(480), 14));
    }

}
