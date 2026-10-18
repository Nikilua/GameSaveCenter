using System;
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
            var confirmationCount = 0;
            var lastConfirmationWasDangerous = false;
            var filterPersistenceEnabled = true;
            using var controller = new DataGridViewConfigurationController(
                owner, "tasks", settings, layout, visibility, sort, selector, nameBox, save, copy, rename, delete,
                () => persistCount++, null, null, enabled => filterPersistenceEnabled = enabled,
                (_, _, _, _, isDangerous) =>
                {
                    confirmationCount++;
                    lastConfirmationWasDangerous = isDangerous;
                    return Task.FromResult(true);
                },
                _ => { });

            Assert.Equal(configId, ((ViewConfigurationDefinition)settings.ViewConfigurations[0]).Id);
            Assert.Equal("紧凑失败任务", selector.SelectedItem?.ToString());
            Assert.Equal(180, grid.Columns[0].Width.Value);
            Assert.Equal(Visibility.Collapsed, grid.Columns[1].Visibility);
            Assert.Equal("detail", sort.ActiveColumnKey);
            Assert.Equal(ListSortDirection.Descending, sort.ActiveDirection);
            Assert.Equal(44d, owner.Resources["GscTableRowHeight"]);
            Assert.False(filterPersistenceEnabled);

            grid.Columns[0].Width = new DataGridLength(230, DataGridLengthUnitType.Pixel);
            Assert.Equal(140, settings.DataGridColumnWidths["v1/tasks/state"]);
            save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(230, settings.ViewConfigurations[0].ColumnWidths["state"]);
            Assert.Equal(140, settings.DataGridColumnWidths["v1/tasks/state"]);
            Assert.Equal(1, persistCount);

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

            nameBox.Text = "重命名副本";
            rename.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal("重命名副本", settings.ViewConfigurations[1].Name);
            Assert.Equal(1, confirmationCount);
            Assert.False(lastConfirmationWasDangerous);
            Assert.Equal(4, persistCount);

            delete.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Single(settings.ViewConfigurations);
            Assert.Equal(2, confirmationCount);
            Assert.True(lastConfirmationWasDangerous);
            Assert.Equal(0, selector.SelectedIndex);
            Assert.Equal(string.Empty, settings.GetActiveViewConfigurationId("tasks"));
            Assert.Equal(5, persistCount);
            Assert.True(filterPersistenceEnabled);

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
