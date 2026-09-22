using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class BackupResultLayerTests
{
    [Theory]
    [InlineData("RetryScheduled", true, true, false, "云端上传排队")]
    [InlineData("Failed", true, true, false, "云端镜像失败")]
    [InlineData("AuthenticationRequired", true, true, false, "云端认证需处理")]
    [InlineData("Transferring", true, false, false, "云端传输中")]
    [InlineData("Uploaded", false, false, true, "待远端校验")]
    [InlineData("RemoteVerified", false, false, false, "远端已校验")]
    [InlineData("Disabled", false, false, false, "本地备份已成功")]
    public void LocalSuccessAndCloudStateRemainSeparatelyActionable(
        string cloudState,
        bool expectedPartialSuccess,
        bool expectedRetry,
        bool expectedVerify,
        string expectedDisplay)
    {
        var result = new BackupResultDto
        {
            LocalState = "Succeeded",
            CloudState = cloudState
        };

        Assert.True(result.LocalBackupSucceeded);
        Assert.Equal(expectedPartialSuccess, result.IsPartialSuccess);
        Assert.Equal(expectedRetry, result.CanRetryCloudUpload);
        Assert.Equal(expectedVerify, result.CanVerifyRemote);
        Assert.Contains(expectedDisplay, result.StateDisplay);

        if (expectedRetry)
            Assert.Contains("不会重新创建本地备份", result.RemediationDisplay);
        else
            Assert.DoesNotContain("重试云端上传", result.RemediationDisplay);
    }

    [Fact]
    public void FailedLocalBackupDoesNotExposeCloudRemediationAsSuccess()
    {
        var result = new BackupResultDto
        {
            LocalState = "Failed",
            CloudState = "RetryScheduled"
        };

        Assert.True(result.HasResult);
        Assert.False(result.LocalBackupSucceeded);
        Assert.False(result.IsPartialSuccess);
        Assert.False(result.CanRetryCloudUpload);
        Assert.Equal("本地备份未完成", result.StateDisplay);
    }
}
