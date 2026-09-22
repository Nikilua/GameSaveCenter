using System;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R22RestoreConfirmationTimeBehaviorTests
{
    [Fact]
    public void RestoreConfirmationShowsRelativeAndFullTimeAlongsideSafetyContext()
    {
        var backup = new BackupVersionDto
        {
            BackupId = "restore-time-22",
            CreatedUtc = DateTime.UtcNow.AddMinutes(-3),
            SourceDevice = "合成设备",
            OperatingSystem = "Windows",
            RestoreReadiness = new RestoreReadinessDto
            {
                Status = RestoreReadinessStatus.Ready,
                Summary = "文件与大小均已验证。"
            }
        };

        var confirmation = DashboardViewModel.BuildRestoreConfirmation("合成游戏", backup);

        Assert.Contains($"版本：{backup.CreatedRelativeDisplay}（{backup.CreatedFullDisplay}）", confirmation, StringComparison.Ordinal);
        Assert.Contains("合成设备 · Windows", confirmation, StringComparison.Ordinal);
        Assert.Contains("PreRestore", confirmation, StringComparison.Ordinal);
        Assert.Contains("启动器和 MOD 管理器均已关闭", confirmation, StringComparison.Ordinal);
        Assert.DoesNotContain($"版本：{backup.CreatedLocal:yyyy-MM-dd HH:mm:ss} ·", confirmation, StringComparison.Ordinal);
    }

    [Fact]
    public void RestoreConfirmationKeepsUnknownBackupTimeExplicit()
    {
        var backup = new BackupVersionDto { BackupId = "restore-time-unknown" };

        var confirmation = DashboardViewModel.BuildRestoreConfirmation("合成游戏", backup);

        Assert.Contains("版本：时间未知（时间未知）", confirmation, StringComparison.Ordinal);
        Assert.Contains("尚未验证该版本的可恢复性。", confirmation, StringComparison.Ordinal);
    }
}
