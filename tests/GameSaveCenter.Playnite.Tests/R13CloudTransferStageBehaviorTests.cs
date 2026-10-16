using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
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
                AttemptCount = 2,
                NextAttemptUtc = DateTime.UtcNow.AddMinutes(18)
            };
            var recovering = new CloudTransferStatusDto
            {
                TransferKey = "Backup:synthetic-recovering",
                GameName = "合成恢复中游戏",
                State = "Transferring",
                LastErrorCode = "RCLONE_TRANSFER_INCOMPLETE",
                LastError = "合成网络中断后恢复"
            };
            var authentication = new CloudTransferStatusDto
            {
                TransferKey = "Backup:synthetic-authentication",
                GameName = "合成认证失败游戏",
                State = "RetryScheduled",
                LastErrorCode = "RCLONE_AUTH_FAILED",
                LastError = "合成认证失败"
            };
            var context = new CloudQueueViewContext(uploaded, verified, retry, recovering, authentication)
            {
                CloudTransferViewSummary = new CloudTransferSummaryDto { TotalCount = 5, GlobalTotalCount = 8, LoadedCount = 5 }
            };
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
                Assert.Equal(5, grid.Items.Count);

                var stateFilter = Assert.Single(FindVisualChildren<ComboBox>(page), control =>
                    System.Windows.Automation.AutomationProperties.GetName(control) == "云端队列状态筛选");
                var kindFilter = Assert.Single(FindVisualChildren<ComboBox>(page), control =>
                    System.Windows.Automation.AutomationProperties.GetName(control) == "云端队列类型筛选");
                var timeFilter = Assert.Single(FindVisualChildren<ComboBox>(page), control =>
                    System.Windows.Automation.AutomationProperties.GetName(control) == "云端队列时间筛选");
                var gameFilter = Assert.Single(FindVisualChildren<TextBox>(page), control =>
                    System.Windows.Automation.AutomationProperties.GetName(control) == "云端队列游戏筛选");
                var deviceFilter = Assert.Single(FindVisualChildren<TextBox>(page), control =>
                    System.Windows.Automation.AutomationProperties.GetName(control) == "云端队列来源设备筛选");
                stateFilter.SelectedValue = "RetryScheduled";
                kindFilter.SelectedValue = "Backup";
                timeFilter.SelectedValue = "7d";
                gameFilter.Text = "合成筛选游戏";
                deviceFilter.Text = "合成设备";
                gameFilter.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
                deviceFilter.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
                PumpLayout(window);

                Assert.Equal("RetryScheduled", context.CloudTransferStateFilter);
                Assert.Equal("Backup", context.CloudTransferKindFilter);
                Assert.Equal("7d", context.CloudTransferTimeFilter);
                Assert.Equal("合成筛选游戏", context.CloudTransferGameFilter);
                Assert.Equal("合成设备", context.CloudTransferSourceDeviceFilter);
                Assert.Contains(FindVisualChildren<TextBlock>(page), label =>
                    label.Text == "当前筛选 5/8 项 · 已加载全部 5 项");
                Assert.Equal("当前筛选 5/8 项",
                    DashboardViewModel.FormatCloudTransferScopeSummary(true, context.CloudTransferViewSummary));
                Assert.Equal("全局 8 项",
                    DashboardViewModel.FormatCloudTransferScopeSummary(false, context.CloudTransferViewSummary));
                Assert.Equal("当前筛选 0/8 项", DashboardViewModel.FormatCloudTransferScopeSummary(
                    true, new CloudTransferSummaryDto { TotalCount = 0, GlobalTotalCount = 8 }));

                var unknownFailure = new CloudTransferStatusDto
                {
                    TransferKey = "Backup:synthetic-unknown",
                    GameName = "合成未知错误游戏",
                    State = "Failed",
                    LastErrorCode = "RCLONE_PROVIDER_NEW_FAILURE",
                    LastError = "合成提供方返回了未分类错误"
                };
                context.CloudTransferItems.Add(unknownFailure);
                PumpLayout(window);

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
                var networkRecovery = FindInspectorText(page, "SelectedCloudTransfer.NetworkRecoveryDisplay");
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
                Assert.Equal(retry.NetworkRecoveryDisplay, networkRecovery.Text);
                Assert.Contains("等待网络恢复", networkRecovery.Text, StringComparison.Ordinal);
                Assert.Contains("按退避时间重试", networkRecovery.Text, StringComparison.Ordinal);
                Assert.Contains("已用 2/6 次自动重试", networkRecovery.Text, StringComparison.Ordinal);
                Assert.Contains("本轮最多 10 项", networkRecovery.Text, StringComparison.Ordinal);

                grid.ScrollIntoView(recovering);
                grid.SelectedItem = recovering;
                PumpLayout(window);
                Assert.Same(recovering, context.SelectedCloudTransfer);
                Assert.Equal(recovering.NetworkRecoveryDisplay, networkRecovery.Text);
                Assert.Contains("网络已恢复", networkRecovery.Text, StringComparison.Ordinal);
                Assert.Contains("按批次上传中", networkRecovery.Text, StringComparison.Ordinal);

                grid.ScrollIntoView(authentication);
                grid.SelectedItem = authentication;
                PumpLayout(window);
                Assert.Same(authentication, context.SelectedCloudTransfer);
                Assert.Equal(string.Empty, authentication.NetworkRecoveryDisplay);
                Assert.Equal(string.Empty, networkRecovery.Text);

                var failureCategory = FindInspectorText(page, "SelectedCloudTransfer.FailureCategoryDisplay");
                var failureNextStep = FindInspectorText(page, "SelectedCloudTransfer.FailureNextStepDisplay");
                var failureHelpPanel = FindVisualAncestor<Border>(failureNextStep);
                Assert.NotNull(failureHelpPanel);
                Assert.Equal(Visibility.Visible, failureHelpPanel!.Visibility);
                Assert.Equal("认证失败", failureCategory.Text);
                Assert.Equal("下一步：前往设置检查云端凭据并重新验证远端，然后再重试。", failureNextStep.Text);

                var rawCode = FindInspectorText(page, "SelectedCloudTransfer.LastErrorCode");
                var rawError = FindInspectorText(page, "SelectedCloudTransfer.LastError");
                var expanderPeer = System.Windows.Automation.Peers.UIElementAutomationPeer.CreatePeerForElement(diagnosticExpander);
                Assert.NotNull(expanderPeer);
                var disclosure = Assert.IsAssignableFrom<System.Windows.Automation.Provider.IExpandCollapseProvider>(
                    expanderPeer!.GetPattern(System.Windows.Automation.Peers.PatternInterface.ExpandCollapse));
                disclosure.Collapse();
                PumpLayout(window);
                Assert.False(diagnosticExpander.IsExpanded);
                Assert.False(rawCode.IsVisible);

                disclosure.Expand();
                PumpLayout(window);
                Assert.True(diagnosticExpander.IsExpanded);
                Assert.Equal("RCLONE_AUTH_FAILED", rawCode.Text);
                Assert.Equal("合成认证失败", rawError.Text);
                Assert.True(rawCode.IsVisible);

                grid.ScrollIntoView(unknownFailure);
                grid.SelectedItem = unknownFailure;
                PumpLayout(window);

                Assert.Same(unknownFailure, context.SelectedCloudTransfer);
                Assert.Equal(Visibility.Collapsed, failureHelpPanel.Visibility);
                Assert.Equal(string.Empty, failureCategory.Text);
                Assert.Equal(string.Empty, failureNextStep.Text);
                Assert.Equal("RCLONE_PROVIDER_NEW_FAILURE", rawCode.Text);
                Assert.Equal("合成提供方返回了未分类错误", rawError.Text);
                Assert.True(rawCode.IsVisible);

                disclosure.Collapse();
                PumpLayout(window);
                Assert.False(diagnosticExpander.IsExpanded);
                Assert.False(rawCode.IsVisible);

                grid.ScrollIntoView(retry);
                grid.SelectedItem = retry;
                PumpLayout(window);
                Assert.Same(retry, context.SelectedCloudTransfer);
                Assert.Equal(retry.NetworkRecoveryDisplay, networkRecovery.Text);

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
    public void MaintenanceInspectorFailureHelpSourceHooksRemainWired()
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

    [Fact]
    public void CloudTransferPageAppendSkipsOverlappingKeysAndAvoidsEmptyReset()
    {
        var existing = new CloudTransferStatusDto { TransferKey = "Backup:existing", State = "Uploaded" };
        var target = new BatchObservableCollection<CloudTransferStatusDto>(new[] { existing });
        var changes = new List<NotifyCollectionChangedAction>();
        target.CollectionChanged += (_, args) => changes.Add(args.Action);

        var appended = DashboardViewModel.AppendCloudTransferPageItems(target, new[]
        {
            new CloudTransferStatusDto { TransferKey = "backup:EXISTING", State = "Failed" },
            new CloudTransferStatusDto { TransferKey = "Media:new" },
            new CloudTransferStatusDto { TransferKey = "media:NEW" }
        });

        Assert.Equal(1, appended);
        Assert.Equal(2, target.Count);
        Assert.Same(existing, target[0]);
        Assert.Equal("Media:new", target[1].TransferKey);
        Assert.Equal(new[] { NotifyCollectionChangedAction.Reset }, changes);

        changes.Clear();
        var duplicateOnly = DashboardViewModel.AppendCloudTransferPageItems(target, new[]
        {
            new CloudTransferStatusDto { TransferKey = "MEDIA:NEW" },
            new CloudTransferStatusDto { TransferKey = "backup:existing" }
        });

        Assert.Equal(0, duplicateOnly);
        Assert.Equal(2, target.Count);
        Assert.Empty(changes);
    }

    [Fact]
    public void CloudTransferRefreshRestoresStableSelectionOrDefersItUntilItsPageLoads()
    {
        var refreshed = new CloudTransferStatusDto { TransferKey = "Backup:selected" };
        var currentPage = new List<CloudTransferStatusDto>
        {
            new() { TransferKey = "Backup:first" },
            refreshed
        };

        var restored = DashboardViewModel.ResolveCloudTransferPageSelection(
            currentPage, "backup:SELECTED", previousSelectedIndex: 0, hasMore: false, out var shouldLoadPending);

        Assert.Same(refreshed, restored);
        Assert.False(shouldLoadPending);

        var firstPage = new List<CloudTransferStatusDto>
        {
            new() { TransferKey = "Backup:first" }
        };
        restored = DashboardViewModel.ResolveCloudTransferPageSelection(
            firstPage, "Backup:later", previousSelectedIndex: 4, hasMore: true, out shouldLoadPending);

        Assert.Null(restored);
        Assert.True(shouldLoadPending);

        restored = DashboardViewModel.ResolveCloudTransferPageSelection(
            firstPage, "Backup:removed", previousSelectedIndex: 4, hasMore: false, out shouldLoadPending);

        Assert.Same(firstPage[0], restored);
        Assert.False(shouldLoadPending);
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

    private static T? FindVisualAncestor<T>(DependencyObject descendant)
        where T : DependencyObject
    {
        for (DependencyObject? current = System.Windows.Media.VisualTreeHelper.GetParent(descendant);
             current != null;
             current = System.Windows.Media.VisualTreeHelper.GetParent(current))
        {
            if (current is T match) return match;
        }

        return null;
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
        private string cloudTransferStateFilter = string.Empty;
        private string cloudTransferKindFilter = string.Empty;
        private string cloudTransferGameFilter = string.Empty;
        private string cloudTransferSourceDeviceFilter = string.Empty;
        private string cloudTransferTimeFilter = string.Empty;

        public CloudQueueViewContext(params CloudTransferStatusDto[] transfers)
        {
            CloudTransferItems = new ObservableCollection<CloudTransferStatusDto>(transfers);
            selectedCloudTransfer = transfers[0];
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<CloudTransferStatusDto> CloudTransferItems { get; }
        public IReadOnlyList<CloudTransferFilterOption> CloudTransferStateOptions { get; } = new[]
        {
            new CloudTransferFilterOption(string.Empty, "全部状态"),
            new CloudTransferFilterOption("RetryScheduled", "下次尝试")
        };
        public IReadOnlyList<CloudTransferFilterOption> CloudTransferKindOptions { get; } = new[]
        {
            new CloudTransferFilterOption(string.Empty, "全部类型"),
            new CloudTransferFilterOption("Backup", "备份")
        };
        public IReadOnlyList<CloudTransferFilterOption> CloudTransferTimeFilterOptions { get; } = new[]
        {
            new CloudTransferFilterOption(string.Empty, "全部时间"),
            new CloudTransferFilterOption("7d", "最近 7 天")
        };
        public CloudTransferSummaryDto CloudTransferViewSummary { get; set; } = new();
        public bool CloudTransferHasActiveFilters
            => !string.IsNullOrWhiteSpace(CloudTransferStateFilter)
               || !string.IsNullOrWhiteSpace(CloudTransferKindFilter)
               || !string.IsNullOrWhiteSpace(CloudTransferGameFilter)
               || !string.IsNullOrWhiteSpace(CloudTransferSourceDeviceFilter)
               || !string.IsNullOrWhiteSpace(CloudTransferTimeFilter);
        public string CloudTransferLoadedSummary
            => $"{DashboardViewModel.FormatCloudTransferScopeSummary(CloudTransferHasActiveFilters, CloudTransferViewSummary)} · 已加载全部 {CloudTransferItems.Count} 项";

        public string CloudTransferStateFilter
        {
            get => cloudTransferStateFilter;
            set => SetFilter(ref cloudTransferStateFilter, value ?? string.Empty, nameof(CloudTransferStateFilter));
        }

        public string CloudTransferKindFilter
        {
            get => cloudTransferKindFilter;
            set => SetFilter(ref cloudTransferKindFilter, value ?? string.Empty, nameof(CloudTransferKindFilter));
        }

        public string CloudTransferGameFilter
        {
            get => cloudTransferGameFilter;
            set => SetFilter(ref cloudTransferGameFilter, value ?? string.Empty, nameof(CloudTransferGameFilter));
        }

        public string CloudTransferSourceDeviceFilter
        {
            get => cloudTransferSourceDeviceFilter;
            set => SetFilter(ref cloudTransferSourceDeviceFilter, value ?? string.Empty, nameof(CloudTransferSourceDeviceFilter));
        }

        public string CloudTransferTimeFilter
        {
            get => cloudTransferTimeFilter;
            set => SetFilter(ref cloudTransferTimeFilter, value ?? string.Empty, nameof(CloudTransferTimeFilter));
        }

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

        private void SetFilter(ref string field, string value, string propertyName)
        {
            if (string.Equals(field, value, StringComparison.Ordinal)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CloudTransferLoadedSummary)));
        }
    }
}
