using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R11SaveWpf")]
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
    public void PreRestoreFailureStopsDangerousExecutionAndExplainsProtectionStage()
    {
        var task = Task(TaskState.Failed, "RESTORE_PRERESTORE_FAILED", "恢复前保护快照失败，已中止危险恢复");
        var steps = RestoreWorkflowProgress.Build(Version("pre-restore-failed", RestoreReadinessStatus.Ready), false, false, false, task, string.Empty, string.Empty);

        var execution = Step(steps, "execution");
        Assert.Equal(RestoreWorkflowStepStatus.Failed, execution.Status);
        Assert.False(execution.IsCompleted);
        Assert.Contains("保护备份阶段失败", execution.Detail);
        Assert.Contains("危险恢复已中止", execution.Detail);
        Assert.Contains("执行结果", RestoreWorkflowProgress.BuildSummary(steps));
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
        Assert.Contains("ResolutionDisplay", source);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding ResolutionDisplay}\"", source);
    }

    [Fact]
    public void SaveCenterRendersBoundFailureAndActiveStageDetails()
    {
        RunSta(() =>
        {
            var failureSteps = RestoreWorkflowProgress.Build(
                Version("running", RestoreReadinessStatus.Ready),
                false,
                false,
                false,
                Task(TaskState.Failed, "RESTORE_GAME_RUNNING", "游戏仍在运行"),
                string.Empty,
                string.Empty);
            var selectedBackup = Version("running", RestoreReadinessStatus.Ready);
            var state = new RestoreWorkflowPageState(selectedBackup, failureSteps, RestoreWorkflowProgress.BuildSummary(failureSteps));
            var view = new SaveCenterView { DataContext = state, Width = 1040, Height = 900 };
            var window = new Window
            {
                Width = 1040,
                Height = 900,
                Content = view,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            try
            {
                window.Show();
                PumpLayout(window);

                var summary = FindVisualChildren<TextBlock>(view)
                    .Single(textBlock => textBlock.Text.StartsWith("恢复流程停在“目标核对”", StringComparison.Ordinal));
                var workflow = FindVisualChildren<ItemsControl>(view)
                    .Single(itemsControl => ReferenceEquals(itemsControl.ItemsSource, state.RestoreWorkflowSteps));
                Assert.Equal(4, workflow.Items.Count);
                Assert.Contains("恢复流程停在“目标核对”", summary.Text);

                var targetTexts = RowTexts(workflow, 2);
                Assert.Contains("目标核对", targetTexts);
                Assert.Contains("失败", targetTexts);
                Assert.Contains("错误码：RESTORE_GAME_RUNNING；游戏仍在运行", targetTexts);

                var executionTexts = RowTexts(workflow, 3);
                Assert.Contains("执行结果", executionTexts);
                Assert.Contains("待处理", executionTexts);
                Assert.Contains(executionTexts, text => text.IndexOf("尚未进入写入当前存档的阶段", StringComparison.Ordinal) >= 0);

                var activeSteps = RestoreWorkflowProgress.Build(
                    Version("active", RestoreReadinessStatus.Warning),
                    false,
                    true,
                    true,
                    null,
                    string.Empty,
                    string.Empty);
                state.Replace(activeSteps, RestoreWorkflowProgress.BuildSummary(activeSteps));
                PumpLayout(window);

                Assert.Contains("恢复流程正在“目标核对”", summary.Text);
                Assert.Contains("已完成", RowTexts(workflow, 0));
                Assert.Contains("需注意", RowTexts(workflow, 1));
                Assert.Contains("进行中", RowTexts(workflow, 2));
                var activeExecutionTexts = RowTexts(workflow, 3);
                Assert.Contains("进行中", activeExecutionTexts);
                Assert.Contains(activeExecutionTexts, text => text.IndexOf("PreRestore", StringComparison.Ordinal) >= 0);
            }
            finally
            {
                window.Close();
            }
        });
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

    private static string[] RowTexts(ItemsControl workflow, int index)
    {
        var container = Assert.IsAssignableFrom<FrameworkElement>(workflow.ItemContainerGenerator.ContainerFromIndex(index));
        return FindVisualChildren<TextBlock>(container).Select(textBlock => textBlock.Text).ToArray();
    }

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        window.UpdateLayout();
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) yield return match;
            foreach (var descendant in FindVisualChildren<T>(child)) yield return descendant;
        }
    }

    private sealed class RestoreWorkflowPageState : INotifyPropertyChanged
    {
        private string restoreWorkflowSummary = string.Empty;

        public RestoreWorkflowPageState(
            BackupVersionDto selectedBackup,
            IReadOnlyList<RestoreWorkflowStepState> steps,
            string summary)
        {
            Backups.Add(selectedBackup);
            SelectedBackup = selectedBackup;
            RestoreWorkflowSummary = summary;
            ReplaceSteps(steps);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<BackupVersionDto> Backups { get; } = new();
        public ObservableCollection<RestoreWorkflowStepState> RestoreWorkflowSteps { get; } = new();
        public int SaveTabIndex { get; set; }
        public BackupVersionDto? SelectedBackup { get; set; }
        public string RestoreWorkflowSummary
        {
            get => restoreWorkflowSummary;
            private set
            {
                restoreWorkflowSummary = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RestoreWorkflowSummary)));
            }
        }

        public void Replace(IReadOnlyList<RestoreWorkflowStepState> steps, string summary)
        {
            ReplaceSteps(steps);
            RestoreWorkflowSummary = summary;
        }

        private void ReplaceSteps(IReadOnlyList<RestoreWorkflowStepState> steps)
        {
            RestoreWorkflowSteps.Clear();
            foreach (var step in steps) RestoreWorkflowSteps.Add(step);
        }
    }
}
