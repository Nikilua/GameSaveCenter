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
