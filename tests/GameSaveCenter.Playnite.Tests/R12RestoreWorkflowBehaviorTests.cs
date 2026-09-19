using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R12RestoreWorkflowBehaviorTests
{
    [Fact]
    public void SelectionWithoutReadinessKeepsExecutionPending()
    {
        var steps = RestoreWorkflowProgress.Build(Version("selected"), false, false, false, null, string.Empty, string.Empty);

        Assert.Equal(4, steps.Count);
        Assert.Equal(RestoreWorkflowStepStatus.Complete, Step(steps, "selection").Status);
        Assert.Equal(RestoreWorkflowStepStatus.Pending, Step(steps, "readiness").Status);
        Assert.Equal(RestoreWorkflowStepStatus.Pending, Step(steps, "target").Status);
        Assert.Equal(RestoreWorkflowStepStatus.Pending, Step(steps, "execution").Status);
        Assert.Contains("执行阶段才会写入当前存档目录", RestoreWorkflowProgress.BuildSummary(steps));
    }

    [Fact]
    public void ReadinessWarningAndActiveExecutionExposeRealSafetyBoundary()
    {
        var backup = Version("warning", RestoreReadinessStatus.Warning);
        backup.RestoreReadiness!.WarningCount = 1;
        backup.RestoreReadiness.ExpectedFileCount = 4;
        backup.RestoreReadiness.ActualFileCount = 3;
        backup.RestoreReadiness.Summary = "归档存在一项可继续处理的警告";

        var steps = RestoreWorkflowProgress.Build(backup, false, true, true, null, string.Empty, string.Empty);

        Assert.Equal(RestoreWorkflowStepStatus.Warning, Step(steps, "readiness").Status);
        Assert.Equal(RestoreWorkflowStepStatus.Active, Step(steps, "target").Status);
        Assert.Equal(RestoreWorkflowStepStatus.Active, Step(steps, "execution").Status);
        Assert.Contains("PreRestore", Step(steps, "execution").Detail);
        Assert.Contains("PreRestore", RestoreWorkflowProgress.BuildSummary(steps));
    }

    [Fact]
    public void TargetFailureStopsBeforeExecution()
    {
        var task = Task(TaskState.Failed, "RESTORE_GAME_RUNNING", "游戏仍在运行");
        var steps = RestoreWorkflowProgress.Build(Version("running", RestoreReadinessStatus.Ready), false, false, false, task, string.Empty, string.Empty);

        Assert.Equal(RestoreWorkflowStepStatus.Failed, Step(steps, "target").Status);
        Assert.Equal(RestoreWorkflowStepStatus.Pending, Step(steps, "execution").Status);
        Assert.Contains("恢复流程停在“目标核对”", RestoreWorkflowProgress.BuildSummary(steps));
        Assert.Contains("尚未进入写入当前存档", Step(steps, "execution").Detail);
    }

    [Fact]
    public void PostRestoreFailureRetainsExecutionFailure()
    {
        var task = Task(TaskState.Failed, "RESTORE_FAILED_ROLLED_BACK", "恢复失败，已回滚到 PreRestore");
        var steps = RestoreWorkflowProgress.Build(Version("rolled-back", RestoreReadinessStatus.Ready), false, false, false, task, string.Empty, string.Empty);

        Assert.Equal(RestoreWorkflowStepStatus.Pending, Step(steps, "target").Status);
        Assert.Equal(RestoreWorkflowStepStatus.Failed, Step(steps, "execution").Status);
        Assert.Contains("RESTORE_FAILED_ROLLED_BACK", Step(steps, "execution").Detail);
        Assert.Contains("当前步骤和失败详情会保留", RestoreWorkflowProgress.BuildSummary(steps));
    }

    [Fact]
    public void SuccessfulTaskCompletesAllStages()
    {
        var task = Task(TaskState.Succeeded, string.Empty, "恢复后校验通过");
        var steps = RestoreWorkflowProgress.Build(Version("success", RestoreReadinessStatus.Ready), false, false, false, task, string.Empty, string.Empty);

        Assert.All(steps, step => Assert.True(step.IsCompleted, step.Key));
        Assert.Equal("四个恢复阶段已完成；任务结果仍可在任务中心追溯。", RestoreWorkflowProgress.BuildSummary(steps));
    }

    [Fact]
    public void SaveCenterProjectsTheFourStagesWithoutReplacingExistingEntryPoints()
    {
        var path = Path.Combine(TestRepositoryContext.Root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml");
        var document = XDocument.Load(path);
        var controls = document.Descendants()
            .Where(element => element.Name.LocalName == "ItemsControl")
            .ToList();

        Assert.Contains(controls, element =>
            (element.Attribute("ItemsSource")?.Value ?? string.Empty).IndexOf("RestoreWorkflowSteps", StringComparison.Ordinal) >= 0);
        var source = File.ReadAllText(path);
        Assert.Contains("ValidateRestoreReadinessCommand", source);
        Assert.Contains("RestoreCommand", source);
        Assert.Contains("PreRestore", source);
        Assert.Contains("当前存档目录", source);
    }

    private static RestoreWorkflowStepState Step(
        System.Collections.Generic.IReadOnlyList<RestoreWorkflowStepState> steps,
        string key)
        => steps.Single(step => string.Equals(step.Key, key, StringComparison.Ordinal));

    private static BackupVersionDto Version(string id, RestoreReadinessStatus? status = null)
        => new BackupVersionDto
        {
            BackupId = id,
            CreatedUtc = new DateTime(2026, 9, 19, 1, 0, 0, DateTimeKind.Utc),
            RestoreReadiness = status == null
                ? null
                : new RestoreReadinessDto
                {
                    Status = status.Value,
                    Summary = status == RestoreReadinessStatus.Ready ? "隔离校验通过" : "归档可继续处理",
                    ExpectedFileCount = 4,
                    ActualFileCount = 4,
                    ExpectedTotalSize = 4096,
                    ActualTotalSize = 4096
                }
        };

    private static TaskStatusDto Task(TaskState state, string errorCode, string message)
        => new TaskStatusDto
        {
            TaskType = "Restore",
            State = state,
            ErrorCode = errorCode,
            ErrorMessage = state == TaskState.Failed ? message : string.Empty,
            Message = message
        };
}
