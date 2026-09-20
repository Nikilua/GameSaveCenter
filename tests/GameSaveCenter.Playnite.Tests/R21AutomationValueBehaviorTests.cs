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
        var media = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));

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
        Assert.Contains("AutomationProperties.Name=\"按状态筛选游戏\"", dashboard);
        Assert.Contains("AutomationProperties.Name=\"按平台筛选游戏\"", dashboard);
        Assert.Contains("AutomationProperties.Name=\"排序游戏\"", dashboard);
        Assert.Contains("AutomationProperties.Name=\"启用当前游戏自动任务\"", dashboard);
        Assert.Contains("AutomationProperties.Name=\"保存当前游戏策略\"", dashboard);
        Assert.Contains("AutomationProperties.Name=\"备份存储占用比例\"", maintenance);
        Assert.Contains("Value=\"{Binding RemoteBackupStageProgress, Mode=OneWay}\" Height=\"6\" Margin=\"0,10,0,0\" AutomationProperties.Name=\"远端备份隔离下载进度\" AutomationProperties.HelpText=\"{Binding RemoteBackupStageProgress, StringFormat={}{0}%}\"", maintenance);
        Assert.Contains("SelectedItem=\"{Binding InboxTargetGame}\" ToolTip=\"批量归类目标游戏；显示名称、平台和 Playnite ID\" ItemTemplate=\"{StaticResource MediaGameTargetTemplate}\" Margin=\"0,0,0,8\" AutomationProperties.Name=\"媒体收件箱归类目标游戏\"", media);
        Assert.Contains("ItemsSource=\"{Binding MediaFilterOptions}\" SelectedItem=\"{Binding MediaFilter, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, TargetNullValue=全部, FallbackValue=全部}\" ToolTip=\"媒体类型筛选\" AutomationProperties.Name=\"媒体类型筛选\"", media);
        Assert.Contains("SelectedItem=\"{Binding MediaTargetGame}\" ToolTip=\"重新归类目标；显示名称、平台和 Playnite ID\" ItemTemplate=\"{StaticResource MediaGameTargetTemplate}\" Margin=\"0,0,0,8\" AutomationProperties.Name=\"重新归类目标游戏\"", media);
    }

    [Fact]
    public void TrainerImportSelectionAndActionsExposeSemanticState()
    {
        var root = TestRepositoryContext.Root;
        var trainer = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));

        Assert.Equal(2, CountOccurrences(trainer, "AutomationProperties.Name=\"选择待导入修改器主程序\""));
        Assert.Equal(2, CountOccurrences(trainer, "AutomationProperties.Name=\"确认导入游戏工具\""));
        Assert.Equal(2, CountOccurrences(trainer, "AutomationProperties.Name=\"取消导入游戏工具\""));

        RunSta(() =>
        {
            var selector = new ComboBox
            {
                ItemsSource = new[] { "工具主程序 A", "工具主程序 B" },
                SelectedIndex = 0
            };
            AutomationProperties.SetName(selector, "选择待导入修改器主程序");
            var confirm = new Button { Content = "确认导入" };
            AutomationProperties.SetName(confirm, "确认导入游戏工具");
            var cancel = new Button { Content = "取消" };
            AutomationProperties.SetName(cancel, "取消导入游戏工具");

            using var host = new PeerHost(selector, confirm, cancel);
            Assert.Equal("选择待导入修改器主程序", GetPeer(selector).GetName());
            Assert.Equal("确认导入游戏工具", GetPeer(confirm).GetName());
            Assert.Equal("取消导入游戏工具", GetPeer(cancel).GetName());
            selector.SelectedIndex = 1;
            host.Pump();
            Assert.Equal("工具主程序 B", selector.SelectedItem);
        });
    }

    [Fact]
    public void TrainerToolSettingsExposeSemanticSelectorAndToggleState()
    {
        var root = TestRepositoryContext.Root;
        var trainer = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));
        foreach (var name in new[]
        {
            "工具版本",
            "已有实例时处理方式",
            "工具风险类别",
            "启用所选工具",
            "随游戏启动所选工具",
            "退出游戏后关闭所选工具",
            "管理员权限启动所选工具"
        })
        {
            Assert.Equal(1, CountOccurrences(trainer, "AutomationProperties.Name=\"" + name + "\""));
        }

        RunSta(() =>
        {
            var version = CreateNamedSelector("工具版本", "版本 1", "版本 2");
            var running = CreateNamedSelector("已有实例时处理方式", "忽略", "重用");
            var risk = CreateNamedSelector("工具风险类别", "未知", "通用工具");
            var toggles = new[]
            {
                CreateNamedToggle("启用所选工具"),
                CreateNamedToggle("随游戏启动所选工具"),
                CreateNamedToggle("退出游戏后关闭所选工具"),
                CreateNamedToggle("管理员权限启动所选工具")
            };

            using var host = new PeerHost(version, running, risk, toggles[0], toggles[1], toggles[2], toggles[3]);
            Assert.Equal("工具版本", GetPeer(version).GetName());
            Assert.Equal("已有实例时处理方式", GetPeer(running).GetName());
            Assert.Equal("工具风险类别", GetPeer(risk).GetName());
            foreach (var toggle in toggles)
            {
                var pattern = Assert.IsAssignableFrom<IToggleProvider>(GetPeer(toggle).GetPattern(PatternInterface.Toggle));
                Assert.Equal(ToggleState.Off, pattern.ToggleState);
                toggle.IsChecked = true;
                host.Pump();
                Assert.Equal(ToggleState.On, pattern.ToggleState);
            }

            version.SelectedIndex = 1;
            host.Pump();
            Assert.Equal("版本 2", version.SelectedItem);
        });
    }

    [Fact]
    public void MediaCenterSelectorsExposeSemanticState()
    {
        RunSta(() =>
        {
            var inboxTarget = CreateNamedSelector("媒体收件箱归类目标游戏", "游戏 A", "游戏 B");
            var mediaFilter = CreateNamedSelector("媒体类型筛选", "全部", "截图");
            var reassignTarget = CreateNamedSelector("重新归类目标游戏", "游戏 A", "游戏 C");

            using var host = new PeerHost(inboxTarget, mediaFilter, reassignTarget);
            Assert.Equal("媒体收件箱归类目标游戏", GetPeer(inboxTarget).GetName());
            Assert.Equal("媒体类型筛选", GetPeer(mediaFilter).GetName());
            Assert.Equal("重新归类目标游戏", GetPeer(reassignTarget).GetName());

            inboxTarget.SelectedIndex = 1;
            mediaFilter.SelectedIndex = 1;
            reassignTarget.SelectedIndex = 1;
            host.Pump();
            Assert.Equal("游戏 B", inboxTarget.SelectedItem);
            Assert.Equal("截图", mediaFilter.SelectedItem);
            Assert.Equal("游戏 C", reassignTarget.SelectedItem);
        });
    }

    [Fact]
    public void DashboardPickerAndPolicyControlsExposeSemanticState()
    {
        RunSta(() =>
        {
            var status = CreateNamedSelector("按状态筛选游戏", "全部", "需注意");
            var platform = CreateNamedSelector("按平台筛选游戏", "全部", "PC");
            var sort = CreateNamedSelector("排序游戏", "名称", "最近游玩");
            var toggles = new[]
            {
                CreateNamedToggle("启用当前游戏自动任务"),
                CreateNamedToggle("当前游戏退出后备份"),
                CreateNamedToggle("当前游戏游玩中备份"),
                CreateNamedToggle("当前游戏退出后归档媒体"),
                CreateNamedToggle("当前游戏游玩中归档媒体"),
                CreateNamedToggle("当前游戏上传云端")
            };
            var save = new Button { Content = "保存策略" };
            AutomationProperties.SetName(save, "保存当前游戏策略");

            using var host = new PeerHost(status, platform, sort, toggles[0], toggles[1], toggles[2], toggles[3], toggles[4], toggles[5], save);
            Assert.Equal("按状态筛选游戏", GetPeer(status).GetName());
            Assert.Equal("按平台筛选游戏", GetPeer(platform).GetName());
            Assert.Equal("排序游戏", GetPeer(sort).GetName());
            Assert.Equal("保存当前游戏策略", GetPeer(save).GetName());
            foreach (var toggle in toggles)
            {
                var pattern = Assert.IsAssignableFrom<IToggleProvider>(GetPeer(toggle).GetPattern(PatternInterface.Toggle));
                Assert.Equal(ToggleState.Off, pattern.ToggleState);
                toggle.IsChecked = true;
                host.Pump();
                Assert.Equal(ToggleState.On, pattern.ToggleState);
            }

            status.SelectedIndex = 1;
            platform.SelectedIndex = 1;
            sort.SelectedIndex = 1;
            host.Pump();
            Assert.Equal("需注意", status.SelectedItem);
            Assert.Equal("PC", platform.SelectedItem);
            Assert.Equal("最近游玩", sort.SelectedItem);
        });
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

    private static ComboBox CreateNamedSelector(string name, params string[] options)
    {
        var selector = new ComboBox
        {
            ItemsSource = options,
            SelectedIndex = 0
        };
        AutomationProperties.SetName(selector, name);
        return selector;
    }

    private static int CountOccurrences(string text, string token)
    {
        var count = 0;
        var offset = 0;
        while ((offset = text.IndexOf(token, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += token.Length;
        }

        return count;
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
