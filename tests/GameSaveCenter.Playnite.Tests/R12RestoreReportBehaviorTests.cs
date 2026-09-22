using System;
using System.IO;
using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R12RestoreReportBehaviorTests
{
    [Fact]
    public void RedactedRestoreReportIncludesTraceableFactsWithoutPathsOrDiagnostics()
    {
        var report = new RestoreReportDto
        {
            PlayniteId = "game-1",
            GameName = "测试游戏",
            BackupId = "backup-b",
            FileCount = 4,
            TotalBytes = 4096,
            PreRestoreBackupId = "pre-1",
            PreRestoreCreated = true,
            Stage = "回滚",
            OutcomeKind = "RolledBack",
            FailureCode = "RESTORE_FAILED_ROLLED_BACK",
            WasRolledBack = true,
            TaskId = "task-1"
        };

        var text = report.ToRedactedText();

        Assert.Contains("backup-b", text);
        Assert.Contains("pre-1", text);
        Assert.Contains("task-1", text);
        Assert.Contains("4 个文件", text);
        Assert.DoesNotContain("C:\\Users", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DiagnosticDetail", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TaskInspectorExposesReportAndKeepsTheExistingTaskDetailCommand()
    {
        var root = TestRepositoryContext.Root;
        var source = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));

        Assert.Contains("TaskRestoreReportCard", source);
        Assert.Contains("SelectedTask.RestoreReport.BackupId", source);
        Assert.Contains("SelectedTask.RestoreReport.TaskId", source);
        Assert.Contains("SelectedTask.RestoreReport.FileScopeDisplay", source);
        Assert.Contains("SelectedTask.RestoreReport.ProtectionDisplay", source);
        Assert.Contains("CopyTaskErrorCommand", source);
    }
}
