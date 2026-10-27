using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class AccessibilitySourceTests
{
    [Fact]
    public void CtrlFFocusesWorkspaceSearchAndAutomationNamesArePresent()
    {
        var root = FindRepositoryRoot();
        var dashboardCode = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));
        var dashboardXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));
        var shellXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));
        var taskXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var mediaXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var trainerXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));

        Assert.Contains("SearchShortcutPolicy.ShouldFocusWorkspaceSearch", dashboardCode);
        Assert.Contains("ProductionShellView.IsGamePickerOpen", dashboardCode);
        Assert.Contains("FocusWorkspaceSearch", dashboardCode);
        Assert.Contains("TrainerSearchTextBox", dashboardCode);
        Assert.Contains("TaskSearchTextBox", dashboardCode);
        Assert.Contains("MediaSearchTextBox", dashboardCode);
        Assert.Contains("ProcessMappingExecutableTextBox", dashboardCode);
        Assert.Contains("AutomationProperties.Name=\"搜索游戏\"", dashboardXaml);
        Assert.Contains("x:Name=\"TaskSearchTextBox\"", taskXaml);
        Assert.Contains("AutomationProperties.Name=\"搜索任务\"", taskXaml);
        Assert.Contains("x:Name=\"MediaSearchTextBox\"", mediaXaml);
        Assert.Contains("AutomationProperties.Name=\"搜索当前游戏媒体\"", mediaXaml);
        Assert.Contains("AutomationProperties.Name=\"搜索 FLiNG 目录\"", trainerXaml);
        Assert.Contains("AutomationProperties.Name=\"清除游戏搜索\"", shellXaml);
        Assert.Contains("AutomationProperties.Name=\"清除媒体搜索\"", mediaXaml);
        Assert.Contains("AutomationProperties.Name=\"清除任务搜索\"", taskXaml);
        Assert.Contains("AutomationProperties.Name=\"清除 FLiNG 搜索\"", trainerXaml);
        Assert.Contains("Padding=\"12,3,38,3\"", shellXaml);
        Assert.Contains("Padding=\"12,3,38,3\"", mediaXaml);
        Assert.Contains("Padding=\"30,7,38,7\"", taskXaml);
        Assert.Contains("Padding=\"12,3,38,3\"", trainerXaml);
    }

    [Fact]
    public void ProductionIconOnlyActionsExposeAutomationNamesAndSharedKeyboardFocus()
    {
        var root = FindRepositoryRoot();
        var viewRoot = Path.Combine(root, "src", "GameSaveCenter.Playnite");
        var developmentViewRoot = Path.GetFullPath(Path.Combine(viewRoot, "Views", "Development"))
                                  + Path.DirectorySeparatorChar;
        var buttonTag = new Regex(
            @"<(?:(?:[A-Za-z_][\w.]*):)?Button\b(?<attributes>[^>]*GscIconOnly[^>]*)>",
            RegexOptions.CultureInvariant);
        var iconButtonCount = 0;

        foreach (var path in Directory.GetFiles(viewRoot, "*.xaml", SearchOption.AllDirectories))
        {
            if (Path.GetFullPath(path).StartsWith(developmentViewRoot, StringComparison.OrdinalIgnoreCase))
                continue;

            var markup = File.ReadAllText(path);
            foreach (Match match in buttonTag.Matches(markup))
            {
                var attributes = match.Groups["attributes"].Value;
                iconButtonCount++;
                Assert.Contains("AutomationProperties.Name=", attributes);
                Assert.True(
                    Regex.IsMatch(attributes, @"\b(?:Command|Click)=", RegexOptions.CultureInvariant),
                    $"Icon-only action has no command or click binding: {path}: {match.Value}");
            }
        }

        var production = File.ReadAllText(Path.Combine(viewRoot, "Themes", "WpfUiProduction.xaml"));
        Assert.True(iconButtonCount >= 10, $"Expected production icon actions, found {iconButtonCount}.");
        Assert.Contains(
            "<Style x:Key=\"GscIconOnlyButtonBase\" TargetType=\"{x:Type ui:Button}\" BasedOn=\"{StaticResource GscWpfUiButton}\">",
            production);
        Assert.Contains(
            "<Setter Property=\"FocusVisualStyle\" Value=\"{DynamicResource GscSharedFocusVisual}\"/>",
            production);
    }

    [Fact]
    public void TaskAndMediaSearchClearAndCountsStayOutsideEditableTextLayout()
    {
        var root = FindRepositoryRoot();
        var taskView = XDocument.Load(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var mediaView = XDocument.Load(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var search = taskView.Descendants().Single(element => element.Attribute(x + "Name")?.Value == "TaskSearchTextBox");
        var searchHost = Assert.IsType<XElement>(search.Parent);
        var clear = searchHost.Elements().Single(element => element.Name.LocalName == "Button");
        var summary = taskView.Descendants().Single(element => element.Attribute(x + "Name")?.Value == "TaskSummaryPanel");
        var filterBar = taskView.Descendants().Single(element => element.Attribute(x + "Name")?.Value == "TaskFilterBar");

        Assert.Equal("{Binding TaskSearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}", search.Attribute("Text")?.Value);
        Assert.Equal("30,7,38,7", search.Attribute("Padding")?.Value);
        Assert.Null(search.Attribute("PreviewKeyDown"));
        Assert.Equal("OnClearSearchTextBoxClick", clear.Attribute("Click")?.Value);
        Assert.Equal("清除任务搜索", clear.Attribute("AutomationProperties.Name")?.Value);
        Assert.Equal("Right", clear.Attribute("HorizontalAlignment")?.Value);
        Assert.DoesNotContain(searchHost.Elements(), element => element.Name.LocalName == "ColumnDefinition");
        Assert.Contains(summary.Descendants(), element => element.Attribute("Text")?.Value?.Contains("TaskTotalCount") == true);
        Assert.Equal("0", summary.Attribute("Grid.Row")?.Value);
        Assert.Equal("1", filterBar.Attribute("Grid.Row")?.Value);
        Assert.Contains(filterBar.Descendants(), element => element.Attribute(x + "Name")?.Value == "TaskSearchBoxHost");
        Assert.DoesNotContain(searchHost.Descendants(), element => element.Attribute("Text")?.Value?.Contains("TaskTotalCount") == true);

        var mediaSearch = mediaView.Descendants().Single(element => element.Attribute(x + "Name")?.Value == "MediaSearchTextBox");
        var mediaSearchHost = Assert.IsType<XElement>(mediaSearch.Parent);
        var mediaClear = mediaSearchHost.Elements().Single(element => element.Name.LocalName == "Button");
        var mediaSummary = mediaView.Descendants().Single(element => element.Attribute(x + "Name")?.Value == "MediaSummaryPanel");
        Assert.Equal("12,3,38,3", mediaSearch.Attribute("Padding")?.Value);
        Assert.Equal("Right", mediaClear.Attribute("HorizontalAlignment")?.Value);
        Assert.Equal("清除媒体搜索", mediaClear.Attribute("AutomationProperties.Name")?.Value);
        Assert.Contains(mediaSummary.Descendants(), element => element.Attribute("Text")?.Value?.Contains("MediaSummary.TotalCount") == true);
        Assert.DoesNotContain(mediaSearchHost.Descendants(), element => element.Attribute("Text")?.Value?.Contains("MediaSummary.TotalCount") == true);
    }

    [Fact]
    public void SharedTextBoxesUseLeadingCaretAlignment()
    {
        var root = FindRepositoryRoot();
        var production = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "WpfUiProduction.xaml"));
        var tokens = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml"));

        Assert.Contains("<Setter Property=\"HorizontalContentAlignment\" Value=\"Left\"/>", production);
        Assert.Contains("<Setter Property=\"TextAlignment\" Value=\"Left\"/>", production);
        Assert.Contains("<Setter Property=\"HorizontalContentAlignment\" Value=\"Left\"/>", tokens);
        Assert.Contains("<Setter Property=\"TextAlignment\" Value=\"Left\"/>", tokens);
        Assert.Contains("HorizontalContentAlignment=\"{TemplateBinding HorizontalContentAlignment}\"", production);
        Assert.Contains("HorizontalContentAlignment=\"{TemplateBinding HorizontalContentAlignment}\"", tokens);
        Assert.Contains("TextElement.Foreground=\"{TemplateBinding Foreground}\"", production);
        Assert.Contains("TextElement.Foreground=\"{TemplateBinding Foreground}\"", tokens);
        Assert.Contains("TextElement.FontFamily=\"{TemplateBinding FontFamily}\"", production);
        Assert.Contains("TextElement.FontFamily=\"{TemplateBinding FontFamily}\"", tokens);
        Assert.Contains("TextElement.FontSize=\"{TemplateBinding FontSize}\"", production);
        Assert.Contains("TextElement.FontSize=\"{TemplateBinding FontSize}\"", tokens);
        Assert.Contains("Padding=\"0\"", production);
        Assert.Contains("Padding=\"0\"", tokens);
        Assert.Contains("Margin=\"0\"", production);
        Assert.Contains("Margin=\"0\"", tokens);
        var productionTextBoxTemplateStart = production.IndexOf("x:Key=\"GscWpfUiTextBoxTemplate\"", StringComparison.Ordinal);
        var productionComboBoxTemplateStart = production.IndexOf("x:Key=\"GscWpfUiComboBoxTemplate\"", productionTextBoxTemplateStart, StringComparison.Ordinal);
        Assert.True(productionTextBoxTemplateStart >= 0 && productionComboBoxTemplateStart > productionTextBoxTemplateStart);
        Assert.DoesNotContain("Margin=\"{TemplateBinding Padding}\"", production.Substring(productionTextBoxTemplateStart, productionComboBoxTemplateStart - productionTextBoxTemplateStart));
        Assert.DoesNotContain("Margin=\"{TemplateBinding Padding}\"", tokens);
    }

    [Fact]
    public void FocusVisualsAndHighContrastFallbackAreShared()
    {
        var root = FindRepositoryRoot();
        var tokens = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml"));
        var dashboardCode = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));

        Assert.Contains("GscSharedFocusVisual", tokens);
        Assert.Contains("SystemParameters.HighContrast", dashboardCode);
    }

    private static string FindRepositoryRoot()
        => TestRepositoryContext.Root;
}
