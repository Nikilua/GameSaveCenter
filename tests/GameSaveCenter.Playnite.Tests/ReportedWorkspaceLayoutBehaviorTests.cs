using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

[Collection("ReportedWorkspaceLayoutWpf")]
public sealed class ReportedWorkspaceLayoutBehaviorTests
{
    private readonly ITestOutputHelper output;
    private readonly ReportedWorkspaceLayoutWpfFixture fixture;
    private readonly Dispatcher dispatcher;

    public ReportedWorkspaceLayoutBehaviorTests(ITestOutputHelper output, ReportedWorkspaceLayoutWpfFixture fixture)
    {
        this.output = output;
        this.fixture = fixture;
        dispatcher = fixture.Dispatcher;
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
        var initialGlobalTargetIsVisibleInBothSummaries = false;
        var globalTargetSwitchUpdatesBothSummaries = false;
        var clearingGlobalTargetUsesEmptyStateInBothSummaries = false;
        var restoringGlobalTargetUpdatesBothSummaries = false;
        var globalTargetBindingDetails = string.Empty;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var mediaContext = CreateMediaContext();
                var view = new MediaCenterView
                {
                    DataContext = mediaContext
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

                var targetSummary = (FrameworkElement)viewType.GetField("MediaInboxGlobalTargetSummary", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var inspectorTargetSummary = (FrameworkElement)viewType.GetField("MediaInboxInspectorGlobalTargetSummary", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var toolbarTargetName = FindBoundText(targetSummary, "SelectedGame.Name");
                var inspectorTargetName = FindBoundText(inspectorTargetSummary, "SelectedGame.Name");
                var initialTarget = mediaContext.SelectedGame!;
                initialGlobalTargetIsVisibleInBothSummaries =
                    toolbarTargetName.Text == initialTarget.Name &&
                    inspectorTargetName.Text == initialTarget.Name &&
                    Equals(toolbarTargetName.ToolTip, initialTarget.IdentityDisplay) &&
                    Equals(inspectorTargetName.ToolTip, initialTarget.IdentityDisplay) &&
                    System.Windows.Automation.AutomationProperties.GetHelpText(toolbarTargetName) == initialTarget.IdentityDisplay &&
                    System.Windows.Automation.AutomationProperties.GetHelpText(inspectorTargetName) == initialTarget.IdentityDisplay;
                var nextTarget = new SyntheticGameTarget
                {
                    Name = "Synthetic Second Target",
                    PlatformDisplay = "GOG",
                    IdentityDisplay = "Playnite ID · synthetic-second-id"
                };
                mediaContext.SelectedGame = nextTarget;
                FlushLayout(window);
                globalTargetSwitchUpdatesBothSummaries =
                    toolbarTargetName.Text == nextTarget.Name &&
                    inspectorTargetName.Text == nextTarget.Name &&
                    Equals(toolbarTargetName.ToolTip, nextTarget.IdentityDisplay) &&
                    Equals(inspectorTargetName.ToolTip, nextTarget.IdentityDisplay) &&
                    System.Windows.Automation.AutomationProperties.GetHelpText(toolbarTargetName) == nextTarget.IdentityDisplay &&
                    System.Windows.Automation.AutomationProperties.GetHelpText(inspectorTargetName) == nextTarget.IdentityDisplay;
                globalTargetBindingDetails = $"switch={toolbarTargetName.Text}/{inspectorTargetName.Text}; identity={toolbarTargetName.ToolTip}/{inspectorTargetName.ToolTip}";
                mediaContext.SelectedGame = null;
                FlushLayout(window);
                clearingGlobalTargetUsesEmptyStateInBothSummaries =
                    toolbarTargetName.Text == "未选择游戏" &&
                    inspectorTargetName.Text == "未选择游戏" &&
                    Equals(toolbarTargetName.ToolTip, "未选择游戏") &&
                    Equals(inspectorTargetName.ToolTip, "未选择游戏") &&
                    System.Windows.Automation.AutomationProperties.GetHelpText(toolbarTargetName) == "未选择游戏" &&
                    System.Windows.Automation.AutomationProperties.GetHelpText(inspectorTargetName) == "未选择游戏";
                globalTargetBindingDetails += $"; cleared={toolbarTargetName.Text}/{inspectorTargetName.Text}; empty={clearingGlobalTargetUsesEmptyStateInBothSummaries}";
                mediaContext.SelectedGame = initialTarget;
                FlushLayout(window);
                restoringGlobalTargetUpdatesBothSummaries =
                    toolbarTargetName.Text == initialTarget.Name &&
                    inspectorTargetName.Text == initialTarget.Name &&
                    Equals(toolbarTargetName.ToolTip, initialTarget.IdentityDisplay) &&
                    Equals(inspectorTargetName.ToolTip, initialTarget.IdentityDisplay) &&
                    System.Windows.Automation.AutomationProperties.GetHelpText(toolbarTargetName) == initialTarget.IdentityDisplay &&
                    System.Windows.Automation.AutomationProperties.GetHelpText(inspectorTargetName) == initialTarget.IdentityDisplay;
                globalTargetBindingDetails += $"; restored={toolbarTargetName.Text}/{inspectorTargetName.Text}";
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
                comboHeight = targetSummary.ActualHeight;
                buttonCenterDelta = buttonCenters.Max(center => Math.Abs(center - CenterY(targetSummary, window)));
                buttonGeometryDetails = $"clear/reset/assign={string.Join("/", buttonHeights.Select(value => value.ToString("0.##")))} DIP, mode={modeComboHeight:0.##} DIP, target={comboHeight:0.##} DIP, centersΔ={string.Join("/", buttonCenters.Select(center => Math.Abs(center - CenterY(targetSummary, window)).ToString("0.##")))} DIP";

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

        output.WriteLine($"{theme} Media Inbox: {buttonGeometryDetails}, max button={buttonHeight:0.##} DIP, spread={buttonHeightSpread:0.##} DIP, target={comboHeight:0.##} DIP, max centerΔ={buttonCenterDelta:0.##} DIP, clear visible={clearButtonVisible}, global target binding={globalTargetBindingDetails}, grid={gridHeight:0.##} DIP, grid/footer overlap={gridToFooterOverlap:0.##} DIP, internal scrollbar contained={gridBarContained} ({gridBarMetrics}), {footerViewportMetrics}");
        Assert.Null(exception);
        Assert.True(comboHeight >= 35, $"synthetic selected game template measured unexpectedly short: {comboHeight:0.##} DIP");
        Assert.True(clearButtonVisible, "the geometry probe must include the selected-media clear action shown in the reported state");
        Assert.InRange(buttonHeight, 32, 42);
        Assert.True(buttonHeightSpread <= 1, $"batch action button heights differ by {buttonHeightSpread:0.##} DIP: {buttonGeometryDetails}");
        Assert.InRange(modeComboHeight, 32, 42);
        Assert.True(buttonCenterDelta <= 1, $"a batch action button center drifted {buttonCenterDelta:0.##} DIP from its game target: {buttonGeometryDetails}");
        Assert.True(initialGlobalTargetIsVisibleInBothSummaries, $"the selected global game's initial name, identity, and accessibility text must appear in both summaries ({globalTargetBindingDetails})");
        Assert.True(globalTargetSwitchUpdatesBothSummaries, $"changing the global game must update toolbar and inspector names, identity tooltips, and accessibility help text ({globalTargetBindingDetails})");
        Assert.True(clearingGlobalTargetUsesEmptyStateInBothSummaries, $"clearing the global game must leave both summaries in the empty state ({globalTargetBindingDetails})");
        Assert.True(restoringGlobalTargetUpdatesBothSummaries, $"restoring the global game must refresh both summaries before geometry is measured ({globalTargetBindingDetails})");
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
    public void MediaInboxPolicyHintCollapsesWhenItemsArePresentAndStaysOutOfIgnoredMode(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var populatedVisibility = Visibility.Visible;
        var emptyVisibility = Visibility.Collapsed;
        var restoredVisibility = Visibility.Visible;
        var ignoredVisibility = Visibility.Visible;
        var populatedHintHeight = 0d;
        var emptyHintHeight = 0d;
        var restoredHintHeight = 0d;
        var populatedBandHeight = 0d;
        var emptyBandHeight = 0d;
        var restoredBandHeight = 0d;
        var visibleCopy = string.Empty;
        var titleAutomationName = string.Empty;
        var accessibleHelp = string.Empty;
        var titleToolTip = string.Empty;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var mediaContext = CreateMediaContext();
                var firstMedia = mediaContext.MediaInboxItems[0];
                var view = new MediaCenterView { DataContext = mediaContext };
                ApplyTheme(view, theme);
                window = CreateWindow(view, 1280, 720);
                window.Show();

                var viewType = typeof(MediaCenterView);
                var hint = (TextBlock)viewType.GetField("MediaInboxInfoDescription", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var title = (TextBlock)viewType.GetField("MediaInboxTitleText", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var band = (FrameworkElement)viewType.GetField("MediaInboxInfoBand", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                FlushLayout(window);
                populatedVisibility = hint.Visibility;
                populatedHintHeight = hint.ActualHeight;
                populatedBandHeight = band.ActualHeight;
                titleAutomationName = AutomationProperties.GetName(title) ?? string.Empty;
                accessibleHelp = AutomationProperties.GetHelpText(title) ?? string.Empty;
                titleToolTip = title.ToolTip as string ?? string.Empty;

                mediaContext.MediaInboxItems.Clear();
                FlushLayout(window);
                emptyVisibility = hint.Visibility;
                emptyHintHeight = hint.ActualHeight;
                emptyBandHeight = band.ActualHeight;
                visibleCopy = hint.Text;

                mediaContext.MediaInboxItems.Add(firstMedia);
                FlushLayout(window);
                restoredVisibility = hint.Visibility;
                restoredHintHeight = hint.ActualHeight;
                restoredBandHeight = band.ActualHeight;

                mediaContext.MediaInboxItems.Clear();
                mediaContext.MediaInboxMode = "已忽略";
                FlushLayout(window);
                ignoredVisibility = hint.Visibility;
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

        output.WriteLine($"{theme} Media Inbox policy copy: helper={populatedVisibility}/{populatedHintHeight:0.##} DIP -> {emptyVisibility}/{emptyHintHeight:0.##} DIP -> {restoredVisibility}/{restoredHintHeight:0.##} DIP; info band={populatedBandHeight:0.##}->{emptyBandHeight:0.##}->{restoredBandHeight:0.##} DIP; ignored={ignoredVisibility}; text={visibleCopy}; title={titleAutomationName}; UIA={accessibleHelp}; tooltip={titleToolTip}");
        Assert.Null(exception);
        Assert.Equal(Visibility.Collapsed, populatedVisibility);
        Assert.Equal(0, populatedHintHeight);
        Assert.Equal(Visibility.Visible, emptyVisibility);
        Assert.True(emptyHintHeight >= 10, $"the no-data policy hint should have a visible line height ({emptyHintHeight:0.##} DIP)");
        Assert.Equal("归属不明的媒体会留在待归类中，不会自动猜测。", visibleCopy);
        Assert.Equal("待归类媒体", titleAutomationName);
        Assert.Contains("公共截图和录像", accessibleHelp, StringComparison.Ordinal);
        Assert.Contains("不会静默猜测", accessibleHelp, StringComparison.Ordinal);
        Assert.Contains("不会静默猜测", titleToolTip, StringComparison.Ordinal);
        Assert.Equal(Visibility.Collapsed, restoredVisibility);
        Assert.Equal(0, restoredHintHeight);
        Assert.InRange(Math.Abs(restoredBandHeight - populatedBandHeight), 0, 1);
        Assert.Equal(Visibility.Collapsed, ignoredVisibility);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void MaintenanceDiagnosticEmptyCopyIsAccurateAndCollapsesWithData(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var emptyVisibility = Visibility.Collapsed;
        var populatedVisibility = Visibility.Visible;
        var restoredVisibility = Visibility.Collapsed;
        var auditEmptyVisibility = Visibility.Collapsed;
        var auditPopulatedVisibility = Visibility.Visible;
        var auditRestoredVisibility = Visibility.Collapsed;
        var copy = string.Empty;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var probe = new MaintenanceEmptyStateCopyProbe();
                var view = new MaintenanceView { DataContext = probe };
                ApplyTheme(view, theme);
                var emptyState = (TextBlock)typeof(MaintenanceView)
                    .GetField("MaintenanceFindingsEmptyStateText", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;

                window = CreateWindow(view, 1100, 720);
                window.Show();
                FlushLayout(window);
                emptyVisibility = emptyState.Visibility;
                copy = emptyState.Text;

                probe.Findings.Add(new { Title = "Synthetic diagnostic" });
                probe.MaintenanceState = "Ready";
                FlushLayout(window);
                populatedVisibility = emptyState.Visibility;

                probe.Findings.Clear();
                probe.MaintenanceState = "Empty";
                FlushLayout(window);
                restoredVisibility = emptyState.Visibility;

                var tabs = (TabControl)typeof(MaintenanceView)
                    .GetField("MaintenanceTabControl", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;
                tabs.SelectedIndex = 4;
                FlushLayout(window);
                var auditEmptyState = (TextBlock)typeof(MaintenanceView)
                    .GetField("MaintenanceAuditFindingsEmptyStateText", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;
                auditEmptyVisibility = auditEmptyState.Visibility;

                probe.Findings.Add(new { Title = "Synthetic diagnostic" });
                probe.MaintenanceState = "Ready";
                FlushLayout(window);
                auditPopulatedVisibility = auditEmptyState.Visibility;

                probe.Findings.Clear();
                probe.MaintenanceState = "Empty";
                FlushLayout(window);
                auditRestoredVisibility = auditEmptyState.Visibility;
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

        output.WriteLine($"{theme} maintenance diagnostics empty hints main={emptyVisibility}->{populatedVisibility}->{restoredVisibility}, audit={auditEmptyVisibility}->{auditPopulatedVisibility}->{auditRestoredVisibility}; copy=\"{copy}\"");
        Assert.Null(exception);
        Assert.Equal("暂无待处理诊断项。", copy);
        Assert.Equal(Visibility.Visible, emptyVisibility);
        Assert.Equal(Visibility.Collapsed, populatedVisibility);
        Assert.Equal(Visibility.Visible, restoredVisibility);
        Assert.Equal(Visibility.Visible, auditEmptyVisibility);
        Assert.Equal(Visibility.Collapsed, auditPopulatedVisibility);
        Assert.Equal(Visibility.Visible, auditRestoredVisibility);
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
    public void RemainingVisibleSaveAndOverviewActionGroupsAreMeasuredAcrossCompactWidths(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var saveSamples = new List<string>();
        var overviewSamples = new List<string>();

        RunSta(() =>
        {
            Window? saveWindow = null;
            Window? overviewWindow = null;
            try
            {
                var saveView = new SaveCenterView { DataContext = new SavePageContext { SaveTabIndex = 1 } };
                ApplyTheme(saveView, theme);
                var saveActions = (WrapPanel)typeof(SaveCenterView)
                    .GetField("SaveCurrentRuleActions", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(saveView)!;
                saveWindow = CreateWindow(saveView, 900, 700);
                saveWindow.Show();
                foreach (var width in new[] { 520d, 560d, 600d, 640d, 680d, 699d, 700d, 760d, 900d })
                {
                    saveWindow.Width = width;
                    saveView.ApplyResponsiveLayout(width, 700);
                    FlushLayout(saveWindow);
                    saveView.ApplyResponsiveLayout(width, 700);
                    FlushLayout(saveWindow);
                    var (rows, gap) = MeasureWrapRows(saveActions, saveView);
                    Assert.Equal(1, rows);
                    Assert.Equal(3, saveActions.Children.OfType<FrameworkElement>()
                        .Count(child => child.Visibility == Visibility.Visible && child.ActualWidth > 0));
                    saveSamples.Add($"{width:0} DIP: {rows} rows, gap={gap:0.##} DIP, panel={saveActions.ActualWidth:0.##} DIP");
                }

                var overviewView = new OverviewView { DataContext = new object() };
                ApplyTheme(overviewView, theme);
                var overviewActions = (WrapPanel)typeof(OverviewView)
                    .GetField("OverviewHomeToolbarActions", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(overviewView)!;
                overviewWindow = CreateWindow(overviewView, 900, 700);
                overviewWindow.Show();
                foreach (var width in new[] { 520d, 560d, 600d, 640d, 680d, 699d, 700d, 720d, 760d, 900d })
                {
                    overviewWindow.Width = width;
                    overviewView.ApplyResponsiveWidth(width);
                    FlushLayout(overviewWindow);
                    overviewView.ApplyResponsiveWidth(width);
                    FlushLayout(overviewWindow);
                    var (rows, gap) = MeasureWrapRows(overviewActions, overviewView);
                    Assert.Equal(1, rows);
                    Assert.Equal(3, overviewActions.Children.OfType<FrameworkElement>()
                        .Count(child => child.Visibility == Visibility.Visible && child.ActualWidth > 0));
                    overviewSamples.Add($"{width:0} DIP: {rows} rows, gap={gap:0.##} DIP, panel={overviewActions.ActualWidth:0.##} DIP");
                }
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                overviewWindow?.Close();
                saveWindow?.Close();
            }
        });

        output.WriteLine($"{theme} Save current-rule actions: {string.Join("; ", saveSamples)}");
        output.WriteLine($"{theme} Overview home toolbar actions: {string.Join("; ", overviewSamples)}");
        Assert.Null(exception);
        Assert.Equal(9, saveSamples.Count);
        Assert.Equal(10, overviewSamples.Count);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void OverviewHeaderCopyHidesAfterDataLoadsAndMetricUsesThemeAccent(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var initialHintVisibility = Visibility.Collapsed;
        var loadedHintVisibility = Visibility.Visible;
        var restoredHintVisibility = Visibility.Collapsed;
        var initialHeaderHeight = 0d;
        var loadedHeaderHeight = 0d;
        var metricAccentColor = Colors.Transparent;
        var themeAccentColor = Colors.Transparent;
        var metricInfoColor = Colors.Transparent;
        var metricMinContrast = 0d;
        var cloudQueueMetricMinContrast = 0d;
        var cloudAttentionTitleMinContrast = 0d;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var probe = new OverviewHeaderCopyProbe();
                var view = new OverviewView { DataContext = probe };
                ApplyTheme(view, theme);
                var viewType = typeof(OverviewView);
                var hint = (TextBlock)viewType.GetField("OverviewHomeEmptyHint", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var toolbar = (Border)viewType.GetField("OverviewHomeToolbar", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var metric = (TextBlock)viewType.GetField("OverviewManagedGamesValue", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var cloudQueueMetric = (TextBlock)viewType.GetField("OverviewCloudQueueValue", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var priorityTitle = (TextBlock)viewType.GetField("OverviewPriorityTitleText", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var cloudStatus = (TextBlock)viewType.GetField("OverviewCloudStatusText", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var actionPanel = (WrapPanel)viewType.GetField("OverviewHomeToolbarActions", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;

                window = CreateWindow(view, 620, 700);
                window.Show();
                view.ApplyResponsiveWidth(620);
                FlushLayout(window);
                var actionButtons = actionPanel.Children.OfType<GameSaveCenter.Playnite.Controls.Button>().ToArray();
                Assert.Equal(3, actionButtons.Length);
                Assert.Same(probe.RefreshCommand, actionButtons[0].Command);
                Assert.Same(probe.BackupAllCommand, actionButtons[1].Command);
                Assert.Same(probe.SyncMediaCommand, actionButtons[2].Command);
                initialHintVisibility = hint.Visibility;
                initialHeaderHeight = toolbar.ActualHeight;
                Assert.Contains("刷新", hint.Text, StringComparison.Ordinal);

                probe.IsDashboardSnapshotLoaded = true;
                FlushLayout(window);
                loadedHintVisibility = hint.Visibility;
                loadedHeaderHeight = toolbar.ActualHeight;
                var accent = Assert.IsType<SolidColorBrush>(view.TryFindResource("GscAccentBrush"));
                var info = Assert.IsType<SolidColorBrush>(view.TryFindResource("GscInfoBrush"));
                var warning = Assert.IsType<SolidColorBrush>(view.TryFindResource("GscWarningBrush"));
                var foreground = Assert.IsType<SolidColorBrush>(metric.Foreground);
                var cloudQueueForeground = Assert.IsType<SolidColorBrush>(cloudQueueMetric.Foreground);
                var priorityForeground = Assert.IsType<SolidColorBrush>(priorityTitle.Foreground);
                var cloudStatusForeground = Assert.IsType<SolidColorBrush>(cloudStatus.Foreground);
                var cloudQueueAction = FindVisualChildren<GameSaveCenter.Playnite.Controls.Button>(view)
                    .Single(button => ReferenceEquals(button.Command, probe.OpenCloudQueueCommand));
                var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
                    view, glassEnabled: true, strengthPercent: 78, themeMode: theme, highContrastOverride: false);
                var glass = Assert.IsAssignableFrom<Brush>(view.TryFindResource("GscGlassStrongBrush"));
                metricMinContrast = AdaptiveThemePaletteContrastGuard.MeasureGradientTextContrast(
                    "Overview managed games metric",
                    foreground.Color,
                    palette.Background,
                    GetContrastStops(glass),
                    Colors.Transparent,
                    Colors.Transparent,
                    pressedOpacity: 1,
                    minimum: 3.0).Min(measurement => measurement.Actual);
                cloudQueueMetricMinContrast = AdaptiveThemePaletteContrastGuard.MeasureGradientTextContrast(
                    "Overview cloud queue metric",
                    cloudQueueForeground.Color,
                    palette.Background,
                    GetContrastStops(glass),
                    Colors.Transparent,
                    Colors.Transparent,
                    pressedOpacity: 1,
                    minimum: 3.0).Min(measurement => measurement.Actual);
                cloudAttentionTitleMinContrast = AdaptiveThemePaletteContrastGuard.MeasureGradientTextContrast(
                    "Overview cloud attention title",
                    priorityForeground.Color,
                    palette.Background,
                    GetContrastStops(glass),
                    Colors.Transparent,
                    Colors.Transparent,
                    pressedOpacity: 1,
                    minimum: 3.0).Min(measurement => measurement.Actual);
                metricAccentColor = foreground.Color;
                themeAccentColor = accent.Color;
                metricInfoColor = info.Color;
                Assert.Equal("4", cloudQueueMetric.Text);
                Assert.Equal(themeAccentColor, cloudQueueForeground.Color);
                Assert.NotEqual(metricInfoColor, cloudQueueForeground.Color);
                Assert.Equal("4 项云端任务需要处理", priorityTitle.Text);
                Assert.Equal(warning.Color, priorityForeground.Color);
                Assert.Equal(info.Color, cloudStatusForeground.Color);
                Assert.Same(probe.OpenCloudQueueCommand, cloudQueueAction.Command);

                probe.IsDashboardSnapshotLoaded = false;
                FlushLayout(window);
                restoredHintVisibility = hint.Visibility;
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

        output.WriteLine($"{theme} overview hint {initialHintVisibility} -> {loadedHintVisibility} -> {restoredHintVisibility}; toolbar {initialHeaderHeight:0.##} -> {loadedHeaderHeight:0.##} DIP; cloud queue=4 metric accent=#{themeAccentColor}, attention title warning, status capsule info=#{metricInfoColor}, command retained, contrast={metricMinContrast:0.##}/{cloudQueueMetricMinContrast:0.##}/{cloudAttentionTitleMinContrast:0.##}:1");
        Assert.Null(exception);
        Assert.Equal(Visibility.Visible, initialHintVisibility);
        Assert.Equal(Visibility.Collapsed, loadedHintVisibility);
        Assert.Equal(Visibility.Visible, restoredHintVisibility);
        Assert.True(loadedHeaderHeight < initialHeaderHeight - 4,
            $"Loaded overview data should reclaim the generic helper line ({initialHeaderHeight:0.##} -> {loadedHeaderHeight:0.##} DIP).");
        Assert.Equal(themeAccentColor, metricAccentColor);
        Assert.NotEqual(metricInfoColor, metricAccentColor);
        Assert.True(metricMinContrast >= 3.0,
            $"Large Overview metric text should maintain at least 3:1 contrast across the card surface ({metricMinContrast:0.##}:1).");
        Assert.True(cloudQueueMetricMinContrast >= 3.0,
            $"The cloud queue count should maintain at least 3:1 contrast across the card surface ({cloudQueueMetricMinContrast:0.##}:1).");
        Assert.True(cloudAttentionTitleMinContrast >= 3.0,
            $"The large cloud attention title should maintain at least 3:1 contrast across the card surface ({cloudAttentionTitleMinContrast:0.##}:1).");
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void MaintenanceActionCategoryUsesSecondaryThemeColor(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var categoryColor = Colors.Transparent;
        var secondaryColor = Colors.Transparent;
        var infoColor = Colors.Transparent;
        var categoryMinContrast = 0d;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var view = new MaintenanceView();
                ApplyTheme(view, theme);
                var template = (DataTemplate)view.FindResource("MaintenanceActionItemTemplate");
                var item = new MaintenanceActionItem
                {
                    Title = "待处理动作",
                    StatusDisplay = "待处理",
                    CategoryDisplay = "云端队列",
                    Detail = "合成布局项"
                };
                var presenter = new ContentPresenter { Content = item, ContentTemplate = template };
                ApplyTheme(presenter, theme);
                var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
                    presenter, glassEnabled: true, strengthPercent: 78, themeMode: theme, highContrastOverride: false);
                var glass = Assert.IsAssignableFrom<Brush>(presenter.TryFindResource("GscGlassStrongBrush"));
                window = CreateWindow(new Border
                {
                    Background = glass,
                    Padding = new Thickness(10),
                    Child = presenter
                }, 420, 180);
                window.Show();
                FlushLayout(window);

                var category = FindVisualChildren<TextBlock>(presenter).Single(text => text.Text == "云端队列");
                categoryColor = Assert.IsType<SolidColorBrush>(category.Foreground).Color;
                secondaryColor = Assert.IsType<SolidColorBrush>(presenter.TryFindResource("GscSecondaryTextBrush")).Color;
                infoColor = Assert.IsType<SolidColorBrush>(presenter.TryFindResource("GscInfoBrush")).Color;
                categoryMinContrast = AdaptiveThemePaletteContrastGuard.MeasureGradientTextContrast(
                    "Maintenance action category",
                    categoryColor,
                    palette.Background,
                    GetContrastStops(glass),
                    Colors.Transparent,
                    Colors.Transparent,
                    pressedOpacity: 1,
                    minimum: 4.5).Min(measurement => measurement.Actual);
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

        output.WriteLine($"{theme} maintenance action category #{categoryColor}; secondary #{secondaryColor}; semantic info #{infoColor}; minimum contrast={categoryMinContrast:0.##}:1");
        Assert.Null(exception);
        Assert.Equal(secondaryColor, categoryColor);
        Assert.NotEqual(infoColor, categoryColor);
        Assert.True(categoryMinContrast >= 4.5,
            $"Small Maintenance category text should maintain at least 4.5:1 contrast across the card surface ({categoryMinContrast:0.##}:1).");
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void TrainerImportEmptyHintHidesWhenConfirmationIsPendingWithoutHidingActions(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var emptyHintVisibility = Visibility.Collapsed;
        var pendingHintVisibility = Visibility.Visible;
        var selectorVisibility = Visibility.Collapsed;
        var confirmVisibility = Visibility.Collapsed;
        var cancelVisibility = Visibility.Collapsed;
        var selectionPreserved = false;
        var actionBindingsPreserved = false;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var probe = new TrainerImportCopyProbe();
                var view = new TrainerCenterView { DataContext = probe };
                ApplyTheme(view, theme);
                window = CreateWindow(view, 1100, 700);
                window.Show();
                var tabs = FindVisualChildren<TabControl>(view).Single();
                Assert.Equal(4, tabs.Items.Count);
                tabs.SelectedIndex = 1;
                FlushLayout(window);

                var hint = (TextBlock)typeof(TrainerCenterView)
                    .GetField("TrainerImportEmptyHint", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;
                var selector = (ComboBox)typeof(TrainerCenterView)
                    .GetField("TrainerImportEntryComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;
                var confirm = FindVisualChildren<ButtonBase>(view)
                    .Single(button => button is ContentControl content && Equals(content.Content, "确认导入"));
                var cancel = FindVisualChildren<ButtonBase>(view)
                    .Single(button => button is ContentControl content && Equals(content.Content, "取消"));

                emptyHintVisibility = hint.Visibility;
                Assert.Contains("已绑定工具", hint.Text, StringComparison.Ordinal);
                Assert.Same(probe.ConfirmGameToolImportCommand, confirm.Command);
                Assert.Same(probe.CancelGameToolImportCommand, cancel.Command);

                probe.HasPendingGameToolEntrySelection = true;
                FlushLayout(window);
                pendingHintVisibility = hint.Visibility;
                selectorVisibility = selector.Visibility;
                confirmVisibility = confirm.Visibility;
                cancelVisibility = cancel.Visibility;
                selectionPreserved = Equals(selector.SelectedItem, probe.SelectedImportEntryCandidate);
                actionBindingsPreserved = ReferenceEquals(probe.ConfirmGameToolImportCommand, confirm.Command)
                    && ReferenceEquals(probe.CancelGameToolImportCommand, cancel.Command);
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

        output.WriteLine($"{theme} trainer import empty hint {emptyHintVisibility} -> {pendingHintVisibility}; selector={selectorVisibility}; confirm={confirmVisibility}; cancel={cancelVisibility}; selectionPreserved={selectionPreserved}; actionsBound={actionBindingsPreserved}");
        Assert.Null(exception);
        Assert.Equal(Visibility.Visible, emptyHintVisibility);
        Assert.Equal(Visibility.Collapsed, pendingHintVisibility);
        Assert.Equal(Visibility.Visible, selectorVisibility);
        Assert.Equal(Visibility.Visible, confirmVisibility);
        Assert.Equal(Visibility.Visible, cancelVisibility);
        Assert.True(selectionPreserved);
        Assert.True(actionBindingsPreserved);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void CompactSaveAndInboxActionWrapsKeepATwelveDipRowGapAndRestoreWideMargins(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var saveCompactRows = 0;
        var saveCompactGap = 0d;
        var saveWideRows = 0;
        var saveWideBottomMargin = 0d;
        var inboxCompactRows = 0;
        var inboxCompactGap = 0d;
        var inboxWideRows = 0;
        var inboxWideBottomMargin = 0d;
        var mediaPresetSamples = new List<string>();
        var mediaSecondaryActionSamples = new List<string>();

        RunSta(() =>
        {
            Window? saveWindow = null;
            Window? mediaWindow = null;
            try
            {
                var saveView = new SaveCenterView { DataContext = new SavePageContext() };
                ApplyTheme(saveView, theme);
                var saveActions = (WrapPanel)typeof(SaveCenterView)
                    .GetField("SaveHistorySummaryActions", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(saveView)!;
                saveWindow = CreateWindow(saveView, 1100, 720);
                saveWindow.Show();
                saveView.ApplyResponsiveLayout(1100, 720);
                FlushLayout(saveWindow);
                saveView.ApplyResponsiveLayout(1100, 720);
                FlushLayout(saveWindow);
                (saveCompactRows, saveCompactGap) = MeasureWrapRows(saveActions, saveView);

                saveWindow.Width = 1800;
                saveView.ApplyResponsiveLayout(1800, 720);
                FlushLayout(saveWindow);
                (saveWideRows, _) = MeasureWrapRows(saveActions, saveView);
                saveWideBottomMargin = saveActions.Children.OfType<FrameworkElement>().Max(child => child.Margin.Bottom);

                var mediaView = new MediaCenterView { DataContext = CreateMediaContext() };
                ApplyTheme(mediaView, theme);
                var tabs = (TabControl)typeof(MediaCenterView)
                    .GetField("MediaTabControl", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(mediaView)!;
                tabs.SelectedIndex = 0;
                var inboxActions = (WrapPanel)typeof(MediaCenterView)
                    .GetField("MediaInboxBatchActionRow", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(mediaView)!;
                mediaWindow = CreateWindow(mediaView, 700, 720);
                mediaWindow.Show();
                mediaView.ApplyResponsiveLayout(700, 720);
                FlushLayout(mediaWindow);
                mediaView.ApplyResponsiveLayout(700, 720);
                FlushLayout(mediaWindow);
                (inboxCompactRows, inboxCompactGap) = MeasureWrapRows(inboxActions, mediaView);

                mediaWindow.Width = 1600;
                mediaView.ApplyResponsiveLayout(1600, 720);
                FlushLayout(mediaWindow);
                (inboxWideRows, _) = MeasureWrapRows(inboxActions, mediaView);
                inboxWideBottomMargin = inboxActions.Children.OfType<FrameworkElement>().Max(child => child.Margin.Bottom);

                var presetRow = (WrapPanel)typeof(MediaCenterView)
                    .GetField("MediaFilterPresetRow", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(mediaView)!;
                foreach (var width in new[] { 520d, 620d, 700d, 720d, 800d, 900d, 1040d })
                {
                    mediaWindow.Width = width;
                    mediaView.ApplyResponsiveLayout(width, 720);
                    FlushLayout(mediaWindow);
                    mediaView.ApplyResponsiveLayout(width, 720);
                    FlushLayout(mediaWindow);
                    var (rows, gap) = MeasureWrapRows(presetRow, mediaView);
                    if (rows > 1)
                    {
                        Assert.True(gap >= 11.5,
                            $"{theme} media filter preset rows at {width:0} DIP need at least 11.5 DIP breathing room; actual gap={gap:0.##} DIP.");
                        Assert.All(presetRow.Children.OfType<FrameworkElement>(), child =>
                            Assert.Equal(12d, child.Margin.Bottom));
                    }
                    else
                    {
                        Assert.All(presetRow.Children.OfType<FrameworkElement>(), child =>
                            Assert.Equal(0d, child.Margin.Bottom));
                    }
                    var contentWidth = presetRow.Children.OfType<FrameworkElement>()
                        .Where(child => child.Visibility == Visibility.Visible)
                        .Sum(child => child.DesiredSize.Width + child.Margin.Left + child.Margin.Right);
                    mediaPresetSamples.Add($"{width:0} DIP: {rows} rows, gap={gap:0.##} DIP, panel={presetRow.ActualWidth:0.##} DIP, content={contentWidth:0.##} DIP");
                }

                mediaWindow.Width = 700d;
                mediaView.ApplyResponsiveLayout(700, 720);
                FlushLayout(mediaWindow);
                Assert.True(MeasureWrapRows(presetRow, mediaView).MinGap >= 11.5,
                    "Media filter preset rows should regain their gap after resizing back to 700 DIP.");
                mediaWindow.Width = 720d;
                mediaView.ApplyResponsiveLayout(720, 720);
                FlushLayout(mediaWindow);
                Assert.Equal(1, MeasureWrapRows(presetRow, mediaView).RowCount);
                Assert.All(presetRow.Children.OfType<FrameworkElement>(), child =>
                    Assert.Equal(0d, child.Margin.Bottom));
                mediaWindow.Width = 700d;
                mediaView.ApplyResponsiveLayout(700, 720);
                FlushLayout(mediaWindow);
                Assert.True(MeasureWrapRows(presetRow, mediaView).MinGap >= 11.5,
                    "Media filter preset row gap should survive a 700→720→700 DIP resize round trip.");

                var secondaryActions = (WrapPanel)typeof(MediaCenterView)
                    .GetField("MediaInboxSecondaryActions", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(mediaView)!;
                foreach (var width in new[] { 520d, 540d, 560d, 570d, 575d, 576d, 577d, 578d, 579d, 580d, 620d, 700d, 720d, 800d })
                {
                    mediaWindow.Width = width;
                    mediaView.ApplyResponsiveLayout(width, 720);
                    FlushLayout(mediaWindow);
                    mediaView.ApplyResponsiveLayout(width, 720);
                    FlushLayout(mediaWindow);
                    var (rows, gap) = MeasureWrapRows(secondaryActions, mediaView);
                    if (rows > 1)
                    {
                        Assert.True(gap >= 11.5,
                            $"{theme} media secondary action rows at {width:0} DIP need at least 11.5 DIP; actual gap={gap:0.##} DIP.");
                        Assert.All(secondaryActions.Children.OfType<FrameworkElement>(), child =>
                            Assert.Equal(12d, child.Margin.Bottom));
                    }
                    else
                    {
                        Assert.All(secondaryActions.Children.OfType<FrameworkElement>(), child =>
                            Assert.Equal(4d, child.Margin.Bottom));
                    }
                    mediaSecondaryActionSamples.Add($"{width:0} DIP: {rows} rows, gap={gap:0.##} DIP, panel={secondaryActions.ActualWidth:0.##} DIP, margins=[{string.Join(";", secondaryActions.Children.OfType<FrameworkElement>().Select(child => $"{child.Margin.Bottom:0.##}"))}]");
                }

                mediaWindow.Width = 576d;
                mediaView.ApplyResponsiveLayout(576, 720);
                FlushLayout(mediaWindow);
                Assert.True(MeasureWrapRows(secondaryActions, mediaView).MinGap >= 11.5,
                    "Media secondary action rows should use the 12 DIP gap at 576 DIP.");
                mediaWindow.Width = 577d;
                mediaView.ApplyResponsiveLayout(577, 720);
                FlushLayout(mediaWindow);
                Assert.Equal(1, MeasureWrapRows(secondaryActions, mediaView).RowCount);
                Assert.All(secondaryActions.Children.OfType<FrameworkElement>(), child =>
                    Assert.Equal(4d, child.Margin.Bottom));
                mediaWindow.Width = 576d;
                mediaView.ApplyResponsiveLayout(576, 720);
                FlushLayout(mediaWindow);
                Assert.True(MeasureWrapRows(secondaryActions, mediaView).MinGap >= 11.5,
                    "Media secondary action row gap should survive a 576→577→576 DIP resize round trip.");
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                mediaWindow?.Close();
                saveWindow?.Close();
            }
        });

        output.WriteLine($"{theme} Save rows={saveCompactRows}, gap={saveCompactGap:0.##} DIP, wide rows={saveWideRows}, restored bottom={saveWideBottomMargin:0.##}; Inbox rows={inboxCompactRows}, gap={inboxCompactGap:0.##} DIP, wide rows={inboxWideRows}, restored bottom={inboxWideBottomMargin:0.##}");
        foreach (var sample in mediaPresetSamples)
            output.WriteLine($"{theme} Media preset {sample}");
        foreach (var sample in mediaSecondaryActionSamples)
            output.WriteLine($"{theme} Media secondary actions {sample}");
        Assert.Null(exception);
        Assert.True(saveCompactRows >= 2, "the compact synthetic save history toolbar must exercise a wrapped row");
        Assert.True(saveCompactGap >= 11.5, $"Save Center action rows need clear separation ({saveCompactGap:0.##} DIP)");
        Assert.Equal(1, saveWideRows);
        Assert.InRange(saveWideBottomMargin, 0, 0.5);
        Assert.True(inboxCompactRows >= 2, "the compact synthetic inbox toolbar must exercise a wrapped row");
        Assert.True(inboxCompactGap >= 11.5, $"Media Inbox action rows need clear separation ({inboxCompactGap:0.##} DIP)");
        Assert.Equal(1, inboxWideRows);
        Assert.InRange(inboxWideBottomMargin, 0, 4.5);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void CompactTaskPresetWrapUsesTwelveDipGapAndRestoresAtBreakpoint(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var compactRows = 0;
        var compactGap = double.NaN;
        var wideRows = 0;
        var restoredRows = 0;
        var compactMargins = new List<double>();
        var wideMargins = new List<double>();
        var restoredMargins = new List<double>();

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var view = new TaskCenterView { DataContext = new TaskSummaryLayoutProbe() };
                ApplyTheme(view, theme);
                var row = (WrapPanel)typeof(TaskCenterView)
                    .GetField("TaskFilterPresetRow", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;
                foreach (var combo in row.Children.OfType<ComboBox>())
                {
                    combo.ItemsSource = new[] { "全部", "失败" };
                    combo.SelectedIndex = 0;
                }
                foreach (var textBox in row.Children.OfType<TextBox>())
                    textBox.Text = "我的预设";

                window = CreateWindow(view, 620, 700);
                window.ResizeMode = ResizeMode.NoResize;
                view.ApplyResponsiveLayout(620, 700);
                window.Show();
                FlushLayout(window);
                view.ApplyResponsiveLayout(view.ActualWidth, view.ActualHeight);
                FlushLayout(window);
                (compactRows, compactGap) = MeasureWrapRows(row, view);
                compactMargins = row.Children.OfType<FrameworkElement>().Select(child => child.Margin.Bottom).ToList();

                window.Width = 660;
                FlushLayout(window);
                view.ApplyResponsiveLayout(view.ActualWidth, view.ActualHeight);
                FlushLayout(window);
                (wideRows, _) = MeasureWrapRows(row, view);
                wideMargins = row.Children.OfType<FrameworkElement>().Select(child => child.Margin.Bottom).ToList();

                window.Width = 620;
                FlushLayout(window);
                view.ApplyResponsiveLayout(view.ActualWidth, view.ActualHeight);
                FlushLayout(window);
                (restoredRows, compactGap) = MeasureWrapRows(row, view);
                restoredMargins = row.Children.OfType<FrameworkElement>().Select(child => child.Margin.Bottom).ToList();
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

        output.WriteLine($"{theme} task preset compact rows={compactRows}, gap={compactGap:0.##} DIP, margins=[{string.Join(",", compactMargins)}]; wide rows={wideRows}, margins=[{string.Join(",", wideMargins)}]; restored rows={restoredRows}, margins=[{string.Join(",", restoredMargins)}]");
        Assert.Null(exception);
        Assert.True(compactRows > 1, "620 DIP must exercise the task preset WrapPanel's compact path.");
        Assert.True(compactGap >= 11.5, $"Compact task preset rows need 12 DIP of separation; actual={compactGap:0.##} DIP.");
        Assert.All(compactMargins, margin => Assert.Equal(12d, margin));
        Assert.Equal(1, wideRows);
        Assert.All(wideMargins, margin => Assert.Equal(0d, margin));
        Assert.True(restoredRows > 1);
        Assert.All(restoredMargins, margin => Assert.Equal(12d, margin));
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void CompactShellStacksPageTitleAboveActionsAndHidesSecondarySubtitle(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var compactRows = 0;
        var compactSubtitleVisibility = Visibility.Visible;
        var compactTitleText = string.Empty;
        var compactActionRows = 0;
        var compactTitleColumnSpan = 0;
        var compactActionsColumnSpan = 0;
        var compactActionsColumnWidth = double.NaN;
        var compactTitleActionsGap = double.NaN;
        var roundTripActionRow = -1;
        var roundTripTitleColumnSpan = 0;
        var wideSubtitleVisibility = Visibility.Collapsed;
        var compactHelpText = string.Empty;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var shell = new AcrylicProductionShellView();
                ApplyTheme(shell, theme);
                typeof(AcrylicProductionShellView)
                    .GetMethod("UpdatePageHeader", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(shell, new object[] { WorkspaceKind.Saves });

                var title = (TextBlock)typeof(AcrylicProductionShellView)
                    .GetField("PageTitleText", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                var subtitle = (TextBlock)typeof(AcrylicProductionShellView)
                    .GetField("PageSubtitleText", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                var actions = (WrapPanel)typeof(AcrylicProductionShellView)
                    .GetField("HeaderActionsPanel", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                var titlePanel = (FrameworkElement)typeof(AcrylicProductionShellView)
                    .GetField("HeaderTitlePanel", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                var actionsColumn = (ColumnDefinition)typeof(AcrylicProductionShellView)
                    .GetField("HeaderActionsColumn", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                var backupSelected = (FrameworkElement)typeof(AcrylicProductionShellView)
                    .GetField("HeaderBackupSelectedButton", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                var backupAll = (FrameworkElement)typeof(AcrylicProductionShellView)
                    .GetField("HeaderBackupButton", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                var mediaSync = (FrameworkElement)typeof(AcrylicProductionShellView)
                    .GetField("HeaderMediaButton", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                backupSelected.Visibility = Visibility.Visible;
                backupAll.Visibility = Visibility.Visible;
                mediaSync.Visibility = Visibility.Collapsed;
                window = CreateWindow(shell, 1040, 720);
                window.Show();
                shell.ApplyResponsiveLayout(1040, 720);
                FlushLayout(window);
                (compactRows, _) = MeasureWrapRows(actions, shell);
                compactSubtitleVisibility = subtitle.Visibility;
                compactTitleText = title.Text;
                compactHelpText = AutomationProperties.GetHelpText(title);
                compactActionRows = Grid.GetRow(actions);
                compactTitleColumnSpan = Grid.GetColumnSpan(titlePanel);
                compactActionsColumnSpan = Grid.GetColumnSpan(actions);
                compactActionsColumnWidth = actionsColumn.ActualWidth;
                compactTitleActionsGap = BoundsIn(actions, shell).Top - BoundsIn(titlePanel, shell).Bottom;

                window.Width = 1500;
                shell.ApplyResponsiveLayout(1500, 720);
                FlushLayout(window);
                wideSubtitleVisibility = subtitle.Visibility;

                window.Width = 1040;
                shell.ApplyResponsiveLayout(1040, 720);
                FlushLayout(window);
                roundTripActionRow = Grid.GetRow(actions);
                roundTripTitleColumnSpan = Grid.GetColumnSpan(titlePanel);
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

        output.WriteLine($"{theme} Compact header title={compactTitleText}, subtitle={compactSubtitleVisibility}, actionsRow={compactActionRows}, title/action spans={compactTitleColumnSpan}/{compactActionsColumnSpan}, actionsColumn={compactActionsColumnWidth:0.##} DIP, title/action gap={compactTitleActionsGap:0.##} DIP, actionRows={compactRows}; wide subtitle={wideSubtitleVisibility}; compact round-trip row/span={roundTripActionRow}/{roundTripTitleColumnSpan}");
        Assert.Null(exception);
        Assert.Equal("存档中心", compactTitleText);
        Assert.Equal(Visibility.Collapsed, compactSubtitleVisibility);
        Assert.Equal(1, compactActionRows);
        Assert.Equal(2, compactTitleColumnSpan);
        Assert.Equal(2, compactActionsColumnSpan);
        Assert.InRange(compactActionsColumnWidth, 0, 0.5);
        Assert.InRange(compactTitleActionsGap, 7.5, 8.5);
        Assert.Equal(1, roundTripActionRow);
        Assert.Equal(2, roundTripTitleColumnSpan);
        Assert.Equal(Visibility.Visible, wideSubtitleVisibility);
        Assert.Contains("路径与恢复点状态", compactHelpText);
        Assert.Equal(1, compactRows);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void ProductionPrimaryBackupIconsMatchButtonTextAndKeepContrastAndCommands(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var iconColors = new List<Color>();
        var labelColors = new List<Color>();
        var minContrasts = new List<double>();
        var commandBindingsPreserved = false;
        var commandExecutionPreserved = false;
        var automationNamesPreserved = false;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var probe = new ProductionHeaderCommandProbe();
                var shell = new AcrylicProductionShellView { DataContext = probe };
                ApplyTheme(shell, theme);
                var shellType = typeof(AcrylicProductionShellView);
                var backupSelected = (GameSaveCenter.Playnite.Controls.Button)shellType
                    .GetField("HeaderBackupSelectedButton", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                var backupAll = (GameSaveCenter.Playnite.Controls.Button)shellType
                    .GetField("HeaderBackupButton", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                backupSelected.Visibility = Visibility.Visible;
                backupAll.Visibility = Visibility.Visible;
                window = CreateWindow(shell, 1320, 760);
                window.Show();
                FlushLayout(window);

                var expectedForeground = Assert.IsType<SolidColorBrush>(shell.TryFindResource("GscOnAccentTextBrush"));
                var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
                    shell, glassEnabled: true, strengthPercent: 78, themeMode: theme, highContrastOverride: false);
                var buttons = new[] { backupSelected, backupAll };
                foreach (var button in buttons)
                {
                    var icon = FindVisualChildren<ThemeAwareIcon>(button).Single();
                    var iconForeground = Assert.IsType<SolidColorBrush>(icon.Foreground);
                    var buttonForeground = Assert.IsType<SolidColorBrush>(button.Foreground);
                    var chrome = Assert.IsType<Border>(button.Template!.FindName("ButtonChrome", button));
                    var hoverOverlay = Assert.IsType<Border>(button.Template.FindName("HoverOverlay", button));
                    var focusOverlay = Assert.IsType<Border>(button.Template.FindName("FocusOverlay", button));
                    var pressedOverlay = Assert.IsType<Border>(button.Template.FindName("PressedOverlay", button));
                    iconColors.Add(iconForeground.Color);
                    labelColors.Add(buttonForeground.Color);
                    // Primary appearance is rendered by the template's ButtonChrome trigger;
                    // Button.Background itself can still be the transparent default value.
                    var stops = GetContrastStops(Assert.IsAssignableFrom<Brush>(chrome.Background));
                    minContrasts.Add(AdaptiveThemePaletteContrastGuard.MeasureGradientTextContrast(
                        "Production primary backup icon",
                        iconForeground.Color,
                        palette.Background,
                        stops,
                        Assert.IsType<SolidColorBrush>(hoverOverlay.Background).Color,
                        Assert.IsType<SolidColorBrush>(focusOverlay.Background).Color,
                        Assert.IsType<SolidColorBrush>(pressedOverlay.Background).Color,
                        pressedOpacity: 0.96,
                        minimum: 4.5).Min(measurement => measurement.Actual));
                }

                commandBindingsPreserved = ReferenceEquals(backupSelected.Command, probe.BackupSelectedCommand)
                    && ReferenceEquals(backupAll.Command, probe.BackupAllCommand);
                backupSelected.Command.Execute(backupSelected.CommandParameter);
                backupAll.Command.Execute(backupAll.CommandParameter);
                commandExecutionPreserved = probe.BackupSelectedExecutions == 1
                    && probe.BackupAllExecutions == 1;
                automationNamesPreserved = AutomationProperties.GetName(backupSelected) == "立即备份当前游戏"
                    && AutomationProperties.GetName(backupAll) == "备份全部游戏";
                Assert.Same(expectedForeground, backupSelected.Foreground);
                Assert.Same(expectedForeground, backupAll.Foreground);
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

        output.WriteLine($"{theme} production backup action icons={string.Join("/", iconColors)} labels={string.Join("/", labelColors)} minimum contrast={string.Join("/", minContrasts.Select(value => value.ToString("0.##")))}:1 commands bound/executed={commandBindingsPreserved}/{commandExecutionPreserved}");
        Assert.Null(exception);
        Assert.Equal(2, iconColors.Count);
        Assert.All(iconColors, color => Assert.Equal(labelColors[0], color));
        Assert.All(minContrasts, contrast => Assert.True(contrast >= 4.5, $"Primary backup icon and label color must maintain at least 4.5:1 contrast across button states ({contrast:0.##}:1)."));
        Assert.True(commandBindingsPreserved);
        Assert.True(commandExecutionPreserved);
        Assert.True(automationNamesPreserved);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void TaskQueueActionsKeepTheirOwnHeightWhenThreeLineSummaryAppears(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var summaryHeight = 0d;
        var retryHeight = 0d;
        var resetHeight = 0d;
        var retryCenterDelta = 0d;
        var resetCenterDelta = 0d;
        var retryAfterSummaryCollapse = 0d;
        var resetAfterSummaryCollapse = 0d;
        var leaveHintForeground = Colors.Transparent;
        var queueFilterForeground = Colors.Transparent;
        var secondaryForeground = Colors.Transparent;
        var infoForeground = Colors.Transparent;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var view = new TaskCenterView { DataContext = new TaskSummaryLayoutProbe() };
                ApplyTheme(view, theme);
                var viewType = typeof(TaskCenterView);
                var summary = (StackPanel)viewType.GetField("TaskQueueSummaryStack", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var retry = (FrameworkElement)viewType.GetField("TaskQueueRetryAllButton", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var reset = (FrameworkElement)viewType.GetField("TaskQueueResetColumnWidthButton", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var hint = (TextBlock)viewType.GetField("TaskLeavePageHint", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var filterSummary = (TextBlock)viewType.GetField("TaskQueueFilterSummary", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                window = CreateWindow(view, 1280, 800);
                view.ApplyResponsiveLayout(1280, 800);
                window.Show();
                FlushLayout(window);
                view.ApplyResponsiveLayout(1280, 800);
                FlushLayout(window);

                summaryHeight = summary.ActualHeight;
                retryHeight = retry.ActualHeight;
                resetHeight = reset.ActualHeight;
                retryCenterDelta = Math.Abs(CenterY(retry, window) - CenterY(summary, window));
                resetCenterDelta = Math.Abs(CenterY(reset, window) - CenterY(summary, window));
                leaveHintForeground = (hint.Foreground as SolidColorBrush)?.Color ?? Colors.Transparent;
                queueFilterForeground = (filterSummary.Foreground as SolidColorBrush)?.Color ?? Colors.Transparent;
                secondaryForeground = (view.FindResource("GscSecondaryTextBrush") as SolidColorBrush)?.Color ?? Colors.Transparent;
                infoForeground = (view.FindResource("GscInfoBrush") as SolidColorBrush)?.Color ?? Colors.Transparent;

                filterSummary.Visibility = Visibility.Collapsed;
                ((TextBlock)summary.Children[2]).Visibility = Visibility.Collapsed;
                FlushLayout(window);
                retryAfterSummaryCollapse = retry.ActualHeight;
                resetAfterSummaryCollapse = reset.ActualHeight;
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

        output.WriteLine($"{theme} Task queue summary={summaryHeight:0.##} DIP, retry/reset={retryHeight:0.##}/{resetHeight:0.##}, centerΔ={retryCenterDelta:0.##}/{resetCenterDelta:0.##}, after summaries hide={retryAfterSummaryCollapse:0.##}/{resetAfterSummaryCollapse:0.##}, hint/filter brushes={leaveHintForeground}/{queueFilterForeground}, secondary/info={secondaryForeground}/{infoForeground}");
        Assert.Null(exception);
        Assert.True(summaryHeight >= 42, "the fixture needs a genuinely multi-line task summary to expose the previous stretch behavior");
        Assert.InRange(retryHeight, 28, 40);
        Assert.InRange(resetHeight, 32, 40);
        Assert.InRange(retryCenterDelta, 0, 1);
        Assert.InRange(resetCenterDelta, 0, 1);
        Assert.InRange(Math.Abs(retryAfterSummaryCollapse - retryHeight), 0, 0.5);
        Assert.InRange(Math.Abs(resetAfterSummaryCollapse - resetHeight), 0, 0.5);
        Assert.Equal(secondaryForeground, leaveHintForeground);
        Assert.Equal(secondaryForeground, queueFilterForeground);
        Assert.NotEqual(infoForeground, leaveHintForeground);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void CloudTransferLoadedCountSharesTheTableTitleBaselineAndCentersItsText(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var pillTextCenterDelta = 0d;
        var pillTitleCenterDelta = 0d;
        var pillHeight = 0d;
        var pillTextHeight = 0d;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var view = new MaintenanceView { DataContext = new CloudTransferHeaderProbe() };
                ApplyTheme(view, theme);
                var tabs = (TabControl)typeof(MaintenanceView)
                    .GetField("MaintenanceTabControl", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;
                tabs.SelectedIndex = 1;
                var frame = (Border)typeof(MaintenanceView)
                    .GetField("CloudTransferTableFrame", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;
                window = CreateWindow(view, 1280, 800);
                window.Show();
                FlushLayout(window);

                var countText = FindBoundText(frame, "CloudTransferLoadedSummary");
                var pill = FindVisualAncestor<Border>(countText, frame);
                var title = FindVisualChildren<TextBlock>(frame).Single(text => text.Text == "传输明细");
                pillTextCenterDelta = Math.Abs(CenterY(countText, pill) - pill.ActualHeight / 2);
                pillTitleCenterDelta = Math.Abs(CenterY(countText, frame) - CenterY(title, frame));
                pillHeight = pill.ActualHeight;
                pillTextHeight = countText.ActualHeight;
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

        output.WriteLine($"{theme} Cloud transfer title/count center delta={pillTitleCenterDelta:0.##} DIP; pill text center delta={pillTextCenterDelta:0.##} DIP; pill={pillHeight:0.##}, text={pillTextHeight:0.##}");
        Assert.Null(exception);
        Assert.True(pillHeight > 0 && pillTextHeight > 0, "the active cloud table header must render the count pill");
        Assert.InRange(pillTextCenterDelta, 0, 0.75);
        Assert.InRange(pillTitleCenterDelta, 0, 1.5);
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
    [InlineData(760d)]
    [InlineData(920d)]
    public void ShortSettingsPageKeepsCategoryAndBodyViewports(double width)
    {
        Exception? exception = null;
        var categoryViewportHeight = 0d;
        var lastCategoryHeight = 0d;
        var lastCategoryBottom = double.NaN;
        var bodyViewportHeight = 0d;
        var resetCollapsedForShortLayout = false;
        var resetActionsAvailableWhenExpanded = false;
        var resetExpandedAfterReturningToTallLayout = false;
        var geometry = string.Empty;
        var resetTransitions = string.Empty;

        RunSta(() =>
        {
            try
            {
                EnsureApplicationResources();
                var view = new GameSaveCenter.Playnite.Settings.GameSaveCenterSettingsView
                {
                    DataContext = new GameSaveCenterSettings()
                };
                var viewType = view.GetType();
                var apply = viewType.GetMethod("ApplyResponsiveLayout", BindingFlags.Instance | BindingFlags.NonPublic)!;
                var host = new Grid { Width = width, Height = 560 };
                host.Children.Add(view);
                apply.Invoke(view, new object[] { width, 560d });
                host.Measure(new Size(width, 560));
                host.Arrange(new Rect(0, 0, width, 560));
                host.UpdateLayout();
                apply.Invoke(view, new object[] { width, 560d });
                host.UpdateLayout();

                var tabs = FindVisualChildren<ListBox>(host).Single(item => item.Name == "SettingsSectionTabs");
                var categoryScroller = FindVisualChildren<ScrollViewer>(tabs).First();
                var categoryPresenter = FindVisualChildren<ScrollContentPresenter>(categoryScroller).FirstOrDefault();
                var bodyScroller = FindVisualChildren<ScrollViewer>(host).Single(item => item.Name == "SettingsScroller");
                var resetExpander = FindVisualChildren<Expander>(host).Single(item => item.Name == "SettingsResetDefaultsExpander");
                var resetFieldCombo = (ComboBox)viewType.GetField("SettingsResetFieldComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var header = FindVisualChildren<FrameworkElement>(host).Single(item => item.Name == "SettingsHeader");
                var headerGrid = FindVisualChildren<Grid>(header).Single(item => item.Name == "SettingsHeaderGrid");
                var lastTab = FindVisualChildren<ListBoxItem>(tabs)
                    .Single(item => tabs.ItemContainerGenerator.IndexFromContainer(item) == tabs.Items.Count - 1);
                categoryViewportHeight = categoryPresenter?.ActualHeight ?? categoryScroller.ActualHeight;
                lastCategoryHeight = lastTab.ActualHeight;
                bodyViewportHeight = bodyScroller.ViewportHeight;
                resetCollapsedForShortLayout = !resetExpander.IsExpanded;
                categoryScroller.ScrollToVerticalOffset(categoryScroller.ScrollableHeight);
                host.UpdateLayout();
                var categoryViewport = (FrameworkElement?)categoryPresenter ?? categoryScroller;
                var lastTabOrigin = lastTab.TransformToAncestor(categoryViewport).Transform(new Point(0, 0));
                lastCategoryBottom = lastTabOrigin.Y + lastTab.ActualHeight;
                var shell = (FrameworkElement)viewType.GetField("SettingsShell", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var workspace = (FrameworkElement)viewType.GetField("SettingsWorkspace", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var rail = (FrameworkElement)viewType.GetField("SettingsCategoryRail", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var headerRows = string.Join(",", headerGrid.RowDefinitions.Select(row => row.ActualHeight.ToString("0.##")));
                geometry = $"shell={shell.ActualWidth:0.##}x{shell.ActualHeight:0.##}, header={header.ActualHeight:0.##}, headerRows=[{headerRows}], workspace={workspace.ActualWidth:0.##}x{workspace.ActualHeight:0.##}, rail={rail.ActualWidth:0.##}x{rail.ActualHeight:0.##}, tabs={tabs.ActualWidth:0.##}x{tabs.ActualHeight:0.##}, categoryViewportDip={categoryViewportHeight:0.##}, logicalViewport={categoryScroller.ViewportHeight:0.##}, canContentScroll={categoryScroller.CanContentScroll}, lastTab={lastCategoryHeight:0.##}/{lastCategoryBottom:0.##}, bodyViewport={bodyViewportHeight:0.##}, resetCollapsed={resetCollapsedForShortLayout}";
                resetExpander.IsExpanded = true;
                host.UpdateLayout();
                resetActionsAvailableWhenExpanded = resetExpander.IsExpanded
                    && resetFieldCombo.Visibility == Visibility.Visible;
                apply.Invoke(view, new object[] { width, 560d });
                host.UpdateLayout();
                host.Height = 900;
                host.Measure(new Size(width, 900));
                host.Arrange(new Rect(0, 0, width, 900));
                host.UpdateLayout();
                apply.Invoke(view, new object[] { width, 900d });
                host.UpdateLayout();
                resetExpandedAfterReturningToTallLayout = resetExpander.IsExpanded;
                resetTransitions = $"expandedActionsAvailable={resetActionsAvailableWhenExpanded}, restoredExpandedAtTallHeight={resetExpandedAfterReturningToTallLayout}";
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        output.WriteLine($"Settings {width:0}x560: {geometry}; reset transition: {resetTransitions}");
        Assert.Null(exception);
        Assert.True(categoryViewportHeight >= lastCategoryHeight - 0.5, geometry);
        Assert.True(lastCategoryBottom <= categoryViewportHeight + 1, geometry);
        Assert.True(bodyViewportHeight >= 160, geometry);
        Assert.True(resetCollapsedForShortLayout, geometry);
        Assert.True(resetActionsAvailableWhenExpanded, geometry);
        Assert.True(resetExpandedAfterReturningToTallLayout, geometry);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void SettingsSectionHeadersUseThemeAccentWithoutChangingTheirActions(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        Brush? backupIconForeground = null;
        Brush? backupIconBackground = null;
        Brush? automationIconForeground = null;
        Brush? automationIconBackground = null;
        Brush? expectedAccent = null;
        Brush? expectedAccentFill = null;
        var backupResetActionName = string.Empty;
        var automationResetActionName = string.Empty;
        var resetActionsEnabled = false;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                EnsureApplicationResources();
                var view = new GameSaveCenter.Playnite.Settings.GameSaveCenterSettingsView();
                ApplyTheme(view, theme);
                var viewType = view.GetType();
                var backupPanel = (FrameworkElement)viewType.GetField("SettingsBackupPanel", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var automationPanel = (FrameworkElement)viewType.GetField("SettingsAutomationPanel", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                backupPanel.Visibility = Visibility.Visible;
                automationPanel.Visibility = Visibility.Visible;
                window = CreateWindow(view, 1100, 760);
                window.Show();
                FlushLayout(window);

                var backupIconBackgroundBorder = (Border)viewType.GetField("SettingsBackupSectionIconBackground", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var backupIcon = (ThemeAwareIcon)viewType.GetField("SettingsBackupSectionIcon", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var automationIconBackgroundBorder = (Border)viewType.GetField("SettingsAutomationSectionIconBackground", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var automationIcon = (ThemeAwareIcon)viewType.GetField("SettingsAutomationSectionIcon", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var backupResetAction = FindVisualChildren<ButtonBase>(backupPanel)
                    .Single(button => AutomationProperties.GetName(button) == "恢复备份与恢复安全设置默认值");
                var automationResetAction = FindVisualChildren<ButtonBase>(automationPanel)
                    .Single(button => AutomationProperties.GetName(button) == "恢复自动化与媒体安全设置默认值");

                backupIconForeground = backupIcon.Foreground;
                backupIconBackground = backupIconBackgroundBorder.Background;
                automationIconForeground = automationIcon.Foreground;
                automationIconBackground = automationIconBackgroundBorder.Background;
                expectedAccent = view.TryFindResource("GscAccentBrush") as Brush;
                expectedAccentFill = view.TryFindResource("GscAccentIconFillBrush") as Brush;
                backupResetActionName = AutomationProperties.GetName(backupResetAction);
                automationResetActionName = AutomationProperties.GetName(automationResetAction);
                resetActionsEnabled = backupResetAction.IsEnabled && automationResetAction.IsEnabled;
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

        output.WriteLine($"Settings headers {theme}: backup={backupIconForeground}/{backupIconBackground}, automation={automationIconForeground}/{automationIconBackground}, reset={backupResetActionName}; {automationResetActionName}, enabled={resetActionsEnabled}");
        Assert.Null(exception);
        Assert.NotNull(expectedAccent);
        Assert.NotNull(expectedAccentFill);
        Assert.Same(expectedAccent, backupIconForeground);
        Assert.Same(expectedAccentFill, backupIconBackground);
        Assert.Same(expectedAccent, automationIconForeground);
        Assert.Same(expectedAccentFill, automationIconBackground);
        Assert.Equal("恢复备份与恢复安全设置默认值", backupResetActionName);
        Assert.Equal("恢复自动化与媒体安全设置默认值", automationResetActionName);
        Assert.True(resetActionsEnabled);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void SettingsHeaderAndPathActionsStayAnchoredToTheirLabelsAndEachOther(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var iconTitleTopDelta = 0d;
        var iconTitleHorizontalGap = 0d;
        var searchTitleLeftDelta = 0d;
        var searchHeight = 0d;
        var searchWidth = 0d;
        var resetControlCenterSpread = 0d;
        var resetControlHeightSpread = 0d;
        var resetControlDetails = string.Empty;
        var resetButtonCount = 0;
        var compactSearchWidth = 0d;
        var compactSearchRightOverflow = 0d;
        var compactSearchTitleLeftDelta = 0d;
        var wideSearchTitleLeftDelta = 0d;
        var windowedIconTitleTopDelta = 0d;
        var windowedSearchTitleLeftDelta = 0d;
        var windowedSearchIconTopGap = 0d;
        var windowedResetControlCenterSpread = 0d;
        var windowedResetControlHeightSpread = 0d;
        var centeredRegressionDelta = 0d;
        var wideShellWidth = 0d;
        var compactSaveHintRow = -1;
        var restoredSaveHintRow = -1;
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
                var resetFieldCombo = (FrameworkElement)viewType.GetField("SettingsResetFieldComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var saveHint = (FrameworkElement)viewType.GetField("SettingsSaveHint", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var pathCombo = (FrameworkElement)viewType.GetField("SettingsPathEditorComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var pathCard = (FrameworkElement)viewType.GetField("SettingsPathEditorCard", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;

                window = CreateWindow(view, 1280, 840);
                window.Show();
                FlushLayout(window);

                var iconBounds = BoundsIn(icon, headerGrid);
                var titleBounds = BoundsIn(title, headerGrid);
                var searchBounds = BoundsIn(search, headerGrid);
                var resetBounds = BoundsIn(resetCard, headerGrid);
                var saveHintBounds = BoundsIn(saveHint, headerGrid);
                iconTitleTopDelta = Math.Abs(iconBounds.Top - titleBounds.Top);
                iconTitleHorizontalGap = titleBounds.Left - iconBounds.Right;
                searchTitleLeftDelta = Math.Abs(searchBounds.Left - titleBounds.Left);
                searchHeight = search.ActualHeight;
                searchWidth = search.ActualWidth;
                resetTitleLeftDelta = Math.Abs(resetBounds.Left - titleBounds.Left);
                resetAfterSearchGap = resetBounds.Top - searchBounds.Bottom;
                saveHintTitleTopDelta = Math.Abs(saveHintBounds.Top - titleBounds.Top);

                var resetButtons = FindVisualChildren<ButtonBase>(resetCard)
                    .Where(button => button.Content is string label && label is "恢复单字段" or "恢复全部默认")
                    .Cast<FrameworkElement>()
                    .ToArray();
                resetButtonCount = resetButtons.Length;
                var resetControls = resetButtons.Concat(new[] { resetFieldCombo }).ToArray();
                var resetCenters = resetControls.Select(control => CenterY(control, resetCard)).ToArray();
                var resetHeights = resetControls.Select(control => control.ActualHeight).ToArray();
                resetControlCenterSpread = resetCenters.Max() - resetCenters.Min();
                resetControlHeightSpread = resetHeights.Max() - resetHeights.Min();
                resetControlDetails = string.Join(", ", resetControls.Select(control => $"{control.GetType().Name}={control.ActualWidth:0.##}×{control.ActualHeight:0.##} DIP @ {CenterY(control, resetCard):0.##}"));

                search.HorizontalAlignment = HorizontalAlignment.Center;
                FlushLayout(window);
                centeredRegressionDelta = Math.Abs(BoundsIn(search, headerGrid).Left - titleBounds.Left);
                search.HorizontalAlignment = HorizontalAlignment.Left;
                FlushLayout(window);

                window.Width = 1880;
                window.Height = 1200;
                FlushLayout(window);
                var wideTitleBounds = BoundsIn(title, headerGrid);
                var wideSearchBounds = BoundsIn(search, headerGrid);
                wideSearchTitleLeftDelta = Math.Abs(wideSearchBounds.Left - wideTitleBounds.Left);
                wideShellWidth = ((FrameworkElement)viewType.GetField("SettingsShell", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!).ActualWidth;

                // The latest user screenshot is 1881×1208 px; the 36-DIP controls appear
                // about 54 px tall, suggesting roughly 150% scaling. Probe the equivalent
                // 1254×800-DIP window as a reproduction hypothesis, not a DPI claim.
                window.Width = 1254;
                window.Height = 800;
                FlushLayout(window);
                var windowedIconBounds = BoundsIn(icon, headerGrid);
                var windowedTitleBounds = BoundsIn(title, headerGrid);
                var windowedSearchBounds = BoundsIn(search, headerGrid);
                windowedIconTitleTopDelta = Math.Abs(windowedIconBounds.Top - windowedTitleBounds.Top);
                windowedSearchTitleLeftDelta = Math.Abs(windowedSearchBounds.Left - windowedTitleBounds.Left);
                windowedSearchIconTopGap = windowedSearchBounds.Top - windowedIconBounds.Top;
                var windowedResetCenters = resetControls.Select(control => CenterY(control, resetCard)).ToArray();
                var windowedResetHeights = resetControls.Select(control => control.ActualHeight).ToArray();
                windowedResetControlCenterSpread = windowedResetCenters.Max() - windowedResetCenters.Min();
                windowedResetControlHeightSpread = windowedResetHeights.Max() - windowedResetHeights.Min();

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
                var compactSearchBounds = BoundsIn(search, headerGrid);
                compactSearchWidth = search.ActualWidth;
                compactSearchRightOverflow = Math.Max(0, compactSearchBounds.Right - headerGrid.ActualWidth);
                compactSearchTitleLeftDelta = Math.Abs(compactSearchBounds.Left - BoundsIn(title, headerGrid).Left);
                compactSaveHintRow = Grid.GetRow(saveHint);

                window.Width = 1280;
                window.Height = 840;
                FlushLayout(window);
                restoredSaveHintRow = Grid.GetRow(saveHint);
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

        output.WriteLine($"{theme} Settings: icon/title topΔ={iconTitleTopDelta:0.##} DIP, horizontal gap={iconTitleHorizontalGap:0.##} DIP, search/title leftΔ={searchTitleLeftDelta:0.##} DIP, centered-negative Δ={centeredRegressionDelta:0.##} DIP, wide shell/search={wideShellWidth:0.##}/{wideSearchTitleLeftDelta:0.##} DIP, windowed 1254×800 icon/title topΔ={windowedIconTitleTopDelta:0.##} DIP, search/title leftΔ={windowedSearchTitleLeftDelta:0.##} DIP, search/icon top gap={windowedSearchIconTopGap:0.##} DIP, reset center/height spread={windowedResetControlCenterSpread:0.##}/{windowedResetControlHeightSpread:0.##} DIP, search size={searchWidth:0.##}×{searchHeight:0.##} DIP, reset/title leftΔ={resetTitleLeftDelta:0.##} DIP, reset/search gap={resetAfterSearchGap:0.##} DIP, reset controls center/height spread={resetControlCenterSpread:0.##}/{resetControlHeightSpread:0.##} DIP ({resetControlDetails}), compact search width/overflow/leftΔ={compactSearchWidth:0.##}/{compactSearchRightOverflow:0.##}/{compactSearchTitleLeftDelta:0.##} DIP, save-hint row={compactSaveHintRow}->{restoredSaveHintRow}, hint/title topΔ={saveHintTitleTopDelta:0.##} DIP, path control center spread={pathControlCenterSpread:0.##} DIP, height spread={pathControlHeightSpread:0.##} DIP ({pathHeightDetails})");
        Assert.Null(exception);
        Assert.True(iconTitleTopDelta <= 12, $"settings icon top is {iconTitleTopDelta:0.##} DIP from the title top");
        Assert.InRange(iconTitleHorizontalGap, 11, 13);
        Assert.True(searchTitleLeftDelta <= 2, $"settings search starts {searchTitleLeftDelta:0.##} DIP away from the title edge");
        Assert.Equal(2, resetButtonCount);
        Assert.InRange(resetControlCenterSpread, 0, 1);
        Assert.InRange(resetControlHeightSpread, 0, 1);
        Assert.True(centeredRegressionDelta >= 40, $"centered search negative control should expose the old alignment defect; measured {centeredRegressionDelta:0.##} DIP");
        Assert.InRange(wideShellWidth, 1320, 1360);
        Assert.InRange(wideSearchTitleLeftDelta, 0, 2);
        Assert.True(windowedIconTitleTopDelta <= 12, $"windowed settings icon top is {windowedIconTitleTopDelta:0.##} DIP from the title top");
        Assert.InRange(windowedSearchTitleLeftDelta, 0, 2);
        Assert.True(windowedSearchIconTopGap >= 36, $"windowed search begins only {windowedSearchIconTopGap:0.##} DIP below the header icon; expected a separate header row");
        Assert.InRange(windowedResetControlCenterSpread, 0, 1);
        Assert.InRange(windowedResetControlHeightSpread, 0, 1);
        Assert.InRange(searchWidth, 500, 520);
        Assert.InRange(searchHeight, 35, 37);
        Assert.InRange(compactSearchWidth, 260, 520);
        Assert.InRange(compactSearchRightOverflow, 0, 1);
        Assert.InRange(compactSearchTitleLeftDelta, 0, 2);
        Assert.Equal(1, compactSaveHintRow);
        Assert.Equal(0, restoredSaveHintRow);
        Assert.True(resetTitleLeftDelta <= 2, $"settings reset card starts {resetTitleLeftDelta:0.##} DIP away from the title edge");
        Assert.True(resetAfterSearchGap >= 8, $"settings reset card starts only {resetAfterSearchGap:0.##} DIP after the search field");
        Assert.True(saveHintTitleTopDelta <= 36, $"settings save hint is {saveHintTitleTopDelta:0.##} DIP below the title");
        Assert.True(pathControlCenterSpread <= 3, $"path combo/actions centers span {pathControlCenterSpread:0.##} DIP");
        Assert.True(pathControlHeightSpread <= 10, $"path combo/actions heights differ by {pathControlHeightSpread:0.##} DIP ({pathHeightDetails})");
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    [InlineData(GameSaveCenterThemeMode.FollowPlaynite)]
    public void SettingsGlassStrengthSliderKeepsTrackHitAreaReadableValueAndKeyboardSteps(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var observations = string.Empty;
        RunSta(() =>
        {
            Window? window = null;
            try
            {
                EnsureApplicationResources();
                var settings = new SliderProbeSettings();
                var view = new GameSaveCenterSettingsView { DataContext = settings };
                ApplyTheme(view, theme);
                var viewType = view.GetType();
                var tabs = (ListBox)viewType.GetField("SettingsSectionTabs", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var slider = (Slider)viewType.GetField("GlassStrengthSlider", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var pageScroller = (ScrollViewer)viewType.GetField("SettingsScroller", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                tabs.SelectedIndex = 2;

                window = CreateWindow(view, 1100, 700);
                window.ShowActivated = true;
                window.Show();
                FlushLayout(window);
                slider.ApplyTemplate();
                FlushLayout(window);
                Assert.True(slider.IsVisible, "the production appearance section must show the glass strength slider");

                var track = Assert.IsType<Track>(slider.Template!.FindName("PART_Track", slider));
                var thumb = Assert.IsType<Thumb>(track.Thumb);
                var decrease = Assert.IsType<RepeatButton>(track.DecreaseRepeatButton);
                var increase = Assert.IsType<RepeatButton>(track.IncreaseRepeatButton);
                var dock = Assert.IsType<DockPanel>(slider.Parent);
                var label = Assert.Single(dock.Children.OfType<TextBlock>());
                var sliderBounds = BoundsIn(slider, dock);
                var labelBounds = BoundsIn(label, dock);
                var dpi = VisualTreeHelper.GetDpi(slider);

                Assert.True(slider.ActualWidth >= 140 && slider.ActualHeight >= 32,
                    $"settings slider must have a usable control footprint; size={slider.ActualWidth:0.##}x{slider.ActualHeight:0.##}");
                Assert.True(thumb.ActualWidth >= 32 && thumb.ActualHeight >= 32,
                    $"settings thumb must remain a readable target; size={thumb.ActualWidth:0.##}x{thumb.ActualHeight:0.##}");
                Assert.True(sliderBounds.Right <= labelBounds.Left + 0.5, "value label must not overlap the slider");
                Assert.Equal("78%", label.Text);
                Assert.Equal(78, settings.GlassEffectStrength);
                Assert.Equal("毛玻璃强度", AutomationProperties.GetName(slider));

                foreach (var button in new[] { decrease, increase })
                {
                    var bounds = BoundsIn(button, slider);
                    Assert.True(bounds.Width >= 25, $"track side needs room for precise activation: {bounds}");
                    foreach (var y in new[] { slider.ActualHeight / 2 - 14, slider.ActualHeight / 2, slider.ActualHeight / 2 + 14 })
                    {
                        var hit = VisualTreeHelper.HitTest(slider, new Point(bounds.Left + bounds.Width / 2, y))?.VisualHit;
                        Assert.True(IsVisualDescendantOf(hit, button),
                            $"both sides of the visible track must respond across the control height; button={button.Command},point={bounds.Left + bounds.Width / 2:0.##},{y:0.##},hit={hit?.GetType().Name ?? "none"}");
                    }
                }

                var peer = UIElementAutomationPeer.CreatePeerForElement(slider);
                var range = Assert.IsAssignableFrom<IRangeValueProvider>(peer!.GetPattern(PatternInterface.RangeValue));
                Assert.Equal(78, range.Value);
                Assert.Equal(1, slider.SmallChange);
                Assert.Equal(10, slider.LargeChange);

                var invokeTrack = typeof(RepeatButton).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException("WPF RepeatButton.OnClick was not found.");
                invokeTrack.Invoke(decrease, null);
                FlushLayout(window);
                Assert.Equal(68, slider.Value);
                Assert.Equal(68, settings.GlassEffectStrength);
                Assert.Equal("68%", label.Text);
                invokeTrack.Invoke(increase, null);
                FlushLayout(window);
                Assert.Equal(78, slider.Value);
                Assert.Equal(78, settings.GlassEffectStrength);

                window.Activate();
                Assert.Same(slider, Keyboard.Focus(slider));
                RaiseSliderKey(slider, window, Key.Right);
                FlushLayout(window);
                Assert.Equal(79, slider.Value);
                Assert.Equal(79, settings.GlassEffectStrength);
                Assert.Equal("79%", label.Text);
                RaiseSliderKey(slider, window, Key.PageUp);
                FlushLayout(window);
                Assert.Equal(89, slider.Value);
                Assert.Equal(89, settings.GlassEffectStrength);
                RaiseSliderKey(slider, window, Key.End);
                FlushLayout(window);
                Assert.Equal(100, slider.Value);
                Assert.Equal("100%", label.Text);
                RaiseSliderKey(slider, window, Key.Home);
                FlushLayout(window);
                Assert.Equal(20, slider.Value);
                Assert.Equal("20%", label.Text);

                window.Width = 560;
                FlushLayout(window);
                sliderBounds = BoundsIn(slider, dock);
                labelBounds = BoundsIn(label, dock);
                Assert.True(slider.IsVisible && slider.ActualWidth >= 140, "compact settings must retain the usable slider");
                Assert.True(sliderBounds.Right <= labelBounds.Left + 0.5, "compact value label must stay beside its own slider");

                window.Height = 640;
                slider.BringIntoView();
                FlushLayout(window);
                var pagePresenter = FindVisualChildren<ScrollContentPresenter>(pageScroller).First();
                var shortSliderBounds = BoundsIn(slider, pageScroller);
                var shortViewport = BoundsIn(pagePresenter, pageScroller);
                Assert.True(shortSliderBounds.Top >= shortViewport.Top - 1 && shortSliderBounds.Bottom <= shortViewport.Bottom + 1,
                    $"the slider must remain reachable in a short settings window; slider={shortSliderBounds},viewport={shortViewport}");

                Keyboard.ClearFocus();
                slider.IsEnabled = false;
                Assert.NotSame(slider, Keyboard.Focus(slider));
                Assert.Throws<ElementNotEnabledException>(() => range.SetValue(55));
                Assert.Equal(20, slider.Value);
                Assert.Equal(20, settings.GlassEffectStrength);
                observations = $"{theme}:dpi={dpi.DpiScaleX:0.##},slider={slider.ActualWidth:0.##}x{slider.ActualHeight:0.##},thumb={thumb.ActualWidth:0.##}x{thumb.ActualHeight:0.##},track={decrease.ActualWidth:0.##}/{increase.ActualWidth:0.##},value={label.Text},compact={sliderBounds}/{labelBounds}";
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

        output.WriteLine(observations);
        Assert.Null(exception);
    }

    private static void RaiseSliderKey(Slider slider, Window window, Key key)
    {
        var source = PresentationSource.FromVisual(window)
            ?? throw new InvalidOperationException("Settings slider is not attached to a WPF presentation source.");
        slider.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        });
        slider.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key)
        {
            RoutedEvent = Keyboard.KeyUpEvent
        });
    }

    private static bool IsVisualDescendantOf(DependencyObject? candidate, DependencyObject ancestor)
    {
        for (var current = candidate; current != null; current = VisualTreeHelper.GetParent(current))
        {
            if (ReferenceEquals(current, ancestor))
                return true;
        }
        return false;
    }

    private sealed class SliderProbeSettings
    {
        public int GlassEffectStrength { get; set; } = 78;
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    [InlineData(GameSaveCenterThemeMode.FollowPlaynite)]
    public void SettingsDenseOptionsKeepEachHelpTextWithItsOwnToggleAtNarrowSizes(GameSaveCenterThemeMode theme)
    {
        Exception? exception = null;
        var observations = new System.Collections.Generic.List<string>();
        RunSta(() =>
        {
            Window? window = null;
            try
            {
                EnsureApplicationResources();
                var settings = new DenseSettingsProbe();
                var view = new GameSaveCenterSettingsView { DataContext = settings };
                ApplyTheme(view, theme);
                var viewType = view.GetType();
                var tabs = (ListBox)viewType.GetField("SettingsSectionTabs", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var automation = (FrameworkElement)viewType.GetField("SettingsAutomationPanel", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var pageScroller = (ScrollViewer)viewType.GetField("SettingsScroller", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                tabs.SelectedIndex = 3;

                window = CreateWindow(view, 1280, 840);
                window.Show();
                FlushLayout(window);
                var toggles = FindVisualChildren<ToggleSwitch>(automation).ToArray();
                var mediaToggle = Assert.Single(toggles, toggle => AutomationProperties.GetName(toggle) == "同步新增截图和录像");
                var safeModeToggle = Assert.Single(toggles, toggle => AutomationProperties.GetName(toggle) == "安全模式");
                var inspectedNames = new[]
                {
                    "随 Playnite 启动 Worker",
                    "检测外部启动的游戏",
                    "会话存档路径检测",
                    "同步新增截图和录像",
                    "任务完成或失败时显示 Playnite 通知",
                    "安全模式",
                    "下次以安全模式启动",
                    "启用恢复可用性巡检"
                };
                var inspected = inspectedNames.Select(name => Assert.Single(toggles, toggle => AutomationProperties.GetName(toggle) == name)).ToArray();
                var sourceGroup = Assert.Single(FindVisualChildren<WrapPanel>(automation),
                    panel => panel.Children.OfType<ToggleSwitch>().Count() == 5);

                foreach (var size in new[] { (Width: 1280d, Height: 840d), (Width: 920d, Height: 700d), (Width: 560d, Height: 640d) })
                {
                    window.Width = size.Width;
                    window.Height = size.Height;
                    FlushLayout(window);
                    Assert.True(automation.IsVisible, "automation settings must remain selected after resize");

                    foreach (var toggle in inspected)
                    {
                        toggle.ApplyTemplate();
                        var track = Assert.IsType<Border>(toggle.Template!.FindName("Track", toggle));
                        var content = FindVisualChildren<ContentPresenter>(toggle).First();
                        var lines = FindVisualChildren<TextBlock>(content).ToArray();
                        Assert.Equal(2, lines.Length);
                        var trackBounds = BoundsIn(track, toggle);
                        var titleBounds = BoundsIn(lines[0], toggle);
                        var helpBounds = BoundsIn(lines[1], toggle);
                        Assert.True(titleBounds.Left >= trackBounds.Right + 4,
                            $"title must stay beside its own switch at {size.Width} DIP: {AutomationProperties.GetName(toggle)} track={trackBounds},title={titleBounds}");
                        Assert.True(Math.Abs(helpBounds.Left - titleBounds.Left) <= 1 && helpBounds.Top >= titleBounds.Bottom - 1,
                            $"help text must stay below its own title at {size.Width} DIP: {AutomationProperties.GetName(toggle)} title={titleBounds},help={helpBounds}");
                        Assert.True(helpBounds.Right <= toggle.ActualWidth + 1 && helpBounds.Bottom <= toggle.ActualHeight + 1,
                            $"help text must fit its own row at {size.Width} DIP: {AutomationProperties.GetName(toggle)} help={helpBounds},row={toggle.ActualWidth:0.##}x{toggle.ActualHeight:0.##}");
                        Assert.True(lines[0].FontWeight.ToOpenTypeWeight() > lines[1].FontWeight.ToOpenTypeWeight(),
                            $"the option title must have stronger weight than its help text: {AutomationProperties.GetName(toggle)}");
                        var titleColor = (lines[0].Foreground as SolidColorBrush)?.Color;
                        var helpColor = (lines[1].Foreground as SolidColorBrush)?.Color;
                        Assert.True(titleColor.HasValue && helpColor.HasValue && titleColor != helpColor,
                            $"the help text must retain the secondary text brush: {AutomationProperties.GetName(toggle)} title={titleColor},help={helpColor}");
                    }

                    var childToggles = sourceGroup.Children.OfType<ToggleSwitch>().ToArray();
                    Assert.Equal(5, childToggles.Length);
                    var childBounds = childToggles.Select(child => BoundsIn(child, sourceGroup)).ToArray();
                    for (var index = 0; index < childBounds.Length; index++)
                    {
                        Assert.True(childBounds[index].Left >= -1 && childBounds[index].Right <= sourceGroup.ActualWidth + 1,
                            $"media source option must remain inside the group at {size.Width} DIP: {childBounds[index]}/{sourceGroup.ActualWidth:0.##}");
                        for (var other = index + 1; other < childBounds.Length; other++)
                            Assert.True(childBounds[index].IntersectsWith(childBounds[other]) == false,
                                $"wrapped source options must not overlap at {size.Width} DIP: {childBounds[index]} and {childBounds[other]}");
                    }
                    observations.Add($"{size.Width:0}x{size.Height:0}:sourceRows={childBounds.Select(bounds => Math.Round(bounds.Top)).Distinct().Count()},safeMode={safeModeToggle.ActualWidth:0.##}x{safeModeToggle.ActualHeight:0.##}");
                }

                safeModeToggle.BringIntoView();
                FlushLayout(window);
                var pagePresenter = FindVisualChildren<ScrollContentPresenter>(pageScroller).First();
                var safeBounds = BoundsIn(safeModeToggle, pageScroller);
                var viewport = BoundsIn(pagePresenter, pageScroller);
                Assert.True(safeBounds.Top >= viewport.Top - 1 && safeBounds.Bottom <= viewport.Bottom + 1,
                    $"the bottom option must remain reachable in the short settings page: option={safeBounds},viewport={viewport}");

                Assert.True(settings.EnableMediaSync);
                Assert.True(sourceGroup.IsEnabled);
                mediaToggle.IsChecked = false;
                FlushLayout(window);
                Assert.False(settings.EnableMediaSync);
                Assert.False(sourceGroup.IsEnabled);
                Assert.True(sourceGroup.Children.OfType<ToggleSwitch>().All(child => !child.IsEnabled),
                    "disabled media source choices must follow the parent value without disappearing or moving under another label");
                mediaToggle.IsChecked = true;
                FlushLayout(window);
                Assert.True(settings.EnableMediaSync && sourceGroup.IsEnabled);
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

        foreach (var observation in observations)
            output.WriteLine($"{theme} {observation}");
        Assert.Null(exception);
    }

    private sealed class DenseSettingsProbe : INotifyPropertyChanged
    {
        private bool enableMediaSync = true;
        public event PropertyChangedEventHandler? PropertyChanged;
        public bool EnableMediaSync
        {
            get => enableMediaSync;
            set
            {
                if (enableMediaSync == value) return;
                enableMediaSync = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableMediaSync)));
            }
        }
        public bool EnableCloudUpload { get; set; } = true;
        public bool EnableTaskNotifications { get; set; } = true;
        public bool HealthInspectionEnabled { get; set; } = true;
    }

    private static MediaPageContext CreateMediaContext()
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
            SelectedGame = target,
            MediaInboxItems = new ObservableCollection<MediaItemDto>(Enumerable.Range(0, 80).Select(index => new MediaItemDto
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
            }))
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

    private void EnsureApplicationResources()
    {
        var application = Application.Current ?? new Application();
        fixture.RegisterApplicationForShutdown(application);
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

    private void RunSta(Action action)
    {
        Exception? exception = null;
        dispatcher.Invoke(new Action(() =>
        {
            try { action(); }
            catch (Exception caught) { exception = caught; }
        }));
        if (exception != null) throw exception;
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

    private static GradientStopCollection GetContrastStops(Brush brush)
    {
        if (brush is LinearGradientBrush linearGradient)
            return linearGradient.GradientStops;
        if (brush is SolidColorBrush solidColor)
        {
            return new GradientStopCollection
            {
                new GradientStop(solidColor.Color, 0),
                new GradientStop(solidColor.Color, 1)
            };
        }

        throw new InvalidOperationException($"Unsupported production surface brush for contrast measurement: {brush.GetType().Name}");
    }

    private static (int RowCount, double MinGap) MeasureWrapRows(WrapPanel panel, Visual ancestor)
    {
        var bounds = panel.Children
            .OfType<FrameworkElement>()
            .Where(child => child.Visibility == Visibility.Visible && child.ActualHeight > 0)
            .Select(child => BoundsIn(child, ancestor))
            .OrderBy(rect => rect.Top)
            .ToArray();
        var rows = new System.Collections.Generic.List<Rect>();
        foreach (var rect in bounds)
        {
            var rowIndex = rows.FindIndex(row => row.Top < rect.Bottom - 0.5 && rect.Top < row.Bottom - 0.5);
            if (rowIndex < 0)
                rows.Add(rect);
            else
            {
                var row = rows[rowIndex];
                rows[rowIndex] = new Rect(
                    Math.Min(row.Left, rect.Left),
                    Math.Min(row.Top, rect.Top),
                    Math.Max(row.Right, rect.Right) - Math.Min(row.Left, rect.Left),
                    Math.Max(row.Bottom, rect.Bottom) - Math.Min(row.Top, rect.Top));
            }
        }

        rows.Sort((left, right) => left.Top.CompareTo(right.Top));
        var gaps = rows.Zip(rows.Skip(1), (previous, next) => next.Top - previous.Bottom).ToArray();
        return (rows.Count, gaps.Length == 0 ? double.PositiveInfinity : gaps.Min());
    }

    private static T FindVisualAncestor<T>(DependencyObject child, DependencyObject stopAt)
        where T : DependencyObject
    {
        for (var current = VisualTreeHelper.GetParent(child); current != null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is T match)
                return match;
            if (ReferenceEquals(current, stopAt))
                break;
        }
        throw new InvalidOperationException($"Could not find {typeof(T).Name} ancestor before the supplied visual root.");
    }

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

    private static TextBlock FindBoundText(DependencyObject root, string bindingPath)
        => FindVisualChildren<TextBlock>(root).Single(textBlock =>
            BindingOperations.GetBinding(textBlock, TextBlock.TextProperty) is Binding binding &&
            string.Equals(binding.Path?.Path, bindingPath, StringComparison.Ordinal));

    private sealed class MediaPageContext : INotifyPropertyChanged
    {
        private SyntheticGameTarget? selectedGame;

        public ObservableCollection<SyntheticGameTarget> Games { get; set; } = new();
        public SyntheticGameTarget? SelectedGame
        {
            get => selectedGame;
            set
            {
                if (ReferenceEquals(selectedGame, value)) return;
                selectedGame = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedGame)));
            }
        }
        public ObservableCollection<MediaItemDto> MediaInboxItems { get; set; } = new();
        private string mediaInboxMode = "待归类";
        public string MediaInboxMode
        {
            get => mediaInboxMode;
            set
            {
                if (string.Equals(mediaInboxMode, value, StringComparison.Ordinal)) return;
                mediaInboxMode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MediaInboxMode)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MediaInboxTitle)));
            }
        }
        public string MediaInboxTitle => MediaInboxMode == "已忽略" ? "已忽略媒体" : "待归类媒体";
        public int MediaTabIndex { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class OverviewHeaderCopyProbe : INotifyPropertyChanged
    {
        private bool isDashboardSnapshotLoaded;

        public bool IsDashboardSnapshotLoaded
        {
            get => isDashboardSnapshotLoaded;
            set
            {
                if (isDashboardSnapshotLoaded == value) return;
                isDashboardSnapshotLoaded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDashboardSnapshotLoaded)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OverviewCloudQueueDisplay)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OverviewCloudAttentionDisplay)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OverviewPriorityKind)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OverviewPriorityTitle)));
            }
        }

        public DashboardSnapshotDto Snapshot { get; } = new()
        {
            ManagedGames = 1302,
            MatchedGames = 1200,
            CloudTransfers = new CloudTransferSummaryDto { FailedCount = 4 }
        };

        public string OverviewManagedGamesDisplay => "1,302";
        public string OverviewCloudQueueDisplay => IsDashboardSnapshotLoaded ? Snapshot.CloudTransfers.QueueCount.ToString() : "—";
        public string OverviewCloudAttentionDisplay => IsDashboardSnapshotLoaded ? $"{Snapshot.CloudTransfers.AttentionCount} 项需关注" : "— 项需关注";
        public string OverviewPriorityKind => IsDashboardSnapshotLoaded ? "Cloud" : "Loading";
        public string OverviewPriorityTitle => IsDashboardSnapshotLoaded ? $"{Snapshot.CloudTransfers.AttentionCount} 项云端任务需要处理" : "正在读取概览数据";
        public string OverviewSnapshotScopeDisplay => "全库 · 合成游戏库";
        public string OverviewSnapshotUpdatedDisplay => "更新于刚刚";
        public ICommand RefreshCommand { get; } = new RelayCommand(_ => { });
        public ICommand BackupAllCommand { get; } = new RelayCommand(_ => { });
        public ICommand SyncMediaCommand { get; } = new RelayCommand(_ => { });
        public ICommand OpenCloudQueueCommand { get; } = new RelayCommand(_ => { });

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class TrainerImportCopyProbe : INotifyPropertyChanged
    {
        private bool hasPendingGameToolEntrySelection;

        public bool HasPendingGameToolEntrySelection
        {
            get => hasPendingGameToolEntrySelection;
            set
            {
                if (hasPendingGameToolEntrySelection == value) return;
                hasPendingGameToolEntrySelection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasPendingGameToolEntrySelection)));
            }
        }

        public ObservableCollection<string> ImportEntryCandidates { get; } = new() { "Synthetic trainer.exe" };
        public string SelectedImportEntryCandidate { get; set; } = "Synthetic trainer.exe";
        public ICommand ConfirmGameToolImportCommand { get; } = new RelayCommand(_ => { });
        public ICommand CancelGameToolImportCommand { get; } = new RelayCommand(_ => { });

        public event PropertyChangedEventHandler? PropertyChanged;
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

    private sealed class TaskSummaryLayoutProbe
    {
        public int RunningTaskCount { get; } = 2;
        public bool TaskHasActiveFilters { get; } = true;
        public string TaskLoadedSummary { get; } = "最近加载 50 条 · 全部任务 4,557 条";
        public string TaskActiveFiltersSummary { get; } = "当前：状态失败 · 类型全部 · 最近任务 · 全部时间";
        public string TaskPageStatusSummary { get; } = "最近更新：刚刚";
        public string TaskPageStatusSummaryFullDisplay { get; } = "合成布局测试的完整任务状态摘要";
    }

    private sealed class ProductionHeaderCommandProbe
    {
        public int BackupSelectedExecutions { get; private set; }
        public int BackupAllExecutions { get; private set; }
        public ICommand BackupSelectedCommand { get; }
        public ICommand BackupAllCommand { get; }

        public ProductionHeaderCommandProbe()
        {
            BackupSelectedCommand = new RelayCommand(_ => BackupSelectedExecutions++);
            BackupAllCommand = new RelayCommand(_ => BackupAllExecutions++);
        }
    }

    private sealed class MaintenanceEmptyStateCopyProbe : INotifyPropertyChanged
    {
        private string maintenanceState = "Empty";

        public ObservableCollection<object> Findings { get; } = new();
        public int MaintenanceTabIndex { get; set; }
        public bool IsWorkerOffline => false;
        public string MaintenanceState
        {
            get => maintenanceState;
            set
            {
                if (string.Equals(maintenanceState, value, StringComparison.Ordinal)) return;
                maintenanceState = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaintenanceState)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaintenancePresenterState)));
            }
        }
        public string MaintenancePresenterState => MaintenanceState;
        public string MaintenanceStateTitle => "暂无需要处理的诊断项";
        public string MaintenanceStateMessage => "没有需要处理的诊断项。";
        public string MaintenanceStateDetail => string.Empty;
        public bool MaintenanceStateOverlayVisible => false;
        public ICommand RefreshDiagnosticsCommand { get; } = new RelayCommand(_ => { });
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class CloudTransferHeaderProbe
    {
        public string CloudTransferLoadedSummary { get; } = "全局 1 项 · 已加载全部 1 项";
        public string CloudTransferStateDetail { get; } = "合成传输数据";
        public object CloudTransferViewSummary { get; } = new { QueueControlDisplay = "自动队列运行中" };
    }
}

[CollectionDefinition("ReportedWorkspaceLayoutWpf", DisableParallelization = true)]
public sealed class ReportedWorkspaceLayoutWpfCollection : ICollectionFixture<ReportedWorkspaceLayoutWpfFixture>
{
}

public sealed class ReportedWorkspaceLayoutWpfFixture : IDisposable
{
    private readonly ManualResetEvent dispatcherReady = new(false);
    private Thread? dispatcherThread;
    private bool ownsDispatcher;
    private Dispatcher? dispatcher;
    private Application? application;
    private Exception? dispatcherStartupException;

    public ReportedWorkspaceLayoutWpfFixture()
    {
        var existingApplication = Application.Current;
        if (existingApplication != null
            && !existingApplication.Dispatcher.HasShutdownStarted
            && !existingApplication.Dispatcher.HasShutdownFinished)
        {
            dispatcher = existingApplication.Dispatcher;
            return;
        }

        ownsDispatcher = true;
        dispatcherThread = new Thread(RunDispatcher)
        {
            IsBackground = true,
            Name = "Reported workspace layout WPF STA"
        };
        dispatcherThread.SetApartmentState(ApartmentState.STA);
        dispatcherThread.Start();

        if (!dispatcherReady.WaitOne(TimeSpan.FromSeconds(10)))
            throw new TimeoutException("Reported workspace layout WPF dispatcher did not initialize within 10 seconds.");
        if (dispatcherStartupException != null)
            throw new InvalidOperationException("Reported workspace layout WPF dispatcher failed to initialize.", dispatcherStartupException);
    }

    public Dispatcher Dispatcher => dispatcher
        ?? throw new InvalidOperationException("Reported workspace layout WPF dispatcher is unavailable.");

    public void RegisterApplicationForShutdown(Application value)
    {
        if (ownsDispatcher)
            application = value;
    }

    public void Dispose()
    {
        if (!ownsDispatcher)
        {
            dispatcherReady.Dispose();
            return;
        }

        var ownedDispatcher = dispatcher;
        var ownedThread = dispatcherThread;
        if (ownedDispatcher == null || ownedThread == null || !ownedThread.IsAlive)
        {
            dispatcherReady.Dispose();
            return;
        }

        try
        {
            ownedDispatcher.Invoke(new Action(() =>
            {
                if (application != null && ReferenceEquals(application.Dispatcher, ownedDispatcher))
                    application.Shutdown();
                if (!ownedDispatcher.HasShutdownStarted)
                    ownedDispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
            }));

            if (!ownedThread.Join(TimeSpan.FromSeconds(10)))
                throw new TimeoutException("Reported workspace layout WPF dispatcher did not shut down within 10 seconds.");
        }
        finally
        {
            dispatcherReady.Dispose();
        }
    }

    private void RunDispatcher()
    {
        try
        {
            dispatcher = Dispatcher.CurrentDispatcher;
        }
        catch (Exception exception)
        {
            dispatcherStartupException = exception;
        }
        finally
        {
            dispatcherReady.Set();
        }

        if (dispatcher != null)
            Dispatcher.Run();
    }
}
