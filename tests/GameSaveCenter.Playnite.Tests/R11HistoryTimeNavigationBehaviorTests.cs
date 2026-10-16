using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R11HistoryTimeNavigationBehaviorTests
{
    [Fact]
    public void LocalCalendarRangesHandleTodayYesterdayAndUnknownDates()
    {
        var localNow = DateTime.SpecifyKind(new DateTime(2026, 11, 1, 12, 0, 0), DateTimeKind.Local);
        var today = Backup("today", localNow.Date.AddHours(9).ToUniversalTime());
        var yesterday = Backup("yesterday", localNow.Date.AddDays(-1).AddHours(23).ToUniversalTime());
        var older = Backup("older", localNow.Date.AddDays(-8).AddHours(12).ToUniversalTime());
        var unknown = Backup("unknown", DateTime.MinValue);

        Assert.True(BackupHistoryDateRange.Matches(today, BackupHistoryDateRange.Today, localNow));
        Assert.False(BackupHistoryDateRange.Matches(yesterday, BackupHistoryDateRange.Today, localNow));
        Assert.True(BackupHistoryDateRange.Matches(yesterday, BackupHistoryDateRange.Yesterday, localNow));
        Assert.True(BackupHistoryDateRange.Matches(today, BackupHistoryDateRange.Last7Days, localNow));
        Assert.False(BackupHistoryDateRange.Matches(older, BackupHistoryDateRange.Last7Days, localNow));
        Assert.False(BackupHistoryDateRange.Matches(unknown, BackupHistoryDateRange.Last30Days, localNow));
        Assert.True(BackupHistoryDateRange.Matches(unknown, BackupHistoryDateRange.All, localNow));
    }

    [Fact]
    public void DaylightSavingTransitionsUseTargetLocalDateAndUtcNavigationOrder()
    {
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        var beforeSpringJump = new DateTime(2026, 3, 8, 6, 59, 0, DateTimeKind.Utc);
        var afterSpringJump = new DateTime(2026, 3, 8, 7, 1, 0, DateTimeKind.Utc);
        var priorLocalDay = Backup("prior-local-day", new DateTime(2026, 3, 8, 4, 59, 59, DateTimeKind.Utc));
        var firstLocalDayMinute = Backup("first-local-day-minute", new DateTime(2026, 3, 8, 5, 0, 0, DateTimeKind.Utc));

        Assert.Equal(new DateTime(2026, 3, 8, 1, 59, 0), BackupHistoryDateRange.ConvertUtcToLocal(beforeSpringJump, eastern));
        Assert.Equal(new DateTime(2026, 3, 8, 3, 1, 0), BackupHistoryDateRange.ConvertUtcToLocal(afterSpringJump, eastern));
        Assert.False(BackupHistoryDateRange.Matches(
            priorLocalDay,
            BackupHistoryDateRange.Today,
            new DateTime(2026, 3, 8, 12, 0, 0),
            eastern));
        Assert.True(BackupHistoryDateRange.Matches(
            firstLocalDayMinute,
            BackupHistoryDateRange.Today,
            new DateTime(2026, 3, 8, 12, 0, 0),
            eastern));

        var repeatedHourFirst = Backup("fall-first-utc", new DateTime(2026, 11, 1, 5, 30, 0, DateTimeKind.Utc));
        var repeatedHourSecond = Backup("fall-second-utc", new DateTime(2026, 11, 1, 6, 30, 0, DateTimeKind.Utc));
        Assert.Equal(
            BackupHistoryDateRange.ConvertUtcToLocal(repeatedHourFirst.CreatedUtc, eastern),
            BackupHistoryDateRange.ConvertUtcToLocal(repeatedHourSecond.CreatedUtc, eastern));
        Assert.Equal(new[] { "fall-second-utc", "fall-first-utc" },
            BackupHistoryDateRange.OrderForNavigation(new[] { repeatedHourFirst, repeatedHourSecond }, recentFirst: true)
                .Select(item => item.BackupId));
    }

    [Fact]
    public void NavigationKeepsSameSecondOrderStableAndLeavesUnknownLast()
    {
        var sameSecond = new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);
        var items = new List<BackupVersionDto>
        {
            Backup("b", sameSecond),
            Backup("a", sameSecond),
            Backup("older", sameSecond.AddMinutes(-1)),
            Backup("unknown", DateTime.MinValue)
        };

        Assert.Equal(new[] { "a", "b", "older", "unknown" },
            BackupHistoryDateRange.OrderForNavigation(items, recentFirst: true).Select(item => item.BackupId));
        Assert.Equal(new[] { "older", "a", "b", "unknown" },
            BackupHistoryDateRange.OrderForNavigation(items, recentFirst: false).Select(item => item.BackupId));
    }

    [Fact]
    public void JumpStatusUsesRelativeTextAndFullTimeEvidence()
    {
        var timestamp = DateTime.UtcNow.AddDays(-2);
        var backup = Backup("jump-time", timestamp);
        var unknown = Backup("jump-unknown", DateTime.MinValue);

        var recent = DashboardViewModel.BuildBackupJumpStatusMessage(backup, recentFirst: true);
        var recentFull = DashboardViewModel.BuildBackupJumpStatusFullDisplay(backup, recentFirst: true);
        var earlier = DashboardViewModel.BuildBackupJumpStatusMessage(backup, recentFirst: false);
        var earlierFull = DashboardViewModel.BuildBackupJumpStatusFullDisplay(backup, recentFirst: false);

        Assert.Contains("已跳到最近版本", recent, StringComparison.Ordinal);
        Assert.Contains(backup.CreatedRelativeDisplay, recent, StringComparison.Ordinal);
        Assert.Contains(backup.CreatedFullDisplay, recentFull, StringComparison.Ordinal);
        Assert.Contains("已跳到较早版本", earlier, StringComparison.Ordinal);
        Assert.Contains(backup.CreatedRelativeDisplay, earlier, StringComparison.Ordinal);
        Assert.Contains(backup.CreatedFullDisplay, earlierFull, StringComparison.Ordinal);
        Assert.Contains(unknown.CreatedRelativeDisplay, DashboardViewModel.BuildBackupJumpStatusMessage(unknown, true), StringComparison.Ordinal);
        Assert.Contains(unknown.CreatedFullDisplay, DashboardViewModel.BuildBackupJumpStatusFullDisplay(unknown, true), StringComparison.Ordinal);
    }

    [Fact]
    public void SaveHistoryExposesLocalRangeAndNavigationCommandsWithoutReplacingTheBackupCollectionBinding()
    {
        var root = TestRepositoryContext.Root;
        var save = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var saveCode = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml.cs"));
        var history = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.BackupHistory.cs"));
        var range = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "BackupHistoryDateRange.cs"));

        Assert.Contains("ItemsSource=\"{Binding Backups}\"", save);
        Assert.Contains("BackupHistoryRangeOptions", save);
        Assert.Contains("BackupHistoryRangeSummary", save);
        Assert.Contains("JumpToRecentBackupCommand", save);
        Assert.Contains("JumpToEarlierBackupCommand", save);
        Assert.Contains("StatusMessageFullDisplay, Mode=OneWay", System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml")));
        Assert.Contains("SaveHistoryGrid.SetBinding(ItemsControl.ItemsSourceProperty", saveCode);
        Assert.Contains("nameof(DashboardViewModel.BackupHistoryView)", saveCode);
        Assert.Contains("BackupHistoryDateRange.Matches", history);
        Assert.Contains("ThenBy(item => item.BackupId", range);
    }

    [Fact]
    public void ProductionSaveHistoryRangeAndNavigationControlsFilterRestoreAndSelectVersions()
    {
        RunSta(() =>
        {
            var today = Backup("today", DateTime.Now.Date.AddHours(10).ToUniversalTime());
            var older = Backup("older", DateTime.Now.Date.AddDays(-10).AddHours(10).ToUniversalTime());
            var unknown = Backup("unknown", DateTime.MinValue);
            var viewModel = CreateBareDashboardViewModel(today, older, unknown);
            SetField(viewModel, "selectedBackup", older);

            var view = new SaveCenterView { DataContext = viewModel, Width = 1040, Height = 700 };
            var window = new Window
            {
                Width = 1040,
                Height = 700,
                Content = view,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            try
            {
                window.Show();
                PumpLayout(window);

                var range = FindVisualChildren<ComboBox>(view)
                    .Single(control => AutomationProperties.GetName(control) == "历史版本日期范围");
                var history = (DataGrid)view.GetType()
                    .GetField("SaveHistoryGrid", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;
                var clearRange = FindVisualChildren<ButtonBase>(view)
                    .Single(control => AutomationProperties.GetName(control) == "清空历史版本日期范围");
                var jumpRecent = FindVisualChildren<ButtonBase>(view)
                    .Single(control => AutomationProperties.GetName(control) == "跳到最近历史版本");
                var jumpEarlier = FindVisualChildren<ButtonBase>(view)
                    .Single(control => AutomationProperties.GetName(control) == "跳到较早历史版本");

                Assert.Same(viewModel.BackupHistoryView, history.ItemsSource);
                Assert.Equal(3, history.Items.Count);

                range.SelectedItem = BackupHistoryDateRange.Last7Days;
                PumpLayout(window);
                Assert.Equal(BackupHistoryDateRange.Last7Days, viewModel.BackupHistoryRange);
                Assert.Single(history.Items.Cast<BackupVersionDto>());
                Assert.Same(today, history.Items[0]);
                Assert.True(jumpRecent.IsEnabled);
                Assert.True(jumpEarlier.IsEnabled);

                InvokeClick(jumpRecent);
                PumpLayout(window);
                Assert.Same(today, viewModel.SelectedBackup);
                Assert.Contains("已跳到最近版本", viewModel.StatusMessage, StringComparison.Ordinal);

                range.SelectedItem = BackupHistoryDateRange.Yesterday;
                PumpLayout(window);
                Assert.Empty(history.Items.Cast<BackupVersionDto>());
                Assert.False(viewModel.JumpToRecentBackupCommand.CanExecute(null));
                Assert.False(viewModel.JumpToEarlierBackupCommand.CanExecute(null));
                Assert.False(jumpRecent.IsEnabled);
                Assert.False(jumpEarlier.IsEnabled);
                Assert.Null(viewModel.SelectedBackup);

                InvokeClick(clearRange);
                PumpLayout(window);
                Assert.Equal(BackupHistoryDateRange.All, viewModel.BackupHistoryRange);
                Assert.Equal(3, history.Items.Count);
                Assert.False(clearRange.IsEnabled);
                Assert.Contains("共 3 个版本", viewModel.BackupHistoryRangeSummary);

                InvokeClick(jumpRecent);
                PumpLayout(window);
                Assert.Same(today, viewModel.SelectedBackup);
                InvokeClick(jumpEarlier);
                PumpLayout(window);
                Assert.Same(older, viewModel.SelectedBackup);
                Assert.Contains("已跳到较早版本", viewModel.StatusMessage, StringComparison.Ordinal);
                Assert.Contains(unknown, history.Items.Cast<BackupVersionDto>());
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static BackupVersionDto Backup(string id, DateTime createdUtc)
        => new BackupVersionDto { BackupId = id, CreatedUtc = createdUtc };

    private static DashboardViewModel CreateBareDashboardViewModel(params BackupVersionDto[] backups)
    {
        var settings = new GameSaveCenterSettings();
        var plugin = (GameSaveCenterPlugin)FormatterServices.GetUninitializedObject(typeof(GameSaveCenterPlugin));
        SetField(plugin, "<Settings>k__BackingField", settings);

        var viewModel = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
        SetField(viewModel, "plugin", plugin);
        SetField(viewModel, "commandRefreshScheduled", true);
        SetField(viewModel, "backupHistoryRange", BackupHistoryDateRange.All);
        SetField(viewModel, "restoreWorkflowSteps", new ObservableCollection<RestoreWorkflowStepState>());
        SetField(viewModel, "restoreWorkflowSummary", "选择一个版本后，按顺序完成四个阶段。");
        SetField(viewModel, "diffPathSearchText", string.Empty);
        SetField(viewModel, "diffPathKindFilter", "全部");
        SetField(viewModel, "diffPathVisibleLimit", 120);
        SetField(viewModel, "<DiffAddedPaths>k__BackingField", new BatchObservableCollection<string>());
        SetField(viewModel, "<DiffModifiedPaths>k__BackingField", new BatchObservableCollection<string>());
        SetField(viewModel, "<DiffRemovedPaths>k__BackingField", new BatchObservableCollection<string>());
        SetField(viewModel, "<Backups>k__BackingField", new BatchObservableCollection<BackupVersionDto>(backups));

        Invoke(viewModel, "OnBackupHistoryInitialize");
        return viewModel;
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        if (root == null) yield break;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T typed) yield return typed;
            foreach (var descendant in FindVisualChildren<T>(child)) yield return descendant;
        }
    }

    private static void InvokeClick(ButtonBase button)
    {
        var onClick = typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(onClick);
        onClick!.Invoke(button, null);
    }

    private static void Invoke(object target, string methodName)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method!.Invoke(target, null);
    }

    private static void SetField(object target, string name, object? value)
    {
        var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        window.UpdateLayout();
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
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}
