using System;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R21SelectableTechnicalTextBehaviorTests
{
    [Fact]
    public void TechnicalTextStyleKeepsVersionReadableAndKeyboardSelectable()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? failure = null;
        string selected = string.Empty;
        string helpText = string.Empty;
        bool focusable = false;
        bool tabStop = false;

        var thread = new Thread(() =>
        {
            try
            {
                var resources = LoadProductionResources();
                var style = Assert.IsType<Style>(resources["GscWpfUiTechnicalTextBox"]);
                var box = new TextBox
                {
                    Style = style,
                    Text = "v0.6.73",
                    Width = 96
                };
                AutomationProperties.SetName(box, "当前插件版本");
                var host = new Window
                {
                    Width = 180,
                    Height = 80,
                    Content = box,
                    ShowInTaskbar = false,
                    WindowStyle = WindowStyle.None
                };

                try
                {
                    host.Show();
                    host.UpdateLayout();
                    Assert.True(box.IsReadOnly);
                    Assert.True(box.SelectionBrush != null);
                    focusable = box.Focusable;
                    tabStop = KeyboardNavigation.GetIsTabStop(box);
                    Assert.True(box.Focus());
                    box.SelectAll();
                    selected = box.SelectedText;
                    helpText = AutomationProperties.GetHelpText(box);
                    Assert.Equal("v0.6.73", box.Text);
                }
                finally
                {
                    host.Close();
                }
            }
            catch (Exception caught)
            {
                failure = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.True(focusable);
        Assert.True(tabStop);
        Assert.Equal("v0.6.73", selected);
        Assert.Equal("可选择文本；按 Ctrl+C 复制", helpText);
    }

    [Fact]
    public void ProductionShellVersionEntrancesUseSelectableTechnicalText()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var dashboard = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));
        var shell = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));

        Assert.Contains("x:Name=\"SidebarVersionText\"", dashboard, StringComparison.Ordinal);
        Assert.Contains("Style=\"{DynamicResource GscWpfUiTechnicalTextBox}\"", dashboard, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"当前插件版本\"", dashboard, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"SidebarProductionVersionText\"", shell, StringComparison.Ordinal);
        Assert.Contains("Style=\"{DynamicResource GscWpfUiTechnicalTextBox}\"", shell, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"当前插件版本\"", shell, StringComparison.Ordinal);
    }

    private static ResourceDictionary LoadProductionResources()
        => (ResourceDictionary)XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>");
}
