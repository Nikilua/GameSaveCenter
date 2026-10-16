using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R11SaveWpf")]
public sealed class R13CloudTransferStageBehaviorTests
{
    [Fact]
    public void MaintenanceQueueBindsStageAndKeepsGuaranteeSeparate()
    {
        var root = TestRepositoryContext.Root;
        var source = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        Assert.Contains("QueuePhaseDisplay", source);
        Assert.Contains("GuaranteeLevelDisplay", source);
        Assert.Contains("QueueControlDisplay", source);
    }

    [Fact]
    public void ProductionMaintenanceQueueRendersUploadedAndVerifiedAsDifferentStates()
    {
        RunSta(() =>
        {
            var uploaded = new CloudTransferStatusDto
            {
                TransferKey = "Backup:synthetic-uploaded",
                GameName = "合成待校验游戏",
                State = "Uploaded",
                UpdatedUtc = new DateTime(2026, 10, 16, 0, 0, 0, DateTimeKind.Utc)
            };
            var verified = new CloudTransferStatusDto
            {
                TransferKey = "Backup:synthetic-verified",
                GameName = "合成已校验游戏",
                State = "RemoteVerified",
                UpdatedUtc = new DateTime(2026, 10, 16, 0, 1, 0, DateTimeKind.Utc),
                LastSuccessfulVerificationUtc = new DateTime(2026, 10, 16, 0, 1, 0, DateTimeKind.Utc)
            };
            var retry = new CloudTransferStatusDto
            {
                TransferKey = "Backup:synthetic-retry",
                GameName = "合成重试游戏",
                State = "RetryScheduled",
                LastErrorCode = "RCLONE_NETWORK_FAILED",
                LastError = "合成网络中断原因",
                NextAttemptUtc = DateTime.UtcNow.AddMinutes(18)
            };
            var context = new CloudQueueViewContext(uploaded, verified, retry);
            var application = Application.Current ?? new Application();
            if (!application.Resources.Contains("BaseTextBlockStyle"))
                application.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            var page = new GameSaveCenter.Playnite.Views.MaintenanceView { DataContext = context };
            var tabs = Assert.IsType<TabControl>(page.FindName("MaintenanceTabControl"));
            BindingOperations.ClearBinding(tabs, TabControl.SelectedIndexProperty);
            tabs.SelectedItem = Assert.Single(tabs.Items.OfType<TabItem>(),
                item => string.Equals(item.Header as string, "云端队列", StringComparison.Ordinal));
            var window = new Window
            {
                Content = page,
                Width = 1440,
                Height = 1000,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
                Opacity = 0.01
            };

            try
            {
                window.Show();
                PumpLayout(window);
                var grid = Assert.Single(FindVisualChildren<DataGrid>(page),
                    candidate => candidate.Name == "CloudTransferGrid");
                Assert.Equal(3, grid.Items.Count);
                grid.ScrollIntoView(uploaded);
                grid.SelectedItem = uploaded;
                PumpLayout(window);

                var phaseColumn = Assert.IsType<DataGridTextColumn>(grid.Columns.Single(column =>
                    string.Equals(column.Header as string, "阶段", StringComparison.Ordinal)));
                var guaranteeColumn = Assert.IsType<DataGridTextColumn>(grid.Columns.Single(column =>
                    string.Equals(column.Header as string, "保证级别", StringComparison.Ordinal)));
                var uploadedPhaseCell = Assert.IsType<TextBlock>(phaseColumn.GetCellContent(uploaded));
                var uploadedGuaranteeCell = Assert.IsType<TextBlock>(guaranteeColumn.GetCellContent(uploaded));
                var detailPhase = FindInspectorText(page, "SelectedCloudTransfer.QueuePhaseDisplay");
                var detailGuarantee = FindInspectorText(page, "SelectedCloudTransfer.GuaranteeLevelDisplay");

                Assert.Equal("⚠ 等待验证", uploadedPhaseCell.Text);
                Assert.Equal("上传命令成功，尚未远端校验", uploadedGuaranteeCell.Text);
                Assert.Equal("等待验证", detailPhase.Text);
                Assert.Equal("上传命令成功，尚未远端校验", detailGuarantee.Text);
                Assert.DoesNotContain("已验证", uploadedPhaseCell.Text, StringComparison.Ordinal);
                Assert.DoesNotContain("远端校验成功", uploadedGuaranteeCell.Text, StringComparison.Ordinal);

                grid.ScrollIntoView(verified);
                grid.SelectedItem = verified;
                PumpLayout(window);

                var verifiedPhaseCell = Assert.IsType<TextBlock>(phaseColumn.GetCellContent(verified));
                var verifiedGuaranteeCell = Assert.IsType<TextBlock>(guaranteeColumn.GetCellContent(verified));
                Assert.Same(verified, context.SelectedCloudTransfer);
                Assert.Equal("已验证", verifiedPhaseCell.Text);
                Assert.Equal("远端校验成功", verifiedGuaranteeCell.Text);
                Assert.Equal("已验证", detailPhase.Text);
                Assert.Equal("远端校验成功", detailGuarantee.Text);

                grid.ScrollIntoView(retry);
                grid.SelectedItem = retry;
                PumpLayout(window);
                var retryTiming = FindInspectorText(page, "SelectedCloudTransfer.RetryTimingRelativeDisplay");
                var diagnosticExpander = Assert.Single(FindVisualChildren<Expander>(page),
                    expander => string.Equals(expander.Header as string, "原始诊断", StringComparison.Ordinal));
                diagnosticExpander.IsExpanded = true;
                PumpLayout(window);
                var lastError = FindInspectorText(page, "SelectedCloudTransfer.LastError");

                Assert.Equal(retry.RetryTimingRelativeDisplay, retryTiming.Text);
                Assert.Contains("分钟", retryTiming.Text, StringComparison.Ordinal);
                Assert.EndsWith("后", retryTiming.Text, StringComparison.Ordinal);
                Assert.Equal(retry.RetryTimingFullDisplay, retryTiming.ToolTip as string);
                Assert.Equal(retry.RetryTimingFullDisplay,
                    System.Windows.Automation.AutomationProperties.GetHelpText(retryTiming));
                Assert.Equal("合成网络中断原因", lastError.Text);

                // A forward wall-clock adjustment that passes the due time is represented by
                // selecting an already-expired timestamp; reevaluation clamps it to an action,
                // never a negative countdown.
                retry.NextAttemptUtc = DateTime.UtcNow.AddMinutes(-2);
                context.SelectedCloudTransfer = verified;
                PumpLayout(window);
                context.SelectedCloudTransfer = retry;
                grid.SelectedItem = retry;
                PumpLayout(window);

                Assert.Equal("可立即重试", retryTiming.Text);
                Assert.DoesNotContain("负", retryTiming.Text, StringComparison.Ordinal);
                Assert.Equal(retry.RetryTimingFullDisplay, retryTiming.ToolTip as string);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void MaintenanceInspectorUsesBoundedRetryTimingWithoutPerItemTimer()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var viewCodeBehind = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml.cs"));
        var contracts = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "CloudTransferDtos.cs"));

        Assert.Contains("SelectedCloudTransfer.RetryTimingRelativeDisplay", view, StringComparison.Ordinal);
        Assert.Contains("SelectedCloudTransfer.RetryTimingFullDisplay", view, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding SelectedCloudTransfer.RetryTimingFullDisplay}\"", view, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectedCloudTransfer.NextAttemptLocal, StringFormat", view, StringComparison.Ordinal);
        Assert.DoesNotContain("DispatcherTimer", viewCodeBehind, StringComparison.Ordinal);
        Assert.DoesNotContain("Timer", contracts, StringComparison.Ordinal);
    }

    [Fact]
    public void MaintenanceInspectorShowsRemoteEvidenceAndKeepsDiagnosticCopyRedacted()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

        Assert.Contains("SelectedCloudTransfer.RemoteObjectDisplay", view, StringComparison.Ordinal);
        Assert.Contains("SelectedCloudTransfer.SourceDeviceDisplay", view, StringComparison.Ordinal);
        Assert.Contains("SelectedCloudTransfer.LastAttemptRelativeDisplay", view, StringComparison.Ordinal);
        Assert.Contains("SelectedCloudTransfer.LastSuccessfulVerificationRelativeDisplay", view, StringComparison.Ordinal);
        Assert.Contains("SelectedCloudTransfer.LastAttemptFullDisplay", view, StringComparison.Ordinal);
        Assert.Contains("SelectedCloudTransfer.LastSuccessfulVerificationFullDisplay", view, StringComparison.Ordinal);
        Assert.Contains("CopyTextWithRetryAsync(DiagnosticSummary", viewModel, StringComparison.Ordinal);
        Assert.Contains("text = ClipboardValueSanitizer.Sanitize", viewModel, StringComparison.Ordinal);

        var copied = GameSaveCenter.Playnite.Infrastructure.ClipboardValueSanitizer.Sanitize(
            "remote=https://user:secret@example.invalid/root?token=query-secret Authorization: Bearer bearer-secret");
        Assert.DoesNotContain("secret", copied, StringComparison.Ordinal);
        Assert.DoesNotContain("query-secret", copied, StringComparison.Ordinal);
        Assert.DoesNotContain("bearer-secret", copied, StringComparison.Ordinal);
        Assert.Contains("[已隐藏]", copied, StringComparison.Ordinal);
    }

    [Fact]
    public void MaintenanceInspectorExplainsBoundedOfflineRecovery()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var retry = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Services", "CloudRetryService.cs"));

        Assert.Contains("SelectedCloudTransfer.NetworkRecoveryDisplay", view, StringComparison.Ordinal);
        Assert.Contains("PollInterval = TimeSpan.FromSeconds(30)", retry, StringComparison.Ordinal);
        Assert.Contains(".Take(10)", retry, StringComparison.Ordinal);
        Assert.DoesNotContain("ShowTaskNotification", retry, StringComparison.Ordinal);
    }

    [Fact]
    public void MaintenanceInspectorSeparatesKnownFailureHelpFromRawDiagnostics()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var contracts = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "CloudFailureExplanation.cs"));
        var classifier = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Infrastructure", "RcloneFailureClassifier.cs"));

        Assert.Contains("SelectedCloudTransfer.HasRecognizedFailure", view, StringComparison.Ordinal);
        Assert.Contains("SelectedCloudTransfer.FailureNextStepDisplay", view, StringComparison.Ordinal);
        Assert.Contains("Header=\"原始诊断\"", view, StringComparison.Ordinal);
        Assert.Contains("RCLONE_NO_SPACE", contracts, StringComparison.Ordinal);
        Assert.Contains("RCLONE_RATE_LIMITED", classifier, StringComparison.Ordinal);
    }

    [Fact]
    public void MaintenanceQueueKeepsFilterScopeAndPagingSelectionContracts()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var transfers = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.CloudTransfers.cs"));
        var worker = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Services", "CloudTransferStateService.cs"));

        Assert.Contains("CloudTransferGameFilter", view, StringComparison.Ordinal);
        Assert.Contains("CloudTransferSourceDeviceFilter", view, StringComparison.Ordinal);
        Assert.Contains("CloudTransferTimeFilterOptions", view, StringComparison.Ordinal);
        Assert.Contains("当前筛选", viewModel, StringComparison.Ordinal);
        Assert.Contains("existingKeys", transfers, StringComparison.Ordinal);
        Assert.Contains("GlobalTotalCount = globalAggregate.TotalCount", worker, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Failed", true)]
    [InlineData("RetryScheduled", true)]
    [InlineData("Transferring", false)]
    [InlineData("Uploaded", false)]
    [InlineData("RemoteVerified", false)]
    public void ManualRetryScopeOnlyAcceptsUnfinishedSelectedTransfers(string state, bool canRetry)
    {
        var transfer = new CloudTransferStatusDto { State = state };

        Assert.Equal(canRetry, transfer.CanManuallyRetry);
        if (canRetry)
        {
            Assert.Contains("仅当前选中项", transfer.ManualRetryScopeDisplay);
            Assert.Contains("不重新执行本地备份", transfer.ManualRetryScopeDisplay);
        }
        else
        {
            Assert.Contains("不会重复提交", transfer.ManualRetryScopeDisplay);
        }
    }

    [Fact]
    public async Task ManualRetryCommandGateBlocksSecondClickWhileSubmissionIsBusy()
    {
        var transfer = new CloudTransferStatusDto { State = "RetryScheduled" };
        var coordinator = new BusyOperationCoordinator();
        var actionCompletion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var busy = false;
        var submissions = 0;
        Task<bool>? firstOperation = null;
        var command = new RelayCommand(
            _ =>
            {
                var attempt = coordinator.TryRunAsync(
                    () => Task.CompletedTask,
                    () =>
                    {
                        submissions++;
                        return actionCompletion.Task;
                    },
                    value => busy = value,
                    _ => { },
                    _ => { },
                    () => { });
                firstOperation ??= attempt;
            },
            _ => !busy && transfer.CanManuallyRetry);

        Assert.True(command.CanExecute(null));
        command.Execute(null);
        Assert.NotNull(firstOperation);
        Assert.Equal(1, submissions);
        Assert.True(busy);

        Assert.False(command.CanExecute(null));
        command.Execute(null); // Exercise the coordinator too, for a race after CanExecute.
        Assert.Equal(1, submissions);

        actionCompletion.SetResult(null);
        Assert.True(await firstOperation!);
        Assert.False(busy);

        transfer.State = "Uploaded";
        Assert.False(command.CanExecute(null));
    }

    [Fact]
    public void SettingsExplainPauseRecoveryAndWindowChangeBoundary()
    {
        var root = TestRepositoryContext.Root;
        var source = File.ReadAllText(Path.Combine(
            root,
            "src",
            "GameSaveCenter.Playnite",
            "Settings",
            "GameSaveCenterSettingsView.xaml"));

        Assert.Contains("暂停云端自动重试队列", source);
        Assert.Contains("恢复开关后会继续处理已保存队列", source);
        Assert.Contains("下一轮 Worker 检查时生效", source);
        Assert.Contains("已开始的上传不会被取消", source);
    }

    private static TextBlock FindInspectorText(DependencyObject root, string bindingPath)
        => Assert.Single(FindVisualChildren<TextBlock>(root), textBlock =>
            BindingOperations.GetBinding(textBlock, TextBlock.TextProperty)?.Path?.Path == bindingPath);

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
            catch (Exception exception) { failure = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(failure);
    }

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        window.UpdateLayout();
    }

    private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, index);
            if (child is T match) yield return match;
            foreach (var descendant in FindVisualChildren<T>(child)) yield return descendant;
        }
    }

    private sealed class CloudQueueViewContext : INotifyPropertyChanged
    {
        private CloudTransferStatusDto selectedCloudTransfer;

        public CloudQueueViewContext(params CloudTransferStatusDto[] transfers)
        {
            CloudTransferItems = new ObservableCollection<CloudTransferStatusDto>(transfers);
            selectedCloudTransfer = transfers[0];
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<CloudTransferStatusDto> CloudTransferItems { get; }

        public CloudTransferStatusDto SelectedCloudTransfer
        {
            get => selectedCloudTransfer;
            set
            {
                if (ReferenceEquals(selectedCloudTransfer, value)) return;
                selectedCloudTransfer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCloudTransfer)));
            }
        }
    }
}
