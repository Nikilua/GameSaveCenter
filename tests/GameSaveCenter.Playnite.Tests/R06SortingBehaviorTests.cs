using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
            var grid = CreateGrid(6);
            grid.ItemsSource = view;

            using (var controller = ProductionDataGridSortProfiles.AttachTasks(grid))
            {
                controller.ApplySortForVerification(4, ListSortDirection.Ascending);
                Assert.Equal(new[] { "two", "ten", "negative", "queued" }, view.Cast<TaskStatusDto>().Select(item => item.TaskId).ToArray());
                Assert.Equal(ListSortDirection.Ascending, grid.Columns[4].SortDirection);
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

    private static TaskStatusDto Task(string id, TaskState state, int progress)
        => new TaskStatusDto
        {
            TaskId = id,
            TaskType = "Backup",
            GameName = id,
            State = state,
            ProgressPercent = progress,
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
