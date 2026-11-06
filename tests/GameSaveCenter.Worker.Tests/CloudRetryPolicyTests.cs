using GameSaveCenter.Worker.Services;
using GameSaveCenter.Worker.Infrastructure;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class CloudRetryPolicyTests
{
    [Fact]
    public void RetryPolicy_UsesBoundedDeterministicBackoff()
    {
        var now = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);
        var expectedMinutes = new[] { 1, 5, 15, 60, 240, 720 };

        Assert.Equal(expectedMinutes.Length, CloudRetryPolicy.MaximumAutomaticRetries);
        for (var retry = 1; retry <= expectedMinutes.Length; retry++)
            Assert.Equal(now.AddMinutes(expectedMinutes[retry - 1]), CloudRetryPolicy.GetNextAttemptUtc(retry, now));

        Assert.Throws<ArgumentOutOfRangeException>(() => CloudRetryPolicy.GetNextAttemptUtc(0, now));
        Assert.Throws<ArgumentOutOfRangeException>(() => CloudRetryPolicy.GetNextAttemptUtc(7, now));

        Assert.False(CloudRetryPolicy.IsAutomaticRetryLimitReached(expectedMinutes.Length - 1));
        Assert.True(CloudRetryPolicy.IsAutomaticRetryLimitReached(expectedMinutes.Length));
        Assert.True(CloudRetryPolicy.IsAutomaticRetryLimitReached(expectedMinutes.Length + 1));
    }

    [Theory]
    [InlineData("authentication failed", RcloneFailureKind.Authentication, "RCLONE_AUTH_FAILED")]
    [InlineData("permission denied", RcloneFailureKind.Permission, "RCLONE_PERMISSION_DENIED")]
    [InlineData("remote not found", RcloneFailureKind.RemoteNotFound, "RCLONE_REMOTE_NOT_FOUND")]
    [InlineData("network timeout", RcloneFailureKind.Network, "RCLONE_NETWORK_FAILED")]
    [InlineData("partial transfer; incomplete", RcloneFailureKind.Incomplete, "RCLONE_TRANSFER_INCOMPLETE")]
    [InlineData("quota exceeded: no space left", RcloneFailureKind.NoSpace, "RCLONE_NO_SPACE")]
    [InlineData("429 too many requests", RcloneFailureKind.RateLimited, "RCLONE_RATE_LIMITED")]
    public void RcloneFailuresBecomeActionableBoundedCodes(string text, RcloneFailureKind expected, string code)
    {
        Assert.Equal(expected, RcloneFailureClassifier.Classify(text));
        Assert.Equal(code, RcloneFailureClassifier.GetErrorCode(expected));
        Assert.Equal(expected is RcloneFailureKind.Network or RcloneFailureKind.Incomplete or RcloneFailureKind.RateLimited, RcloneFailureClassifier.IsRetryable(code));
    }
}
