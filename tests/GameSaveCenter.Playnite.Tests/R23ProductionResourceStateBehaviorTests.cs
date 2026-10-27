using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R23ProductionResourcesWpf")]
public sealed class R23ProductionResourceStateBehaviorTests
{
    [Fact]
    public void AcrylicNavigationExposesFocusAndDisabledStatesAfterSelectionClearsInBothThemes()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var root = new Grid { Resources = resources };
            var navigation = new RadioButton
            {
                Style = Assert.IsType<Style>(resources["AcrylicNavItem"]),
                Content = "存档中心",
                Width = 180,
                Height = 48
            };
            var external = new TextBox { Width = 120, Height = 32, Margin = new Thickness(0, 60, 0, 0) };
            root.Children.Add(navigation);
            root.Children.Add(external);
            var window = CreateWindow(root, 320, 160);

            try
            {
                window.Show();
                FlushLayout(window);
                navigation.ApplyTemplate();
                var chrome = Assert.IsType<Border>(navigation.Template.FindName("NavChrome", navigation));
                Color? lightPrimaryText = null;

                foreach (var mode in Themes)
                {
                    ApplyTheme(root, mode);
                    var primaryText = BrushColor(resources["GscPrimaryTextBrush"]);
                    if (mode == GameSaveCenterThemeMode.Light)
                        lightPrimaryText = primaryText;
                    else
                        Assert.NotEqual(lightPrimaryText, primaryText);
                    navigation.IsEnabled = true;
                    navigation.IsChecked = false;
                    FlushLayout(window);

                    Assert.Equal(Colors.Transparent, BrushColor(chrome.Background));
                    Assert.Equal(1d, chrome.Opacity);
                    Assert.Equal(new Thickness(1), chrome.BorderThickness);

                    navigation.IsChecked = true;
                    FlushLayout(window);
                    Assert.Equal(BrushColor(resources["GscAccentTintStrongBrush"]), BrushColor(chrome.Background));
                    Assert.Equal(BrushColor(resources["GscAccentTintStrongBrush"]), BrushColor(chrome.BorderBrush));

                    navigation.IsChecked = false;
                    Assert.Same(navigation, Keyboard.Focus(navigation));
                    FlushLayout(window);
                    Assert.True(navigation.IsKeyboardFocusWithin);
                    Assert.Same(resources["GscSharedFocusVisual"], navigation.FocusVisualStyle);
                    Assert.Equal(BrushColor(resources["GscAccentBrush"]), BrushColor(chrome.BorderBrush));
                    Assert.Equal(new Thickness(2), chrome.BorderThickness);

                    Assert.Same(external, Keyboard.Focus(external));
                    FlushLayout(window);
                    Assert.False(navigation.IsKeyboardFocusWithin);
                    Assert.Equal(Colors.Transparent, BrushColor(chrome.BorderBrush));
                    Assert.Equal(new Thickness(1), chrome.BorderThickness);

                    navigation.IsEnabled = false;
                    FlushLayout(window);
                    Assert.False(navigation.IsEnabled);
                    Assert.Equal(0.46d, chrome.Opacity);
                    Assert.NotSame(navigation, Keyboard.Focus(navigation));
                    Assert.Equal(0.46d, chrome.Opacity);
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void AcrylicNavigationKeepsKeyboardFocusOutlineVisibleWhileSelectedInBothThemes()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var root = new Grid { Resources = resources };
            var navigation = new RadioButton
            {
                Style = Assert.IsType<Style>(resources["AcrylicNavItem"]),
                Content = "媒体中心",
                Width = 180,
                Height = 48
            };
            var external = new TextBox { Width = 120, Height = 32, Margin = new Thickness(0, 60, 0, 0) };
            root.Children.Add(navigation);
            root.Children.Add(external);
            var window = CreateWindow(root, 320, 160);

            try
            {
                window.Show();
                FlushLayout(window);
                navigation.ApplyTemplate();
                var chrome = Assert.IsType<Border>(navigation.Template.FindName("NavChrome", navigation));

                foreach (var mode in Themes)
                {
                    ApplyTheme(root, mode);
                    navigation.IsEnabled = true;
                    navigation.IsChecked = true;
                    Assert.Same(navigation, Keyboard.Focus(navigation));
                    FlushLayout(window);

                    Assert.True(navigation.IsChecked, $"{mode}: current page selection was lost");
                    Assert.True(navigation.IsKeyboardFocusWithin, $"{mode}: keyboard focus was not retained");
                    Assert.Equal(BrushColor(resources["GscAccentTintStrongBrush"]), BrushColor(chrome.Background));
                    Assert.Equal(BrushColor(resources["GscAccentBrush"]), BrushColor(chrome.BorderBrush));
                    Assert.Equal(new Thickness(2), chrome.BorderThickness);

                    Assert.Same(external, Keyboard.Focus(external));
                    FlushLayout(window);
                    Assert.True(navigation.IsChecked, $"{mode}: focus movement changed the current page");
                    Assert.False(navigation.IsKeyboardFocusWithin);
                    Assert.Equal(BrushColor(resources["GscAccentTintStrongBrush"]), BrushColor(chrome.Background));
                    Assert.Equal(BrushColor(resources["GscAccentTintStrongBrush"]), BrushColor(chrome.BorderBrush));
                    Assert.Equal(new Thickness(1), chrome.BorderThickness);

                    navigation.IsChecked = false;
                    Assert.Same(navigation, Keyboard.Focus(navigation));
                    FlushLayout(window);
                    Assert.False(navigation.IsChecked);
                    Assert.True(navigation.IsKeyboardFocusWithin);
                    Assert.Equal(Colors.Transparent, BrushColor(chrome.Background));
                    Assert.Equal(BrushColor(resources["GscAccentBrush"]), BrushColor(chrome.BorderBrush));
                    Assert.Equal(new Thickness(2), chrome.BorderThickness);

                    Assert.Same(external, Keyboard.Focus(external));
                    navigation.IsEnabled = false;
                    FlushLayout(window);
                    Assert.False(navigation.IsKeyboardFocusWithin);
                    Assert.Equal(0.46d, chrome.Opacity);
                    Assert.NotSame(navigation, Keyboard.Focus(navigation));
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void SettingsSectionTabsUseTheirGeneratedStyleForSelectionFocusAndDisabledNegativeInBothThemes()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var root = new Grid { Resources = resources };
            var tabs = new ListBox
            {
                Style = Assert.IsType<Style>(resources["GscSettingsSectionTabs"]),
                ItemsSource = new[] { "常规", "存档" },
                Width = 240,
                Height = 150,
                SelectedIndex = 0
            };
            var external = new TextBox { Width = 120, Height = 32, Margin = new Thickness(0, 170, 0, 0) };
            root.Children.Add(tabs);
            root.Children.Add(external);
            var window = CreateWindow(root, 320, 250);

            try
            {
                window.Show();
                FlushLayout(window);
                var first = Assert.IsType<ListBoxItem>(tabs.ItemContainerGenerator.ContainerFromIndex(0));
                var second = Assert.IsType<ListBoxItem>(tabs.ItemContainerGenerator.ContainerFromIndex(1));
                first.ApplyTemplate();
                second.ApplyTemplate();
                var firstChrome = Assert.IsType<Border>(first.Template.FindName("TabChrome", first));
                var secondChrome = Assert.IsType<Border>(second.Template.FindName("TabChrome", second));
                Color? lightPrimaryText = null;

                foreach (var mode in Themes)
                {
                    ApplyTheme(root, mode);
                    var primaryText = BrushColor(resources["GscPrimaryTextBrush"]);
                    if (mode == GameSaveCenterThemeMode.Light)
                        lightPrimaryText = primaryText;
                    else
                        Assert.NotEqual(lightPrimaryText, primaryText);
                    tabs.SelectedIndex = 0;
                    first.IsEnabled = true;
                    second.IsEnabled = true;
                    FlushLayout(window);

                    Assert.Equal(BrushColor(resources["GscAccentTintBrush"]), BrushColor(firstChrome.Background));
                    Assert.NotEqual(BrushColor(resources["GscAccentTintBrush"]), BrushColor(secondChrome.Background));
                    Assert.Same(resources["GscSettingsSectionTabItem"], first.Style);
                    Assert.Same(resources["GscSharedFocusVisual"], first.FocusVisualStyle);

                    Assert.Same(first, Keyboard.Focus(first));
                    FlushLayout(window);
                    Assert.True(first.IsKeyboardFocusWithin);
                    Assert.Same(external, Keyboard.Focus(external));
                    FlushLayout(window);
                    Assert.False(first.IsKeyboardFocusWithin);

                    tabs.SelectedIndex = 0;
                    second.IsEnabled = false;
                    FlushLayout(window);
                    Assert.Equal(0, tabs.SelectedIndex);
                    Assert.Equal(0.45d, secondChrome.Opacity);
                    Assert.Equal(BrushColor(resources["GscDisabledTextBrush"]), BrushColor(second.Foreground));
                    Assert.NotSame(second, Keyboard.Focus(second));
                    Assert.Equal(0.45d, secondChrome.Opacity);
                    Assert.Equal(BrushColor(resources["GscAccentTintBrush"]), BrushColor(firstChrome.Background));
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void WorkspaceTabHeadersKeepPaddingAndRoundedEdgesWithChineseEnglishAndCountContentInBothThemes()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var root = new Grid { Resources = resources };
            var style = Assert.IsType<Style>(resources["GscRedesignWorkspaceTabItem"]);
            var shortHeader = new TextBlock
            {
                Text = "历史版本",
                TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.None,
                VerticalAlignment = VerticalAlignment.Center
            };
            var longHeader = new TextBlock
            {
                Text = "Validation and restore history",
                TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.None,
                VerticalAlignment = VerticalAlignment.Center
            };
            var countHeader = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var countLabel = new TextBlock { Text = "待归类", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) };
            var countBadgeText = new TextBlock { Text = "200", FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };
            var countBadge = new Border
            {
                Background = Assert.IsAssignableFrom<Brush>(resources["GscAccentTintStrongBrush"]),
                BorderBrush = Assert.IsAssignableFrom<Brush>(resources["GscAccentBrush"]),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(5, 1, 5, 1),
                Child = countBadgeText
            };
            countHeader.Children.Add(countLabel);
            countHeader.Children.Add(countBadge);

            var items = new[]
            {
                new TabItem { Style = style, Header = shortHeader, Content = new Border { Height = 24 }, IsSelected = true },
                new TabItem { Style = style, Header = longHeader, Content = new Border { Height = 24 } },
                new TabItem { Style = style, Header = countHeader, Content = new Border { Height = 24 } }
            };
            var tabs = new TabControl
            {
                Style = Assert.IsType<Style>(resources["GscRedesignWorkspaceTabControl"]),
                Width = 700,
                Height = 140,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            foreach (var item in items)
                tabs.Items.Add(item);
            root.Children.Add(tabs);
            var window = CreateWindow(root, 820, 220);

            try
            {
                window.Show();
                FlushLayout(window);
                var measuredWidthsByTheme = new Dictionary<GameSaveCenterThemeMode, double[]>();

                foreach (var mode in Themes)
                {
                    ApplyTheme(root, mode);
                    FlushLayout(window);
                    var widths = new double[items.Length];
                    tabs.ApplyTemplate();
                    var contentHost = Assert.IsType<ContentPresenter>(tabs.Template.FindName("PART_SelectedContentHost", tabs));
                    double? baselineContentWidth = null;

                    foreach (var item in items)
                    {
                        tabs.SelectedItem = item;
                        FlushLayout(window);
                        Assert.True(contentHost.ActualWidth > 0, $"{mode}/{item.Header}: selected content viewport was unavailable.");
                        if (baselineContentWidth.HasValue)
                        {
                            Assert.True(Math.Abs(contentHost.ActualWidth - baselineContentWidth.Value) <= 0.25,
                                $"{mode}/{item.Header}: changing tab header width changed the content viewport from {baselineContentWidth.Value} to {contentHost.ActualWidth}.");
                        }
                        else
                        {
                            baselineContentWidth = contentHost.ActualWidth;
                        }

                        item.ApplyTemplate();
                        FlushLayout(window);
                        var chrome = Assert.IsType<Border>(item.Template.FindName("Chrome", item));
                        var header = Assert.IsAssignableFrom<FrameworkElement>(item.Header);
                        var headerBounds = BoundsRelativeTo(header, chrome);
                        var safeLeft = chrome.BorderThickness.Left + chrome.Padding.Left;
                        var safeRight = chrome.ActualWidth - chrome.BorderThickness.Right - chrome.Padding.Right;

                        Assert.True(item.IsVisible && item.ActualWidth > 0 && item.ActualHeight >= 36);
                        Assert.Equal(new CornerRadius(11), chrome.CornerRadius);
                        Assert.False(chrome.ClipToBounds);
                        Assert.True(headerBounds.Left >= safeLeft - 0.25,
                            $"{mode}/{item.Header}: header clipped the leading rounded padding; bounds={headerBounds}, safeLeft={safeLeft}.");
                        Assert.True(headerBounds.Right <= safeRight + 0.25,
                            $"{mode}/{item.Header}: header clipped the trailing rounded padding; bounds={headerBounds}, safeRight={safeRight}.");
                        widths[Array.IndexOf(items, item)] = item.ActualWidth;

                        if (header is TextBlock textHeader)
                        {
                            Assert.Equal(TextWrapping.NoWrap, textHeader.TextWrapping);
                            Assert.Equal(TextTrimming.None, textHeader.TextTrimming);
                            Assert.True(textHeader.ActualWidth + 0.25 >= textHeader.DesiredSize.Width,
                                $"{mode}/{textHeader.Text}: natural text width was clipped.");
                        }
                    }

                    var countLabelBounds = BoundsRelativeTo(countLabel, Assert.IsType<Border>(items[2].Template.FindName("Chrome", items[2])));
                    var countBadgeBounds = BoundsRelativeTo(countBadge, Assert.IsType<Border>(items[2].Template.FindName("Chrome", items[2])));
                    var countTextBounds = BoundsRelativeTo(countBadgeText, Assert.IsType<Border>(items[2].Template.FindName("Chrome", items[2])));
                    Assert.False(countLabelBounds.IntersectsWith(countBadgeBounds), $"{mode}: count badge overlapped its label.");
                    Assert.True(countBadgeBounds.Contains(countTextBounds), $"{mode}: count text escaped its badge.");
                    measuredWidthsByTheme[mode] = widths;
                }

                var lightWidths = measuredWidthsByTheme[GameSaveCenterThemeMode.Light];
                var darkWidths = measuredWidthsByTheme[GameSaveCenterThemeMode.Dark];
                for (var index = 0; index < items.Length; index++)
                {
                    Assert.True(Math.Abs(lightWidths[index] - darkWidths[index]) <= 0.25,
                        $"Tab {index} width changed across themes: light={lightWidths[index]}, dark={darkWidths[index]}.");
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void WorkspaceTabContentsStretchAcrossNarrowViewportRegardlessOfHeaderWidthInBothThemes()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var root = new Grid { Resources = resources };
            var style = Assert.IsType<Style>(resources["GscRedesignWorkspaceTabItem"]);
            var items = new[]
            {
                new TabItem
                {
                    Style = style,
                    Header = "常规",
                    Content = new Border { Child = new TextBlock { Text = "short page" } },
                    IsSelected = true
                },
                new TabItem
                {
                    Style = style,
                    Header = "Validation and restore history",
                    Content = new Border { Child = new TextBlock { Text = "long header page" } }
                },
                new TabItem
                {
                    Style = style,
                    Header = "待归类 + 200",
                    Content = new Border { Child = new TextBlock { Text = "count page" } }
                }
            };
            var tabs = new TabControl
            {
                Style = Assert.IsType<Style>(resources["GscRedesignWorkspaceTabControl"]),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            foreach (var item in items)
                tabs.Items.Add(item);
            root.Children.Add(tabs);
            var window = CreateWindow(root, 380, 240);

            try
            {
                window.Show();
                FlushLayout(window);
                tabs.ApplyTemplate();
                var contentHost = Assert.IsType<ContentPresenter>(tabs.Template.FindName("PART_SelectedContentHost", tabs));

                foreach (var mode in Themes)
                {
                    ApplyTheme(root, mode);
                    foreach (var item in items)
                    {
                        tabs.SelectedItem = item;
                        FlushLayout(window);

                        var content = Assert.IsType<Border>(item.Content);
                        var hostBounds = BoundsRelativeTo(contentHost, tabs);
                        var contentBounds = BoundsRelativeTo(content, contentHost);
                        Assert.True(tabs.ActualWidth > 0 && tabs.ActualHeight > 0,
                            $"{mode}/{item.Header}: narrow TabControl was not laid out.");
                        Assert.True(contentHost.ActualWidth > 0 && contentHost.ActualHeight > 0,
                            $"{mode}/{item.Header}: selected content viewport was empty.");
                        Assert.True(Math.Abs(hostBounds.Left) <= 0.25,
                            $"{mode}/{item.Header}: selected content viewport was horizontally offset: {hostBounds}.");
                        Assert.True(Math.Abs(contentBounds.Left) <= 0.25 && Math.Abs(contentBounds.Top) <= 0.25,
                            $"{mode}/{item.Header}: selected page content was offset inside its viewport: {contentBounds}.");
                        Assert.True(Math.Abs(content.ActualWidth - contentHost.ActualWidth) <= 0.25,
                            $"{mode}/{item.Header}: selected page did not stretch to the narrow viewport; content={content.ActualWidth}, viewport={contentHost.ActualWidth}.");
                        Assert.True(Math.Abs(content.ActualHeight - contentHost.ActualHeight) <= 0.25,
                            $"{mode}/{item.Header}: selected page did not stretch vertically; content={content.ActualHeight}, viewport={contentHost.ActualHeight}.");
                    }
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void WorkspaceTabOverflowKeepsProductionHeadersMouseAndKeyboardReachableInBothThemes()
    {
        RunSta(() =>
        {
            var visualCaptureRoot = Environment.GetEnvironmentVariable("GSC_Q11_06_VISUAL_OUTPUT");
            if (!string.IsNullOrWhiteSpace(visualCaptureRoot))
            {
                Directory.CreateDirectory(visualCaptureRoot);
                File.WriteAllText(
                    Path.Combine(visualCaptureRoot, "q11-06-tab-overflow-captures.txt"),
                    "RenderTargetBitmap of the production shared TabControl template at 96 DPI; not a screen capture or physical-DPI measurement."
                    + Environment.NewLine);
            }

            var pages = new[]
            {
                (FileName: "SaveCenterView.xaml", ExpectedCount: 4),
                (FileName: "MediaCenterView.xaml", ExpectedCount: 4),
                (FileName: "MaintenanceView.xaml", ExpectedCount: 6),
                (FileName: "TrainerCenterView.xaml", ExpectedCount: 4)
            };
            var productionHeaders = pages.Select(page =>
                (page.FileName, Headers: ReadTopLevelWorkspaceTabHeaders(page.FileName))).ToArray();
            foreach (var page in productionHeaders)
                Assert.Equal(pages.Single(expected => expected.FileName == page.FileName).ExpectedCount, page.Headers.Length);

            var resources = LoadProductionResources();
            var tabControlStyle = Assert.IsType<Style>(resources["GscRedesignWorkspaceTabControl"]);
            var tabItemStyle = Assert.IsType<Style>(resources["GscRedesignWorkspaceTabItem"]);

            foreach (var page in productionHeaders)
            {
                foreach (var theme in Themes)
                {
                    var root = new Grid { Resources = resources };
                    root.SetResourceReference(Panel.BackgroundProperty, "GscGlassFillBrush");
                    root.SetResourceReference(TextElement.ForegroundProperty, "GscPrimaryTextBrush");
                    root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    var beforeTabs = new Button { Content = "前一控件", Height = 30, Width = 110 };
                    Grid.SetRow(beforeTabs, 0);
                    root.Children.Add(beforeTabs);

                    var tabs = new TabControl
                    {
                        Style = tabControlStyle,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Stretch
                    };
                    foreach (var header in page.Headers)
                    {
                        tabs.Items.Add(new TabItem
                        {
                            Style = tabItemStyle,
                            Header = header,
                            Content = new Border { Child = new TextBlock { Text = page.FileName + ": " + header } }
                        });
                    }
                    Grid.SetRow(tabs, 1);
                    root.Children.Add(tabs);
                    var window = CreateWindow(root, 320, 240);

                    try
                    {
                        window.Show();
                        tabs.ApplyTemplate();
                        FlushLayout(window);
                        var headerViewer = Assert.IsType<ScrollViewer>(tabs.Template.FindName("HeaderScrollViewer", tabs));
                        var contentHost = Assert.IsType<ContentPresenter>(tabs.Template.FindName("PART_SelectedContentHost", tabs));
                        var horizontalBar = Assert.Single(FindVisualChildren<ScrollBar>(headerViewer),
                            scrollBar => scrollBar.Orientation == Orientation.Horizontal);

                        ApplyTheme(root, theme);
                        tabs.SelectedIndex = 0;
                        headerViewer.ScrollToHorizontalOffset(0);
                        FlushLayout(window);
                        CaptureQ11TabVisualIfRequested(
                            visualCaptureRoot, root, page.FileName, theme, "first", tabs, headerViewer, horizontalBar);

                        Assert.Equal(page.Headers.Length, tabs.Items.Count);
                        Assert.Equal(ScrollBarVisibility.Auto, headerViewer.HorizontalScrollBarVisibility);
                        Assert.True(headerViewer.ViewportWidth > 0, $"{page.FileName}/{theme}: header viewport has no width.");
                        Assert.True(headerViewer.ScrollableWidth > 24,
                            $"{page.FileName}/{theme}: current production headers do not exercise the narrow overflow path; " +
                            $"count={page.Headers.Length}, viewport={headerViewer.ViewportWidth:0.##}, extent={headerViewer.ExtentWidth:0.##}.");
                        Assert.True(horizontalBar.IsVisible && horizontalBar.ActualHeight >= 10,
                            $"{page.FileName}/{theme}: the horizontal mouse scroll channel is not visible.");

                        var baselineContentWidth = contentHost.ActualWidth;
                        var firstTab = Assert.IsType<TabItem>(tabs.Items[0]);
                        Assert.Same(beforeTabs, Keyboard.Focus(beforeTabs));
                        Assert.True(beforeTabs.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)),
                            $"{page.FileName}/{theme}: Tab could not enter the workspace headers.");
                        var focusedTab = Assert.IsType<TabItem>(Keyboard.FocusedElement is DependencyObject focused
                            ? FindVisualAncestor<TabItem>(focused)
                            : null);
                        Assert.Same(firstTab, focusedTab);

                        for (var index = 1; index < tabs.Items.Count; index++)
                        {
                            var source = Keyboard.FocusedElement as FrameworkElement ?? (FrameworkElement)tabs.Items[index - 1];
                            RaiseWorkspaceTabKeyDown(source, window, Key.Right);
                            FlushLayout(window);
                            var selected = Assert.IsType<TabItem>(tabs.Items[index]);
                            Assert.Equal(index, tabs.SelectedIndex);
                            Assert.True(selected.IsKeyboardFocusWithin,
                                $"{page.FileName}/{theme}: Right did not move keyboard focus to '{page.Headers[index]}'.");
                            Assert.True(IsFullyInsideViewport(selected, headerViewer),
                                $"{page.FileName}/{theme}: keyboard-selected '{page.Headers[index]}' was not scrolled into the header viewport.");
                            Assert.True(Math.Abs(contentHost.ActualWidth - baselineContentWidth) <= 0.25,
                                $"{page.FileName}/{theme}: overflowing headers changed the selected content width.");
                        }

                        for (var index = tabs.Items.Count - 2; index >= 0; index--)
                        {
                            var source = Keyboard.FocusedElement as FrameworkElement ?? (FrameworkElement)tabs.Items[index + 1];
                            RaiseWorkspaceTabKeyDown(source, window, Key.Left);
                            FlushLayout(window);
                            var selected = Assert.IsType<TabItem>(tabs.Items[index]);
                            Assert.Equal(index, tabs.SelectedIndex);
                            Assert.True(selected.IsKeyboardFocusWithin,
                                $"{page.FileName}/{theme}: Left did not return keyboard focus to '{page.Headers[index]}'.");
                            Assert.True(IsFullyInsideViewport(selected, headerViewer),
                                $"{page.FileName}/{theme}: keyboard-selected '{page.Headers[index]}' was not scrolled back into view.");
                        }

                        Assert.InRange(headerViewer.HorizontalOffset, 0, 0.5);
                        tabs.SelectedIndex = 0;
                        headerViewer.ScrollToHorizontalOffset(0);
                        FlushLayout(window);

                        var increaseTrackButton = GetScrollTrackButton(horizontalBar, increase: true);
                        var previousOffset = headerViewer.HorizontalOffset;
                        var scrollSteps = 0;
                        while (headerViewer.HorizontalOffset < headerViewer.ScrollableWidth - 0.5 && scrollSteps++ < 8)
                        {
                            InvokeRepeatButtonClick(increaseTrackButton);
                            FlushLayout(window);
                            Assert.True(headerViewer.HorizontalOffset > previousOffset + 0.5,
                                $"{page.FileName}/{theme}: mouse scroll-bar page action did not advance the header viewport.");
                            previousOffset = headerViewer.HorizontalOffset;
                        }
                        Assert.True(headerViewer.HorizontalOffset >= headerViewer.ScrollableWidth - 0.5,
                            $"{page.FileName}/{theme}: mouse scroll-bar actions could not reach the final tab.");

                        var lastTab = Assert.IsType<TabItem>(tabs.Items[tabs.Items.Count - 1]);
                        Assert.True(IsFullyInsideViewport(lastTab, headerViewer),
                            $"{page.FileName}/{theme}: final tab is clipped after mouse scrolling.");
                        RaiseWorkspaceTabMouseClick(lastTab, window);
                        FlushLayout(window);
                        Assert.Equal(tabs.Items.Count - 1, tabs.SelectedIndex);
                        CaptureQ11TabVisualIfRequested(
                            visualCaptureRoot, root, page.FileName, theme, "last", tabs, headerViewer, horizontalBar);

                        var decreaseTrackButton = GetScrollTrackButton(horizontalBar, increase: false);
                        previousOffset = headerViewer.HorizontalOffset;
                        scrollSteps = 0;
                        while (headerViewer.HorizontalOffset > 0.5 && scrollSteps++ < 8)
                        {
                            InvokeRepeatButtonClick(decreaseTrackButton);
                            FlushLayout(window);
                            Assert.True(headerViewer.HorizontalOffset < previousOffset - 0.5,
                                $"{page.FileName}/{theme}: mouse scroll-bar page action did not return toward the first tab.");
                            previousOffset = headerViewer.HorizontalOffset;
                        }
                        var firstVisibleTab = Assert.IsType<TabItem>(tabs.Items[0]);
                        Assert.True(IsFullyInsideViewport(firstVisibleTab, headerViewer));
                        RaiseWorkspaceTabMouseClick(firstVisibleTab, window);
                        FlushLayout(window);
                        Assert.Equal(0, tabs.SelectedIndex);
                    }
                    finally
                    {
                        window.Close();
                    }
                }
            }

            var wideRoot = new Grid { Resources = resources };
            wideRoot.SetResourceReference(Panel.BackgroundProperty, "GscGlassFillBrush");
            wideRoot.SetResourceReference(TextElement.ForegroundProperty, "GscPrimaryTextBrush");
            var wideTabs = new TabControl
            {
                Style = tabControlStyle,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            foreach (var header in productionHeaders.Single(page => page.FileName == "MaintenanceView.xaml").Headers)
                wideTabs.Items.Add(new TabItem { Style = tabItemStyle, Header = header, Content = new Border() });
            wideRoot.Children.Add(wideTabs);
            var wideWindow = CreateWindow(wideRoot, 1280, 280);
            try
            {
                wideWindow.Show();
                wideTabs.ApplyTemplate();
                FlushLayout(wideWindow);
                var wideViewer = Assert.IsType<ScrollViewer>(wideTabs.Template.FindName("HeaderScrollViewer", wideTabs));
                Assert.True(wideViewer.ScrollableWidth <= 0.5,
                    $"a wide workspace should fit its current six maintenance tabs; scrollable={wideViewer.ScrollableWidth:0.##} DIP.");
                var wideBar = Assert.Single(FindVisualChildren<ScrollBar>(wideViewer),
                    scrollBar => scrollBar.Orientation == Orientation.Horizontal);
                Assert.Equal(Visibility.Collapsed, wideBar.Visibility);
                foreach (var theme in Themes)
                {
                    ApplyTheme(wideRoot, theme);
                    FlushLayout(wideWindow);
                    CaptureQ11TabVisualIfRequested(
                        visualCaptureRoot, wideRoot, "MaintenanceView", theme, "wide", wideTabs, wideViewer, wideBar);
                }
            }
            finally
            {
                wideWindow.Close();
            }
        });
    }

    private static void CaptureQ11TabVisualIfRequested(
        string? outputRoot,
        Grid root,
        string pageName,
        GameSaveCenterThemeMode theme,
        string state,
        TabControl tabs,
        ScrollViewer headerViewer,
        ScrollBar horizontalBar)
    {
        if (string.IsNullOrWhiteSpace(outputRoot))
            return;

        Assert.True(root.ActualWidth > 0 && root.ActualHeight > 0,
            $"{pageName}/{theme}/{state}: cannot capture an unmeasured visual root.");
        var fileName = $"{Path.GetFileNameWithoutExtension(pageName)}-{theme.ToString().ToLowerInvariant()}-{state}.png";
        var bitmap = new RenderTargetBitmap(
            Math.Max(1, (int)Math.Ceiling(root.ActualWidth)),
            Math.Max(1, (int)Math.Ceiling(root.ActualHeight)),
            96,
            96,
            PixelFormats.Pbgra32);
        bitmap.Render(root);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using (var stream = File.Create(Path.Combine(outputRoot, fileName)))
            encoder.Save(stream);

        var selectedHeader = (tabs.SelectedItem as TabItem)?.Header?.ToString() ?? string.Empty;
        var row = string.Join("|", new[]
        {
            Path.GetFileNameWithoutExtension(pageName),
            theme.ToString(),
            state,
            $"{root.ActualWidth.ToString("0.##", CultureInfo.InvariantCulture)}x{root.ActualHeight.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"{headerViewer.ViewportWidth.ToString("0.##", CultureInfo.InvariantCulture)}/{headerViewer.ExtentWidth.ToString("0.##", CultureInfo.InvariantCulture)}",
            headerViewer.HorizontalOffset.ToString("0.##", CultureInfo.InvariantCulture),
            tabs.SelectedIndex.ToString(CultureInfo.InvariantCulture),
            selectedHeader.Replace("|", " "),
            horizontalBar.Visibility.ToString(),
            horizontalBar.ActualHeight.ToString("0.##", CultureInfo.InvariantCulture),
            fileName
        });
        File.AppendAllText(Path.Combine(outputRoot, "q11-06-tab-overflow-captures.txt"), row + Environment.NewLine);
    }

    [Fact]
    public void EachProductionPageGridKeepsSelectedFocusAndDisabledRowStatesAcrossThemes()
    {
        RunSta(() =>
        {
            var cases = new (string Name, Func<UserControl> Create, string Field, string StyleKey, string RowStyleKey)[]
            {
                ("Task", () => new TaskCenterView(), "TaskGrid", "TaskDataGrid", "GscStableDataGridRow"),
                ("Media Inbox", () => new MediaCenterView(), "MediaInboxGrid", "MediaDataGrid", "MediaInboxStableRowStyle"),
                ("Save", () => new SaveCenterView(), "SaveHistoryGrid", "SaveDataGrid", "GscStableDataGridRow"),
                ("Maintenance", () => new MaintenanceView(), "FindingsGrid", "MaintenanceDataGrid", "GscStableDataGridRow")
            };
            var lightPrimaryTextByPage = new Dictionary<string, Color>(StringComparer.Ordinal);

            foreach (var theme in Themes)
            {
                foreach (var fixture in cases)
                {
                    var page = fixture.Create();
                    var grid = Assert.IsType<DataGrid>(page.GetType()
                        .GetField(fixture.Field, BindingFlags.Instance | BindingFlags.NonPublic)!
                        .GetValue(page));
                    var rows = fixture.Name == "Media Inbox"
                        ? new ObservableCollection<object> { CreateMedia("one"), CreateMedia("two") }
                        : new ObservableCollection<object> { new ProbeRow("one"), new ProbeRow("two") };
                    grid.ItemsSource = rows;
                    ApplyTheme(page, theme);
                    var primaryText = BrushColor(page.Resources["GscPrimaryTextBrush"]);
                    if (theme == GameSaveCenterThemeMode.Light)
                        lightPrimaryTextByPage[fixture.Name] = primaryText;
                    else
                        Assert.NotEqual(lightPrimaryTextByPage[fixture.Name], primaryText);

                    var root = new Grid();
                    root.Children.Add(page);
                    var external = new TextBox
                    {
                        Width = 120,
                        Height = 28,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    root.Children.Add(external);
                    var window = CreateWindow(root, 1280, 900);
                    try
                    {
                        window.Show();
                        FlushLayout(window);
                        Assert.NotNull(grid.Style);
                        Assert.Same(page.Resources[fixture.StyleKey], grid.Style);
                        Assert.Equal(typeof(DataGrid), grid.Style!.TargetType);
                        Assert.NotNull(grid.RowStyle);
                        Assert.Same(page.Resources[fixture.RowStyleKey], grid.RowStyle);
                        Assert.Equal(typeof(DataGridRow), grid.RowStyle!.TargetType);

                        var row = grid.ItemContainerGenerator.ContainerFromItem(rows[0]) as DataGridRow;
                        Assert.NotNull(row);
                        Assert.Same(external, Keyboard.Focus(external));
                        FlushLayout(window);
                        var unselectedGeometry = CaptureCellAndTextGeometry(row!, grid);

                        grid.SelectedItem = rows[0];
                        Assert.Same(external, Keyboard.Focus(external));
                        FlushLayout(window);
                        Assert.True(row!.IsSelected, $"{fixture.Name} did not apply selection to the realized row");
                        Assert.False(Selector.GetIsSelectionActive(row), $"{fixture.Name} selection should be inactive while focus is outside the grid");
                        var inactiveSelectionGeometry = CaptureCellAndTextGeometry(row, grid);
                        AssertRowGeometryUnchanged(unselectedGeometry, inactiveSelectionGeometry, fixture.Name, theme, "inactive selection");

                        var cell = FindVisualChildren<DataGridCell>(row)
                            .FirstOrDefault(candidate => candidate.IsVisible && candidate.ActualWidth > 0);
                        Assert.NotNull(cell);
                        Assert.Same(cell, Keyboard.Focus(cell));
                        FlushLayout(window);
                        Assert.True(row.IsKeyboardFocusWithin, $"{fixture.Name} row did not retain keyboard focus");
                        var keyboardFocusedGeometry = CaptureCellAndTextGeometry(row, grid);
                        AssertRowGeometryUnchanged(unselectedGeometry, keyboardFocusedGeometry, fixture.Name, theme, "keyboard-focused selection");
                        var chrome = Assert.IsType<Border>(row.Template.FindName("RowChrome", row));
                        Assert.Equal(BrushColor(page.Resources["GscAccentBrush"]), BrushColor(chrome.BorderBrush));
                        Assert.Equal(new Thickness(2), chrome.BorderThickness);

                        grid.IsEnabled = false;
                        FlushLayout(window);
                        Assert.False(row.IsEnabled, $"{fixture.Name} row remained enabled with its DataGrid disabled");
                        Assert.False(cell!.IsEnabled, $"{fixture.Name} cell remained enabled with its DataGrid disabled");
                        Assert.Equal(0.42d, row.Opacity);
                        Assert.Equal(1d, chrome.Opacity);
                        Assert.Same(external, Keyboard.Focus(external));
                        FlushLayout(window);
                        Assert.NotSame(cell, Keyboard.Focus(cell));
                    }
                    finally
                    {
                        window.Close();
                    }
                }
            }
        });
    }

    [Fact]
    public void TaskSelectionKeepsFailureBadgeAndCancelActionUsableAcrossThemes()
    {
        RunSta(() =>
        {
            foreach (var theme in Themes)
            {
                var failed = new TaskStatusDto
                {
                    TaskId = "synthetic-failed",
                    TaskType = "Backup",
                    GameName = "Synthetic Game",
                    State = TaskState.Failed,
                    ErrorCode = "SYNTHETIC_FAILURE",
                    ErrorMessage = "Synthetic failure for selection-state verification.",
                    CreatedUtc = new DateTime(2026, 10, 1)
                };
                var running = new TaskStatusDto
                {
                    TaskId = "synthetic-running",
                    TaskType = "Backup",
                    GameName = "Synthetic Game",
                    State = TaskState.Running,
                    StageMessage = "正在执行",
                    ProgressPercent = 25,
                    CreatedUtc = new DateTime(2026, 10, 2)
                };
                var fixture = new TaskSelectionFixture(null);
                var page = new TaskCenterView { DataContext = fixture };
                var grid = Assert.IsType<DataGrid>(page.FindName("TaskGrid"));
                grid.ItemsSource = new ObservableCollection<TaskStatusDto> { failed, running };
                ApplyTheme(page, theme);

                var root = new Grid();
                root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Grid.SetRow(page, 0);
                root.Children.Add(page);
                var externalFocus = new TextBox { Height = 28, Text = "Synthetic focus target" };
                Grid.SetRow(externalFocus, 1);
                root.Children.Add(externalFocus);
                var window = CreateWindow(root, 1280, 900);

                try
                {
                    window.Show();
                    FlushLayout(window);
                    Assert.True(grid.IsVisible && grid.ActualWidth > 0 && grid.ActualHeight > 0,
                        $"{theme}: the production task table was not realized.");

                    var hoverRow = Assert.IsType<DataGridRow>(grid.ItemContainerGenerator.ContainerFromItem(running));
                    var hoverBackground = Assert.IsType<Border>(hoverRow.Template.FindName("RowBackground", hoverRow));
                    ChangeMouseOverForTest(hoverRow);
                    FlushLayout(window);
                    Assert.True(hoverRow.IsMouseOver, $"{theme}: the controlled hover state was not applied to the production task row.");
                    Assert.Equal(BrushColor(page.Resources["GscRowHoverBrush"]), BrushColor(hoverBackground.Background));
                    ChangeMouseOverForTest(externalFocus);
                    FlushLayout(window);
                    Assert.False(hoverRow.IsMouseOver, $"{theme}: the controlled hover state did not clear.");
                    Assert.Equal(BrushColor(page.Resources["GscTableRowBrush"]), BrushColor(hoverBackground.Background));

                    grid.SelectedItem = failed;
                    Assert.Same(externalFocus, Keyboard.Focus(externalFocus));
                    FlushLayout(window);
                    var failedRow = Assert.IsType<DataGridRow>(grid.ItemContainerGenerator.ContainerFromItem(failed));
                    var statusCell = Assert.Single(FindVisualChildren<DataGridCell>(failedRow),
                        cell => Equals(cell.Column.Header, "状态"));
                    var statusPresenter = Assert.Single(FindVisualChildren<ContentPresenter>(statusCell),
                        presenter => presenter.ContentTemplate != null);
                    var statusPill = Assert.IsType<Border>(statusPresenter.ContentTemplate!.FindName("TaskStatusPill", statusPresenter));
                    var statusText = Assert.IsType<TextBlock>(statusPill.Child);
                    var failedRowBackground = Assert.IsType<Border>(failedRow.Template.FindName("RowBackground", failedRow));

                    Assert.True(failedRow.IsSelected);
                    Assert.False(Selector.GetIsSelectionActive(failedRow));
                    Assert.True(statusPill.IsVisible && statusPill.ActualWidth > 0 && statusPill.ActualHeight > 0,
                        $"{theme}: the failed task status pill was not visible inside its selected row.");
                    Assert.Contains("失败", statusText.Text, StringComparison.Ordinal);
                    Assert.Equal(BrushColor(page.Resources["GscErrorTintBrush"]), BrushColor(statusPill.Background));
                    Assert.Equal(BrushColor(page.Resources["GscErrorBrush"]), BrushColor(statusPill.BorderBrush));
                    Assert.Equal(BrushColor(page.Resources["GscSelectionInactiveBrush"]), BrushColor(failedRowBackground.Background));
                    Assert.NotEqual(BrushColor(statusPill.Background), BrushColor(failedRowBackground.Background));
                    var failedCancel = Assert.Single(FindVisualChildren<GameSaveCenter.Playnite.Controls.Button>(page),
                        button => AutomationProperties.GetName(button) == "取消任务");
                    Assert.True(failedCancel.IsVisible);
                    Assert.False(failedCancel.IsEnabled,
                        $"{theme}: a failed terminal task must not enable the cancel action.");
                    var statusPoint = statusPill.TransformToAncestor(grid).Transform(
                        new Point(statusPill.ActualWidth / 2, statusPill.ActualHeight / 2));
                    var statusHit = grid.InputHitTest(statusPoint) as DependencyObject;
                    Assert.NotNull(statusHit);
                    Assert.True(ReferenceEquals(statusPill, FindVisualAncestor<Border>(statusHit!)),
                        $"{theme}: hit testing the failed status badge landed on '{(statusHit as FrameworkElement)?.Name ?? statusHit.GetType().Name}'.");

                    grid.SelectedItem = running;
                    Assert.Same(externalFocus, Keyboard.Focus(externalFocus));
                    FlushLayout(window);
                    var runningRow = Assert.IsType<DataGridRow>(grid.ItemContainerGenerator.ContainerFromItem(running));
                    var runningCell = FindVisualChildren<DataGridCell>(runningRow)
                        .First(cell => cell.Visibility == Visibility.Visible && cell.ActualWidth > 0);
                    Assert.Same(runningCell, Keyboard.Focus(runningCell));
                    FlushLayout(window);
                    var cancel = Assert.Single(FindVisualChildren<GameSaveCenter.Playnite.Controls.Button>(page),
                        button => AutomationProperties.GetName(button) == "取消任务");
                    Assert.True(runningRow.IsKeyboardFocusWithin);
                    Assert.True(cancel.IsVisible && cancel.IsEnabled,
                        $"{theme}: the enabled task-cancel action was hidden or disabled while a task row was selected.");
                    Assert.Same(fixture.CancelCommand, cancel.Command);

                    var clickCount = 0;
                    cancel.Click += (_, _) => clickCount++;
                    var peer = FrameworkElementAutomationPeer.CreatePeerForElement(cancel);
                    Assert.NotNull(peer);
                    var invoke = Assert.IsAssignableFrom<IInvokeProvider>(peer!.GetPattern(PatternInterface.Invoke));
                    invoke.Invoke();
                    cancel.Dispatcher.Invoke(DispatcherPriority.Background, new Action(cancel.UpdateLayout));
                    Assert.True(fixture.CancelCommand.ExecutionCount == 1 && clickCount == 1,
                        $"{theme}: cancel Invoke should execute its bound command and raise Click (command={fixture.CancelCommand.ExecutionCount}, click={clickCount}).");
                }
                finally
                {
                    window.Close();
                }
            }
        });
    }

    private static readonly GameSaveCenterThemeMode[] Themes =
    {
        GameSaveCenterThemeMode.Light,
        GameSaveCenterThemeMode.Dark
    };

    private static MediaItemDto CreateMedia(string id)
        => new()
        {
            MediaId = id,
            Kind = MediaKind.Screenshot,
            Source = MediaSourceKind.WindowsScreenshot,
            ArchivePath = "C:\\archive\\" + id + ".png",
            OriginalPath = "C:\\source\\" + id + ".png",
            Sha256 = id,
            CapturedUtc = DateTime.UtcNow,
            ClassificationState = "Assigned"
        };

    private static ResourceDictionary LoadProductionResources()
        => Assert.IsType<ResourceDictionary>(XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/AcrylicProductionResources.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>"));

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

    private static Window CreateWindow(UIElement content, double width, double height)
        => new()
        {
            Content = content,
            Width = width,
            Height = height,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = true,
            Opacity = 0.01
        };

    private static Color BrushColor(object? value)
        => Assert.IsType<SolidColorBrush>(value).Color;

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
                yield return match;
            foreach (var nested in FindVisualChildren<T>(child))
                yield return nested;
        }
    }

    private static T? FindVisualAncestor<T>(DependencyObject element)
        where T : DependencyObject
    {
        for (DependencyObject? current = element; current != null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is T match)
                return match;
        }

        return null;
    }

    private static string[] ReadTopLevelWorkspaceTabHeaders(string fileName)
    {
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        var path = Path.Combine(TestRepositoryContext.Root, "src", "GameSaveCenter.Playnite", "Views", fileName);
        var page = XDocument.Load(path);
        var tabControl = page.Descendants(presentation + "TabControl").FirstOrDefault()
            ?? throw new InvalidOperationException($"{fileName} has no production TabControl.");
        return tabControl.Elements(presentation + "TabItem")
            .Select(tab => (string?)tab.Attribute("Header") ?? string.Empty)
            .ToArray();
    }

    private static bool IsFullyInsideViewport(FrameworkElement element, ScrollViewer viewer)
    {
        var bounds = BoundsRelativeTo(element, viewer);
        return bounds.Left >= -0.5 && bounds.Right <= viewer.ViewportWidth + 0.5;
    }

    private static RepeatButton GetScrollTrackButton(ScrollBar scrollBar, bool increase)
    {
        scrollBar.ApplyTemplate();
        var track = Assert.IsType<Track>(scrollBar.Template!.FindName("PART_Track", scrollBar));
        return Assert.IsType<RepeatButton>(increase ? track.IncreaseRepeatButton : track.DecreaseRepeatButton);
    }

    private static void InvokeRepeatButtonClick(RepeatButton button)
        => (typeof(RepeatButton).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("WPF RepeatButton.OnClick was not found."))
            .Invoke(button, Array.Empty<object>());

    private static void RaiseWorkspaceTabKeyDown(FrameworkElement source, Window host, Key keyValue)
    {
        var presentationSource = PresentationSource.FromVisual(host)
            ?? throw new InvalidOperationException("The workspace tab test window has no presentation source.");
        source.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, presentationSource, 0, keyValue)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent
        });
        source.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, presentationSource, 0, keyValue)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        });
    }

    private static void RaiseWorkspaceTabMouseClick(TabItem item, Window host)
    {
        _ = PresentationSource.FromVisual(host)
            ?? throw new InvalidOperationException("The workspace tab test window has no presentation source.");
        item.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonDownEvent
        });
        item.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonUpEvent
        });
    }

    private static Rect[] CaptureCellAndTextGeometry(DataGridRow row, DataGrid grid)
    {
        var cells = FindVisualChildren<DataGridCell>(row)
            .Where(cell => cell.Visibility == Visibility.Visible && cell.ActualWidth > 0 && cell.ActualHeight > 0)
            .OrderBy(cell => cell.Column.DisplayIndex)
            .ToArray();
        if (cells.Length == 0)
            throw new InvalidOperationException("The realized row has no visible cells to measure.");

        return cells.SelectMany(cell =>
        {
            var content = FindVisualChildren<TextBlock>(cell)
                .Where(text => text.Visibility == Visibility.Visible && text.ActualWidth > 0 && text.ActualHeight > 0)
                .Select(text => BoundsRelativeTo(text, grid));
            return new[] { BoundsRelativeTo(cell, grid) }.Concat(content);
        }).ToArray();
    }

    private static Rect BoundsRelativeTo(FrameworkElement element, FrameworkElement ancestor)
    {
        var origin = element.TransformToAncestor(ancestor).Transform(new Point(0, 0));
        return new Rect(origin.X, origin.Y, element.ActualWidth, element.ActualHeight);
    }

    private static void AssertRowGeometryUnchanged(Rect[] expected, Rect[] actual, string page, GameSaveCenterThemeMode theme, string state)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (var index = 0; index < expected.Length; index++)
        {
            Assert.True(Math.Abs(expected[index].X - actual[index].X) <= 0.25,
                $"{page}/{theme} {state} shifted cell/content {index} horizontally from {expected[index]} to {actual[index]}.");
            Assert.True(Math.Abs(expected[index].Y - actual[index].Y) <= 0.25,
                $"{page}/{theme} {state} shifted cell/content {index} vertically from {expected[index]} to {actual[index]}.");
            Assert.True(Math.Abs(expected[index].Width - actual[index].Width) <= 0.25,
                $"{page}/{theme} {state} changed cell/content {index} width from {expected[index]} to {actual[index]}.");
            Assert.True(Math.Abs(expected[index].Height - actual[index].Height) <= 0.25,
                $"{page}/{theme} {state} changed cell/content {index} height from {expected[index]} to {actual[index]}.");
        }
    }

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.UpdateLayout();
    }

    private static void ChangeMouseOverForTest(System.Windows.IInputElement target)
    {
        var changeMouseOver = typeof(MouseDevice).GetMethod("ChangeMouseOver", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("WPF MouseDevice.ChangeMouseOver was not found.");
        changeMouseOver.Invoke(Mouse.PrimaryDevice, new object[] { target, Environment.TickCount });
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private sealed class ProbeRow
    {
        public ProbeRow(string name) => Name = name;
        public string Name { get; }
    }

    private sealed class TaskSelectionFixture
    {
        private TaskStatusDto? selectedTask;

        public TaskSelectionFixture(TaskStatusDto? selectedTask)
        {
            this.selectedTask = selectedTask;
            CancelCommand = new SelectionCancelCommand(() => SelectedTask?.CanCancel == true);
        }

        public TaskStatusDto? SelectedTask
        {
            get => selectedTask;
            set
            {
                selectedTask = value;
                CancelCommand.RaiseCanExecuteChanged();
            }
        }

        public SelectionCancelCommand CancelCommand { get; }
        public System.Windows.Input.ICommand CancelTaskCommand => CancelCommand;
        public System.Windows.Input.ICommand RetryTaskCommand { get; } = new SelectionCancelCommand(() => false);
        public System.Windows.Input.ICommand CopyTaskErrorCommand { get; } = new SelectionCancelCommand(() => false);
        public bool IsCancellingTask => false;
    }

    private sealed class SelectionCancelCommand : System.Windows.Input.ICommand
    {
        private readonly Func<bool> canExecute;

        public SelectionCancelCommand(Func<bool> canExecute) => this.canExecute = canExecute;

        public int ExecutionCount { get; private set; }
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => canExecute();

        public void Execute(object? parameter) => ExecutionCount++;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

}

[CollectionDefinition("R23ProductionResourcesWpf", DisableParallelization = true)]
public sealed class R23ProductionResourcesWpfCollection
{
}
