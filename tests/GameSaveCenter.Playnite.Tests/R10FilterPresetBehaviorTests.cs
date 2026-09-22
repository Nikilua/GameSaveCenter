using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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
    public void PresetMutationUsesConfirmationForRenameAndDelete()
    {
        var root = TestRepositoryContext.Root;
        var implementation = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.FilterPresets.cs"));
        var taskXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var mediaXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));

        Assert.Contains("plugin.ConfirmAsync(\"重命名筛选预设\"", implementation, StringComparison.Ordinal);
        Assert.Contains("plugin.ConfirmAsync(\"删除筛选预设\"", implementation, StringComparison.Ordinal);
        Assert.Contains("TaskFilterPresetApplyButton", taskXaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding DeleteTaskFilterPresetCommand}\"", taskXaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ApplyMediaFilterPresetCommand}\"", mediaXaml, StringComparison.Ordinal);
        Assert.Contains("MediaInboxMode = preset.MediaInboxMode", implementation, StringComparison.Ordinal);
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
