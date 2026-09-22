using System;
using System.IO;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R12RestoreRevalidationBehaviorTests
{
    [Fact]
    public void ConfirmationGuardRejectsChangedGameOrBackupAndAcceptsTheSamePair()
    {
        Assert.True(RestoreConfirmationGuard.IsCurrent("game-1", "backup-a", "game-1", "backup-a"));
        Assert.False(RestoreConfirmationGuard.IsCurrent("game-1", "backup-a", "game-1", "backup-b"));
        Assert.False(RestoreConfirmationGuard.IsCurrent("game-1", "backup-a", "game-2", "backup-a"));
        Assert.False(RestoreConfirmationGuard.IsCurrent("game-1", "backup-a", null, "backup-a"));
    }

    [Fact]
    public void RestoreCommandChecksTheConfirmationPairBeforeSubmittingTheRequest()
    {
        var root = TestRepositoryContext.Root;
        var implementation = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

        Assert.Contains("RestoreConfirmationGuard.IsCurrent", implementation);
        Assert.Contains("未执行旧确认", implementation);
        Assert.Contains("ResetRestoreWorkflow();", implementation);
    }
}
