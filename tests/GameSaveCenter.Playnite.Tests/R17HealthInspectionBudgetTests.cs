using System;
using System.IO;
using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R17HealthInspectionBudgetTests
{
    [Fact]
    public void RunningStateShowsCandidateAndBoundedPlanWithoutClaimingFullScan()
    {
        var state = new HealthInspectionStateDto
        {
            Enabled = true,
            IntervalMinutes = 15,
            MaxDurationSeconds = 30,
            LastStartedUtc = DateTime.UtcNow,
            LastStatus = "Running",
            CursorPlayniteId = "game-1",
            CursorBackupId = "backup-1"
        };

        Assert.Equal("当前候选：game-1 / backup-1", state.CurrentCandidateDisplay);
        Assert.Contains("单次预算 30 秒", state.NextPlanDisplay, StringComparison.Ordinal);
        Assert.Equal("进行中；当前摘要不代表整库已检查", state.ProgressDisplay);
    }

    [Fact]
    public void CancelledStateSeparatesCompletionFromLastSuccessAndNextPlan()
    {
        var state = new HealthInspectionStateDto
        {
            Enabled = true,
            IntervalMinutes = 60,
            MaxDurationSeconds = 120,
            LastStatus = "Cancelled",
            LastCompletedUtc = new DateTime(2026, 9, 20, 4, 5, 0, DateTimeKind.Utc),
            LastSummary = "结束状态：已取消"
        };

        Assert.Equal("本轮已取消；不代表整库已检查", state.ProgressDisplay);
        Assert.Contains("2026-09-20", state.LastCompletedLocalDisplay, StringComparison.Ordinal);
        Assert.Contains("每 60 分钟", state.NextPlanDisplay, StringComparison.Ordinal);
        Assert.DoesNotContain("尚未结束", state.LastCompletedLocalDisplay, StringComparison.Ordinal);
    }

    [Fact]
    public void MaintenanceSurfaceShowsScopeCandidateCompletionSuccessAndPlan()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var actions = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.MaintenanceActions.cs"));

        Assert.Contains("Snapshot.HealthInspection.ProgressDisplay", view, StringComparison.Ordinal);
        Assert.Contains("Snapshot.HealthInspection.CurrentCandidateDisplay", view, StringComparison.Ordinal);
        Assert.Contains("Snapshot.HealthInspection.LastCompletedRelativeDisplay", view, StringComparison.Ordinal);
        Assert.Contains("Snapshot.HealthInspection.NextPlanRelativeDisplay", view, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding Snapshot.HealthInspection.LastCompletedFullDisplay}\"", view, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding Snapshot.HealthInspection.NextPlanFullDisplay}\"", view, StringComparison.Ordinal);
        Assert.Contains("LastAttemptDisplay = inspection.LastCompletedLocalDisplay", actions, StringComparison.Ordinal);
        Assert.Contains("最近完成：{LastAttemptDisplay}", actions, StringComparison.Ordinal);
    }
}
