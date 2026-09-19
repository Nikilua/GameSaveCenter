using System;
using System.IO;
using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R15TaskStageTests
{
    [Fact]
    public void TaskStageUsesLastWorkerEventAndDoesNotInventUnknownProgress()
    {
        var running = new TaskStatusDto
        {
            TaskType = "Backup",
            State = TaskState.Running,
            ProgressPercent = 0,
            Message = "正在执行",
            StageMessage = "正在执行"
        };

        Assert.Equal("阶段未知", running.StageDisplay);
        Assert.Equal("—", running.ProgressDisplay);

        running.ProgressPercent = 10;
        running.Message = "正在扫描存档";
        running.StageMessage = running.Message;
        Assert.Equal("扫描中", running.StageDisplay);
        Assert.Equal("10%", running.ProgressDisplay);

        var failed = new TaskStatusDto
        {
            TaskType = "Backup",
            State = TaskState.Failed,
            ProgressPercent = 10,
            Message = "执行失败",
            StageMessage = "正在扫描存档",
            ErrorCode = "LUDUSAVI_FAILED",
            ErrorMessage = "合成失败"
        };

        Assert.Equal("扫描中", failed.StageDisplay);
        Assert.Contains("合成失败", failed.DetailMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void TaskCenterExposesStageColumnAndSeparateStageDetail()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var store = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Persistence", "SqliteStateStore.cs"));
        var queries = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Persistence", "SqliteStateStore.TaskQueries.cs"));

        Assert.Contains("Header=\"阶段\"", view, StringComparison.Ordinal);
        Assert.Contains("SelectedTask.StageDisplay", view, StringComparison.Ordinal);
        Assert.Contains("SelectedTask.StageMessage", view, StringComparison.Ordinal);
        Assert.Contains("stage_message", store, StringComparison.Ordinal);
        Assert.Contains("stage_message", queries, StringComparison.Ordinal);
    }
}
