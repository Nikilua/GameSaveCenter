using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using GameSaveCenter.Playnite.Converters;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R03LongPathTests
{
    [Fact]
    public void PreviewKeepsDriveAndFileNameWithoutChangingTheSource()
    {
        var source = LongPath();
        var converter = new PathDisplayConverter();
        var preview = Assert.IsType<string>(converter.Convert(source, typeof(string), null!, CultureInfo.InvariantCulture));

        Assert.True(source.Length > 260);
        Assert.StartsWith(@"D:\GameSaveCenter\", preview, StringComparison.Ordinal);
        Assert.EndsWith("final-save-slot-999.dat", preview, StringComparison.Ordinal);
        Assert.Contains("…", preview);
        Assert.DoesNotContain("\r", preview);
        Assert.DoesNotContain("\n", preview);
        Assert.Equal(LongPath(), source);
    }

    [Fact]
    public void DetailTextBoxStaysWithinFiniteViewportAndKeepsSelectableExactText()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? failure = null;
        var source = LongPath();
        double actualWidth = 0;
        string selectedText = string.Empty;

        var thread = new Thread(() =>
        {
            try
            {
                var resources = LoadProductionResources();
                var style = Assert.IsType<Style>(resources["GscWpfUiPathDetailTextBox"]);
                Assert.True(SetterValue(style, TextBox.IsReadOnlyProperty) is bool readOnly && readOnly);
                Assert.Equal(TextWrapping.NoWrap, SetterValue(style, TextBox.TextWrappingProperty));
                Assert.Equal(ScrollBarVisibility.Hidden, SetterValue(style, ScrollViewer.HorizontalScrollBarVisibilityProperty));
                Assert.Equal(ScrollBarVisibility.Disabled, SetterValue(style, ScrollViewer.VerticalScrollBarVisibilityProperty));

                var box = new TextBox { Style = style, Text = source };
                var host = new Border { Width = 340, Height = 34, Child = box };
                host.Measure(new Size(340, 34));
                host.Arrange(new Rect(0, 0, 340, 34));
                host.UpdateLayout();

                actualWidth = box.ActualWidth;
                box.SelectAll();
                selectedText = box.SelectedText;
                Assert.Equal(source, box.Text);
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
        Assert.InRange(actualWidth, 0, 340.5);
        Assert.Equal(source, selectedText);
    }

    [Fact]
    public void ProductionPathEntrancesKeepPreviewAndExactCopySeparate()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var tokens = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml"));
        var production = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "WpfUiProduction.xaml"));
        var dashboard = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var trainer = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));

        Assert.Contains("GscPathDisplayConverter", tokens);
        Assert.Contains("x:Key=\"GscWpfUiPathDetailTextBox\"", production);
        Assert.Contains("CopyPathCommand", dashboard);
        Assert.Contains("CopyTextWithRetryAsync(path", dashboard);
        Assert.Contains("Converter={StaticResource GscPathDisplayConverter}", save);
        Assert.Contains("Style=\"{StaticResource GscWpfUiPathDetailTextBox}\"", save);
        Assert.Contains("Command=\"{Binding CopyPathCommand}\"", save);
        Assert.Contains("Converter={StaticResource GscPathDisplayConverter}", media);
        Assert.Contains("Style=\"{StaticResource GscWpfUiPathDetailTextBox}\"", media);
        Assert.Contains("Command=\"{Binding CopyPathCommand}\"", media);
        Assert.Contains("Converter={StaticResource GscPathDisplayConverter}", trainer);
        Assert.Contains("Style=\"{StaticResource GscWpfUiPathDetailTextBox}\"", trainer);
        Assert.Contains("Command=\"{Binding CopyPathCommand}\"", trainer);
        Assert.DoesNotContain("Text=\"{Binding SelectedCandidate.Path}\"", save);
        Assert.DoesNotContain("Text=\"{Binding SelectedMedia.ArchivePath}\" Style=\"{StaticResource MediaPathText}\"", media);
    }

    private static string LongPath()
        => @"D:\GameSaveCenter\" + string.Concat(Enumerable.Repeat(@"profiles\slot-2026\", 22)) + "final-save-slot-999.dat";

    private static object SetterValue(Style style, DependencyProperty property)
        => style.Setters.OfType<Setter>().Single(setter => setter.Property == property).Value;

    private static ResourceDictionary LoadProductionResources()
        => (ResourceDictionary)XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>");
}
