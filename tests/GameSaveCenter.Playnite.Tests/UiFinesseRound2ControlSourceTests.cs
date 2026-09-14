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
    public void MediaInboxKeepsAReadablePrimaryViewportBeforePageOverflow()
    {
        var view = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml");

        Assert.Contains("x:Name=\"MediaInboxPageScrollViewer\"", view);
        Assert.Contains("x:Name=\"MediaInboxLayout\" Grid.Row=\"1\" MinHeight=\"212\"", view);
        Assert.Contains("x:Name=\"MediaInboxTableFrame\"", view);
        Assert.Contains("Padding=\"14,12,14,12\" MinHeight=\"212\"", view);
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
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        if (directory == null)
            throw new InvalidOperationException("Repository root not found.");

        return File.ReadAllText(Path.Combine(new[] { directory.FullName }.Concat(parts).ToArray()));
    }
}
