using System;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Threading;
using NativeToggleSwitch = GameSaveCenter.Playnite.Controls.ToggleSwitch;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R21AutomationValueBehaviorTests
{
    [Fact]
    public void NamedControlsExposeActionStateAndRangeValueThroughWpfPeers()
    {
        RunSta(() =>
        {
            var action = new Button { Content = "\uE10B" };
            AutomationProperties.SetName(action, "刷新");

            var selector = new ComboBox
            {
                ItemsSource = new[] { "全部", "失败" },
                SelectedIndex = 0
            };
            AutomationProperties.SetName(selector, "任务状态筛选");

            var toggle = new NativeToggleSwitch { Content = "启用", IsChecked = false };
            AutomationProperties.SetName(toggle, "启用备份策略");

            var progress = new ProgressBar { Minimum = 0, Maximum = 100, Value = 42 };
            AutomationProperties.SetName(progress, "任务完成进度");

            using var host = new PeerHost(action, selector, toggle, progress);
            Assert.Equal("刷新", GetPeer(action).GetName());
            Assert.Equal("任务状态筛选", GetPeer(selector).GetName());
            Assert.Equal("启用备份策略", GetPeer(toggle).GetName());
            Assert.Equal("任务完成进度", GetPeer(progress).GetName());
            Assert.NotEqual(action.Content, GetPeer(action).GetName());

            var togglePattern = Assert.IsAssignableFrom<IToggleProvider>(GetPeer(toggle).GetPattern(PatternInterface.Toggle));
            Assert.Equal(ToggleState.Off, togglePattern.ToggleState);
            toggle.IsChecked = true;
            host.Pump();
            Assert.Equal(ToggleState.On, togglePattern.ToggleState);

            var rangePattern = Assert.IsAssignableFrom<IRangeValueProvider>(GetPeer(progress).GetPattern(PatternInterface.RangeValue));
            Assert.Equal(42, rangePattern.Value);
            Assert.Equal(0, rangePattern.Minimum);
            Assert.Equal(100, rangePattern.Maximum);
        });
    }

    [Fact]
    public void ProductionProgressAndTaskSelectorContractsUseSemanticNames()
    {
        var root = TestRepositoryContext.Root;
        var overview = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));
        var task = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var trainer = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));
        var dashboard = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));
        var maintenance = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        Assert.Contains("AutomationProperties.Name=\"已匹配游戏占比\"", overview);
        Assert.Contains("AutomationProperties.Name=\"需注意游戏占比\"", overview);
        Assert.Contains("AutomationProperties.Name=\"任务完成进度\"", overview);
        Assert.Contains("AutomationProperties.Name=\"任务状态筛选\"", task);
        Assert.Contains("AutomationProperties.Name=\"任务类型筛选\"", task);
        Assert.Contains("AutomationProperties.Name=\"任务游戏筛选\"", task);
        Assert.Contains("Value=\"{Binding ProgressValue, Mode=OneWay}\" AutomationProperties.Name=\"任务进度\" AutomationProperties.HelpText=\"{Binding ProgressDisplay, Mode=OneWay}\"", task);
        Assert.Contains("Value=\"{Binding SelectedTask.ProgressValue, Mode=OneWay}\" AutomationProperties.Name=\"所选任务进度\" AutomationProperties.HelpText=\"{Binding SelectedTask.ProgressDisplay, Mode=OneWay}\"", task);
        Assert.Contains("AutomationProperties.Name=\"修改器下载进度\"", trainer);
        Assert.Contains("AutomationProperties.Name=\"当前操作进度\"", dashboard);
        Assert.Contains("AutomationProperties.Name=\"备份存储占用比例\"", maintenance);
        Assert.Contains("Value=\"{Binding RemoteBackupStageProgress, Mode=OneWay}\" Height=\"6\" Margin=\"0,10,0,0\" AutomationProperties.Name=\"远端备份隔离下载进度\" AutomationProperties.HelpText=\"{Binding RemoteBackupStageProgress, StringFormat={}{0}%}\"", maintenance);
    }

    [Fact]
    public void PolicyToggleAndSelectorPeersExposeSemanticState()
    {
        RunSta(() =>
        {
            var toggles = new[]
            {
                CreateNamedToggle("启用备份策略"),
                CreateNamedToggle("游戏退出后自动备份"),
                CreateNamedToggle("游玩中定期备份"),
                CreateNamedToggle("备份后自动上传云端")
            };
            var selector = new ComboBox
            {
                ItemsSource = new[] { "标准", "严格" },
                SelectedIndex = 0
            };
            AutomationProperties.SetName(selector, "异常保护等级");

            using var host = new PeerHost(toggles[0], toggles[1], toggles[2], toggles[3], selector);
            foreach (var toggle in toggles)
            {
                var peer = GetPeer(toggle);
                Assert.Equal(AutomationProperties.GetName(toggle), peer.GetName());
                var pattern = Assert.IsAssignableFrom<IToggleProvider>(peer.GetPattern(PatternInterface.Toggle));
                Assert.Equal(ToggleState.Off, pattern.ToggleState);
                toggle.IsChecked = true;
                host.Pump();
                Assert.Equal(ToggleState.On, pattern.ToggleState);
            }

            Assert.Equal("异常保护等级", GetPeer(selector).GetName());
            selector.SelectedIndex = 1;
            host.Pump();
            Assert.Equal("严格", selector.SelectedItem);
        });
    }

    [Fact]
    public void SaveCenterPolicyLabelsRemainAttachedToNamedInteractiveControls()
    {
        var root = TestRepositoryContext.Root;
        var save = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));

        Assert.Contains("IsChecked=\"{Binding SelectedGame.Policy.Enabled}\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\" AutomationProperties.Name=\"启用备份策略\"", save);
        Assert.Contains("IsChecked=\"{Binding SelectedGame.Policy.BackupOnGameStop}\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\" AutomationProperties.Name=\"游戏退出后自动备份\"", save);
        Assert.Contains("IsChecked=\"{Binding SelectedGame.Policy.BackupDuringPlay}\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\" AutomationProperties.Name=\"游玩中定期备份\"", save);
        Assert.Contains("IsChecked=\"{Binding SelectedGame.Policy.UploadAfterBackup}\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\" AutomationProperties.Name=\"备份后自动上传云端\"", save);
        Assert.Contains("SelectedValue=\"{Binding SelectedGame.Policy.AnomalyProtectionLevel}\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\" AutomationProperties.Name=\"异常保护等级\"", save);
        Assert.Contains("SelectedItem=\"{Binding SelectedPolicyTemplate}\" Margin=\"0,12,0,0\" ToolTip=\"内置模板不可直接修改；新建副本后可编辑\" AutomationProperties.Name=\"选择策略模板\"", save);
        Assert.Contains("IsChecked=\"{Binding LockSelectedBackup}\" VerticalAlignment=\"Center\" ToolTip=\"锁定并保存后，保留预览会跳过此版本；取消锁定并保存后才会重新按策略评估。\" AutomationProperties.Name=\"锁定所选版本\"", save);
    }

    private static AutomationPeer GetPeer(FrameworkElement element)
        => FrameworkElementAutomationPeer.CreatePeerForElement(element)
            ?? throw new Xunit.Sdk.XunitException(element.GetType().Name + " did not create an AutomationPeer.");

    private static NativeToggleSwitch CreateNamedToggle(string name)
    {
        var toggle = new NativeToggleSwitch { Content = name, IsChecked = false };
        AutomationProperties.SetName(toggle, name);
        return toggle;
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
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private sealed class PeerHost : IDisposable
    {
        private readonly Window window;

        public PeerHost(params UIElement[] children)
        {
            var panel = new StackPanel();
            foreach (var child in children)
                panel.Children.Add(child);

            window = new Window
            {
                Content = panel,
                Width = 640,
                Height = 320,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };
            window.Show();
            Pump();
        }

        public void Pump()
            => Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => window.UpdateLayout()));

        public void Dispose()
        {
            if (window.IsVisible)
                window.Close();
        }
    }
}
