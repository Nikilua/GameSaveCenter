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
