using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Core.Tests;

public sealed class MediaDisplayTests
{
    [Theory]
    [InlineData("Synced", "已同步")]
    [InlineData("Pending", "待上传")]
    [InlineData("Failed", "失败")]
    [InlineData("RetryScheduled", "等待重试")]
    [InlineData("NotApplicable", "不适用")]
    public void MediaCloudStateUsesUserFacingText(string state, string expected)
    {
        var media = new MediaItemDto { CloudState = state };

        Assert.Equal(expected, media.CloudStateDisplay);
    }

    [Fact]
    public void GameCloudRetryStateUsesUserFacingText()
    {
        var game = new GameStatusDto { CloudState = "RetryScheduled" };

        Assert.Equal("等待重试", game.CloudStateDisplay);
    }
}
