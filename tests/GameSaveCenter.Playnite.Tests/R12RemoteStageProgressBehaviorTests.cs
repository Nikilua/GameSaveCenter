using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;
using GscButton=GameSaveCenter.Playnite.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R11SaveWpf")]
public sealed class R12RemoteStageProgressBehaviorTests
{
    [Fact]
    public void DownloadVerificationAndReadyStatesRemainDistinct()
    {
        var downloading=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="RemoteStage",State=TaskState.Running,ProgressPercent=24,Message="正在下载到隔离区"
        });
        var verifying=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="RemoteStage",State=TaskState.Running,ProgressPercent=70,Message="一致性校验通过，正在确认目标 Ludusavi 版本"
        });
        var ready=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="RemoteStage",State=TaskState.Succeeded,ProgressPercent=100
        });

        Assert.True(downloading.IsActive);
        Assert.True(verifying.IsActive);
        Assert.False(ready.IsActive);
        Assert.Equal(24,downloading.Progress);
        Assert.Equal(70,verifying.Progress);
        Assert.Equal(100,ready.Progress);
        Assert.NotEqual(downloading.StatusText,verifying.StatusText);
        Assert.Contains("等待恢复确认",ready.StatusText);
        Assert.DoesNotContain("恢复完成",ready.StatusText);
    }

    [Fact]
    public void CancellationAndFailureAreTerminalAndDoNotAdvertiseRestore()
    {
        var cancelled=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="RemoteStage",State=TaskState.Cancelled,ProgressPercent=48
        });
        var failed=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="RemoteStage",State=TaskState.Failed,ProgressPercent=60,ErrorMessage="隔离版本不存在"
        });

        Assert.False(cancelled.IsActive);
        Assert.False(failed.IsActive);
        Assert.Contains("隔离区",cancelled.StatusText);
        Assert.Contains("隔离版本不存在",failed.StatusText);
        Assert.DoesNotContain("恢复完成",cancelled.StatusText+failed.StatusText);
    }

    [Fact]
    public void CancellationKeepsWorkerCleanupOutcomeInTheStatusProjection()
    {
        const string cleaned="远端备份下载已取消；隔离区已清理。";
        const string residual="远端备份下载已取消；隔离区清理失败，仍有残留内容需要维护中心复核。";
        var cleanupSucceeded=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="RemoteStage",State=TaskState.Cancelled,Message=cleaned
        });
        var cleanupFailed=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="RemoteStage",State=TaskState.Cancelled,Message=residual
        });

        Assert.False(cleanupSucceeded.IsActive);
        Assert.False(cleanupFailed.IsActive);
        Assert.Equal(cleaned,cleanupSucceeded.StatusText);
        Assert.Equal(residual,cleanupFailed.StatusText);
        Assert.NotEqual(cleanupSucceeded.StatusText,cleanupFailed.StatusText);
        Assert.DoesNotContain("恢复完成",cleanupSucceeded.StatusText+cleanupFailed.StatusText);
    }

    [Fact]
    public void ProductionMaintenancePageShowsActiveProgressAndResidualCancellationButHidesCancelAction()
    {
        RunSta(() =>
        {
            var running=RemoteBackupStageProjection.FromTask(new TaskStatusDto
            {
                TaskType="RemoteStage",State=TaskState.Running,ProgressPercent=43,Message="已下载到隔离区，正在校验完整性"
            });
            var context=new RemoteStageViewContext(running);
            var page=new MaintenanceView { DataContext=context };
            var tabs=Assert.IsType<TabControl>(page.FindName("MaintenanceTabControl"));
            BindingOperations.ClearBinding(tabs,TabControl.SelectedIndexProperty);
            tabs.SelectedIndex=2;
            var window=new Window
            {
                Title="GameSaveCenter remote-stage progress regression",
                Content=page,Width=1440,Height=1000,WindowStyle=WindowStyle.None,
                ShowInTaskbar=false,ShowActivated=false,Opacity=0.01
            };
            // Keep a meaningful UIA name without the unnamed Window peer's native-title fallback.
            System.Windows.Automation.AutomationProperties.SetName(window,window.Title);

            try
            {
                window.Show();
                var windowPeer=Assert.IsType<System.Windows.Automation.Peers.WindowAutomationPeer>(
                    System.Windows.Automation.Peers.UIElementAutomationPeer.CreatePeerForElement(window));
                Assert.Equal(window.Title,windowPeer.GetName());
                PumpLayout(window);
                var progress=Assert.Single(FindVisualChildren<ProgressBar>(page),
                    item=>System.Windows.Automation.AutomationProperties.GetName(item)=="远端备份隔离下载进度");
                var cancel=Assert.Single(FindVisualChildren<GscButton>(page),
                    item=>BindingOperations.GetBinding(item,ButtonBase.CommandProperty)?.Path?.Path==nameof(RemoteStageViewContext.CancelRemoteBackupStageCommand));
                var status=Assert.Single(FindVisualChildren<TextBlock>(page),
                    item=>BindingOperations.GetBinding(item,TextBlock.TextProperty)?.Path?.Path==nameof(RemoteStageViewContext.StagedRemoteBackupStatus));

                Assert.True(progress.IsVisible);
                Assert.Equal(43,progress.Value);
                Assert.True(cancel.IsVisible);
                Assert.True(cancel.IsEnabled);
                Assert.Equal("已下载到隔离区，正在校验完整性",status.Text);

                const string residual="远端备份下载已取消；隔离区清理失败，仍有残留内容需要维护中心复核。";
                var cancelled=RemoteBackupStageProjection.FromTask(new TaskStatusDto
                {
                    TaskType="RemoteStage",State=TaskState.Cancelled,ProgressPercent=43,Message=residual
                });
                context.Apply(cancelled);
                context.CancelRemoteBackupStageCommand.SetCanExecute(false);
                PumpLayout(window);

                Assert.False(progress.IsVisible);
                Assert.False(cancel.IsVisible);
                Assert.False(cancel.IsEnabled);
                Assert.Equal(residual,status.Text);
                Assert.Equal(residual,System.Windows.Automation.AutomationProperties.GetHelpText(status));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void OtherTaskTypesDoNotChangeRemoteStageProjection()
    {
        var projection=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="Restore",State=TaskState.Running,ProgressPercent=60,Message="正在恢复"
        });

        Assert.False(projection.IsRelevant);
        Assert.False(projection.IsActive);
        Assert.Equal(0,projection.Progress);
        Assert.Equal(string.Empty,projection.StatusText);
    }

    private static void RunSta(Action action)
    {
        Exception? failure=null;
        var thread=new Thread(() =>
        {
            try
            {
                action();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch(Exception exception) { failure=exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(failure);
    }

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle,new Action(() => { }));
        window.UpdateLayout();
    }

    private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T:DependencyObject
    {
        for(var index=0;index<VisualTreeHelper.GetChildrenCount(root);index++)
        {
            var child=VisualTreeHelper.GetChild(root,index);
            if(child is T match)yield return match;
            foreach(var descendant in FindVisualChildren<T>(child))yield return descendant;
        }
    }

    private sealed class RemoteStageViewContext : INotifyPropertyChanged
    {
        private bool isRemoteBackupStageActive;
        private int remoteBackupStageProgress;
        private string stagedRemoteBackupStatus=string.Empty;

        public RemoteStageViewContext(RemoteBackupStageProjection projection)
        {
            CancelRemoteBackupStageCommand=new ToggleCommand(true);
            Apply(projection);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public bool IsRemoteBackupStageActive=>isRemoteBackupStageActive;
        public int RemoteBackupStageProgress=>remoteBackupStageProgress;
        public string StagedRemoteBackupStatus=>stagedRemoteBackupStatus;
        public string StagedRemoteBackupStatusFullDisplay=>stagedRemoteBackupStatus;
        public ToggleCommand CancelRemoteBackupStageCommand { get; }

        public void Apply(RemoteBackupStageProjection projection)
        {
            isRemoteBackupStageActive=projection.IsActive;
            remoteBackupStageProgress=projection.Progress;
            stagedRemoteBackupStatus=projection.StatusText;
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(IsRemoteBackupStageActive)));
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(RemoteBackupStageProgress)));
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(StagedRemoteBackupStatus)));
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(StagedRemoteBackupStatusFullDisplay)));
        }
    }

    private sealed class ToggleCommand : System.Windows.Input.ICommand
    {
        private bool canExecute;
        public ToggleCommand(bool canExecute)=>this.canExecute=canExecute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter)=>canExecute;
        public void Execute(object? parameter) { }
        public void SetCanExecute(bool value)
        {
            canExecute=value;
            CanExecuteChanged?.Invoke(this,EventArgs.Empty);
        }
    }
}
