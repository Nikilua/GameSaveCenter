using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

[Collection("ReportedWorkspaceLayoutWpf")]
public sealed class ReportedWorkspaceLayoutBehaviorTests
{
    private static readonly Lazy<Dispatcher> FallbackStaDispatcher = new(CreateFallbackStaDispatcher);
    private readonly ITestOutputHelper output;

    public ReportedWorkspaceLayoutBehaviorTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void CompactInboxKeepsTheAssignButtonCompactAndTheGridInsideItsFrameRow(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var buttonHeight = 0d;
        var comboHeight = 0d;
        var buttonCenterDelta = 0d;
        var gridToFooterOverlap = 0d;
        var gridHeight = 0d;
        var pageScrollable = false;
        var footerReachable = false;
        var footerViewportMetrics = string.Empty;
        var pageScrollProperties = string.Empty;
        var inboxTabVisible = false;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var view = new MediaCenterView
                {
                    DataContext = CreateMediaContext()
                };
                ApplyTheme(view, theme);
                var viewType = typeof(MediaCenterView);
                var tabs = (TabControl)viewType.GetField("MediaTabControl", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var actions = (FrameworkElement)viewType.GetField("MediaInboxTargetActions", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var grid = (DataGrid)viewType.GetField("MediaInboxGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var frame = (Border)viewType.GetField("MediaInboxTableFrame", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var footer = (FrameworkElement)viewType.GetField("MediaInboxFooter", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var pageScroller = (ScrollViewer)viewType.GetField("MediaInboxPageScrollViewer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                tabs.SelectedIndex = 0;

                window = CreateWindow(view, 1280, 720);
                view.ApplyResponsiveLayout(1280, 720);
                window.Show();
                FlushLayout(window);
                view.ApplyResponsiveLayout(1280, 720);
                FlushLayout(window);

                var targetCombo = (ComboBox)viewType.GetField("MediaInboxTargetGameComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var assignButton = (ButtonBase)viewType.GetField("MediaInboxAssignSelectedButton", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                buttonHeight = assignButton.ActualHeight;
                comboHeight = targetCombo.ActualHeight;
                buttonCenterDelta = Math.Abs(CenterY(assignButton, actions) - CenterY(targetCombo, actions));

                var gridBounds = BoundsIn(grid, frame);
                var footerBounds = BoundsIn(footer, frame);
                gridHeight = grid.ActualHeight;
                gridToFooterOverlap = Math.Max(0, gridBounds.Bottom - footerBounds.Top);
                pageScrollable = pageScroller.ScrollableHeight > 0.5;
                inboxTabVisible = pageScroller.IsVisible;
                var verticalScrollBar = FindVisualChildren<ScrollBar>(pageScroller)
                    .FirstOrDefault(scrollBar => scrollBar.Orientation == Orientation.Vertical);
                pageScrollProperties = $"loaded={pageScroller.IsLoaded}, visible={pageScroller.IsVisible}, enabled={pageScroller.IsEnabled}, mode={pageScroller.VerticalScrollBarVisibility}/{pageScroller.ComputedVerticalScrollBarVisibility}, contentScroll={pageScroller.CanContentScroll}, bar={verticalScrollBar?.Value:0.##}/{verticalScrollBar?.Maximum:0.##}";
                pageScroller.ScrollToVerticalOffset(pageScroller.ScrollableHeight);
                pageScroller.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => { }));
                FlushLayout(window);
                pageScrollProperties += $", afterScroll={pageScroller.VerticalOffset:0.##}/{pageScroller.ScrollableHeight:0.##}, barAfter={verticalScrollBar?.Value:0.##}";
                var footerInPage = BoundsIn(footer, pageScroller);
                footerReachable = footerInPage.Top >= -1 && footerInPage.Bottom <= pageScroller.ViewportHeight + 1;
                footerViewportMetrics = $"offset={pageScroller.VerticalOffset:0.##}, extent={pageScroller.ExtentHeight:0.##}, viewport={pageScroller.ViewportHeight:0.##}, footer={footerInPage.Top:0.##}..{footerInPage.Bottom:0.##}; {pageScrollProperties}";
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        output.WriteLine($"{theme} Media Inbox: button={buttonHeight:0.##} DIP, target={comboHeight:0.##} DIP, centerΔ={buttonCenterDelta:0.##} DIP, grid={gridHeight:0.##} DIP, grid/footer overlap={gridToFooterOverlap:0.##} DIP, {footerViewportMetrics}");
        Assert.Null(exception);
        Assert.True(comboHeight >= 35, $"synthetic selected game template measured unexpectedly short: {comboHeight:0.##} DIP");
        Assert.True(buttonHeight <= 52, $"assign button stretched to {buttonHeight:0.##} DIP beside a {comboHeight:0.##} DIP game target");
        Assert.True(buttonCenterDelta <= 1, $"assign button center drifted {buttonCenterDelta:0.##} DIP from its game target");
        Assert.True(gridHeight < 500, $"compact inbox grid exceeded its finite viewport: {gridHeight:0.##} DIP");
        Assert.True(gridToFooterOverlap <= 1, $"inbox grid overlaps the footer by {gridToFooterOverlap:0.##} DIP");
        Assert.True(inboxTabVisible, "the geometry probe must measure the selected inbox tab, not a hidden tab template");
        Assert.True(pageScrollable, "compact page fallback should retain its page-level scroll channel");
        Assert.True(footerReachable, $"the page scroll channel should bring the footer into view ({footerViewportMetrics})");
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void WindowedSaveHistoryDoesNotExpandTheSummaryCardAroundItsActions(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var summaryToGridGap = 0d;
        var detailsInspectorVisible = false;
        var summaryActionsRow = -1;
        var wideActionsRow = -1;
        var restoredWindowedActionsRow = -1;
        var summaryActionsContentGap = 0d;
        var summaryTrailingWhitespace = 0d;
        var summaryGeometryDetails = string.Empty;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var view = new SaveCenterView
                {
                    DataContext = new SavePageContext()
                };
                ApplyTheme(view, theme);
                var viewType = typeof(SaveCenterView);
                var summary = (Border)viewType.GetField("SaveHistorySummaryCard", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var actions = (FrameworkElement)viewType.GetField("SaveHistorySummaryActions", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var contentStack = (FrameworkElement)viewType.GetField("SaveHistorySummaryContentStack", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var grid = (DataGrid)viewType.GetField("SaveHistoryGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var inspector = (ScrollViewer)viewType.GetField("SaveHistoryActionsScrollViewer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;

                window = CreateWindow(view, 1400, 900);
                view.ApplyResponsiveLayout(1400, 900);
                window.Show();
                FlushLayout(window);
                view.ApplyResponsiveLayout(1400, 900);
                FlushLayout(window);

                var summaryBounds = BoundsIn(summary, view);
                var actionsBounds = BoundsIn(actions, summary);
                var contentBounds = BoundsIn(contentStack, summary);
                var gridBounds = BoundsIn(grid, view);
                summaryToGridGap = gridBounds.Top - summaryBounds.Bottom;
                summaryActionsRow = Grid.GetRow(actions);
                summaryActionsContentGap = actionsBounds.Top - contentBounds.Bottom;
                summaryTrailingWhitespace = summary.ActualHeight - actionsBounds.Bottom;
                detailsInspectorVisible = inspector.Visibility == Visibility.Visible && inspector.ActualWidth > 0 && inspector.ActualHeight > 0;
                summaryGeometryDetails = $"summary={summary.ActualWidth:0.##}×{summary.ActualHeight:0.##}, content={contentStack.ActualWidth:0.##}×{contentStack.ActualHeight:0.##}, actions={actions.ActualWidth:0.##}×{actions.ActualHeight:0.##}@row{summaryActionsRow}, selected={grid.SelectedItem != null}, inspector={inspector.Visibility}/{inspector.ActualWidth:0.##}×{inspector.ActualHeight:0.##}";

                window.Width = 2100;
                view.ApplyResponsiveLayout(2100, 900);
                FlushLayout(window);
                wideActionsRow = Grid.GetRow(actions);

                window.Width = 1400;
                view.ApplyResponsiveLayout(1400, 900);
                FlushLayout(window);
                restoredWindowedActionsRow = Grid.GetRow(actions);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        output.WriteLine($"{theme} Save history: {summaryGeometryDetails}; wideRow={wideActionsRow}, windowedAgainRow={restoredWindowedActionsRow}, action/content gap={summaryActionsContentGap:0.##} DIP, trailing whitespace={summaryTrailingWhitespace:0.##} DIP");
        Assert.Null(exception);
        Assert.True(detailsInspectorVisible, $"the synthetic selected-version inspector must be visible to reproduce the reported side-by-side layout ({summaryGeometryDetails})");
        Assert.Equal(1, summaryActionsRow);
        Assert.Equal(0, wideActionsRow);
        Assert.Equal(1, restoredWindowedActionsRow);
        Assert.InRange(summaryActionsContentGap, 8, 14);
        Assert.InRange(summaryTrailingWhitespace, 0, 18);
        Assert.InRange(summaryToGridGap, -1, 18);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void FailedTaskChromeTracksItsOwnRealizedRowBounds(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var errorChromeTopOffset = 0d;
        var errorChromeBottomOffset = 0d;
        var rowCount = 0;
        var realizedRowsAfterRecycle = 0;
        var miscoloredRows = 0;
        var realizedRowHeightSpread = 0d;
        var failedRowsChecked = 0;
        var recycledTargetRealized = false;
        var taskGridMetrics = string.Empty;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var view = new TaskCenterView();
                ApplyTheme(view, theme);
                var viewType = typeof(TaskCenterView);
                var grid = (DataGrid)viewType.GetField("TaskGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var tasks = Enumerable.Range(0, 2400)
                    .Select(index => new TaskStatusDto
                    {
                        TaskId = "layout-task-" + index,
                        TaskType = "MediaClassification",
                        GameId = "synthetic-game-" + index,
                        GameName = "Synthetic Game " + index,
                        State = index % 4 == 1 ? TaskState.Failed : TaskState.Succeeded,
                        ProgressPercent = index % 4 == 1 ? 82 : 100,
                        Message = index % 4 == 1 ? "Synthetic failure for row geometry" : "Synthetic success row"
                    })
                    .ToArray();
                grid.ItemsSource = tasks;

                window = CreateWindow(view, 1480, 900);
                window.Show();
                FlushLayout(window);
                view.ApplyResponsiveLayout(1480, 900);
                FlushLayout(window);
                taskGridMetrics = $"theme={theme}, grid={grid.ActualWidth:0.##}×{grid.ActualHeight:0.##}, visible={grid.IsVisible}, loaded={grid.IsLoaded}, visibility={grid.Visibility}, items={grid.Items.Count}, virtualized={VirtualizingPanel.GetIsVirtualizing(grid)}";

                var errorTint = grid.FindResource("GscErrorTintBrush") as Brush;
                var errorStroke = grid.FindResource("GscErrorBrush") as Brush;
                var initialRows = FindVisualChildren<DataGridRow>(grid)
                    .Where(candidate => candidate.Visibility == Visibility.Visible && candidate.ActualHeight > 0)
                    .ToArray();
                rowCount = initialRows.Length;

                ValidateRows(initialRows);
                grid.ScrollIntoView(tasks[1500]);
                FlushLayout(window);
                grid.UpdateLayout();
                FlushLayout(window);
                recycledTargetRealized = grid.ItemContainerGenerator.ContainerFromItem(tasks[1500]) is DataGridRow;
                var recycledRows = FindVisualChildren<DataGridRow>(grid)
                    .Where(candidate => candidate.Visibility == Visibility.Visible && candidate.ActualHeight > 0)
                    .ToArray();
                realizedRowsAfterRecycle = recycledRows.Length;
                ValidateRows(recycledRows);

                void ValidateRows(DataGridRow[] rows)
                {
                    var heights = rows.Select(row => row.ActualHeight).ToArray();
                    if (heights.Length > 0)
                        realizedRowHeightSpread = Math.Max(realizedRowHeightSpread, heights.Max() - heights.Min());

                    foreach (var row in rows)
                    {
                        if (!(row.DataContext is TaskStatusDto task))
                            throw new InvalidOperationException("A realized task row lost its task data context.");

                        row.ApplyTemplate();
                        var chrome = Assert.IsType<Border>(row.Template.FindName("RowChrome", row));
                        var rowBounds = BoundsIn(row, grid);
                        var chromeBounds = BoundsIn(chrome, grid);
                        errorChromeTopOffset = Math.Max(errorChromeTopOffset, chromeBounds.Top - rowBounds.Top);
                        errorChromeBottomOffset = Math.Max(errorChromeBottomOffset, rowBounds.Bottom - chromeBounds.Bottom);

                        var isErrorStyle = ReferenceEquals(chrome.Background, errorTint)
                            && ReferenceEquals(chrome.BorderBrush, errorStroke);
                        if (task.State == TaskState.Failed)
                            failedRowsChecked++;
                        if ((task.State == TaskState.Failed) != isErrorStyle)
                            miscoloredRows++;
                    }
                }
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        output.WriteLine($"{theme} Task grid: initial/recycled rows={rowCount}/{realizedRowsAfterRecycle}, far row reached={recycledTargetRealized}, failed rows checked={failedRowsChecked}, chrome status mismatches={miscoloredRows}, row-height spread={realizedRowHeightSpread:0.##} DIP, {taskGridMetrics}");
        Assert.Null(exception);
        Assert.True(rowCount is >= 5 and <= 16, $"initial viewport realized {rowCount} task rows; {taskGridMetrics}");
        Assert.True(realizedRowsAfterRecycle is >= 5 and <= 16, $"scrolled viewport realized {realizedRowsAfterRecycle} task rows; {taskGridMetrics}");
        Assert.True(recycledTargetRealized, "ScrollIntoView must reach the far synthetic item so this probe covers recycled containers");
        Assert.True(failedRowsChecked >= 4, $"expected failed rows in both realized viewports; checked {failedRowsChecked}");
        Assert.True(miscoloredRows == 0, $"a recycled task row retained stale failure chrome on {miscoloredRows} realized row(s)");
        Assert.True(realizedRowHeightSpread <= 1, $"realized task rows have a {realizedRowHeightSpread:0.##} DIP height spread");
        Assert.InRange(errorChromeTopOffset, 0, 4);
        Assert.InRange(errorChromeBottomOffset, 0, 4);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void SettingsHeaderAndPathActionsStayAnchoredToTheirLabelsAndEachOther(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var iconTitleCenterDelta = 0d;
        var searchTitleLeftDelta = 0d;
        var saveHintTitleTopDelta = 0d;
        var pathControlCenterSpread = 0d;
        var pathControlHeightSpread = 0d;
        var pathHeightDetails = string.Empty;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                EnsureApplicationResources();
                var view = new GameSaveCenter.Playnite.Settings.GameSaveCenterSettingsView();
                ApplyTheme(view, theme);
                var viewType = view.GetType();
                var headerGrid = (Grid)viewType.GetField("SettingsHeaderGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var icon = (FrameworkElement)viewType.GetField("SettingsHeaderIcon", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var title = (FrameworkElement)viewType.GetField("SettingsHeaderTitle", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var search = (FrameworkElement)viewType.GetField("SettingsSearchTextBox", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var saveHint = (FrameworkElement)viewType.GetField("SettingsSaveHint", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var pathCombo = (FrameworkElement)viewType.GetField("SettingsPathEditorComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var pathCard = (FrameworkElement)viewType.GetField("SettingsPathEditorCard", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;

                window = CreateWindow(view, 1280, 900);
                window.Show();
                FlushLayout(window);
                viewType.GetMethod("ApplyResponsiveLayout", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(view, new object[] { 1280d, 900d });
                FlushLayout(window);

                var iconBounds = BoundsIn(icon, headerGrid);
                var titleBounds = BoundsIn(title, headerGrid);
                var searchBounds = BoundsIn(search, headerGrid);
                var saveHintBounds = BoundsIn(saveHint, headerGrid);
                iconTitleCenterDelta = Math.Abs((iconBounds.Top + iconBounds.Height / 2) - (titleBounds.Top + titleBounds.Height / 2));
                searchTitleLeftDelta = Math.Abs(searchBounds.Left - titleBounds.Left);
                saveHintTitleTopDelta = Math.Abs(saveHintBounds.Top - titleBounds.Top);

                var buttons = FindVisualChildren<ButtonBase>(pathCard)
                    .Where(button => button.Content is string label && label is "浏览" or "校验" or "打开" or "复制")
                    .Cast<FrameworkElement>()
                    .ToArray();
                var pathControls = buttons.Concat(new[] { pathCombo }).ToArray();
                var pathCenters = pathControls.Select(control => CenterY(control, pathCard)).ToArray();
                var pathHeights = pathControls.Select(control => control.ActualHeight).ToArray();
                pathControlCenterSpread = pathCenters.Max() - pathCenters.Min();
                pathControlHeightSpread = pathHeights.Max() - pathHeights.Min();
                pathHeightDetails = string.Join(", ", pathControls.Select(control => $"{control.GetType().Name}/{(control is ContentControl content ? content.Content : "combo")}: {control.ActualHeight:0.##}"));
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        output.WriteLine($"{theme} Settings: icon/title centerΔ={iconTitleCenterDelta:0.##} DIP, search/title leftΔ={searchTitleLeftDelta:0.##} DIP, hint/title topΔ={saveHintTitleTopDelta:0.##} DIP, path control center spread={pathControlCenterSpread:0.##} DIP, height spread={pathControlHeightSpread:0.##} DIP ({pathHeightDetails})");
        Assert.Null(exception);
        Assert.True(iconTitleCenterDelta <= 20, $"settings icon center is {iconTitleCenterDelta:0.##} DIP from the title center");
        Assert.True(searchTitleLeftDelta <= 2, $"settings search starts {searchTitleLeftDelta:0.##} DIP away from the title edge");
        Assert.True(saveHintTitleTopDelta <= 36, $"settings save hint is {saveHintTitleTopDelta:0.##} DIP below the title");
        Assert.True(pathControlCenterSpread <= 3, $"path combo/actions centers span {pathControlCenterSpread:0.##} DIP");
        Assert.True(pathControlHeightSpread <= 10, $"path combo/actions heights differ by {pathControlHeightSpread:0.##} DIP ({pathHeightDetails})");
    }

    private static object CreateMediaContext()
    {
        var target = new SyntheticGameTarget
        {
            Name = "Synthetic Bongo Cat Game",
            PlatformDisplay = "Steam",
            IdentityDisplay = "Playnite ID · synthetic-game-id"
        };
        return new MediaPageContext
        {
            Games = new ObservableCollection<SyntheticGameTarget> { target },
            InboxTargetGame = target,
            MediaInboxItems = Enumerable.Range(0, 80).Select(index => new MediaItemDto
            {
                MediaId = "layout-media-" + index,
                Kind = MediaKind.Screenshot,
                Source = MediaSourceKind.WindowsScreenshot,
                ArchivePath = @"C:\synthetic\archive-" + index + ".png",
                OriginalPath = @"C:\synthetic\source-" + index + ".png",
                Sha256 = "synthetic-sha-" + index,
                CapturedUtc = DateTime.UtcNow.AddMinutes(-index),
                ClassificationState = "Inbox",
                ClassificationReason = "Synthetic layout row"
            }).ToArray()
        };
    }

    private static Window CreateWindow(UIElement content, double width, double height)
        => new()
        {
            Content = content,
            Width = width,
            Height = height,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };

    private static void EnsureApplicationResources()
    {
        var application = Application.Current ?? new Application();
        application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        if (!application.Resources.Contains("BaseTextBlockStyle"))
            application.Resources.Add("BaseTextBlockStyle", new Style(typeof(TextBlock)));
    }

    private static void ApplyTheme(FrameworkElement host, GameSaveCenterThemeMode mode)
    {
        var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
            host,
            glassEnabled: true,
            strengthPercent: 78,
            themeMode: mode,
            highContrastOverride: false);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(host.Resources, palette, glassEnabled: true, motionEnabled: true);
        host.UpdateLayout();
    }

    private static void RunSta(Action action)
    {
        Exception? exception = null;
        GetTestDispatcher().Invoke(new Action(() =>
        {
            try { action(); }
            catch (Exception caught) { exception = caught; }
        }));
        if (exception != null) throw exception;
    }

    private static Dispatcher GetTestDispatcher()
    {
        var application = Application.Current;
        if (application != null
            && !application.Dispatcher.HasShutdownStarted
            && !application.Dispatcher.HasShutdownFinished)
            return application.Dispatcher;

        return FallbackStaDispatcher.Value;
    }

    private static Dispatcher CreateFallbackStaDispatcher()
    {
        var ready = new ManualResetEvent(false);
        Dispatcher? dispatcher = null;
        var thread = new Thread(() =>
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            ready.Set();
            Dispatcher.Run();
        })
        {
            IsBackground = true,
            Name = "Reported workspace layout WPF STA"
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        ready.WaitOne();
        return dispatcher!;
    }

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(window.UpdateLayout));
        window.UpdateLayout();
    }

    private static double CenterY(FrameworkElement element, Visual ancestor)
        => BoundsIn(element, ancestor).Top + BoundsIn(element, ancestor).Height / 2;

    private static Rect BoundsIn(FrameworkElement element, Visual ancestor)
        => element.TransformToAncestor(ancestor).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

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

    private sealed class MediaPageContext
    {
        public ObservableCollection<SyntheticGameTarget> Games { get; set; } = new();
        public SyntheticGameTarget? InboxTargetGame { get; set; }
        public MediaItemDto[] MediaInboxItems { get; set; } = Array.Empty<MediaItemDto>();
        public string MediaInboxMode { get; set; } = "待归类";
        public int MediaTabIndex { get; set; }
    }

    private sealed class SyntheticGameTarget
    {
        public string Name { get; set; } = string.Empty;
        public string PlatformDisplay { get; set; } = string.Empty;
        public string IdentityDisplay { get; set; } = string.Empty;
    }

    private sealed class SavePageContext
    {
        public SavePageContext()
        {
            var backup = new { CreatedUtc = DateTime.UtcNow, Kind = "普通备份", GameName = "Synthetic Game" };
            Backups.Add(backup);
            SelectedBackup = backup;
        }

        public ObservableCollection<object> Backups { get; } = new();
        public object? SelectedBackup { get; set; }
        public int SaveTabIndex { get; set; }
        public string BackupHistoryRangeSummary { get; } = "全部时间 · 共 24 个版本；此文字仅用于验证窗口宽度变化时摘要卡不会被操作按钮撑高。";
        public bool SaveDetailsStaleVisible { get; } = true;
        public string SaveDetailsStateDetail { get; } = "合成旧状态：最近一次读取已过期，仍保留上次成功读取的历史版本和候选路径。当前窗口变窄时，较长说明应换行但不能让按钮在垂直方向居中落入大片空白。";
        public BackupPreviewDto BackupPreview { get; } = new()
        {
            State = "Ready",
            PathCount = 2,
            TotalBytes = 4096,
            Summary = "已完成合成预览；当前扫描范围包含两个受控测试目录，预览不创建归档。",
            Paths = new System.Collections.Generic.List<BackupPreviewPathDto>
            {
                new() { Path = @"C:\synthetic\save-root-one", SizeBytes = 2048 },
                new() { Path = @"C:\synthetic\save-root-two", SizeBytes = 2048 }
            }
        };
        public BackupResultDto BackupResult { get; } = new()
        {
            LocalState = "Succeeded",
            CloudState = "Failed",
            Summary = "合成备份已保留本地版本；云端镜像失败，当前 UI 仅用于验证长说明与多操作行的布局。",
            Remediation = "可在网络恢复并完成认证后单独重试云端上传；本地备份不会被覆盖。"
        };
    }
}

[CollectionDefinition("ReportedWorkspaceLayoutWpf", DisableParallelization = true)]
public sealed class ReportedWorkspaceLayoutWpfCollection
{
}
