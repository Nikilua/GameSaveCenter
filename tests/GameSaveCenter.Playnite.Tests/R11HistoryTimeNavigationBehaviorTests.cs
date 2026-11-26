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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
    public void GalleryHistoryReturnsToUnfilteredListInsteadOfFocusingNewestVersion()
    {
        RunSta(() =>
        {
            var now = DateTime.UtcNow;
            var model = CreateBareDashboardViewModel(Backup("newest", now), Backup("older", now.AddDays(-2)));
            SetField(model, "saveTabIndex", 4);
            model.BackupHistorySearchText = "newest";
            SetField(model, "isShowingFocusedHistoryBackup", true);

            Invoke(model, "OpenHistoryFromBackupGallery");

            Assert.Equal(0, model.SaveTabIndex);
            Assert.False(model.IsShowingFocusedHistoryBackup);
            Assert.Empty(model.BackupHistorySearchText);
            Assert.Equal(new[] { "newest", "older" }, model.BackupHistoryView.Cast<BackupVersionDto>().Select(item => item.BackupId));
        });
    }

    [Fact]
    public void GalleryHistoryRequestsInitialLoadAndKeepsFollowingTheVersionCollection()
    {
        RunSta(() =>
        {
            var model = CreateBareDashboardViewModel();
            SetField(model, "currentWorkspace", WorkspaceKind.Saves);
            SetField(model, "saveTabIndex", 4);
            WorkspaceKind? requested = null;
            model.WorkspaceLoadSchedulerOverrideForAudit = workspace => requested = workspace;

            Invoke(model, "OpenHistoryFromBackupGallery");
            var view = model.BackupHistoryView;
            var now = DateTime.UtcNow;
            model.Backups.ReplaceAll(new[] { Backup("newest", now) });
            model.Backups.ApplyBatch(() => model.Backups.Add(Backup("older", now.AddDays(-2))));

            Assert.Equal(WorkspaceKind.Saves, requested);
            Assert.Same(view, model.BackupHistoryView);
            Assert.Equal(new[] { "newest", "older" }, view.Cast<BackupVersionDto>().Select(item => item.BackupId));
        });
    }

    [Theory]
    [InlineData(-8, "备份前 8 分钟")]
    [InlineData(3, "备份后 3 分钟")]
    [InlineData(0, "与备份同一时间")]
    public void CoverCandidateUsesSelectedBackupTime(int minutes, string label)
    {
        var backup = new DateTime(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc);
        var media = new MediaItemDto { OriginalPath = "castle.png", CapturedUtc = backup.AddMinutes(minutes) };
        var candidate = new DashboardViewModel.BackupCoverCandidate(media, backup);

        Assert.Same(media, candidate.Media);
        Assert.Equal(label + " · castle.png", candidate.Display);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void GalleryBindsCardNavigationAndRecapSelectionInANarrowWindow(GameSaveCenterThemeMode theme)
    {
        RunSta(() =>
        {
            var now = DateTime.UtcNow;
            var state = new GalleryPageState(Backup("cover-version", now));
            var view = new SaveCenterView { DataContext = state };
            var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(view, true, 78, theme, false);
            AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(view.Resources, palette, glassEnabled: true, motionEnabled: false);
            var window = new Window { Width = 980, Height = 640, Content = view, ShowInTaskbar = false };
            try
            {
                window.Show();
                PumpLayout(window);
                var gallery = FindVisualChildren<ListBox>(view).Single(item =>
                    AutomationProperties.GetName(item) == "带封面的存档进度");
                Assert.True(gallery.IsVisible);
                var open = FindVisualChildren<ButtonBase>(gallery).Single(item => Equals(item.Content, "查看存档版本"));
                InvokeClick(open);
                Assert.Same(state.BackupGalleryItems[0], state.Opened);
                var check = FindVisualChildren<CheckBox>(gallery).Single(item => Equals(item.Content, "加入回顾"));
                check.IsChecked = true;
                PumpLayout(window);
                Assert.True(state.BackupGalleryItems[0].IsSelected);
            }
            finally { window.Close(); }
        });
    }

    private sealed class GalleryPageState
    {
        public GalleryPageState(BackupVersionDto backup)
        {
            BackupGalleryItems = new[] { new BackupGalleryCardViewModel(new BackupCoverGalleryEntryDto { Backup = backup }, () => true) };
            OpenBackupGalleryVersionCommand = new RelayCommand(item => Opened = (BackupGalleryCardViewModel)item!);
        }

        public int SaveTabIndex { get; set; } = 4;
        public BackupGalleryCardViewModel[] BackupGalleryItems { get; }
        public System.Windows.Input.ICommand OpenBackupGalleryVersionCommand { get; }
        public BackupGalleryCardViewModel? Opened { get; private set; }
    }

    [Fact]
    public void RecapPngPreservesTheRenderedPreviewAndCleansUpAfterFailedPublish()
    {
        RunSta(() =>
        {
            var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "gsc-recap-tests", Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(root);
            try
            {
                var pixels = new byte[] { 70, 105, 245, 255 };
                var photo = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, pixels, 4);
                var shortPreview = ProgressRecapRenderer.Render(new[]
                {
                    new ProgressRecapCard(photo, "2026-11-20", "王城门口"),
                    new ProgressRecapCard(null, "2026-11-24", "重新出发")
                });
                var preview = ProgressRecapRenderer.Render(new[]
                {
                    new ProgressRecapCard(photo, "2026-11-20", string.Concat(Enumerable.Repeat("沿着王城门口继续探索。", 14))),
                    new ProgressRecapCard(null, "2026-11-24", "重新出发")
                });
                Assert.True(preview.IsFrozen);
                Assert.True(preview.PixelHeight > shortPreview.PixelHeight);
                var destination = System.IO.Path.Combine(root, "recap.png");
                ProgressRecapRenderer.SavePng(preview, destination);
                using (var stream = System.IO.File.OpenRead(destination))
                {
                    var png = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];
                    Assert.Equal(preview.PixelWidth, png.PixelWidth);
                    Assert.Equal(preview.PixelHeight, png.PixelHeight);
                    Assert.Equal(ReadPixels(preview), ReadPixels(png));
                }
                var occupied = System.IO.Path.Combine(root, "existing");
                System.IO.Directory.CreateDirectory(occupied);
                Assert.ThrowsAny<System.IO.IOException>(() => ProgressRecapRenderer.SavePng(preview, occupied));
                Assert.True(System.IO.Directory.Exists(occupied));
                Assert.Equal(new[] { destination }, System.IO.Directory.GetFiles(root));
            }
            finally { System.IO.Directory.Delete(root, true); }
        });
    }

    private static byte[] ReadPixels(BitmapSource source)
    {
        var bitmap = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        var stride = bitmap.PixelWidth * 4;
        var pixels = new byte[stride * bitmap.PixelHeight];
        bitmap.CopyPixels(pixels, stride, 0);
        return pixels;
    }

    [Fact]
    public void PinsKeepBothSearchResultsOutsideTheLoadedHistory()
    {
        RunSta(() =>
        {
            var now = DateTime.UtcNow;
            var loaded = Backup("loaded", now);
            var first = Backup("older-A", now.AddDays(-40));
            var second = Backup("older-B", now.AddDays(-10));
            first.PlayniteId = second.PlayniteId = loaded.PlayniteId = "game";
            var model = CreateBareDashboardViewModel(loaded);
            using var picker = new GamePickerViewModel
            {
                SelectedItem = new GamePickerItem(new GameStatusDto { PlayniteId = "game", Name = "Demo" })
            };
            SetField(model, "gamePicker", picker);
            SetField(model, "currentWorkspace", WorkspaceKind.Saves);

            model.SelectedBackup = first;
            model.PinHistoryComparisonACommand.Execute(null);
            Assert.Same(first, model.CompareLeftBackup);
            Assert.Null(model.CompareRightBackup);
            model.BackupHistorySearchText = "other term";
            model.SelectedBackup = second;
            model.PinHistoryComparisonBCommand.Execute(null);

            Assert.Same(first, model.CompareLeftBackup);
            Assert.Same(second, model.CompareRightBackup);
            Assert.True(model.OpenPinnedHistoryComparisonCommand.CanExecute(null));
            Assert.Equal(new[] { first, second, loaded }, model.HistoryComparisonChoices);
            Assert.Single(model.Backups);
            model.ClearHistoryComparisonPinsCommand.Execute(null);
            Assert.False(model.HasPinnedHistoryComparison);
            Assert.Same(loaded, Assert.Single(model.HistoryComparisonChoices));
        });
    }

    [Fact]
    public void ArrivalAtTheTopInsertsOnceWithoutResettingExistingRows()
    {
        RunSta(() =>
        {
            var now = DateTime.UtcNow;
            var oldHead = Backup("old", now.AddMinutes(-1));
            var newHead = Backup("new", now);
            var model = CreateBareDashboardViewModel(oldHead);
            SetField(model, "selectedBackup", oldHead);
            var notifications = new List<System.Collections.Specialized.NotifyCollectionChangedAction>();
            model.Backups.CollectionChanged += (_, e) => notifications.Add(e.Action);
            var page = new BackupVersionPageDto
            {
                Items = new List<BackupVersionDto> { newHead, oldHead },
                TotalCount = 2
            };
            var apply = typeof(DashboardViewModel).GetMethod("ApplyBackupArrivalPage", BindingFlags.Instance | BindingFlags.NonPublic)!;

            Assert.True((bool)apply.Invoke(model, new object[] { page })!);
            Assert.True((bool)apply.Invoke(model, new object[] { page })!);
            Assert.Equal(new[] { newHead, oldHead }, model.Backups);
            Assert.Equal(System.Collections.Specialized.NotifyCollectionChangedAction.Add, Assert.Single(notifications));
            Assert.Same(oldHead, model.SelectedBackup);
            Assert.Same(newHead, Assert.Single(model.RecentBackupArrivals));
            Assert.False(model.HasPendingBackupArrivals);
        });
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void NewVersionsWaitOutsideTheViewportAndRevealTogetherOnlyOnRequest(GameSaveCenterThemeMode theme)
    {
        RunSta(() =>
        {
            var now = DateTime.UtcNow;
            var initial = Enumerable.Range(1, 40).Select(day => Backup($"old-{day}", now.AddDays(-day))).ToArray();
            var first = Backup("new-1", now.AddMinutes(-1));
            var second = Backup("new-2", now);
            var model = CreateBareDashboardViewModel(initial);
            SetField(model, "selectedBackup", initial[15]);
            var view = new SaveCenterView
            {
                DataContext = model,
                PreferencePersistenceOverrideForBehaviorTest = () => { },
                SystemMotionEnabledProviderForBehaviorTest = () => false
            };
            var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(view, true, 78, theme, false);
            AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(view.Resources, palette, glassEnabled: true, motionEnabled: false);
            var window = new Window { Width = 980, Height = 640, Content = view, ShowInTaskbar = false };
            var apply = typeof(DashboardViewModel).GetMethod("ApplyBackupArrivalPage", BindingFlags.Instance | BindingFlags.NonPublic)!;
            try
            {
                window.Show();
                PumpLayout(window);
                view.ApplyResponsiveLayout(view.ActualWidth, view.ActualHeight);
                PumpLayout(window);
                var history = FindVisualChildren<DataGrid>(view).Single(grid => grid.Name == "SaveHistoryGrid");
                var scroll = (ScrollViewer)history.Template.FindName("DG_ScrollViewer", history);
                history.ScrollIntoView(initial[30]);
                PumpLayout(window);
                Assert.True(scroll.VerticalOffset > 0, $"History viewport={scroll.ViewportHeight}, extent={scroll.ExtentHeight}");
                Assert.True(model.IsReadingOlderBackups, "The history view must report that older rows are visible.");
                FocusManager.SetFocusedElement(window, history);
                var offset = scroll.VerticalOffset;
                var currency = history.CurrentItem;
                var focus = Keyboard.FocusedElement;
                var firstPage = new BackupVersionPageDto { Items = new[] { first }.Concat(initial).ToList(), TotalCount = 41 };
                var secondPage = new BackupVersionPageDto { Items = new[] { second, first }.Concat(initial).ToList(), TotalCount = 42 };
                Assert.True((bool)apply.Invoke(model, new object[] { firstPage })!);
                Assert.True((bool)apply.Invoke(model, new object[] { secondPage })!);
                Assert.True((bool)apply.Invoke(model, new object[] { secondPage })!);
                PumpLayout(window);
                Assert.Equal("有 2 个新版本", model.PendingBackupArrivalText);
                Assert.Equal(offset, scroll.VerticalOffset);
                Assert.Same(initial[15], history.SelectedItem);
                Assert.Same(currency, history.CurrentItem);
                Assert.Same(focus, Keyboard.FocusedElement);
                Assert.Same(history, FocusManager.GetFocusedElement(window));
                Assert.Equal(40, history.Items.Count);

                var reveal = FindVisualChildren<ButtonBase>(view).Single(button => button.Name == "SaveHistoryArrivalsButton");
                Assert.True(reveal.IsVisible);
                InvokeClick(reveal);
                PumpLayout(window);
                Assert.Same(second, history.SelectedItem);
                Assert.Same(second, history.CurrentItem);
                Assert.Equal(42, history.Items.Count);
                Assert.False(model.HasPendingBackupArrivals);
                Assert.Contains(second, model.RecentBackupArrivals);
                Assert.Contains(first, model.RecentBackupArrivals);
                Assert.Contains(FindVisualChildren<TextBlock>(history),
                    label => label.IsVisible && AutomationProperties.GetName(label) == "新版本");
                Assert.True((bool)apply.Invoke(model, new object[] { secondPage })!);
                Assert.False(model.HasPendingBackupArrivals);
            }
            finally { window.Close(); }
        });
    }

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
        Assert.Contains("BackupHistorySearchText", save);
        Assert.Contains("Command=\"{Binding SearchAllBackupHistoryCommand}\"", save);
        Assert.Contains("StatusMessageFullDisplay, Mode=OneWay", System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml")));
        Assert.Contains("SaveHistoryGrid.SetBinding(ItemsControl.ItemsSourceProperty", saveCode);
        Assert.Contains("nameof(DashboardViewModel.BackupHistoryView)", saveCode);
        Assert.Contains("BackupHistoryDateRange.Matches", history);
        Assert.Contains("ContainsBackupHistoryText", history);
        Assert.Contains("ThenBy(item => item.BackupId", range);
    }

    [Fact]
    public void ProductionSaveHistorySearchFiltersLoadedMetadataAndKeepsMatchingSelection()
    {
        RunSta(() =>
        {
            var selected = Backup("archive-id-Alpha", DateTime.Now.Date.AddHours(10).ToUniversalTime());
            selected.Comment = "manual-checkpoint-alpha";
            selected.SourceDevice = "PLAYER-DESKTOP-01";
            selected.OperatingSystem = "Windows 11";
            var other = Backup("archive-id-Beta", DateTime.Now.Date.AddDays(-1).AddHours(10).ToUniversalTime());
            other.Comment = "weekly snapshot";
            other.SourceDevice = "LAPTOP-02";
            var viewModel = CreateBareDashboardViewModel(selected, other);
            SetField(viewModel, "selectedBackup", selected);
            var submittedText = string.Empty;
            SetField(viewModel, "<SearchAllBackupHistoryCommand>k__BackingField",
                new RelayCommand(_ => submittedText = viewModel.BackupHistorySearchText));

            var view = new SaveCenterView
            {
                DataContext = viewModel,
                Width = 1040,
                Height = 700,
                PreferencePersistenceOverrideForBehaviorTest = () => { }
            };
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

                var search = FindVisualChildren<TextBox>(view)
                    .Single(control => AutomationProperties.GetName(control) == "搜索存档历史");
                var history = (DataGrid)view.GetType()
                    .GetField("SaveHistoryGrid", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;
                var clear = FindVisualChildren<ButtonBase>(view)
                    .Single(control => AutomationProperties.GetName(control) == "清除存档历史搜索");

                Assert.Equal(2, viewModel.Backups.Count);
                Assert.Equal(2, history.Items.Count);
                Assert.Same(selected, history.SelectedItem);

                search.Text = "player-desktop";
                PumpLayout(window);
                Assert.Equal("player-desktop", viewModel.BackupHistorySearchText);
                Assert.Single(viewModel.BackupHistoryView.Cast<BackupVersionDto>());
                Assert.Same(selected, history.SelectedItem);
                Assert.Same(selected, viewModel.SelectedBackup);
                Assert.Equal(2, viewModel.Backups.Count);

                search.Text = "not-a-match";
                PumpLayout(window);
                Assert.Empty(viewModel.BackupHistoryView.Cast<BackupVersionDto>());
                Assert.Null(history.SelectedItem);
                Assert.Null(viewModel.SelectedBackup);

                var source = PresentationSource.FromVisual(search);
                Assert.NotNull(source);
                var enter = new KeyEventArgs(Keyboard.PrimaryDevice, source!, Environment.TickCount, Key.Enter)
                {
                    RoutedEvent = Keyboard.PreviewKeyDownEvent
                };
                search.RaiseEvent(enter);
                Assert.True(enter.Handled);
                Assert.Equal("not-a-match", submittedText);

                var escape = new KeyEventArgs(Keyboard.PrimaryDevice, source!, Environment.TickCount, Key.Escape)
                {
                    RoutedEvent = Keyboard.PreviewKeyDownEvent
                };
                search.RaiseEvent(escape);
                PumpLayout(window);
                Assert.True(escape.Handled);
                Assert.Equal(string.Empty, viewModel.BackupHistorySearchText);
                Assert.Equal(2, history.Items.Count);

                search.Text = "archive-id-alpha";
                PumpLayout(window);
                Assert.Single(history.Items.Cast<BackupVersionDto>());
                InvokeClick(clear);
                PumpLayout(window);
                Assert.Equal(string.Empty, viewModel.BackupHistorySearchText);
                Assert.Equal(2, history.Items.Count);
            }
            finally
            {
                window.Close();
            }
        });
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

            // This behavior fixture uses an uninitialized Playnite Plugin so debounced
            // column-width persistence must stay in the synthetic host boundary.
            var view = new SaveCenterView
            {
                DataContext = viewModel,
                Width = 1040,
                Height = 700,
                PreferencePersistenceOverrideForBehaviorTest = () => { }
            };
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
        SetField(viewModel, "backupHistorySearchText", string.Empty);
        SetField(viewModel, "restoreWorkflowSteps", new ObservableCollection<RestoreWorkflowStepState>());
        SetField(viewModel, "restoreWorkflowSummary", "选择一个版本后，按顺序完成四个阶段。");
        SetField(viewModel, "diffPathSearchText", string.Empty);
        SetField(viewModel, "diffPathKindFilter", "全部");
        SetField(viewModel, "diffPathVisibleLimit", 120);
        SetField(viewModel, "<VisibleDiffDirectories>k__BackingField", new BatchObservableCollection<BackupDiffDirectory>());
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
