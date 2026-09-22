using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;
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

    private static BackupVersionDto Backup(string id, DateTime createdUtc)
        => new BackupVersionDto { BackupId = id, CreatedUtc = createdUtc };
}
