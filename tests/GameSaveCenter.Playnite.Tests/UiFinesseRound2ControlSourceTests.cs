using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class UiFinesseRound2ControlSourceTests
{
    [Fact]
    public void SharedCheckboxExposesAnExplicitIndeterminateMark()
    {
        var tokens = Read("src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml");

        Assert.Contains("x:Key=\"GscCheckBox\"", tokens);
        Assert.Contains("x:Name=\"IndeterminateMark\"", tokens);
        Assert.Contains("<Trigger Property=\"IsChecked\" Value=\"{x:Null}\">", tokens);
        Assert.Contains("IndeterminateMark\" Property=\"Visibility\" Value=\"Visible\"", tokens);
        Assert.Contains("x:Key=\"GscDataGridCheckBox\"", tokens);
    }

    [Fact]
    public void IndeterminateProgressStopsItsSweepWhenTheStateEnds()
    {
        var tokens = Read("src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml");

        Assert.Contains("<Trigger Property=\"IsIndeterminate\" Value=\"True\">", tokens);
        Assert.Contains("<BeginStoryboard x:Name=\"GscIndeterminateSweep\">", tokens);
        Assert.Contains("<Storyboard RepeatBehavior=\"Forever\">", tokens);
        Assert.Contains("<StopStoryboard BeginStoryboardName=\"GscIndeterminateSweep\"/>", tokens);
        Assert.Contains("<Trigger Property=\"IsIndeterminate\" Value=\"True\">", tokens);
        Assert.Contains("正在进行；完成时间取决于实际任务。", tokens);
    }

    [Fact]
    public void DevelopmentFixtureCoversSelectionInputPopupButtonToggleAndSliderStates()
    {
        var fixture = Read("src", "GameSaveCenter.Playnite", "Views", "Development", "UiFrameworkProbeView.xaml");

        Assert.Contains("Style=\"{StaticResource GscWpfUiTextBox}\"", fixture);
        Assert.Contains("Style=\"{StaticResource GscWpfUiComboBox}\"", fixture);
        Assert.Contains("Style=\"{StaticResource GscWpfUiPrimaryButton}\"", fixture);
        Assert.Contains("IsEnabled=\"False\"", fixture);
        Assert.Contains("Style=\"{StaticResource GscWpfUiToggleSwitch}\"", fixture);
        Assert.Contains("IsThreeState=\"True\"", fixture);
        Assert.Contains("Style=\"{StaticResource GscSlider}\"", fixture);
        Assert.Contains("Style=\"{StaticResource ProbeDataGrid}\"", fixture);
    }

    [Fact]
    public void DashboardBusyButtonsKeepTheirContentSlotAndExposeTheSharedIndicator()
    {
        var controls = Read("src", "GameSaveCenter.Playnite", "Controls", "NativeWpfControls.cs");
        var production = Read("src", "GameSaveCenter.Playnite", "Themes", "WpfUiProduction.xaml");
        var dashboard = Read("src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml");

        Assert.Contains("IsBusyProperty", controls);
        Assert.Contains("x:Name=\"BusyIndicatorHost\"", production);
        Assert.Contains("<Trigger Property=\"IsBusyIndicatorVisible\" Value=\"True\">", production);
        Assert.Contains("ContentPresenter Content=\"{TemplateBinding Content}\"", production);
        Assert.Contains("IsBusy=\"{Binding IsBusy}\"", dashboard);
        Assert.Equal(3, dashboard.Split(new[] { "IsBusy=\"{Binding IsBusy}\"" }, StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public void DangerousConfirmationKeepsCancelAsTheInitialFocusTarget()
    {
        var dashboardCode = Read("src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs");

        Assert.Contains("DialogConfirmButton.SetResourceReference(Control.BackgroundProperty, request.IsDangerous ? \"GscErrorBrush\" : \"GscAccentBrush\")", dashboardCode);
        Assert.Contains("OpenDialog(request.IsDangerous ? DialogCancelButton : DialogConfirmButton)", dashboardCode);
        Assert.Contains("if (IsLoaded && DialogOverlay.Visibility == Visibility.Visible && !dialogLifecycle.IsClosing)", dashboardCode);
    }

    [Fact]
    public void ProductionButtonClearsPressedLayersWhenItsTemplateIsUnloaded()
    {
        var controls = Read("src", "GameSaveCenter.Playnite", "Controls", "NativeWpfControls.cs");

        Assert.Contains("Unloaded += OnButtonUnloaded;", controls);
        Assert.Contains("private void OnButtonUnloaded(object sender, RoutedEventArgs e)", controls);
        Assert.Contains("ResetInteractionLayers();", controls);
        Assert.Contains("chrome.RenderTransform is ScaleTransform scale && !scale.IsFrozen", controls);
        Assert.Contains("scale.ScaleX = 1;", controls);
        Assert.Contains("scale.ScaleY = 1;", controls);
        Assert.Contains("ResetOverlay(\"PressedOverlay\")", controls);
        Assert.Contains("ResetOverlay(\"FocusOverlay\")", controls);
    }

    [Fact]
    public void SelectedAcrylicNavigationKeepsItsStrongStateWhenHovered()
    {
        var resources = Read("src", "GameSaveCenter.Playnite", "Themes", "AcrylicProductionResources.xaml");

        Assert.Contains("<MultiTrigger>", resources);
        Assert.Contains("<Condition Property=\"IsChecked\" Value=\"True\"/>", resources);
        Assert.Contains("<Condition Property=\"IsMouseOver\" Value=\"True\"/>", resources);
        Assert.Contains("GscAccentTintStrongBrush", resources);
        Assert.Contains("GscSelectionTextBrush", resources);
    }

    [Fact]
    public void MediaInboxKeepsAReadablePrimaryViewportBeforePageOverflow()
    {
        var view = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml");

        Assert.Contains("x:Name=\"MediaInboxPageScrollViewer\"", view);
        Assert.Contains("x:Name=\"MediaInboxLayout\" Grid.Row=\"1\" MinHeight=\"0\"", view);
        Assert.Contains("x:Name=\"MediaInboxTableFrame\"", view);
        Assert.Contains("Padding=\"14,12,14,12\" MinHeight=\"0\"", view);
        Assert.Contains("x:Name=\"MediaInboxGrid\"", view);
        Assert.Contains("VirtualizingPanel.ScrollUnit=\"Item\"", view);
    }

    [Fact]
    public void ToolbarAndTooltipContractsKeepTheirSharedResponsiveBoundaries()
    {
        var tokens = Read("src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml");
        var dashboard = Read("src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml");
        var trainer = Read("src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml.cs");

        Assert.Contains("<Style TargetType=\"ToolTip\">", tokens);
        Assert.Contains("FontFamily\" Value=\"{DynamicResource GscUiFontFamily}\"", tokens);
        Assert.Contains("MaxWidth\" Value=\"420\"", tokens);
        Assert.Contains("Padding\" Value=\"10,7\"", tokens);
        Assert.Contains("ToolTipService.InitialShowDelay=\"350\"", dashboard);
        Assert.Contains("ToolTipService.ShowDuration=\"18000\"", dashboard);
        Assert.Contains("var stackInstalled = width < 980", trainer);
        Assert.Contains("Grid.SetRow(TrainerToolsToolbar, stackInstalled ? 1 : 0)", trainer);
        Assert.Contains("TrainerToolsToolbar.HorizontalAlignment = stackInstalled", trainer);
    }

    [Fact]
    public void FloatingShellsUseOneTooltipDelayContractForQuickPointerMoves()
    {
        var dashboard = Read("src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml");
        var shell = Read("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml");
        var settings = Read("src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml");

        foreach (var view in new[] { dashboard, shell, settings })
        {
            Assert.Contains("ToolTipService.InitialShowDelay=\"350\"", view);
            Assert.Contains("ToolTipService.BetweenShowDelay=\"100\"", view);
        }

        Assert.Contains("ToolTipService.ShowDuration=\"18000\"", dashboard);
        Assert.Contains("MaxWidth\" Value=\"420\"", Read("src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml"));
        Assert.Contains("AutomationProperties.Name=\"游戏选择器\"", shell);
        Assert.Contains("AutomationProperties.Name=\"选择当前游戏\"", dashboard);
    }

    [Fact]
    public void ComboPopupClosesOutsideAndKeepsItsReadingSurfaceBounded()
    {
        var tokens = Read("src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml");
        var production = Read("src", "GameSaveCenter.Playnite", "Themes", "WpfUiProduction.xaml");

        foreach (var template in new[] { tokens, production })
        {
            Assert.Contains("Placement=\"Bottom\"", template);
            Assert.Contains("Focusable=\"False\"", template);
            Assert.Contains("StaysOpen=\"False\"", template);
            Assert.Contains("AllowsTransparency=\"{DynamicResource GscPopupAllowsTransparency}\"", template);
            Assert.Contains("PopupAnimation=\"{DynamicResource GscPopupAnimation}\"", template);
            Assert.Contains("Background=\"{DynamicResource GscPopupBrush}\"", template);
            Assert.Contains("Effect=\"{DynamicResource GscPopupEffect}\"", template);
        }

        Assert.Contains("MaxHeight=\"320\"", tokens);
        Assert.Contains("MaxHeight=\"{TemplateBinding MaxDropDownHeight}\"", production);
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", tokens);
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", production);
        Assert.Contains("KeyboardNavigation.DirectionalNavigation=\"Contained\"", tokens);
        Assert.Contains("KeyboardNavigation.DirectionalNavigation=\"Contained\"", production);
    }

    [Fact]
    public void CrossScreenSurfaceStaysHostOwnedAndPopupResourcesRemainReflowSafe()
    {
        var dashboard = Read("src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml");
        var dashboardCode = Read("src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs");

        Assert.Contains("x:Name=\"GameBrowserScrim\"", dashboard);
        Assert.Contains("x:Name=\"GameBrowserPanel\"", dashboard);
        Assert.DoesNotContain("<Popup", dashboard);
        Assert.Contains("Grid.SetRowSpan(GameBrowserPanel, 2)", dashboardCode);
        Assert.Contains("GameBrowserPanel.Visibility = gameBrowserVisibility", dashboardCode);
        Assert.Contains("GameBrowserScrim.Visibility = gameBrowserVisibility", dashboardCode);
        Assert.Contains("never a WPF Popup", dashboardCode);
        Assert.DoesNotContain("WindowStartupLocation", dashboardCode);
        Assert.DoesNotContain("new Window", dashboardCode);
    }

    [Fact]
    public void TransientSurfacesKeepThemeSensitiveResourcesDynamic()
    {
        var tokens = Read("src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml");
        var production = Read("src", "GameSaveCenter.Playnite", "Themes", "WpfUiProduction.xaml");
        var redesign = Read("src", "GameSaveCenter.Playnite", "Themes", "Redesign.xaml");
        var dashboard = Read("src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs");

        Assert.Contains("AllowsTransparency=\"{DynamicResource GscPopupAllowsTransparency}\"", production);
        Assert.Contains("PopupAnimation=\"{DynamicResource GscPopupAnimation}\"", production);
        Assert.Contains("Background=\"{DynamicResource GscPopupBrush}\"", production);
        Assert.Contains("Effect=\"{DynamicResource GscPopupEffect}\"", production);
        Assert.Contains("<Style TargetType=\"ToolTip\">", tokens);
        Assert.Contains("Background\" Value=\"{DynamicResource GscPopupBrush}\"", tokens);
        Assert.Contains("Effect\" Value=\"{DynamicResource GscDialogEffect}\"", redesign);
        Assert.Contains("Effect\" Value=\"{DynamicResource GscPopupEffect}\"", redesign);
        Assert.Contains("AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(ProductionShellView.Resources", dashboard);
        Assert.Contains("foreach (var workspaceView in ProductionShellView.WorkspaceViews)", dashboard);
    }

    [Fact]
    public void OverviewCloudCardKeepsQueueAndGuaranteeStatesDistinct()
    {
        var overview = Read("src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml");

        Assert.Contains("Snapshot.CloudTransfers.QueueControlDisplay", overview);
        Assert.Contains("Snapshot.CloudTransfers.GuaranteeDisplay", overview);
        Assert.Contains("<Run Text=\" · \"/>", overview);
        Assert.Contains("AutomationProperties.Name=\"打开云端队列\"", overview);
    }

    [Fact]
    public void OverviewEmptyActivityStateKeepsAReadableViewport()
    {
        var overview = Read("src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml");

        Assert.Contains("x:Name=\"OverviewActivityEmptyState\"", overview);
        Assert.Contains("MinHeight=\"120\"", overview);
        Assert.Contains("DataTrigger Binding=\"{Binding Activities.Count}\" Value=\"0\"", overview);
    }

    [Fact]
    public void LowCostProbeLocksReducedMaterialAndMotionCoverage()
    {
        var harness = Read("tests", "GameSaveCenter.RenderHarness", "Program.cs");

        Assert.Contains("args[0].Equals(\"lowcostprobe\"", harness);
        Assert.Contains("glassEnabled: false, motionEnabled: false", harness);
        Assert.Contains("GscPopupAllowsTransparency", harness);
        Assert.Contains("GscPopupAnimation", harness);
        Assert.Contains("visibleEffects", harness);
        Assert.Contains("unexpectedHorizontalOverflow", harness);
    }

    [Fact]
    public void SettingsThemeTransitionProbeCapturesOpenPopupAndTooltipAcrossModes()
    {
        var harness = Read("tests", "GameSaveCenter.RenderHarness", "Program.cs");

        Assert.Contains("RunSettingsThemeTransitionProbe(outputRoot, report)", harness);
        Assert.Contains("args[0].Equals(\"settingsthemeprobe\", StringComparison.OrdinalIgnoreCase)", harness);
        Assert.Contains("selector.SetCurrentValue(ComboBox.IsDropDownOpenProperty, true)", harness);
        Assert.Contains("Template?.FindName(\"PART_Popup\", selector)", harness);
        Assert.Contains("new ToolTip", harness);
        Assert.Contains("Settings-theme-switch-light-open-1040x700.png", harness);
        Assert.Contains("Settings-theme-switch-dark-open-1040x700.png", harness);
        Assert.Contains("GameSaveCenterThemeMode.Dark", harness);
    }

    [Fact]
    public void MotionProbeCapturesProductionTransitionAndUnloadCleanup()
    {
        var harness = Read("tests", "GameSaveCenter.RenderHarness", "Program.cs");

        Assert.Contains("args[0].Equals(\"motionprobe\", StringComparison.OrdinalIgnoreCase)", harness);
        Assert.Contains("RunMotionProbe(outputRoot, report)", harness);
        Assert.Contains("motion-{themeName}-collapsed-mid.png", harness);
        Assert.Contains("motion-{themeName}-reentry-end.png", harness);
        Assert.Contains("DependencyPropertyHelper.GetValueSource(layer, UIElement.OpacityProperty).IsAnimated", harness);
        Assert.Contains("window.Close();", harness);
        Assert.Contains("MotionProbeBoundary", harness);
    }

    [Fact]
    public void MotionHotChangeProbeNormalizesAnActiveTransitionAndKeepsDisabledReentryImmediate()
    {
        var harness = Read("tests", "GameSaveCenter.RenderHarness", "Program.cs");

        Assert.Contains("args[0].Equals(\"motionhotprobe\", StringComparison.OrdinalIgnoreCase)", harness);
        Assert.Contains("RunMotionHotChangeProbe(outputRoot, report)", harness);
        Assert.Contains("motionEnabled = false;", harness);
        Assert.Contains("shell.NormalizeMotionIfDisabled();", harness);
        Assert.Contains("motion-hot-{themeName}-disabled-final.png", harness);
        Assert.Contains("disabledReentryAnimated", harness);
        Assert.Contains("MotionHotChangeBoundary", harness);
    }

    [Fact]
    public void MotionCycleProbeCoversOneHundredLoadedAndUnloadedProductionShellCycles()
    {
        var harness = Read("tests", "GameSaveCenter.RenderHarness", "Program.cs");

        Assert.Contains("args[0].Equals(\"motioncycleprobe\", StringComparison.OrdinalIgnoreCase)", harness);
        Assert.Contains("RunMotionCycleProbe(outputRoot, report)", harness);
        Assert.Contains("for (var cycle = 0; cycle < 100; cycle++)", harness);
        Assert.Contains("host.Content = null;", harness);
        Assert.Contains("loadedCount != 101 || unloadedCount != 101", harness);
        Assert.Contains("MotionCycleBoundary", harness);
    }

    [Fact]
    public void MotionReentryProbeStartsTheLatestIntentFromTheRenderedWidth()
    {
        var harness = Read("tests", "GameSaveCenter.RenderHarness", "Program.cs");

        Assert.Contains("args[0].Equals(\"motionreentryprobe\", StringComparison.OrdinalIgnoreCase)", harness);
        Assert.Contains("RunMotionReentryProbe(outputRoot, report)", harness);
        Assert.Contains("var immediateWidth = shell.SidebarWidthForAudit;", harness);
        Assert.Contains("Math.Abs(immediateWidth - interruptedWidth) > 1.5", harness);
        Assert.Contains("motion-reentry-{themeName}-takeover.png", harness);
        Assert.Contains("MotionReentryBoundary", harness);
    }

    [Fact]
    public void EnduranceProbeUsesARealDispatcherWindowWithoutForcedGc()
    {
        var harness = Read("tests", "GameSaveCenter.RenderHarness", "Program.cs");
        var start = harness.IndexOf("private static int RunEnduranceProbe", StringComparison.Ordinal);
        var end = harness.IndexOf("private static void AppendEnduranceSummary", start, StringComparison.Ordinal);

        Assert.True(start >= 0);
        Assert.True(end > start);
        var probe = harness.Substring(start, end - start);
        Assert.Contains("args[0].Equals(\"enduranceprobe\"", harness);
        Assert.Contains("durationSeconds = 1800", harness);
        Assert.Contains("new Window", probe);
        Assert.Contains("new DispatcherTimer", probe);
        Assert.Contains("GC.GetTotalMemory(false)", probe);
        Assert.DoesNotContain("GC.Collect", probe);
        Assert.Contains("workspace navigation, Media preview segment", probe);
    }

    [Fact]
    public void RenderingProxyReportsPercentileAndSlowFrameRatioSeparately()
    {
        var harness = Read("tests", "GameSaveCenter.RenderHarness", "Program.cs");

        Assert.Contains("CompositionTarget.Rendering", harness);
        Assert.Contains("frameGapP95", harness);
        Assert.Contains("slowFrameRatio", harness);
        Assert.Contains("1000d / 60d", harness);
        Assert.Contains("CalculatePercentile", harness);
    }

    [Fact]
    public void EnduranceProbeRecordsUiActionHotspotBoundariesWithoutClaimingEtwStacks()
    {
        var harness = Read("tests", "GameSaveCenter.RenderHarness", "Program.cs");

        Assert.Contains("actionDurationsMs", harness);
        Assert.Contains("ui_action_p95_ms", harness);
        Assert.Contains("ui_action_slow_over_100ms", harness);
        Assert.Contains("slowActionStacks", harness);
        Assert.Contains("captured after action completion", harness);
        Assert.Contains("no reproducible >100ms action", harness);
    }

    private static string Read(params string[] parts)
        => File.ReadAllText(Path.Combine(new[] { TestRepositoryContext.Root }.Concat(parts).ToArray()));
}
