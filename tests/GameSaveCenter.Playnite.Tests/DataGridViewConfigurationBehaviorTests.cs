using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class DataGridViewConfigurationBehaviorTests
{
    [Fact]
    public void SavedViewAppliesAndRestoresWithoutChangingDefaultColumnPreferences()
    {
        RunSta(() =>
        {
            var configId = Guid.NewGuid().ToString("N");
            var firstFilterPresetId = Guid.NewGuid().ToString("N");
            var secondFilterPresetId = Guid.NewGuid().ToString("N");
            var settings = new GameSaveCenterSettings
            {
                DataGridColumnWidths = new() { ["v1/tasks/state"] = 140 },
                DataGridHiddenColumnKeys = new() { ["v1/tasks"] = new() { "detail" } },
                ViewConfigurations = new()
                {
                    new ViewConfigurationDefinition
                    {
                        Id = configId,
                        Name = "紧凑失败任务",
                        WorkspaceKey = "tasks",
                        FilterPresetId = firstFilterPresetId,
                        SortColumnKey = "detail",
                        SortDirection = "Descending",
                        VisibleColumnKeys = new() { "state" },
                        ColumnWidths = new() { ["state"] = 180 },
                        DensityMode = TableDensityPolicy.CompactMode
                    }
                },
                ActiveViewConfigurationIds = new() { ["tasks"] = configId }
            };
            var grid = new DataGrid
            {
                ItemsSource = new ObservableCollection<ViewRow> { new("a", 1, "Alpha"), new("b", 2, "Beta") },
                Columns =
                {
                    new DataGridTextColumn { Header = "状态", Binding = new Binding(nameof(ViewRow.Value)), Width = new DataGridLength(90) },
                    new DataGridTextColumn { Header = "详情", Binding = new Binding(nameof(ViewRow.Name)), Width = new DataGridLength(1, DataGridLengthUnitType.Star) }
                }
            };
            var owner = CreateDensityOwner();
            var pickerButton = new Button();
            var persistCount = 0;
            var layout = new DataGridColumnLayoutController(grid, "tasks", new[] { "state", "detail" }, settings, () => persistCount++);
            var visibility = new DataGridColumnVisibilityController(grid, pickerButton, "tasks", new[] { "state", "detail" }, new[] { "state" }, settings, () => persistCount++);
            var sort = new DataGridStableSortController(grid, new[]
            {
                DataGridSortColumn.Create<ViewRow>("state", nameof(ViewRow.Value), ListSortDirection.Ascending,
                    (left, right) => left.Value.CompareTo(right.Value), _ => false, row => row.Id),
                DataGridSortColumn.Create<ViewRow>("detail", nameof(ViewRow.Name), ListSortDirection.Ascending,
                    (left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal), _ => false, row => row.Id)
            }, 0);
            var selector = new ComboBox();
            var nameBox = new TextBox();
            var save = new Button();
            var copy = new Button();
            var rename = new Button();
            var delete = new Button();
            var dirtyStatus = new TextBlock();
            var restore = new Button();
            var resetLayout = new Button();
            var filterState = new TestFilterState(firstFilterPresetId, "preset-a");
            filterState.AddPreset(secondFilterPresetId, "manual");
            var confirmationCount = 0;
            var lastConfirmationWasDangerous = false;
            var lastStatus = string.Empty;
            var filterPersistenceEnabled = true;
            using var controller = new DataGridViewConfigurationController(
                owner, "tasks", settings, layout, visibility, sort, selector, nameBox, save, copy, rename, delete,
                dirtyStatus, restore,
                resetLayout,
                () => persistCount++, () => filterState.SelectedPresetId, filterState.ApplyPreset,
                enabled => filterPersistenceEnabled = enabled, filterState,
                () => filterState.CurrentFingerprint, filterState.GetFingerprint,
                (_, _, _, _, isDangerous) =>
                {
                    confirmationCount++;
                    lastConfirmationWasDangerous = isDangerous;
                    return Task.FromResult(true);
                },
                message => lastStatus = message);

            Assert.Equal(configId, ((ViewConfigurationDefinition)settings.ViewConfigurations[0]).Id);
            Assert.Equal("紧凑失败任务", selector.SelectedItem?.ToString());
            Assert.Equal(180, grid.Columns[0].Width.Value);
            Assert.Equal(Visibility.Collapsed, grid.Columns[1].Visibility);
            Assert.Equal("detail", sort.ActiveColumnKey);
            Assert.Equal(ListSortDirection.Descending, sort.ActiveDirection);
            Assert.Equal(44d, owner.Resources["GscTableRowHeight"]);
            Assert.False(filterPersistenceEnabled);
            Assert.Equal(Visibility.Collapsed, dirtyStatus.Visibility);
            Assert.False(restore.IsEnabled);

            grid.Columns[0].Width = new DataGridLength(230, DataGridLengthUnitType.Pixel);
            Assert.Equal(140, settings.DataGridColumnWidths["v1/tasks/state"]);
            Assert.Equal(180, settings.ViewConfigurations[0].ColumnWidths["state"]);
            Assert.Equal(Visibility.Visible, dirtyStatus.Visibility);
            Assert.True(restore.IsEnabled);
            restore.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(180, grid.Columns[0].Width.Value);
            Assert.Equal(Visibility.Collapsed, dirtyStatus.Visibility);

            grid.Columns[0].Width = new DataGridLength(230, DataGridLengthUnitType.Pixel);
            save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(230, settings.ViewConfigurations[0].ColumnWidths["state"]);
            Assert.Equal(140, settings.DataGridColumnWidths["v1/tasks/state"]);
            Assert.Equal(1, persistCount);
            Assert.Equal(Visibility.Collapsed, dirtyStatus.Visibility);

            grid.Columns[1].Visibility = Visibility.Visible;
            Assert.Equal(Visibility.Visible, dirtyStatus.Visibility);
            restore.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(Visibility.Collapsed, grid.Columns[1].Visibility);
            Assert.Equal(Visibility.Collapsed, dirtyStatus.Visibility);

            selector.SelectedIndex = 0;
            Assert.Equal("默认视图", selector.SelectedItem?.ToString());
            Assert.Equal(140, grid.Columns[0].Width.Value);
            Assert.Equal(Visibility.Collapsed, grid.Columns[1].Visibility);
            Assert.Equal("state", sort.ActiveColumnKey);
            Assert.Equal(ListSortDirection.Ascending, sort.ActiveDirection);
            Assert.Equal(52d, owner.Resources["GscTableRowHeight"]);
            Assert.Equal(string.Empty, settings.GetActiveViewConfigurationId("tasks"));
            Assert.Equal(2, persistCount);
            Assert.True(filterPersistenceEnabled);

            nameBox.Text = "默认副本";
            copy.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(2, settings.ViewConfigurations.Count);
            Assert.Equal("默认副本", settings.ViewConfigurations[1].Name);
            Assert.Equal("默认副本", nameBox.Text);
            Assert.Equal(3, persistCount);
            Assert.False(filterPersistenceEnabled);

            filterState.SetCurrentFingerprint("manual");
            Assert.Equal(Visibility.Visible, dirtyStatus.Visibility);
            Assert.Contains("筛选", dirtyStatus.Text, StringComparison.Ordinal);
            save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Contains("请先保存并应用筛选预设", lastStatus, StringComparison.Ordinal);
            Assert.Equal(string.Empty, settings.ViewConfigurations[1].FilterPresetId);

            restore.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal("default", filterState.CurrentFingerprint);
            Assert.Equal(Visibility.Collapsed, dirtyStatus.Visibility);

            filterState.SelectPreset(secondFilterPresetId);
            filterState.SetCurrentFingerprint("manual");
            Assert.Equal(Visibility.Visible, dirtyStatus.Visibility);
            save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(secondFilterPresetId, settings.ViewConfigurations[1].FilterPresetId);
            Assert.Equal(Visibility.Collapsed, dirtyStatus.Visibility);
            Assert.Equal(4, persistCount);

            nameBox.Text = "重命名副本";
            rename.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal("重命名副本", settings.ViewConfigurations[1].Name);
            Assert.Equal(1, confirmationCount);
            Assert.False(lastConfirmationWasDangerous);
            Assert.Equal(5, persistCount);

            delete.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Single(settings.ViewConfigurations);
            Assert.Equal(2, confirmationCount);
            Assert.True(lastConfirmationWasDangerous);
            Assert.Equal(0, selector.SelectedIndex);
            Assert.Equal(string.Empty, settings.GetActiveViewConfigurationId("tasks"));
            Assert.Equal(6, persistCount);
            Assert.True(filterPersistenceEnabled);

            controller.Dispose();
            visibility.Dispose();
            layout.Dispose();
            sort.Dispose();
        });
    }

    [Fact]
    public void LayoutResetPreviewsScopeAndPreservesFiltersAndBusinessSettings()
    {
        RunSta(() =>
        {
            var configurationId = Guid.NewGuid().ToString("N");
            var otherConfigurationId = Guid.NewGuid().ToString("N");
            var presetId = Guid.NewGuid().ToString("N");
            var settings = new GameSaveCenterSettings
            {
                EnableCloudUpload = true,
                DefaultTableDensityMode = TableDensityPolicy.ComfortableMode,
                FilterPresets = new()
                {
                    new FilterPresetDefinition { Id = presetId, Name = "失败任务", Workspace = FilterPresetDefinition.TasksWorkspace }
                },
                DataGridColumnWidths = new() { ["v1/tasks/state"] = 140 },
                DataGridHiddenColumnKeys = new() { ["v1/tasks"] = new() { "detail" } },
                ViewConfigurations = new()
                {
                    new ViewConfigurationDefinition
                    {
                        Id = configurationId,
                        Name = "紧凑失败任务",
                        WorkspaceKey = "tasks",
                        FilterPresetId = presetId,
                        SortColumnKey = "detail",
                        SortDirection = "Descending",
                        VisibleColumnKeys = new() { "state" },
                        ColumnWidths = new() { ["state"] = 180 },
                        DensityMode = TableDensityPolicy.CompactMode
                    },
                    new ViewConfigurationDefinition
                    {
                        Id = otherConfigurationId,
                        Name = "备份候选",
                        WorkspaceKey = "save-candidates",
                        VisibleColumnKeys = new() { "state" }
                    }
                },
                ActiveViewConfigurationIds = new() { ["tasks"] = configurationId }
            };
            var grid = new DataGrid
            {
                ItemsSource = new ObservableCollection<ViewRow> { new("a", 1, "Alpha"), new("b", 2, "Beta") },
                Columns =
                {
                    new DataGridTextColumn { Header = "状态", Binding = new Binding(nameof(ViewRow.Value)), Width = new DataGridLength(90) },
                    new DataGridTextColumn { Header = "详情", Binding = new Binding(nameof(ViewRow.Name)), Width = new DataGridLength(1, DataGridLengthUnitType.Star) }
                }
            };
            var owner = CreateDensityOwner();
            var pickerButton = new Button();
            var resetButton = new Button();
            var persistCount = 0;
            var layout = new DataGridColumnLayoutController(grid, "tasks", new[] { "state", "detail" }, settings, () => persistCount++);
            var visibility = new DataGridColumnVisibilityController(grid, pickerButton, "tasks", new[] { "state", "detail" }, new[] { "state" }, settings, () => persistCount++);
            var sort = new DataGridStableSortController(grid, new[]
            {
                DataGridSortColumn.Create<ViewRow>("state", nameof(ViewRow.Value), ListSortDirection.Ascending,
                    (left, right) => left.Value.CompareTo(right.Value), _ => false, row => row.Id),
                DataGridSortColumn.Create<ViewRow>("detail", nameof(ViewRow.Name), ListSortDirection.Ascending,
                    (left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal), _ => false, row => row.Id)
            }, 0);
            var filterState = new TestFilterState(presetId, "preset-state");
            var selector = new ComboBox();
            var acceptConfirmation = false;
            var confirmationCount = 0;
            var lastConfirmationTitle = string.Empty;
            var lastConfirmationMessage = string.Empty;
            var lastStatus = string.Empty;
            using var controller = new DataGridViewConfigurationController(
                owner, "tasks", settings, layout, visibility, sort, selector, new TextBox(),
                new Button(), new Button(), new Button(), new Button(), new TextBlock(), new Button(), resetButton,
                () => persistCount++, () => filterState.SelectedPresetId, filterState.ApplyPreset,
                _ => { }, filterState, () => filterState.CurrentFingerprint, _ => "preset-state",
                (title, message, _, _, _) =>
                {
                    confirmationCount++;
                    lastConfirmationTitle = title;
                    lastConfirmationMessage = message;
                    return Task.FromResult(acceptConfirmation);
                },
                message => lastStatus = message);

            var resetCurrent = (MenuItem)resetButton.ContextMenu!.Items[0];
            resetCurrent.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Assert.Equal(1, confirmationCount);
            Assert.Equal("确认重置当前表格布局", lastConfirmationTitle);
            Assert.Contains("紧凑失败任务", lastConfirmationMessage, StringComparison.Ordinal);
            Assert.Contains("失败任务", lastConfirmationMessage, StringComparison.Ordinal);
            Assert.Equal(180, grid.Columns[0].Width.Value);
            Assert.Equal(180, settings.ViewConfigurations[0].ColumnWidths["state"]);
            Assert.Equal(0, persistCount);

            acceptConfirmation = true;
            resetCurrent.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            var resetConfiguration = settings.ViewConfigurations.Single(item => item.Id == configurationId);
            Assert.Equal("紧凑失败任务", resetConfiguration.Name);
            Assert.Equal(presetId, resetConfiguration.FilterPresetId);
            Assert.Empty(resetConfiguration.ColumnWidths);
            Assert.Equal(new[] { "state", "detail" }, resetConfiguration.VisibleColumnKeys);
            Assert.Equal("state", resetConfiguration.SortColumnKey);
            Assert.Equal("Ascending", resetConfiguration.SortDirection);
            Assert.Equal(TableDensityPolicy.ComfortableMode, resetConfiguration.DensityMode);
            Assert.Equal(140, settings.DataGridColumnWidths["v1/tasks/state"]);
            Assert.Equal(new[] { "detail" }, settings.DataGridHiddenColumnKeys["v1/tasks"]);
            Assert.Equal("preset-state", filterState.CurrentFingerprint);
            Assert.Single(settings.FilterPresets);
            Assert.Equal(1, persistCount);

            var resetAll = (MenuItem)resetButton.ContextMenu.Items[2];
            resetAll.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Assert.Equal(3, confirmationCount);
            Assert.Equal("确认重置全部工作视图", lastConfirmationTitle);
            Assert.Contains("任务队列：紧凑失败任务", lastConfirmationMessage, StringComparison.Ordinal);
            Assert.Contains("存档候选路径：备份候选", lastConfirmationMessage, StringComparison.Ordinal);
            Assert.Contains("保留全部 1 个筛选预设", lastConfirmationMessage, StringComparison.Ordinal);
            Assert.Empty(settings.ViewConfigurations);
            Assert.Empty(settings.ActiveViewConfigurationIds);
            Assert.Empty(settings.DataGridColumnWidths);
            Assert.Empty(settings.DataGridHiddenColumnKeys);
            Assert.Equal(TableDensityPolicy.ComfortableMode, settings.DefaultTableDensityMode);
            Assert.Single(settings.FilterPresets);
            Assert.True(settings.EnableCloudUpload);
            Assert.Equal(presetId, filterState.SelectedPresetId);
            Assert.Equal("preset-state", filterState.CurrentFingerprint);
            Assert.Equal("默认视图", selector.SelectedItem?.ToString());
            Assert.Equal(90, grid.Columns[0].Width.Value);
            Assert.Equal(Visibility.Visible, grid.Columns[1].Visibility);
            Assert.Equal("state", sort.ActiveColumnKey);
            Assert.Equal(2, persistCount);
            Assert.Contains("业务设置均已保留", lastStatus, StringComparison.Ordinal);

            settings.DataGridColumnWidths = new()
            {
                ["v1/tasks/state"] = 130,
                ["v1/save-history/state"] = 210
            };
            settings.DataGridHiddenColumnKeys = new()
            {
                ["v1/tasks"] = new() { "detail" },
                ["v1/save-history"] = new() { "detail" }
            };
            resetCurrent.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Assert.Equal(4, confirmationCount);
            Assert.Contains("仅重置“任务队列”表格", lastConfirmationMessage, StringComparison.Ordinal);
            Assert.False(settings.DataGridColumnWidths.ContainsKey("v1/tasks/state"));
            Assert.False(settings.DataGridHiddenColumnKeys.ContainsKey("v1/tasks"));
            Assert.Equal(210, settings.DataGridColumnWidths["v1/save-history/state"]);
            Assert.Contains("v1/save-history", settings.DataGridHiddenColumnKeys.Keys);
            Assert.Equal(3, persistCount);

            controller.Dispose();
            visibility.Dispose();
            layout.Dispose();
            sort.Dispose();
        });
    }

    private static Grid CreateDensityOwner()
    {
        var owner = new Grid();
        owner.Resources["GscTableRowHeightComfortable"] = 52d;
        owner.Resources["GscTableRowHeightCompact"] = 44d;
        owner.Resources["GscTableHeaderHeightComfortable"] = 42d;
        owner.Resources["GscTableHeaderHeightCompact"] = 38d;
        owner.Resources["GscTableCellPaddingComfortable"] = new Thickness(12, 0, 12, 0);
        owner.Resources["GscTableCellPaddingCompact"] = new Thickness(10, 0, 10, 0);
        return owner;
    }

    private sealed class TestFilterState : INotifyPropertyChanged
    {
        private readonly Dictionary<string, string> presetFingerprints = new(StringComparer.Ordinal);
        private string selectedPresetId;
        private string currentFingerprint;

        internal TestFilterState(string selectedPresetId, string currentFingerprint)
        {
            this.selectedPresetId = selectedPresetId;
            this.currentFingerprint = currentFingerprint;
            presetFingerprints[string.Empty] = "default";
            presetFingerprints[selectedPresetId] = currentFingerprint;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        internal string SelectedPresetId
        {
            get => selectedPresetId;
            private set
            {
                if (string.Equals(selectedPresetId, value, StringComparison.Ordinal)) return;
                selectedPresetId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedTaskFilterPreset"));
            }
        }

        internal string CurrentFingerprint
        {
            get => currentFingerprint;
            private set
            {
                if (string.Equals(currentFingerprint, value, StringComparison.Ordinal)) return;
                currentFingerprint = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TaskSearchText"));
            }
        }

        internal string? GetFingerprint(string presetId)
            => presetFingerprints.TryGetValue(presetId ?? string.Empty, out var value) ? value : null;

        internal void AddPreset(string id, string fingerprint)
            => presetFingerprints[id] = fingerprint;

        internal void SelectPreset(string id)
            => SelectedPresetId = id;

        internal void SetCurrentFingerprint(string value)
            => CurrentFingerprint = value;

        internal void ApplyPreset(string id)
        {
            SelectedPresetId = id ?? string.Empty;
            if (GetFingerprint(SelectedPresetId) is string fingerprint)
                CurrentFingerprint = fingerprint;
        }
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception error) { failure = error; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null) throw new Xunit.Sdk.XunitException($"STA view-configuration interaction failed: {failure}");
    }

    private sealed class ViewRow
    {
        internal ViewRow(string id, int value, string name)
        {
            Id = id;
            Value = value;
            Name = name;
        }

        public string Id { get; }
        public int Value { get; }
        public string Name { get; }
    }
}
