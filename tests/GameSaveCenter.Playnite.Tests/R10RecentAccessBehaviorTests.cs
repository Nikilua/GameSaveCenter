using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Newtonsoft.Json;
using PlayniteButton = GameSaveCenter.Playnite.Controls.Button;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R10RecentAccessBehaviorTests
{
    [Fact]
    public void RecentAccessRecordsAreBoundedStableIdOnlyAndPruneRemovedGames()
    {
        var now = DateTime.UtcNow;
        var source = Enumerable.Range(0, 10)
            .Select(index => new RecentAccessRecord
            {
                PlayniteId = $"game-{index}",
                Workspace = index == 0 ? "Unknown" : "Saves",
                TabIndex = 99,
                LastAccessUtc = now.AddMinutes(-index)
            })
            .ToList();
        source.Add(new RecentAccessRecord
        {
            PlayniteId = " game-1 ",
            Workspace = "Media",
            TabIndex = 2,
            LastAccessUtc = now.AddMinutes(1)
        });
        source.Add(new RecentAccessRecord { PlayniteId = "   " });

        var normalized = RecentAccessRecord.NormalizeMany(source);

        Assert.Equal(RecentAccessRecord.MaxEntries, normalized.Count);
        Assert.Equal("game-1", normalized[0].PlayniteId);
        Assert.Equal("Media", normalized[0].Workspace);
        Assert.Equal(2, normalized[0].TabIndex);
        Assert.Equal("Overview", normalized.Single(item => item.PlayniteId == "game-0").Workspace);
        Assert.DoesNotContain(normalized, item => item.PlayniteId == "game-9");

        var existing = RecentAccessRecord.KeepExistingGames(normalized, new[] { "game-1", "game-3" });
        Assert.Equal(new[] { "game-1", "game-3" }, existing.Select(item => item.PlayniteId));
        var olderExisting = RecentAccessRecord.KeepExistingGames(source, new[] { "game-9" });
        Assert.Equal("game-9", Assert.Single(olderExisting).PlayniteId);

        var json = JsonConvert.SerializeObject(normalized);
        Assert.Contains("PlayniteId", json);
        Assert.Contains("Workspace", json);
        Assert.DoesNotContain("GameName", json);
        Assert.DoesNotContain("Path", json);
        Assert.DoesNotContain("Archive", json);
    }

    [Fact]
    public void OverviewRecentAccessEntryUsesItsCommandAndKeepsTaskHistorySeparate()
    {
        Exception? exception = null;
        var clicks = 0;
        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                var data = new OverviewRecentAccessData(() => clicks++);
                data.RecentAccessItems.Add(new RecentAccessItem(
                    new RecentAccessRecord
                    {
                        PlayniteId = "game-1",
                        Workspace = RecentAccessRecord.MediaWorkspace,
                        TabIndex = 1,
                        LastAccessUtc = new DateTime(2026, 9, 19, 1, 2, 3, DateTimeKind.Utc)
                    },
                    "长名称测试游戏"));

                var overview = new OverviewView
                {
                    DataContext = data,
                    Width = 1280,
                    Height = 980
                };
                var host = new Grid { Width = 1280, Height = 980 };
                host.Children.Add(overview);
                host.Measure(new Size(1280, 980));
                host.Arrange(new Rect(0, 0, 1280, 980));
                overview.UpdateLayout();

                var button = FindVisualDescendants<PlayniteButton>(overview)
                    .SingleOrDefault(candidate => AutomationProperties.GetName(candidate) == "打开最近访问对象");
                Assert.NotNull(button);
                Assert.Same(data.OpenRecentAccessCommand, button!.Command);
                Assert.Equal("game-1", ((RecentAccessItem)button.CommandParameter!).PlayniteId);
                Assert.True(button.Command!.CanExecute(button.CommandParameter));

                typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(button, Array.Empty<object>());
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Equal(1, clicks);
    }

    private static List<T> FindVisualDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        var matches = new List<T>();
        Visit(root, matches);
        return matches;
    }

    private static void Visit<T>(DependencyObject? current, List<T> matches)
        where T : DependencyObject
    {
        if (current == null) return;
        if (current is T match) matches.Add(match);
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(current); index++)
            Visit(VisualTreeHelper.GetChild(current, index), matches);
    }

    private sealed class OverviewRecentAccessData
    {
        public OverviewRecentAccessData(Action onClick)
        {
            OpenRecentAccessCommand = new CountingCommand(onClick);
            OpenActivityCommand = new CountingCommand(() => { });
            OpenCloudQueueCommand = new CountingCommand(() => { });
            RefreshCommand = new CountingCommand(() => { });
        }

        public DashboardSnapshotDto Snapshot { get; } = new();
        public ObservableCollection<RecentAccessItem> RecentAccessItems { get; } = new();
        public ObservableCollection<TaskStatusDto> OverviewTasks { get; } = new();
        public ObservableCollection<ActivityEntryDto> Activities { get; } = new();
        public ICommand OpenRecentAccessCommand { get; }
        public ICommand OpenActivityCommand { get; }
        public ICommand OpenCloudQueueCommand { get; }
        public ICommand RefreshCommand { get; }
    }

    private sealed class CountingCommand : ICommand
    {
        private readonly Action execute;

        public CountingCommand(Action execute) => this.execute = execute;

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute();
    }
}
