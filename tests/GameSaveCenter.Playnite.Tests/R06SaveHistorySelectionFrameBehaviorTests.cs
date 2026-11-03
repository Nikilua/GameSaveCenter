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

public sealed class R06SaveHistorySelectionFrameBehaviorTests
{
    [Fact]
    public void SelectedSaveHistoryFrameContainsVisibleTextAfterHorizontalScrollInBothThemes()
    {
        RunSta(() =>
        {
            EnsureApplicationResources();
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
            var window = new Window
            {
                Width = 1280,
                Height = 760,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };

            try
            {
                foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
                {
                    var view = new SaveCenterView { DataContext = fixture, Width = 1280, Height = 760 };
                    window.Content = view;
                    ApplyTheme(view, view.Resources, theme);
                    view.ApplyResponsiveLayout(1280, 760);
                    if (!window.IsVisible)
                        window.Show();
                    PumpLayout(window);

                    var grid = Assert.IsType<DataGrid>(view.FindName("SaveHistoryGrid"));
                    var row = grid.ItemContainerGenerator.ContainerFromItem(selected) as DataGridRow
                        ?? throw new InvalidOperationException($"Save-history row was not realized in {theme}; grid={grid.ActualWidth:0.##}x{grid.ActualHeight:0.##}, items={grid.Items.Count}.");
                    grid.SelectedItem = selected;
                    PumpLayout(window);

                    var scroll = Assert.Single(FindVisualChildren<ScrollViewer>(grid));
                    Assert.True(scroll.ScrollableWidth > 0,
                        $"{theme}: fixture must retain the horizontal-scroll viewport shown in the user screenshot; extent={scroll.ExtentWidth:0.##}, viewport={scroll.ViewportWidth:0.##}.");
                    scroll.ScrollToHorizontalOffset(Math.Min(88, scroll.ScrollableWidth));
                    PumpLayout(window);

                    var frame = BoundsRelativeTo(ChromeFor(row), grid);
                    var visibleText = FindVisualChildren<DataGridCell>(row)
                        .SelectMany(cell => FindVisualChildren<TextBlock>(cell))
                        .Select(text => new
                        {
                            text.Text,
                            Bounds = VisibleBoundsRelativeTo(text, grid)
                        })
                        .Where(item => !item.Bounds.IsEmpty && item.Bounds.Width > 0 && !string.IsNullOrWhiteSpace(item.Text))
                        .ToArray();
                    var rightmost = visibleText.OrderByDescending(item => item.Bounds.Right).FirstOrDefault()
                        ?? throw new InvalidOperationException($"{theme}: no visible save-history cell text was measured after horizontal scrolling.");
                    var diagnostic = $"{theme}: grid={grid.ActualWidth:0.##}, row={row.ActualWidth:0.##}, frame={frame}, rightmostText={rightmost.Bounds} '{rightmost.Text}', offset={scroll.HorizontalOffset:0.##}, viewport={scroll.ViewportWidth:0.##}, extent={scroll.ExtentWidth:0.##}";

                    Assert.True(rightmost.Bounds.Right <= frame.Right - 4, diagnostic);
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
        public object? SelectedGame => null;
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
}
