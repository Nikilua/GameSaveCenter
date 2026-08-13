using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using Xunit;

namespace GameSaveCenter.Core.Tests;

public sealed class GameHealthAssessmentTests
{
    private static readonly DateTime Now = new(2026, 8, 12, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void RecentlyPlayedWithoutBackup_IsRisk()
    {
        var result = Assess(new GameHealthInput { LudusaviMatched = true, LastPlayedUtc = Now.AddDays(-2) });

        Assert.Equal(GameHealthState.Risk, result.State);
        Assert.Contains(result.Reasons, reason => reason.Contains("尚未发现本地备份", StringComparison.Ordinal));
    }

    [Fact]
    public void InactiveGameWithoutBackup_IsUnknown()
    {
        var result = Assess(new GameHealthInput { LudusaviMatched = true, LastPlayedUtc = Now.AddDays(-45) });

        Assert.Equal(GameHealthState.Unknown, result.State);
    }

    [Fact]
    public void RecentReadyBackup_IsHealthy()
    {
        var result = Assess(new GameHealthInput
        {
            LudusaviMatched = true,
            LastPlayedUtc = Now.AddDays(-2),
            LastBackupUtc = Now.AddDays(-2),
            BackupVersionCount = 1,
            LatestRestoreReadinessStatus = RestoreReadinessStatus.Ready
        });

        Assert.Equal(GameHealthState.Healthy, result.State);
        Assert.Contains("本地备份正常", result.Reasons);
    }

    [Fact]
    public void MissingReadiness_IsAttentionAndExplained()
    {
        var result = Assess(new GameHealthInput
        {
            LudusaviMatched = true,
            LastPlayedUtc = Now.AddDays(-3),
            LastBackupUtc = Now.AddDays(-3),
            BackupVersionCount = 1
        });

        Assert.Equal(GameHealthState.Attention, result.State);
        Assert.Contains("最新恢复点尚未验证", result.Reasons);
    }

    [Fact]
    public void RepeatedFailuresOrCorruptedReadiness_IsRisk()
    {
        var failures = Assess(new GameHealthInput
        {
            LudusaviMatched = true,
            LastPlayedUtc = Now.AddDays(-1),
            LastBackupUtc = Now.AddDays(-1),
            BackupVersionCount = 1,
            RecentBackupFailureCount = 3
        });
        var corrupted = Assess(new GameHealthInput
        {
            LudusaviMatched = true,
            LastPlayedUtc = Now.AddDays(-1),
            LastBackupUtc = Now.AddDays(-1),
            BackupVersionCount = 1,
            LatestRestoreReadinessStatus = RestoreReadinessStatus.Corrupted
        });

        Assert.Equal(GameHealthState.Risk, failures.State);
        Assert.Equal(GameHealthState.Risk, corrupted.State);
    }

    [Fact]
    public void UnmatchedGame_IsUnknownRegardlessOfActivity()
    {
        var result = Assess(new GameHealthInput
        {
            LudusaviMatched = false,
            LastPlayedUtc = Now.AddDays(-1),
            LastBackupUtc = Now.AddDays(-1),
            BackupVersionCount = 1
        });

        Assert.Equal(GameHealthState.Unknown, result.State);
    }

    [Fact]
    public void CloudFailureOnlyMattersWhenCloudIsEnabled()
    {
        var local = new GameHealthInput
        {
            LudusaviMatched = true, LastPlayedUtc = Now.AddDays(-1), LastBackupUtc = Now.AddDays(-1),
            BackupVersionCount = 1, LatestRestoreReadinessStatus = RestoreReadinessStatus.Ready,
            CloudState = "Failed", CloudEnabled = false
        };
        var disabled = Assess(local);
        local.CloudEnabled = true;
        var enabled = Assess(local);

        Assert.Equal(GameHealthState.Healthy, disabled.State);
        Assert.Equal(GameHealthState.Attention, enabled.State);
        Assert.Contains(enabled.Reasons, x => x.Contains("云端", StringComparison.Ordinal));
    }

    [Fact]
    public void OpenBackupAnomalyOverridesOtherwiseHealthyState()
    {
        var result = Assess(new GameHealthInput
        {
            LudusaviMatched = true, LastPlayedUtc = Now.AddDays(-1), LastBackupUtc = Now.AddDays(-1),
            BackupVersionCount = 1, LatestRestoreReadinessStatus = RestoreReadinessStatus.Ready,
            OpenFindingErrorCount = 1
        });

        Assert.Equal(GameHealthState.Risk, result.State);
        Assert.Contains(result.Reasons, x => x.Contains("备份错误", StringComparison.Ordinal));
    }

    private static GameHealthAssessment Assess(GameHealthInput input)
        => new GameHealthAssessmentService().Assess(input, Now);
}
