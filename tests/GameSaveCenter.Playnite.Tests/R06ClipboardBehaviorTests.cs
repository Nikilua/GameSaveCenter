using System;
using System.Collections.ObjectModel;
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

public sealed class R06ClipboardBehaviorTests
{
    [Fact]
    public void SelectedRowsUseDisplayedOrderStableSeparatorsAndNoDuplicateIds()
    {
        RunSta(() =>
        {
            var first = Task("same-id", "第一条", "password=do-not-copy");
            var duplicate = Task("same-id", "重复对象", "token=also-do-not-copy");
            var last = Task("last-id", "最后一条", "路径 D:\\GameSaveCenter\\保存");
            var grid = CreateGrid(new[] { "本地时间", "任务", "阶段", "游戏", "状态", "进度", "详情" });
            DataGridClipboardBehavior.SetProfile(grid, "Task");
            grid.ItemsSource = new ObservableCollection<TaskStatusDto> { first, duplicate, last };
            grid.SelectedItems.Add(last);
            grid.SelectedItems.Add(first);
            grid.SelectedItems.Add(duplicate);

            var text = DataGridClipboardBehavior.BuildCopyTextForVerification(grid, copyCell: false);
            var rows = text.Split(new[] { "\r\n" }, StringSplitOptions.None);

            Assert.Equal(2, rows.Length);
            Assert.StartsWith(first.CreatedLocal.ToString("yyyy-MM-dd HH:mm:ss") + "\t存档备份\t阶段未知\t第一条", rows[0], StringComparison.Ordinal);
            Assert.Contains("\t最后一条\t", rows[1], StringComparison.Ordinal);
            Assert.DoesNotContain("重复对象", text, StringComparison.Ordinal);
            Assert.DoesNotContain("do-not-copy", text, StringComparison.Ordinal);
            Assert.DoesNotContain("also-do-not-copy", text, StringComparison.Ordinal);
            Assert.Contains("[已隐藏]", text, StringComparison.Ordinal);
            Assert.Contains("\t", text, StringComparison.Ordinal);
            Assert.DoesNotContain("...", text, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void CurrentCellKeepsFullTechnicalValueAndRedactsCredentialSyntax()
    {
        RunSta(() =>
        {
            var path = @"D:\GameSaveCenter\profiles\very-long-save-file-name-0001.dat";
            var candidate = new SavePathCandidateDto
            {
                PlayniteId = "game-1",
                Path = path,
                Score = 0.75,
                Reasons = new System.Collections.Generic.List<string> { "命中路径" }
            };
            var grid = CreateGrid(new[] { "可信度", "状态", "路径", "依据" });
            DataGridClipboardBehavior.SetProfile(grid, "SaveCandidate");
            grid.ItemsSource = new ObservableCollection<SavePathCandidateDto> { candidate };
            grid.CurrentCell = new DataGridCellInfo(candidate, grid.Columns[2]);

            var copiedPath = DataGridClipboardBehavior.BuildCopyTextForVerification(grid, copyCell: true);
            Assert.Equal(path, copiedPath);
            Assert.DoesNotContain("...", copiedPath, StringComparison.Ordinal);

            var secret = DataGridClipboardFormatter.FormatCellForVerification(
                "Task",
                "详情",
                Task("secret-id", "安全任务", "Authorization: Bearer hidden-token"));
            Assert.DoesNotContain("hidden-token", secret, StringComparison.Ordinal);
            Assert.Contains("[已隐藏]", secret, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void ProductionTablesExposeExplicitCopyProfilesAndShortcutContract()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var behavior = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "DataGridClipboardBehavior.cs"));
        var viewModel = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var save = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var task = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var media = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var maintenance = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        Assert.Contains("Ctrl+C", save, StringComparison.Ordinal);
        Assert.Contains("Profile=\"SaveHistory\"", save, StringComparison.Ordinal);
        Assert.Contains("Profile=\"SaveCandidate\"", save, StringComparison.Ordinal);
        Assert.Contains("Profile=\"Task\"", task, StringComparison.Ordinal);
        Assert.Contains("Profile=\"MediaInbox\"", media, StringComparison.Ordinal);
        Assert.Contains("Profile=\"Finding\"", maintenance, StringComparison.Ordinal);
        Assert.Contains("Ctrl+Shift+C", behavior, StringComparison.Ordinal);
        Assert.Contains("ClipboardCopyMode.None", behavior, StringComparison.Ordinal);
        Assert.Contains("ClipboardValueSanitizer", behavior, StringComparison.Ordinal);
        Assert.Contains("text = ClipboardValueSanitizer.Sanitize", viewModel, StringComparison.Ordinal);
        Assert.DoesNotContain("item.ToString()", behavior, StringComparison.Ordinal);
    }

    private static TaskStatusDto Task(string id, string gameName, string detail)
        => new TaskStatusDto
        {
            TaskId = id,
            TaskType = "Backup",
            GameName = gameName,
            State = TaskState.Failed,
            ProgressPercent = 10,
            CreatedUtc = new DateTime(2026, 9, 18, 12, 0, 0, DateTimeKind.Utc),
            ErrorCode = "TEST_FAILURE",
            ErrorMessage = detail
        };

    private static DataGrid CreateGrid(string[] headers)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            SelectionMode = DataGridSelectionMode.Extended,
            SelectionUnit = DataGridSelectionUnit.FullRow
        };
        foreach (var header in headers)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = header,
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
