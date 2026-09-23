using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using GameSaveCenter.Playnite.Views;
using PlayniteButton = GameSaveCenter.Playnite.Controls.Button;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R02ActionPriorityTests
{
    [Fact]
    public void PrimaryRegionsUseOnePrimaryRoleAndMutuallyExclusiveInboxModes()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var overview = LoadXaml(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml");
        var save = LoadXaml(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml");
        var media = LoadXaml(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml");

        Assert.Single(PrimaryButtons(FindNamed(overview, "OverviewHomeToolbarActions")));
        Assert.Single(PrimaryButtons(FindNamed(overview, "OverviewCurrentGameCard")));
        Assert.Single(PrimaryButtons(FindNamed(media, "MediaInboxBatchActionRow")));

        var restores = media.Descendants()
            .Where(element => element.Name.LocalName == "Button"
                              && (element.Attribute("Command")?.Value.IndexOf("RestoreIgnoredMediaBatchCommand", StringComparison.Ordinal) ?? -1) >= 0)
            .ToArray();
        var apply = media.Descendants()
            .Single(element => element.Name.LocalName == "Button"
                               && (element.Attribute("Command")?.Value.IndexOf("ApplyMediaClassificationCommand", StringComparison.Ordinal) ?? -1) >= 0);

        Assert.Equal(2, restores.Length);
        Assert.All(restores, restore =>
        {
            Assert.Contains("GscWpfUiPrimaryActionButton", restore.Descendants().Single(element => element.Name.LocalName == "Style").Attribute("BasedOn")?.Value);
            Assert.True(HasVisibilityTrigger(restore, "已忽略", "Visible"), "恢复到待归类必须只在已忽略模式占据主动作位置。");
            Assert.True(HasInitialVisibility(restore, "Collapsed"), "条件恢复按钮默认必须不可见，避免与应用建议同时出现。");
        });
        Assert.Contains("GscWpfUiPrimaryActionButton", apply.Descendants().Single(element => element.Name.LocalName == "Style").Attribute("BasedOn")?.Value);
        Assert.True(HasVisibilityTrigger(apply, "已忽略", "Collapsed"), "应用建议必须在已忽略模式退出主动作位置。");

        var restoreButton = save.Descendants()
            .Single(element => element.Name.LocalName == "Button"
                               && string.Equals(element.Attribute("Command")?.Value, "{Binding RestoreCommand}", StringComparison.Ordinal));
        var deleteSourceButton = media.Descendants()
            .Single(element => element.Name.LocalName == "Button"
                               && (element.Attribute("Command")?.Value.IndexOf("DeleteMediaSourceCommand", StringComparison.Ordinal) ?? -1) >= 0);
        var deleteTemplateButton = save.Descendants()
            .Single(element => element.Name.LocalName == "Button"
                               && (element.Attribute("Command")?.Value.IndexOf("DeletePolicyTemplateCommand", StringComparison.Ordinal) ?? -1) >= 0);

        Assert.Contains("GscWpfUiDangerActionButton", restoreButton.Attribute("Style")?.Value);
        Assert.Contains("GscWpfUiContextDangerButton", deleteSourceButton.Attribute("Style")?.Value);
        Assert.Contains("GscIconOnlyDangerButton", deleteTemplateButton.Attribute("Style")?.Value);
    }

    [Fact]
    public void SharedDangerVariantsResolveAppearanceWithoutChangingActionGeometry()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? exception = null;
        string? primaryAppearance = null;
        string? dangerAppearance = null;
        double primaryMinHeight = 0;
        double dangerMinHeight = 0;
        double contextDangerMinHeight = 0;
        Brush? primaryChromeBrush = null;
        Brush? dangerChromeBrush = null;

        var thread = new Thread(() =>
        {
            try
            {
                var resources = (ResourceDictionary)XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>");
                var primary = new GameSaveCenter.Playnite.Controls.Button
                {
                    Style = Assert.IsType<Style>(resources["GscWpfUiPrimaryActionButton"]),
                    Content = "正常主动作"
                };
                var danger = new GameSaveCenter.Playnite.Controls.Button
                {
                    Style = Assert.IsType<Style>(resources["GscWpfUiDangerActionButton"]),
                    Content = "危险动作"
                };
                var contextDanger = new GameSaveCenter.Playnite.Controls.Button
                {
                    Style = Assert.IsType<Style>(resources["GscWpfUiContextDangerButton"]),
                    Content = "移除"
                };
                var panel = new StackPanel();
                panel.Children.Add(primary);
                panel.Children.Add(danger);
                panel.Children.Add(contextDanger);
                var host = new Border { Resources = resources, Child = panel };
                host.Measure(new Size(480, 180));
                host.Arrange(new Rect(0, 0, 480, 180));
                host.UpdateLayout();
                primary.ApplyTemplate();
                danger.ApplyTemplate();
                contextDanger.ApplyTemplate();

                primaryAppearance = primary.Appearance;
                dangerAppearance = danger.Appearance;
                primaryMinHeight = primary.MinHeight;
                dangerMinHeight = danger.MinHeight;
                contextDangerMinHeight = contextDanger.MinHeight;
                primaryChromeBrush = (primary.Template.FindName("ButtonChrome", primary) as Border)?.Background;
                dangerChromeBrush = (danger.Template.FindName("ButtonChrome", danger) as Border)?.Background;
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Equal("Primary", primaryAppearance);
        Assert.Equal("Danger", dangerAppearance);
        Assert.Equal(primaryMinHeight, dangerMinHeight);
        Assert.Equal(36, primaryMinHeight);
        Assert.Equal(30, contextDangerMinHeight);
        Assert.IsType<LinearGradientBrush>(primaryChromeBrush);
        Assert.IsType<SolidColorBrush>(dangerChromeBrush);
        Assert.NotEqual(((SolidColorBrush)dangerChromeBrush!).Color, ((LinearGradientBrush)primaryChromeBrush!).GradientStops[0].Color);
    }

    [Fact]
    public void InboxModeSwitchKeepsExactlyOneModeSpecificPrimaryActionVisibleInProductionView()
    {
        Exception? exception = null;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var context = new InboxModeContext("待归类");
                var view = new MediaCenterView
                {
                    DataContext = context,
                    Width = 1280,
                    Height = 820
                };
                window = new Window
                {
                    Content = view,
                    Width = 1280,
                    Height = 820,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };

                window.Show();
                FlushLayout(window);

                var buttons = FindVisualDescendants<PlayniteButton>(view).ToArray();
                var apply = Assert.Single(buttons, button =>
                    string.Equals(button.Content as string, "应用所选高置信建议", StringComparison.Ordinal));
                var restore = Assert.Single(buttons, button =>
                    AutomationProperties.GetName(button) == "恢复所选已忽略媒体到待归类");
                var modeSelector = Assert.Single(FindVisualDescendants<ComboBox>(view), combo =>
                    AutomationProperties.GetName(combo) == "媒体收件箱视图");

                AssertInboxModeActionPair(apply, restore, applyVisible: true);

                modeSelector.SelectedItem = "已忽略";
                FlushLayout(window);
                Assert.Equal("已忽略", context.MediaInboxMode);
                AssertInboxModeActionPair(apply, restore, applyVisible: false);

                modeSelector.SelectedItem = "待归类";
                FlushLayout(window);
                Assert.Equal("待归类", context.MediaInboxMode);
                AssertInboxModeActionPair(apply, restore, applyVisible: true);
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

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
    }

    private static void AssertInboxModeActionPair(PlayniteButton apply, PlayniteButton restore, bool applyVisible)
    {
        var expectedVisible = applyVisible ? apply : restore;
        var expectedHidden = applyVisible ? restore : apply;

        Assert.Equal(Visibility.Visible, expectedVisible.Visibility);
        Assert.True(expectedVisible.IsVisible, "the active mode action must be visible in the hosted production view");
        Assert.Equal(Visibility.Collapsed, expectedHidden.Visibility);
        Assert.False(expectedHidden.IsVisible, "the inactive mode action must not remain in the visible action row");
        Assert.Equal("Primary", expectedVisible.Appearance);
        Assert.Equal("Primary", expectedHidden.Appearance);
        Assert.Equal(1, new[] { apply, restore }.Count(button =>
            button.Visibility == Visibility.Visible
            && string.Equals(button.Appearance, "Primary", StringComparison.Ordinal)));
    }

    private static void FlushLayout(Window window)
        => window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(window.UpdateLayout));

    private static System.Collections.Generic.IEnumerable<T> FindVisualDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        if (root is T match)
            yield return match;

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            foreach (var child in FindVisualDescendants<T>(VisualTreeHelper.GetChild(root, index)))
                yield return child;
        }
    }

    private sealed class InboxModeContext : INotifyPropertyChanged
    {
        private string mediaInboxMode;

        public InboxModeContext(string mediaInboxMode) => this.mediaInboxMode = mediaInboxMode;

        public int MediaTabIndex { get; set; }

        public string[] MediaInboxModeOptions { get; } = new[] { "待归类", "已忽略" };

        public string MediaInboxMode
        {
            get => mediaInboxMode;
            set
            {
                if (string.Equals(mediaInboxMode, value, StringComparison.Ordinal))
                    return;

                mediaInboxMode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MediaInboxMode)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private static XElement LoadXaml(string root, params string[] parts)
        => XDocument.Parse(File.ReadAllText(Path.Combine(new[] { root }.Concat(parts).ToArray()))).Root!;

    private static XElement FindNamed(XElement root, string name)
        => root.Descendants().Single(element => element.Attribute(XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml"))?.Value == name);

    private static XElement[] PrimaryButtons(XElement root)
        => root.Descendants()
            .Where(element => element.Name.LocalName == "Button")
            .Where(IsPrimary)
            .ToArray();

    private static bool IsPrimary(XElement button)
    {
        var style = button.Attribute("Style")?.Value ?? string.Empty;
        return style.IndexOf("Primary", StringComparison.Ordinal) >= 0
               || string.Equals(button.Attribute("Appearance")?.Value, "Primary", StringComparison.Ordinal);
    }

    private static bool HasVisibilityTrigger(XElement button, string mode, string visibility)
        => button.Descendants().Any(element => element.Name.LocalName == "DataTrigger"
                                               && element.Attribute("Value")?.Value == mode
                                               && element.Descendants().Any(setter => setter.Name.LocalName == "Setter"
                                                   && setter.Attribute("Property")?.Value == "Visibility"
                                                   && setter.Attribute("Value")?.Value == visibility));

    private static bool HasInitialVisibility(XElement button, string visibility)
        => button.Descendants().Any(element => element.Name.LocalName == "Setter"
                                               && element.Parent?.Name.LocalName == "Style"
                                               && element.Attribute("Property")?.Value == "Visibility"
                                               && element.Attribute("Value")?.Value == visibility);
}
