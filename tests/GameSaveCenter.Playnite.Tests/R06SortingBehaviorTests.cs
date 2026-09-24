using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R06SortingBehaviorTests
{
    [Fact]
    public void SaveHistoryUsesRawTimeAndNumericValuesWithStableUnknownLastOrdering()
    {
        RunSta(() =>
        {
            var items = new ObservableCollection<BackupVersionDto>
            {
                Backup("b", new DateTime(2026, 9, 18), 10, 200),
                Backup("a", new DateTime(2026, 9, 18), 2, 20),
                Backup("unknown", DateTime.MinValue, -1, -1),
                Backup("c", new DateTime(2026, 9, 17), 100, 1000)
            };
            var view = new ListCollectionView(items);
            var grid = CreateGrid(7);
            grid.ItemsSource = view;

            using (var controller = ProductionDataGridSortProfiles.AttachSaveHistory(grid))
            {
                Assert.Equal(new[] { "a", "b", "c", "unknown" }, view.Cast<BackupVersionDto>().Select(item => item.BackupId).ToArray());
                Assert.Equal(ListSortDirection.Descending, grid.Columns[0].SortDirection);

                controller.ApplySortForVerification(2, ListSortDirection.Ascending);
                Assert.Equal(new[] { "a", "b", "c", "unknown" }, view.Cast<BackupVersionDto>().Select(item => item.BackupId).ToArray());
                Assert.Equal(ListSortDirection.Ascending, grid.Columns[2].SortDirection);
                Assert.Null(grid.Columns[0].SortDirection);

                controller.ToggleSortForVerification(2);
                Assert.Equal(new[] { "c", "b", "a", "unknown" }, view.Cast<BackupVersionDto>().Select(item => item.BackupId).ToArray());
                Assert.Equal(ListSortDirection.Descending, grid.Columns[2].SortDirection);

                controller.ToggleSortForVerification(2);
                Assert.Equal(ListSortDirection.Ascending, grid.Columns[2].SortDirection);

                items.Clear();
                items.Add(Backup("unknown", DateTime.MinValue, -1, -1));
                items.Add(Backup("c", new DateTime(2026, 9, 17), 100, 1000));
                items.Add(Backup("b", new DateTime(2026, 9, 18), 10, 200));
                items.Add(Backup("a", new DateTime(2026, 9, 18), 2, 20));
                Assert.Equal(new[] { "a", "b", "c", "unknown" }, view.Cast<BackupVersionDto>().Select(item => item.BackupId).ToArray());
            }
        });
    }

    [Fact]
    public void TaskProgressSortUsesNumericValueAndQueuesWithUnknownProgressLast()
    {
        RunSta(() =>
        {
            var items = new ObservableCollection<TaskStatusDto>
            {
                Task("ten", TaskState.Running, 10),
                Task("two", TaskState.Running, 2),
                Task("queued", TaskState.Queued, 0),
                Task("negative", TaskState.Running, -1)
            };
            var view = new ListCollectionView(items);
            var grid = CreateGrid(7);
            grid.ItemsSource = view;

            using (var controller = ProductionDataGridSortProfiles.AttachTasks(grid))
            {
                controller.ApplySortForVerification(5, ListSortDirection.Ascending);
                Assert.Equal(new[] { "two", "ten", "negative", "queued" }, view.Cast<TaskStatusDto>().Select(item => item.TaskId).ToArray());
                Assert.Equal(ListSortDirection.Ascending, grid.Columns[5].SortDirection);
            }
        });
    }

    [Fact]
    public void TaskStageSortKeepsUnknownStageLast()
    {
        RunSta(() =>
        {
            var items = new ObservableCollection<TaskStatusDto>
            {
                Task("upload", TaskState.Running, 10, "正在复制到云端"),
                Task("scan", TaskState.Running, 10, "正在扫描"),
                Task("unknown", TaskState.Running, 10, "")
            };
            var view = new ListCollectionView(items);
            var grid = CreateGrid(7);
            grid.ItemsSource = view;

            using (var controller = ProductionDataGridSortProfiles.AttachTasks(grid))
            {
                controller.ApplySortForVerification(2, ListSortDirection.Ascending);
                Assert.Equal(new[] { "upload", "scan", "unknown" }, view.Cast<TaskStatusDto>().Select(item => item.TaskId).ToArray());
                Assert.Equal(ListSortDirection.Ascending, grid.Columns[2].SortDirection);
            }
        });
    }

    [Fact]
    public void MediaSourceSortKeepsUnknownSourceLastInBothDirections()
    {
        RunSta(() =>
        {
            var items = new ObservableCollection<MediaItemDto>
            {
                Media("steam", MediaSourceKind.Steam),
                Media("unknown", MediaSourceKind.Unknown),
                Media("custom", MediaSourceKind.Custom),
                Media("xbox", MediaSourceKind.XboxGameBar)
            };
            var view = new ListCollectionView(items);
            var grid = CreateGrid(5);
            grid.ItemsSource = view;

            using (var controller = ProductionDataGridSortProfiles.AttachMediaInbox(grid))
            {
                controller.ApplySortForVerification(2, ListSortDirection.Ascending);
                Assert.Equal(new[] { "steam", "xbox", "custom", "unknown" }, view.Cast<MediaItemDto>().Select(item => item.MediaId).ToArray());

                controller.ApplySortForVerification(2, ListSortDirection.Descending);
                Assert.Equal(new[] { "custom", "xbox", "steam", "unknown" }, view.Cast<MediaItemDto>().Select(item => item.MediaId).ToArray());
                Assert.Equal(ListSortDirection.Descending, grid.Columns[2].SortDirection);
            }
        });
    }

    [Fact]
    public void ClickingARealColumnHeaderAppliesTheStableSortInBothDirections()
    {
        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var items = new ObservableCollection<BackupVersionDto>
                {
                    Backup("b", new DateTime(2026, 9, 18), 10, 20),
                    Backup("small", new DateTime(2026, 9, 18), 2, 10),
                    Backup("a", new DateTime(2026, 9, 18), 10, 20)
                };
                var view = new ListCollectionView(items);
                var grid = CreateGrid(7);
                grid.ItemsSource = view;

                using var controller = ProductionDataGridSortProfiles.AttachSaveHistory(grid);
                window = new Window
                {
                    Content = grid,
                    Width = 720,
                    Height = 260,
                    ShowInTaskbar = false,
                    WindowStyle = WindowStyle.ToolWindow
                };
                window.Show();
                window.UpdateLayout();
                grid.UpdateLayout();

                var header = FindVisualChildren<DataGridColumnHeader>(grid)
                    .Single(columnHeader => ReferenceEquals(columnHeader.Column, grid.Columns[2]));
                var onClick = typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new Xunit.Sdk.XunitException("WPF ButtonBase.OnClick was not found.");

                onClick.Invoke(header, null);
                Assert.Equal(new[] { "small", "a", "b" }, view.Cast<BackupVersionDto>().Select(item => item.BackupId).ToArray());
                Assert.Equal(ListSortDirection.Ascending, grid.Columns[2].SortDirection);

                onClick.Invoke(header, null);
                Assert.Equal(new[] { "a", "b", "small" }, view.Cast<BackupVersionDto>().Select(item => item.BackupId).ToArray());
                Assert.Equal(ListSortDirection.Descending, grid.Columns[2].SortDirection);
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Fact]
    public void ClickingAHeaderAfterItsCollectionViewWasDetachedDoesNotRefreshTheDeadView()
    {
        RunSta(() =>
        {
            var items = new ObservableCollection<BackupVersionDto>
            {
                Backup("b", new DateTime(2026, 9, 18), 10, 20),
                Backup("a", new DateTime(2026, 9, 18), 2, 10)
            };
            var view = new ListCollectionView(items);
            var grid = CreateGrid(7);
            grid.ItemsSource = view;

            using (var controller = ProductionDataGridSortProfiles.AttachSaveHistory(grid))
            {
                var activeKey = controller.ActiveColumnKey;
                var activeDirection = controller.ActiveDirection;
                Window? window = null;
                try
                {
                    window = new Window
                    {
                        Content = grid,
                        Width = 720,
                        Height = 260,
                        ShowInTaskbar = false,
                        WindowStyle = WindowStyle.ToolWindow
                    };
                    window.Show();
                    window.UpdateLayout();
                    grid.UpdateLayout();

                    var header = FindVisualChildren<DataGridColumnHeader>(grid)
                        .Single(columnHeader => ReferenceEquals(columnHeader.Column, grid.Columns[2]));
                    var onClick = typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?? throw new Xunit.Sdk.XunitException("WPF ButtonBase.OnClick was not found.");

                    view.DetachFromSourceCollection();
                    Assert.Null(view.SourceCollection);

                    var sortClick = Record.Exception(() => onClick.Invoke(header, null));

                    Assert.Null(sortClick);
                }
                finally
                {
                    window?.Close();
                }

                Assert.Equal(activeKey, controller.ActiveColumnKey);
                Assert.Equal(activeDirection, controller.ActiveDirection);
                Assert.Equal(activeDirection, grid.Columns[0].SortDirection);
                Assert.Null(grid.Columns[2].SortDirection);
            }
        });
    }

    [Fact]
    public void ProductionTablesAttachStableProfilesAndKeepSharedArrowContract()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var controller = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "DataGridStableSortController.cs"));
        var save = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml.cs"));
        var task = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml.cs"));
        var media = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml.cs"));
        var theme = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "WpfUiProduction.xaml"));

        Assert.Contains("StableCandidateKey", controller);
        Assert.Contains("IsUnknownProgress", controller);
        Assert.Contains("AttachSaveHistory", save);
        Assert.Contains("AttachSaveCandidates", save);
        Assert.Contains("AttachTasks", task);
        Assert.Contains("AttachMediaInbox", media);
        Assert.Contains("SortDirection\" Value=\"Ascending\"", theme);
        Assert.Contains("SortDirection\" Value=\"Descending\"", theme);
    }

    private static BackupVersionDto Backup(string id, DateTime createdUtc, int fileCount, long totalBytes)
        => new BackupVersionDto
        {
            BackupId = id,
            CreatedUtc = createdUtc,
            FileCount = fileCount,
            TotalBytes = totalBytes,
            SourceDevice = id,
            Comment = id
        };

    private static TaskStatusDto Task(string id, TaskState state, int progress, string stageMessage = "")
        => new TaskStatusDto
        {
            TaskId = id,
            TaskType = "Backup",
            GameName = id,
            State = state,
            ProgressPercent = progress,
            StageMessage = stageMessage,
            CreatedUtc = new DateTime(2026, 9, 18)
        };

    private static MediaItemDto Media(string id, MediaSourceKind source)
        => new MediaItemDto
        {
            MediaId = id,
            Source = source,
            Kind = MediaKind.Screenshot,
            OriginalPath = id + ".png",
            CapturedUtc = new DateTime(2026, 9, 18)
        };

    private static DataGrid CreateGrid(int columnCount)
    {
        var grid = new DataGrid { AutoGenerateColumns = false, CanUserAddRows = false };
        for (var index = 0; index < columnCount; index++)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "列" + index,
                Binding = new Binding(".")
            });
        }
        return grid;
    }

    private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) yield return match;
            foreach (var nested in FindVisualChildren<T>(child)) yield return nested;
        }
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
}
