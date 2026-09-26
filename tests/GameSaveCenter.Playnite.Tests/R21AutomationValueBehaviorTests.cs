using System;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
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
        Assert.Contains("AutomationProperties.Name=\"媒体收件箱当前全局目标游戏\"", media);
        Assert.Contains("AutomationProperties.Name=\"当前全局目标游戏\"", media);
        Assert.Contains("Text=\"{Binding SelectedGame.Name, Mode=OneWay, TargetNullValue=未选择游戏, FallbackValue=未选择游戏}\"", media);
        Assert.Contains("ItemsSource=\"{Binding MediaFilterOptions}\" SelectedItem=\"{Binding MediaFilter, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, TargetNullValue=全部, FallbackValue=全部}\" ToolTip=\"媒体类型筛选\" AutomationProperties.Name=\"媒体类型筛选\"", media);
        Assert.Contains("SelectedItem=\"{Binding MediaTargetGame}\" ToolTip=\"重新归类目标；显示名称、平台和 Playnite ID\" ItemTemplate=\"{StaticResource MediaGameTargetTemplate}\" Margin=\"0,0,0,8\" AutomationProperties.Name=\"重新归类目标游戏\"", media);
        Assert.Contains("SelectedItem=\"{Binding ProcessMappingTargetGame}\" Margin=\"0,0,0,8\" AutomationProperties.Name=\"进程映射目标游戏\"", maintenance);
        Assert.Contains("AutomationProperties.Name=\"媒体收件箱视图\"", media);
        Assert.Contains("AutomationProperties.Name=\"媒体筛选预设\"", media);
        Assert.Contains("AutomationProperties.Name=\"媒体归类批次状态筛选\"", media);
        Assert.Contains("AutomationProperties.Name=\"调整归类建议目标\"", media);
        Assert.Contains("AutomationProperties.Name=\"云端队列状态筛选\"", maintenance);
        Assert.Contains("AutomationProperties.Name=\"云端队列类型筛选\"", maintenance);
        Assert.Contains("AutomationProperties.Name=\"云端队列时间筛选\"", maintenance);
    }

    [Fact]
    public void TaskProgressPeerExposesBoundValueAndUnknownStatus()
    {
        RunSta(() =>
        {
            var progress = new ProgressBar { Minimum = 0, Maximum = 100 };
            AutomationProperties.SetName(progress, "任务进度");
            progress.SetBinding(ProgressBar.ValueProperty, new Binding(nameof(TaskStatusDto.ProgressValue))
            {
                Mode = BindingMode.OneWay
            });
            progress.SetBinding(AutomationProperties.HelpTextProperty, new Binding(nameof(TaskStatusDto.ProgressDisplay))
            {
                Mode = BindingMode.OneWay
            });

            var running = new TaskStatusDto
            {
                State = TaskState.Running,
                ProgressPercent = 42,
                StageMessage = "正在扫描"
            };
            progress.DataContext = running;

            using var host = new PeerHost(progress);
            var peer = GetPeer(progress);
            Assert.Equal("任务进度", peer.GetName());
            var range = Assert.IsAssignableFrom<IRangeValueProvider>(peer.GetPattern(PatternInterface.RangeValue));
            Assert.Equal(42, range.Value);
            Assert.Equal(0, range.Minimum);
            Assert.Equal(100, range.Maximum);
            Assert.Equal("42%", peer.GetHelpText());

            progress.DataContext = new TaskStatusDto
            {
                State = TaskState.Running,
                ProgressPercent = -1,
                StageMessage = "阶段未知"
            };
            host.Pump();
            Assert.Equal(0, range.Value);
            Assert.Equal("—", peer.GetHelpText());

            progress.DataContext = new TaskStatusDto
            {
                State = TaskState.Running,
                ProgressPercent = 120,
                StageMessage = "正在收尾"
            };
            host.Pump();
            Assert.Equal(100, range.Value);
            Assert.Equal("100%", peer.GetHelpText());
        });
    }

    [Fact]
    public void SelectorAndTogglePeersExposeEmptyAndIndeterminateStates()
    {
        RunSta(() =>
        {
            var selector = new ComboBox
            {
                ItemsSource = new[] { "全部", "失败" },
                SelectedIndex = -1
            };
            AutomationProperties.SetName(selector, "任务状态筛选");

            var toggle = new NativeToggleSwitch
            {
                IsThreeState = true,
                IsChecked = null,
                Content = "启用策略"
            };
            AutomationProperties.SetName(toggle, "启用备份策略");

            using var host = new PeerHost(selector, toggle);
            var selectorPeer = GetPeer(selector);
            Assert.Equal("任务状态筛选", selectorPeer.GetName());
            var selection = Assert.IsAssignableFrom<ISelectionProvider>(selectorPeer.GetPattern(PatternInterface.Selection));
            Assert.False(selection.CanSelectMultiple);
            Assert.Null(selection.GetSelection());

            selector.SelectedIndex = 1;
            host.Pump();
            Assert.Single(selection.GetSelection());
            Assert.Equal("失败", selector.SelectedItem);

            var togglePeer = GetPeer(toggle);
            Assert.Equal("启用备份策略", togglePeer.GetName());
            var togglePattern = Assert.IsAssignableFrom<IToggleProvider>(togglePeer.GetPattern(PatternInterface.Toggle));
            Assert.Equal(ToggleState.Indeterminate, togglePattern.ToggleState);

            toggle.IsChecked = false;
            host.Pump();
            Assert.Equal(ToggleState.Off, togglePattern.ToggleState);

            toggle.IsChecked = true;
            host.Pump();
            Assert.Equal(ToggleState.On, togglePattern.ToggleState);
        });
    }

    [Fact]
    public void MediaCenterFavoriteToggleExposesSemanticState()
    {
        var root = TestRepositoryContext.Root;
        var media = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));

        Assert.Contains("IsChecked=\"{Binding MediaFavorite}\"", media);
        Assert.Contains("AutomationProperties.Name=\"收藏当前媒体\"", media);

        RunSta(() =>
        {
            var toggle = CreateNamedToggle("收藏当前媒体");
            using var host = new PeerHost(toggle);

            var peer = GetPeer(toggle);
            Assert.Equal("收藏当前媒体", peer.GetName());
            var pattern = Assert.IsAssignableFrom<IToggleProvider>(peer.GetPattern(PatternInterface.Toggle));
            Assert.Equal(ToggleState.Off, pattern.ToggleState);

            toggle.IsChecked = true;
            host.Pump();
            Assert.Equal(ToggleState.On, pattern.ToggleState);

            toggle.IsChecked = false;
            host.Pump();
            Assert.Equal(ToggleState.Off, pattern.ToggleState);
        });
    }

    [Fact]
    public void MediaCenterMetadataControlsExposeSemanticValueAndActions()
    {
        var root = TestRepositoryContext.Root;
        var media = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));

        Assert.Contains("Text=\"{Binding MediaComment, UpdateSourceTrigger=PropertyChanged}\"", media);
        Assert.Contains("AutomationProperties.Name=\"当前媒体备注\"", media);
        Assert.Contains("Command=\"{Binding UpdateMediaMetadataCommand}\"", media);
        Assert.Contains("AutomationProperties.Name=\"保存当前媒体元数据\"", media);
        Assert.Contains("Command=\"{Binding ReassignMediaCommand}\"", media);
        Assert.Contains("AutomationProperties.Name=\"移动并归类当前媒体\"", media);

        RunSta(() =>
        {
            var comment = new TextBox { Text = "原备注" };
            AutomationProperties.SetName(comment, "当前媒体备注");
            var saved = false;
            var save = new Button { Content = "保存元数据" };
            AutomationProperties.SetName(save, "保存当前媒体元数据");
            save.Click += (_, _) => saved = true;
            var reassigned = false;
            var reassign = new Button { Content = "移动并归类" };
            AutomationProperties.SetName(reassign, "移动并归类当前媒体");
            reassign.Click += (_, _) => reassigned = true;

            using var host = new PeerHost(comment, save, reassign);
            Assert.Equal("当前媒体备注", GetPeer(comment).GetName());
            Assert.Equal("保存当前媒体元数据", GetPeer(save).GetName());
            Assert.Equal("移动并归类当前媒体", GetPeer(reassign).GetName());

            var value = Assert.IsAssignableFrom<IValueProvider>(GetPeer(comment).GetPattern(PatternInterface.Value));
            Assert.Equal("原备注", value.Value);
            value.SetValue("更新备注");
            host.Pump();
            Assert.Equal("更新备注", comment.Text);

            var savePattern = Assert.IsAssignableFrom<IInvokeProvider>(GetPeer(save).GetPattern(PatternInterface.Invoke));
            savePattern.Invoke();
            host.Pump();
            Assert.True(saved);

            var reassignPattern = Assert.IsAssignableFrom<IInvokeProvider>(GetPeer(reassign).GetPattern(PatternInterface.Invoke));
            reassignPattern.Invoke();
            host.Pump();
            Assert.True(reassigned);
        });
    }

    [Fact]
    public void MediaCenterBatchActionsExposeStableNamesAndInvokeChannels()
    {
        var root = TestRepositoryContext.Root;
        var media = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        foreach (var name in new[] { "收藏所选媒体", "取消收藏所选媒体", "为所选媒体应用当前备注" })
            Assert.Equal(2, CountOccurrences(media, "AutomationProperties.Name=\"" + name + "\""));

        RunSta(() =>
        {
            var invoked = new bool[3];
            var buttons = new[]
            {
                CreateNamedAction("收藏所选媒体", () => invoked[0] = true),
                CreateNamedAction("取消收藏所选媒体", () => invoked[1] = true),
                CreateNamedAction("为所选媒体应用当前备注", () => invoked[2] = true)
            };

            using var host = new PeerHost(buttons[0], buttons[1], buttons[2]);
            for (var index = 0; index < buttons.Length; index++)
            {
                Assert.Equal(AutomationProperties.GetName(buttons[index]), GetPeer(buttons[index]).GetName());
                var invoke = Assert.IsAssignableFrom<IInvokeProvider>(GetPeer(buttons[index]).GetPattern(PatternInterface.Invoke));
                invoke.Invoke();
                host.Pump();
                Assert.True(invoked[index]);
            }
        });
    }

    [Fact]
    public void MediaCenterBatchActionsRejectEmptySelectionBeforeMetadataWrite()
    {
        var root = TestRepositoryContext.Root;
        var implementation = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Media.cs"));
        Assert.Contains("if(selected.Count==0)throw new InvalidOperationException(\"请先在媒体列表中选择一个或多个项目。\");", implementation);

        var viewModelType = typeof(GameSaveCenter.Playnite.ViewModels.DashboardViewModel);
        var viewModel = (GameSaveCenter.Playnite.ViewModels.DashboardViewModel)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(viewModelType);
        var method = viewModelType.GetMethod("UpdateMediaMetadataBatchAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException("UpdateMediaMetadataBatchAsync was not found.");

        foreach (var selection in new object?[] { null, new System.Collections.ArrayList() })
        {
            var task = (System.Threading.Tasks.Task)method.Invoke(viewModel, new object?[] { selection, true, false })!;
            var error = Assert.Throws<InvalidOperationException>(() => task.GetAwaiter().GetResult());
            Assert.Equal("请先在媒体列表中选择一个或多个项目。", error.Message);
        }
    }

    [Fact]
    public void MediaBatchCommandsRefreshBusyCanExecuteState()
    {
        var root = TestRepositoryContext.Root;
        var implementation = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var refreshStart = implementation.IndexOf("private void RaiseCommandStatesCore()", StringComparison.Ordinal);
        Assert.True(refreshStart >= 0);
        var refreshBlock = implementation.Substring(refreshStart, Math.Min(1800, implementation.Length - refreshStart));
        Assert.Contains("UpdateMediaMetadataCommand, FavoriteSelectedMediaCommand, UnfavoriteSelectedMediaCommand, CommentSelectedMediaCommand", refreshBlock);

        RunSta(() =>
        {
            var busy = false;
            var command = new GameSaveCenter.Playnite.ViewModels.RelayCommand(_ => { }, _ => !busy);
            var button = new Button { Content = "批量动作", Command = command };

            using var host = new PeerHost(button);
            Assert.True(button.IsEnabled);

            busy = true;
            command.RaiseCanExecuteChanged();
            host.Pump();
            Assert.False(button.IsEnabled);

            busy = false;
            command.RaiseCanExecuteChanged();
            host.Pump();
            Assert.True(button.IsEnabled);
        });
    }

    [Fact]
    public void MediaCenterListExposesSemanticNameAndSelectionState()
    {
        var root = TestRepositoryContext.Root;
        var media = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        Assert.Contains("x:Name=\"MediaGrid\"", media);
        Assert.Contains("SelectionMode=\"Extended\" AutomationProperties.Name=\"当前游戏媒体列表\"", media);

        RunSta(() =>
        {
            var list = new ListBox
            {
                ItemsSource = new[] { "媒体 A", "媒体 B" },
                SelectionMode = SelectionMode.Extended,
                SelectedIndex = 0
            };
            AutomationProperties.SetName(list, "当前游戏媒体列表");

            using var host = new PeerHost(list);
            var peer = GetPeer(list);
            Assert.Equal("当前游戏媒体列表", peer.GetName());

            var selection = Assert.IsAssignableFrom<ISelectionProvider>(peer.GetPattern(PatternInterface.Selection));
            Assert.True(selection.CanSelectMultiple);
            Assert.Single(selection.GetSelection());

            list.SelectedIndex = 1;
            host.Pump();
            Assert.Equal("媒体 B", list.SelectedItem);
            Assert.Single(selection.GetSelection());
        });
    }

    [Fact]
    public void MediaDetailNavigationValueExposesSelectionBoundaries()
    {
        var root = TestRepositoryContext.Root;
        var mediaView = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        Assert.Contains("Text=\"{Binding MediaDetailNavigationDisplay}\"", mediaView);
        Assert.Contains("AutomationProperties.Name=\"媒体详情位置\"", mediaView);

        var viewModelType = typeof(GameSaveCenter.Playnite.ViewModels.DashboardViewModel);
        var viewModel = (GameSaveCenter.Playnite.ViewModels.DashboardViewModel)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(viewModelType);
        var media = new GameSaveCenter.Playnite.Infrastructure.BatchObservableCollection<GameSaveCenter.Contracts.MediaItemDto>
        {
            new GameSaveCenter.Contracts.MediaItemDto { MediaId = "media-a" },
            new GameSaveCenter.Contracts.MediaItemDto { MediaId = "media-b" }
        };
        viewModelType.GetField("<Media>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(viewModel, media);

        viewModelType.GetField("selectedMedia", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(viewModel, media[0]);
        Assert.Equal("1 / 2", viewModel.MediaDetailNavigationDisplay);
        Assert.False(viewModel.CanNavigatePreviousMedia);
        Assert.True(viewModel.CanNavigateNextMedia);

        viewModelType.GetField("selectedMedia", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(viewModel, media[1]);
        Assert.Equal("2 / 2", viewModel.MediaDetailNavigationDisplay);
        Assert.True(viewModel.CanNavigatePreviousMedia);
        Assert.False(viewModel.CanNavigateNextMedia);

        viewModelType.GetField("selectedMedia", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(viewModel, null);
        Assert.Equal("未选择媒体", viewModel.MediaDetailNavigationDisplay);
        Assert.False(viewModel.CanNavigatePreviousMedia);
        Assert.False(viewModel.CanNavigateNextMedia);

        RunSta(() =>
        {
            var position = new TextBlock { Text = "1 / 2" };
            AutomationProperties.SetName(position, "媒体详情位置");

            using var host = new PeerHost(position);
            var peer = GetPeer(position);
            Assert.Equal("媒体详情位置", peer.GetName());
            Assert.Equal("1 / 2", position.Text);
        });
    }

    [Fact]
    public void MaintenanceProcessMappingSelectorExposesSemanticState()
    {
        RunSta(() =>
        {
            var selector = CreateNamedSelector("进程映射目标游戏", "游戏 A", "游戏 B");

            using var host = new PeerHost(selector);
            Assert.Equal("进程映射目标游戏", GetPeer(selector).GetName());
            selector.SelectedIndex = 1;
            host.Pump();
            Assert.Equal("游戏 B", selector.SelectedItem);
        });
    }

    [Fact]
    public void ExistingMediaCenterSelectorsExposeSemanticState()
    {
        RunSta(() =>
        {
            var mode = CreateNamedSelector("媒体收件箱视图", "待归类", "已忽略");
            var preset = CreateNamedSelector("媒体筛选预设", "全部媒体", "最近导入");
            var history = CreateNamedSelector("媒体归类批次状态筛选", "全部", "已完成");
            var suggestion = CreateNamedSelector("调整归类建议目标", "游戏 A", "游戏 B");

            using var host = new PeerHost(mode, preset, history, suggestion);
            Assert.Equal("媒体收件箱视图", GetPeer(mode).GetName());
            Assert.Equal("媒体筛选预设", GetPeer(preset).GetName());
            Assert.Equal("媒体归类批次状态筛选", GetPeer(history).GetName());
            Assert.Equal("调整归类建议目标", GetPeer(suggestion).GetName());

            mode.SelectedIndex = 1;
            preset.SelectedIndex = 1;
            history.SelectedIndex = 1;
            suggestion.SelectedIndex = 1;
            host.Pump();

            Assert.Equal("已忽略", mode.SelectedItem);
            Assert.Equal("最近导入", preset.SelectedItem);
            Assert.Equal("已完成", history.SelectedItem);
            Assert.Equal("游戏 B", suggestion.SelectedItem);
        });
    }

    [Fact]
    public void MaintenanceCloudTransferSelectorsExposeSemanticState()
    {
        RunSta(() =>
        {
            var state = CreateNamedSelector("云端队列状态筛选", "全部", "待处理");
            var kind = CreateNamedSelector("云端队列类型筛选", "全部", "媒体");
            var time = CreateNamedSelector("云端队列时间筛选", "全部时间", "最近一天");

            using var host = new PeerHost(state, kind, time);
            Assert.Equal("云端队列状态筛选", GetPeer(state).GetName());
            Assert.Equal("云端队列类型筛选", GetPeer(kind).GetName());
            Assert.Equal("云端队列时间筛选", GetPeer(time).GetName());

            state.SelectedIndex = 1;
            kind.SelectedIndex = 1;
            time.SelectedIndex = 1;
            host.Pump();

            Assert.Equal("待处理", state.SelectedItem);
            Assert.Equal("媒体", kind.SelectedItem);
            Assert.Equal("最近一天", time.SelectedItem);
        });
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

    [Fact]
    public void SaveCenterPolicyControlsExposeExternalLabelStateThroughWpfPeers()
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
            var anomaly = CreateNamedSelector("异常保护等级", "标准", "严格");
            var template = CreateNamedSelector("选择策略模板", "默认模板", "仅本地模板");
            var lockSelected = new CheckBox { Content = "锁定", IsChecked = false };
            AutomationProperties.SetName(lockSelected, "锁定所选版本");

            using var host = new PeerHost(toggles[0], toggles[1], toggles[2], toggles[3], anomaly, template, lockSelected);
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

            Assert.Equal("异常保护等级", GetPeer(anomaly).GetName());
            anomaly.SelectedIndex = 1;
            template.SelectedIndex = 1;
            host.Pump();
            Assert.Equal("严格", anomaly.SelectedItem);
            Assert.Equal("仅本地模板", template.SelectedItem);

            var lockPeer = GetPeer(lockSelected);
            Assert.Equal("锁定所选版本", lockPeer.GetName());
            var lockPattern = Assert.IsAssignableFrom<IToggleProvider>(lockPeer.GetPattern(PatternInterface.Toggle));
            Assert.Equal(ToggleState.Off, lockPattern.ToggleState);
            lockSelected.IsChecked = true;
            host.Pump();
            Assert.Equal(ToggleState.On, lockPattern.ToggleState);
        });
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

    private static Button CreateNamedAction(string name, Action action)
    {
        var button = new Button { Content = name };
        AutomationProperties.SetName(button, name);
        button.Click += (_, _) => action();
        return button;
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
