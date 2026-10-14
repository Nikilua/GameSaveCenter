using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Newtonsoft.Json;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R10FilterPresetBehaviorTests
{
    [Fact]
    public void InvalidAndLegacyPresetSettingsFallBackWithoutKeepingLiveObjects()
    {
        var validId = Guid.NewGuid().ToString("N");
        var settings = new GameSaveCenterSettings
        {
            FilterPresets = new()
            {
                new FilterPresetDefinition { Name = string.Empty },
                new FilterPresetDefinition { Name = "未知页面", Workspace = "Unknown" },
                new FilterPresetDefinition
                {
                    Id = validId,
                    Name = "失败任务",
                    Workspace = FilterPresetDefinition.TasksWorkspace,
                    TaskStatusFilter = "removed-state",
                    TaskHistoryScope = "removed-scope",
                    TaskHistoryRange = "removed-range"
                },
                new FilterPresetDefinition
                {
                    Id = validId,
                    Name = "重复项",
                    Workspace = FilterPresetDefinition.TasksWorkspace
                }
            }
        };

        var preset = Assert.Single(settings.FilterPresets);
        Assert.Equal(validId, preset.Id);
        Assert.Equal("全部", preset.TaskStatusFilter);
        Assert.Equal("最近任务", preset.TaskHistoryScope);
        Assert.Equal("全部时间", preset.TaskHistoryRange);
        Assert.All(typeof(FilterPresetDefinition).GetProperties(BindingFlags.Public | BindingFlags.Instance), property =>
            Assert.Equal(typeof(string), property.PropertyType));
    }

    [Fact]
    public void PresetSettingsRoundTripKeepsOnlyStableScalarFilterState()
    {
        var source = new GameSaveCenterSettings
        {
            FilterPresets = new()
            {
                new FilterPresetDefinition
                {
                    Name = "未归类媒体",
                    Workspace = FilterPresetDefinition.MediaWorkspace,
                    MediaInboxMode = "待归类",
                    MediaFilter = "录像",
                    MediaSearchText = "capture"
                }
            }
        };

        var json = JsonConvert.SerializeObject(source);
        var imported = JsonConvert.DeserializeObject<GameSaveCenterSettings>(json)!;
        var preset = Assert.Single(imported.FilterPresets);

        Assert.Equal("未归类媒体", preset.Name);
        Assert.Equal(FilterPresetDefinition.MediaWorkspace, preset.Workspace);
        Assert.Equal("待归类", preset.MediaInboxMode);
        Assert.Equal("录像", preset.MediaFilter);
        Assert.Equal("capture", preset.MediaSearchText);
        Assert.DoesNotContain("TaskStatusDto", json, StringComparison.Ordinal);
        Assert.DoesNotContain("MediaItemDto", json, StringComparison.Ordinal);
    }

    [Fact]
    public void TaskPresetToolbarInvokesBoundApplyCommandThroughRealButton()
    {
        RunSta(() =>
        {
            var view = new TaskCenterView();
            var binding = new TestPresetBinding();
            binding.TaskFilterPresets.Add(new FilterPresetDefinition
            {
                Name = "失败任务",
                Workspace = FilterPresetDefinition.TasksWorkspace,
                TaskStatusFilter = "失败"
            });
            view.DataContext = binding;

            using var host = new WindowHost(view);
            host.Window.UpdateLayout();

            var combo = (ComboBox)view.FindName("TaskFilterPresetComboBox")!;
            var apply = (Button)view.FindName("TaskFilterPresetApplyButton")!;
            var filters = (Grid)view.FindName("TaskFiltersPanel")!;
            view.ApplyResponsiveLayout(700, 640);
            combo.SelectedIndex = 0;
            host.Window.UpdateLayout();

            Assert.Equal(GridUnitType.Auto, filters.RowDefinitions[1].Height.GridUnitType);
            Assert.Same(binding.TaskFilterPresets[0], binding.SelectedTaskFilterPreset);
            Assert.True(apply.Command!.CanExecute(null));
            // WPF's RaiseEvent does not call ButtonBase.OnClick. Execute the command
            // already bound to the real instantiated button to verify the binding path.
            apply.Command.Execute(null);
            Assert.Equal(1, binding.ApplyCount);
        });
    }

    [Fact]
    public void ProductionTaskPresetSnapshotAppliesFiltersAndFallsBackWhenDynamicOptionsExpire()
    {
        RunSta(() =>
        {
            var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
            var tasks = new BatchObservableCollection<TaskStatusDto>();
            SetAutoPropertyBackingField(dashboard, nameof(DashboardViewModel.Tasks), tasks);
            SetAutoPropertyBackingField(dashboard, nameof(DashboardViewModel.TasksView), CollectionViewSource.GetDefaultView(tasks));
            SetAutoPropertyBackingField(dashboard, nameof(DashboardViewModel.TaskStatusFilterOptions), new[] { "全部", "运行中", "等待中", "失败", "已完成" });
            SetAutoPropertyBackingField(dashboard, nameof(DashboardViewModel.TaskHistoryRangeOptions), new[] { "全部时间", "今天", "昨天", "近7天", "近30天" });
            SetAutoPropertyBackingField(dashboard, nameof(DashboardViewModel.TaskGameFilterOptions), new ObservableCollection<string> { "全部", "星界边境" });
            SetAutoPropertyBackingField(dashboard, nameof(DashboardViewModel.TaskTypeFilterOptions), new ObservableCollection<string> { "全部", "备份" });
            var searchRefresh = new DebouncedRefresh(() => { }, TimeSpan.FromHours(1));
            SetField(dashboard, "taskSearchRefresh", searchRefresh);
            SetField(dashboard, "taskHistoryQueryRefresh", new DebouncedRefresh(() => { }, TimeSpan.FromHours(1)));
            SetField(dashboard, "taskPageRequests", new LatestRequestCoordinator());
            SetField(dashboard, "taskHistoryActive", false);
            SetField(dashboard, "isBusy", true);
            SetField(dashboard, "taskNavigationGameName", string.Empty);

            try
            {
                SetField(dashboard, "taskSearchText", "capture");
                SetField(dashboard, "taskStatusFilter", "失败");
                SetField(dashboard, "taskGameFilter", "星界边境");
                SetField(dashboard, "taskTypeFilter", "备份");
                SetField(dashboard, "taskHistoryScope", "最近任务");
                SetField(dashboard, "taskHistoryRange", "全部时间");

                var snapshotMethod = typeof(DashboardViewModel).GetMethod("CreateTaskFilterPreset", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(snapshotMethod);
                var snapshot = Assert.IsType<FilterPresetDefinition>(snapshotMethod!.Invoke(dashboard, new object[] { "失败备份" }));
                Assert.Equal(FilterPresetDefinition.TasksWorkspace, snapshot.Workspace);
                Assert.Equal("capture", snapshot.TaskSearchText);
                Assert.Equal("失败", snapshot.TaskStatusFilter);
                Assert.Equal("星界边境", snapshot.TaskGameFilter);
                Assert.Equal("备份", snapshot.TaskTypeFilter);

                SetField(dashboard, "taskSearchText", string.Empty);
                SetField(dashboard, "taskStatusFilter", "全部");
                SetField(dashboard, "taskGameFilter", "全部");
                SetField(dashboard, "taskTypeFilter", "全部");
                SetField(dashboard, "selectedTaskFilterPreset", snapshot);
                var applyMethod = typeof(DashboardViewModel).GetMethod("ApplyTaskFilterPreset", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(applyMethod);
                applyMethod!.Invoke(dashboard, null);

                Assert.Equal("capture", dashboard.TaskSearchText);
                Assert.Equal("失败", dashboard.TaskStatusFilter);
                Assert.Equal("星界边境", dashboard.TaskGameFilter);
                Assert.Equal("备份", dashboard.TaskTypeFilter);
                Assert.Equal("已应用任务筛选预设“失败备份”", dashboard.StatusMessage);

                var stale = new FilterPresetDefinition
                {
                    Name = "旧选项",
                    Workspace = FilterPresetDefinition.TasksWorkspace,
                    TaskStatusFilter = "已移除状态",
                    TaskGameFilter = "已移除游戏",
                    TaskTypeFilter = "已移除类型"
                };
                SetField(dashboard, "taskSearchText", string.Empty);
                SetField(dashboard, "taskStatusFilter", "全部");
                SetField(dashboard, "taskGameFilter", "全部");
                SetField(dashboard, "taskTypeFilter", "全部");
                SetField(dashboard, "selectedTaskFilterPreset", stale);
                applyMethod.Invoke(dashboard, null);

                Assert.Equal("全部", dashboard.TaskStatusFilter);
                Assert.Equal("全部", dashboard.TaskGameFilter);
                Assert.Equal("全部", dashboard.TaskTypeFilter);
            }
            finally
            {
                searchRefresh.Cancel();
                ((LatestRequestCoordinator)GetField(dashboard, "taskPageRequests")!).Dispose();
                ((DebouncedRefresh)GetField(dashboard, "taskHistoryQueryRefresh")!).Cancel();
            }
        });
    }

    [Fact]
    public async Task CancellingPresetRenameOrDeleteLeavesSelectionAndStoredPresetUntouched()
    {
        await AssertCancelledPresetMutationIsUnchanged("RenameTaskFilterPresetAsync", "重命名筛选预设", "重命名", false, "新名称");
        await AssertCancelledPresetMutationIsUnchanged("DeleteTaskFilterPresetAsync", "删除筛选预设", "删除", true, "原名称");
    }

    [Fact]
    public void PresetBindingsAndApplicationRemainConnected()
    {
        var root = TestRepositoryContext.Root;
        var implementation = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.FilterPresets.cs"));
        var taskXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var mediaXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));

        Assert.Contains("TaskFilterPresetApplyButton", taskXaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding DeleteTaskFilterPresetCommand}\"", taskXaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ApplyMediaFilterPresetCommand}\"", mediaXaml, StringComparison.Ordinal);
        Assert.Contains("MediaInboxMode = preset.MediaInboxMode", implementation, StringComparison.Ordinal);
    }

    private static async Task AssertCancelledPresetMutationIsUnchanged(string methodName, string expectedTitle, string expectedConfirmText, bool expectedDangerous, string draft)
    {
        var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
        var selected = new FilterPresetDefinition
        {
            Name = "原名称",
            Workspace = FilterPresetDefinition.TasksWorkspace,
            TaskStatusFilter = "失败"
        };
        var other = new FilterPresetDefinition
        {
            Name = "另一个预设",
            Workspace = FilterPresetDefinition.TasksWorkspace
        };
        var presets = new ObservableCollection<FilterPresetDefinition> { selected, other };
        SetAutoPropertyBackingField(dashboard, nameof(DashboardViewModel.TaskFilterPresets), presets);
        SetField(dashboard, "selectedTaskFilterPreset", selected);
        SetField(dashboard, "taskFilterPresetNameDraft", draft);

        var confirmationCalled = false;
        dashboard.FilterPresetConfirmationOverrideForAudit = (title, message, confirmText, cancelText, isDangerous) =>
        {
            confirmationCalled = true;
            Assert.Equal(expectedTitle, title);
            Assert.Contains("原名称", message, StringComparison.Ordinal);
            Assert.Equal(expectedConfirmText, confirmText);
            Assert.Equal("取消", cancelText);
            Assert.Equal(expectedDangerous, isDangerous);
            return Task.FromResult(false);
        };

        var mutation = typeof(DashboardViewModel).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(mutation);
        var pending = (Task?)mutation!.Invoke(dashboard, null);
        Assert.NotNull(pending);
        await pending!;

        Assert.True(confirmationCalled);
        Assert.Equal(2, presets.Count);
        Assert.Same(selected, presets[0]);
        Assert.Same(other, presets[1]);
        Assert.Equal("原名称", selected.Name);
        Assert.Same(selected, dashboard.SelectedTaskFilterPreset);
        Assert.Equal(draft, dashboard.TaskFilterPresetNameDraft);
    }

    private static void SetAutoPropertyBackingField(object target, string propertyName, object value)
    {
        var field = typeof(DashboardViewModel).GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }

    private static void SetField(object target, string fieldName, object value)
    {
        var field = typeof(DashboardViewModel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }

    private static object? GetField(object target, string fieldName)
    {
        var field = typeof(DashboardViewModel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return field!.GetValue(target);
    }

    private sealed class TestPresetBinding
    {
        public ObservableCollection<FilterPresetDefinition> TaskFilterPresets { get; } = new();
        public FilterPresetDefinition? SelectedTaskFilterPreset { get; set; }
        public int ApplyCount { get; private set; }
        public ICommand ApplyTaskFilterPresetCommand { get; }

        public TestPresetBinding()
        {
            ApplyTaskFilterPresetCommand = new RelayCommand(_ => ApplyCount++);
        }
    }

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(FrameworkElement content)
        {
            Window = new Window
            {
                Content = content,
                Width = 1000,
                Height = 720,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };
            Window.Show();
            Window.UpdateLayout();
        }

        public Window Window { get; }

        public void Dispose()
        {
            Window.Close();
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception exception) { failure = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}
