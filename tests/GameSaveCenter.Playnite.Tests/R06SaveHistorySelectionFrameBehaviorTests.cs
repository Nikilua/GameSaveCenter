using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using Xunit;
using static GameSaveCenter.Playnite.Tests.R06SelectionStateBehaviorTests;

namespace GameSaveCenter.Playnite.Tests;

[Collection("SelectionFrameWpf")]
public sealed class R06SaveHistorySelectionFrameBehaviorTests
{
    [Fact]
    public void SelectedSaveHistoryFrameContainsVisibleTextAfterHorizontalScrollInBothThemes()
    {
        RunSta(() =>
        {
            var selected = new BackupVersionDto
            {
                BackupId = "selected-save-history-frame",
                CreatedUtc = new DateTime(2026, 7, 29, 13, 5, 0, DateTimeKind.Utc),
                FileCount = 2,
                TotalBytes = 11_786,
                SourceDevice = "SULT-设备信息较长用于横向滚动边界测量",
                Comment = "选择框右侧必须覆盖当前可见文字；滚动范围与完整备注仍应保留。"
            };
            var fixture = new SaveHistoryFixture(selected);
            var view = new SaveCenterView { DataContext = fixture };
            var window = new Window
            {
                Width = 1280,
                Height = 760,
                Content = view,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };

            try
            {
                foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
                {
                    ApplyTheme(view, view.Resources, theme);
                    if (!window.IsVisible)
                        window.Show();
                    view.ApplyResponsiveLayout(1280, 760);
                    PumpLayout(window);

                    var grid = Assert.IsType<DataGrid>(view.FindName("SaveHistoryGrid"));
                    // Match the table pane width in the supplied capture. At the full page
                    // width the star-sized note column expands and leaves no scroll range.
                    grid.Width = 740;
                    grid.HorizontalAlignment = HorizontalAlignment.Left;
                    PumpLayout(window);
                    var row = grid.ItemContainerGenerator.ContainerFromItem(selected) as DataGridRow
                        ?? throw new InvalidOperationException($"Save-history row was not realized in {theme}; window={window.ActualWidth:0.##}x{window.ActualHeight:0.##}/loaded={window.IsLoaded}, grid={grid.ActualWidth:0.##}x{grid.ActualHeight:0.##}, visible={grid.IsVisible}, loaded={grid.IsLoaded}, items={grid.Items.Count}, view={view.ActualWidth:0.##}x{view.ActualHeight:0.##}/loaded={view.IsLoaded}, page={(view.FindName("SaveHistoryPageScrollViewer") as FrameworkElement)?.ActualWidth:0.##}x{(view.FindName("SaveHistoryPageScrollViewer") as FrameworkElement)?.ActualHeight:0.##}, frame={(view.FindName("SaveHistoryTableFrame") as FrameworkElement)?.ActualWidth:0.##}x{(view.FindName("SaveHistoryTableFrame") as FrameworkElement)?.ActualHeight:0.##}, tab={R06SelectionStateBehaviorTests.FindVisualChildren<TabControl>(view).SingleOrDefault()?.SelectedIndex}.");
                    grid.SelectedItem = selected;
                    PumpLayout(window);

                    var scroll = Assert.Single(FindVisualChildren<ScrollViewer>(grid));
                    Assert.True(scroll.ScrollableWidth > 0,
                        $"{theme}: fixture must retain the horizontal-scroll viewport shown in the user screenshot; extent={scroll.ExtentWidth:0.##}, viewport={scroll.ViewportWidth:0.##}.");
                    scroll.ScrollToHorizontalOffset(Math.Min(88, scroll.ScrollableWidth));
                    PumpLayout(window);

                    var frame = VisibleBoundsRelativeTo(ChromeFor(row), grid);
                    var visibleContent = FindVisualChildren<DataGridCell>(row)
                        .SelectMany(cell => FindVisualChildren<FrameworkElement>(cell)
                            .Where(element => element is TextBlock
                                || element is Border border && !string.Equals(border.Name, "CellChrome", StringComparison.Ordinal))
                            .Select(element => new
                            {
                                Cell = cell.Column.DisplayIndex,
                                Element = element,
                                Bounds = VisibleBoundsRelativeTo(element, grid),
                                Value = element is TextBlock text ? text.Text : element.Name
                            }))
                        .Where(item => !item.Bounds.IsEmpty && item.Bounds.Width > 0 && item.Bounds.Height > 0)
                        .ToArray();
                    var rightmost = visibleContent.OrderByDescending(item => item.Bounds.Right).FirstOrDefault()
                        ?? throw new InvalidOperationException($"{theme}: no visible save-history cell content was measured after horizontal scrolling.");
                    var diagnostic = $"{theme}: grid={grid.ActualWidth:0.##}, row={row.ActualWidth:0.##}, visibleFrame={frame}, ancestors={DescribeVisualAncestors(ChromeFor(row), grid)}, rightmostCell[{rightmost.Cell}]={rightmost.Bounds}, element={rightmost.Element.GetType().Name}/{rightmost.Element.Name} '{rightmost.Value}', offset={scroll.HorizontalOffset:0.##}, viewport={scroll.ViewportWidth:0.##}, extent={scroll.ExtentWidth:0.##}";

                    Assert.True(frame.Width > 0 && frame.Height > 0, $"Selected frame is not visible: {diagnostic}");
                    Assert.All(visibleContent, item => Assert.True(
                        item.Bounds.Right <= frame.Right - 1d,
                        $"Visible save-history content escaped the selected frame: {diagnostic}; offendingCell[{item.Cell}]={item.Bounds} element={item.Element.GetType().Name}/{item.Element.Name} '{item.Value}'."));
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    private sealed class SaveHistoryFixture
    {
        public SaveHistoryFixture(BackupVersionDto selected)
        {
            SelectedBackup = selected;
            Backups = new ObservableCollection<BackupVersionDto> { selected };
        }

        public ObservableCollection<BackupVersionDto> Backups { get; }
        public ObservableCollection<SavePathCandidateDto> SaveCandidates { get; } = new();
        public BackupVersionDto SelectedBackup { get; set; }
        public object SelectedGame { get; } = new SaveHistoryGame();
        public int SaveTabIndex { get; set; }
        public bool SaveHistoryStateOverlayVisible => false;
        public bool SaveCandidateStateOverlayVisible => false;
        public bool SaveDetailsStaleVisible => false;
        public string SaveDetailsState => "Ready";
        public string SaveDetailsPresenterState => "Ready";
        public string SaveDetailsStateTitle => "已读取存档信息";
        public string SaveDetailsStateMessage => string.Empty;
        public string SaveDetailsStateDetail => string.Empty;
        public string BackupHistoryRangeSummary => "1 个合成版本";
        public string RestoreAvailabilityHint => "可先验证可恢复性；不会在此测试中执行恢复。";
    }

    private sealed class SaveHistoryGame
    {
        public string Name => "合成存档游戏";
        public string IdentityDisplay => "合成存档游戏 · 测试平台";
    }
}
