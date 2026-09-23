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
    public void CompactInboxKeepsBatchButtonsCompactAndTheGridInsideItsFrameRow(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var buttonHeight = 0d;
        var buttonHeightSpread = 0d;
        var modeComboHeight = 0d;
        var comboHeight = 0d;
        var buttonCenterDelta = 0d;
        var clearButtonVisible = false;
        var buttonGeometryDetails = string.Empty;
        var gridToFooterOverlap = 0d;
        var gridHeight = 0d;
        var gridBarContained = false;
        var gridBarMetrics = string.Empty;
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
                var modeCombo = (ComboBox)viewType.GetField("MediaInboxModeCombo", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var clearButton = (ButtonBase)viewType.GetField("MediaInboxClearSelectionButton", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var resetButton = (ButtonBase)viewType.GetField("MediaInboxResetColumnWidthButton", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var assignButton = (ButtonBase)viewType.GetField("MediaInboxAssignSelectedButton", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                grid.SelectedIndex = 0;
                FlushLayout(window);
                grid.ScrollIntoView(grid.Items[grid.Items.Count - 1]);
                grid.UpdateLayout();
                FlushLayout(window);
                clearButtonVisible = clearButton.Visibility == Visibility.Visible && clearButton.ActualHeight > 0;
                var batchButtons = new FrameworkElement[] { clearButton, resetButton, assignButton };
                var buttonHeights = batchButtons.Select(button => button.ActualHeight).ToArray();
                var buttonCenters = batchButtons.Select(button => CenterY(button, window)).ToArray();
                buttonHeight = buttonHeights.Max();
                buttonHeightSpread = buttonHeights.Max() - buttonHeights.Min();
                modeComboHeight = modeCombo.ActualHeight;
                comboHeight = targetCombo.ActualHeight;
                buttonCenterDelta = buttonCenters.Max(center => Math.Abs(center - CenterY(targetCombo, window)));
                buttonGeometryDetails = $"clear/reset/assign={string.Join("/", buttonHeights.Select(value => value.ToString("0.##")))} DIP, mode={modeComboHeight:0.##} DIP, centersΔ={string.Join("/", buttonCenters.Select(center => Math.Abs(center - CenterY(targetCombo, window)).ToString("0.##")))} DIP";

                var gridBounds = BoundsIn(grid, frame);
                var footerBounds = BoundsIn(footer, frame);
                gridHeight = grid.ActualHeight;
                gridToFooterOverlap = Math.Max(0, gridBounds.Bottom - footerBounds.Top);
                var internalGridScrollViewer = FindVisualChildren<ScrollViewer>(grid)
                    .FirstOrDefault(scrollViewer => scrollViewer.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled && scrollViewer.ActualHeight > 0);
                var internalGridScrollBar = FindVisualChildren<ScrollBar>(grid)
                    .FirstOrDefault(scrollBar => scrollBar.Orientation == Orientation.Vertical && scrollBar.ActualHeight > 0);
                if (internalGridScrollBar != null)
                {
                    var gridBarBounds = BoundsIn(internalGridScrollBar, frame);
                    gridBarContained = gridBounds.Contains(gridBarBounds) && gridBarBounds.Bottom <= footerBounds.Top + 1;
                    gridBarMetrics = $"bar={gridBarBounds.Top:0.##}..{gridBarBounds.Bottom:0.##}, grid={gridBounds.Top:0.##}..{gridBounds.Bottom:0.##}, footerTop={footerBounds.Top:0.##}, internalOffset={internalGridScrollViewer?.VerticalOffset:0.##}/{internalGridScrollViewer?.ScrollableHeight:0.##}";
                }
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

        output.WriteLine($"{theme} Media Inbox: {buttonGeometryDetails}, max button={buttonHeight:0.##} DIP, spread={buttonHeightSpread:0.##} DIP, target={comboHeight:0.##} DIP, max centerΔ={buttonCenterDelta:0.##} DIP, clear visible={clearButtonVisible}, grid={gridHeight:0.##} DIP, grid/footer overlap={gridToFooterOverlap:0.##} DIP, internal scrollbar contained={gridBarContained} ({gridBarMetrics}), {footerViewportMetrics}");
        Assert.Null(exception);
        Assert.True(comboHeight >= 35, $"synthetic selected game template measured unexpectedly short: {comboHeight:0.##} DIP");
        Assert.True(clearButtonVisible, "the geometry probe must include the selected-media clear action shown in the reported state");
        Assert.InRange(buttonHeight, 32, 42);
        Assert.True(buttonHeightSpread <= 1, $"batch action button heights differ by {buttonHeightSpread:0.##} DIP: {buttonGeometryDetails}");
        Assert.InRange(modeComboHeight, 32, 42);
        Assert.True(buttonCenterDelta <= 1, $"a batch action button center drifted {buttonCenterDelta:0.##} DIP from its game target: {buttonGeometryDetails}");
        Assert.True(gridHeight < 500, $"compact inbox grid exceeded its finite viewport: {gridHeight:0.##} DIP");
        Assert.True(gridToFooterOverlap <= 1, $"inbox grid overlaps the footer by {gridToFooterOverlap:0.##} DIP");
        Assert.True(gridBarContained, $"the DataGrid's own scrollbar must stay inside its finite table frame and above the footer ({gridBarMetrics})");
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
    public void FailedTaskStatusStaysInItsCellAcrossRecyclingWithoutAFullRowFrame(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var rowChromePositionError = 0d;
        var rowCount = 0;
        var realizedRowsAfterRecycle = 0;
        var errorPresentationMismatches = 0;
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

                window = CreateWindow(view, 2048, 1152);
                window.Show();
                FlushLayout(window);
                view.ApplyResponsiveLayout(2048, 1152);
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
                        rowChromePositionError = Math.Max(rowChromePositionError, Math.Max(
                            Math.Max(Math.Abs(chromeBounds.Top - rowBounds.Top), Math.Abs(rowBounds.Bottom - chromeBounds.Bottom)),
                            Math.Max(Math.Abs(chromeBounds.Left - rowBounds.Left), Math.Abs(rowBounds.Right - chromeBounds.Right))));

                        var statusPill = FindVisualChildren<Border>(row).FirstOrDefault(candidate => candidate.Name == "TaskStatusPill");
                        var statusPillIsError = statusPill != null
                            && ReferenceEquals(statusPill.Background, errorTint)
                            && ReferenceEquals(statusPill.BorderBrush, errorStroke);
                        var rowHasErrorFrame = ReferenceEquals(chrome.Background, errorTint)
                            || ReferenceEquals(chrome.BorderBrush, errorStroke);
                        if (task.State == TaskState.Failed)
                        {
                            failedRowsChecked++;
                            if (!statusPillIsError || rowHasErrorFrame)
                                errorPresentationMismatches++;
                        }
                        else if (statusPillIsError || rowHasErrorFrame)
                            errorPresentationMismatches++;
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

        output.WriteLine($"{theme} Task grid: initial/recycled rows={rowCount}/{realizedRowsAfterRecycle}, far row reached={recycledTargetRealized}, failed rows checked={failedRowsChecked}, error-presentation mismatches={errorPresentationMismatches}, row chrome position error={rowChromePositionError:0.##} DIP, row-height spread={realizedRowHeightSpread:0.##} DIP, {taskGridMetrics}");
        Assert.Null(exception);
        Assert.True(rowCount is >= 5 and <= 32, $"initial viewport realized {rowCount} task rows; {taskGridMetrics}");
        Assert.True(realizedRowsAfterRecycle is >= 5 and <= 32, $"scrolled viewport realized {realizedRowsAfterRecycle} task rows; {taskGridMetrics}");
        Assert.True(recycledTargetRealized, "ScrollIntoView must reach the far synthetic item so this probe covers recycled containers");
        Assert.True(failedRowsChecked >= 4, $"expected failed rows in both realized viewports; checked {failedRowsChecked}");
        Assert.True(errorPresentationMismatches == 0, $"failure status left its status cell or a full-row error frame appeared on {errorPresentationMismatches} realized row(s)");
        Assert.True(realizedRowHeightSpread <= 1, $"realized task rows have a {realizedRowHeightSpread:0.##} DIP height spread");
        Assert.InRange(rowChromePositionError, 0, 1);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void SettingsHeaderAndPathActionsStayAnchoredToTheirLabelsAndEachOther(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var iconTitleTopDelta = 0d;
        var searchTitleLeftDelta = 0d;
        var searchHeight = 0d;
        var searchWidth = 0d;
        var compactSearchWidth = 0d;
        var compactSearchRightOverflow = 0d;
        var resetTitleLeftDelta = 0d;
        var resetAfterSearchGap = 0d;
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
                var resetCard = (FrameworkElement)viewType.GetField("SettingsResetDefaultsCard", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var saveHint = (FrameworkElement)viewType.GetField("SettingsSaveHint", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var pathCombo = (FrameworkElement)viewType.GetField("SettingsPathEditorComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var pathCard = (FrameworkElement)viewType.GetField("SettingsPathEditorCard", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;

                window = CreateWindow(view, 1280, 840);
                window.Show();
                FlushLayout(window);
                viewType.GetMethod("ApplyResponsiveLayout", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(view, new object[] { 1280d, 840d });
                FlushLayout(window);

                var iconBounds = BoundsIn(icon, headerGrid);
                var titleBounds = BoundsIn(title, headerGrid);
                var searchBounds = BoundsIn(search, headerGrid);
                var resetBounds = BoundsIn(resetCard, headerGrid);
                var saveHintBounds = BoundsIn(saveHint, headerGrid);
                iconTitleTopDelta = Math.Abs(iconBounds.Top - titleBounds.Top);
                searchTitleLeftDelta = Math.Abs(searchBounds.Left - titleBounds.Left);
                searchHeight = search.ActualHeight;
                searchWidth = search.ActualWidth;
                resetTitleLeftDelta = Math.Abs(resetBounds.Left - titleBounds.Left);
                resetAfterSearchGap = resetBounds.Top - searchBounds.Bottom;
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

                window.Width = 560;
                FlushLayout(window);
                viewType.GetMethod("ApplyResponsiveLayout", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(view, new object[] { 560d, 840d });
                FlushLayout(window);
                var compactSearchBounds = BoundsIn(search, headerGrid);
                compactSearchWidth = search.ActualWidth;
                compactSearchRightOverflow = Math.Max(0, compactSearchBounds.Right - headerGrid.ActualWidth);
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

        output.WriteLine($"{theme} Settings: icon/title topΔ={iconTitleTopDelta:0.##} DIP, search/title leftΔ={searchTitleLeftDelta:0.##} DIP, search size={searchWidth:0.##}×{searchHeight:0.##} DIP, reset/title leftΔ={resetTitleLeftDelta:0.##} DIP, reset/search gap={resetAfterSearchGap:0.##} DIP, compact search width/overflow={compactSearchWidth:0.##}/{compactSearchRightOverflow:0.##} DIP, hint/title topΔ={saveHintTitleTopDelta:0.##} DIP, path control center spread={pathControlCenterSpread:0.##} DIP, height spread={pathControlHeightSpread:0.##} DIP ({pathHeightDetails})");
        Assert.Null(exception);
        Assert.True(iconTitleTopDelta <= 12, $"settings icon top is {iconTitleTopDelta:0.##} DIP from the title top");
        Assert.True(searchTitleLeftDelta <= 2, $"settings search starts {searchTitleLeftDelta:0.##} DIP away from the title edge");
        Assert.InRange(searchWidth, 500, 520);
        Assert.InRange(searchHeight, 35, 37);
        Assert.InRange(compactSearchWidth, 260, 520);
        Assert.InRange(compactSearchRightOverflow, 0, 1);
        Assert.True(resetTitleLeftDelta <= 2, $"settings reset card starts {resetTitleLeftDelta:0.##} DIP away from the title edge");
        Assert.True(resetAfterSearchGap >= 8, $"settings reset card starts only {resetAfterSearchGap:0.##} DIP after the search field");
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
