using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

public sealed class CommandDirectoryResultLayoutBehaviorTests
{
    private readonly ITestOutputHelper output;
    public CommandDirectoryResultLayoutBehaviorTests(ITestOutputHelper output) => this.output = output;

    [Fact]
    public void LongResultsKeepTheirMetadataAccessibleAtBothThemesAndTwoHundredPercentText()
    {
        Exception? failure = null;
        var scenarios = new List<string>();
        var thread = new Thread(() =>
        {
            try
            {
                foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
                foreach (var scale in new[] { 1d, 2d })
                foreach (var width in new[] { 620d, 860d, 1040d })
                    scenarios.Add(Probe(theme, scale, width));
            }
            catch (Exception error) { failure = error; }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(90)), "Command result layout matrix did not finish.");
        foreach (var scenario in scenarios) output.WriteLine(scenario);
        if (failure != null) throw new InvalidOperationException("Command result layout failed.", failure);
        Assert.Equal(12, scenarios.Count);
    }

    private static string Probe(GameSaveCenterThemeMode theme, double scale, double width)
    {
        var executions = 0;
        var name = "定位到合成长名称工作区的搜索框 — Search the synthetic workspace with a deliberately long bilingual command name";
        var target = "搜索框：合成长名称工作区 · synthetic-workspace-stable-id";
        var reason = "当前页面没有可用搜索框；请进入支持搜索的工作区后重试。 "
            + string.Join(" ", Enumerable.Repeat("The current synthetic workspace has no available search field.", 4));
        var item = new CommandDirectoryItem("workspace.focus-search", name, new[] { "Ctrl+F" }, "键盘导航",
            CommandDirectoryScope.Workspace, new RelayCommand(_ => executions++, _ => false),
            unavailableReason: () => reason,
            contextPreview: () => "目标工作区：合成工作区；仅定位到现有搜索框。 " + target,
            targetDisplay: () => target,
            shortcutDisplay: KeyboardShortcutHelpCatalog.WorkspaceSearchGesture);
        item.SetPreferenceState(pinned: true, recent: true, mayAddPin: true);

        var shell = new AcrylicProductionShellView { MotionEnabledProvider = () => false };
        var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
        SetField(dashboard, "gamePicker", FormatterServices.GetUninitializedObject(typeof(GamePickerViewModel)));
        SetField(dashboard, "<Games>k__BackingField", new BatchObservableCollection<GameStatusDto>());
        SetField(dashboard, "<Tasks>k__BackingField", new BatchObservableCollection<TaskStatusDto>());
        SetField(dashboard, "currentWorkspace", WorkspaceKind.Tasks);
        SetField(shell, "viewModel", dashboard);
        SetField(shell, "commandDirectoryItems", new[] { item });
        var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(shell, true, 78, theme, false);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(shell.Resources, palette, true, false);
        shell.Resources["GscBodyFontSize"] = 14d * scale;
        shell.Resources["GscCaptionFontSize"] = 12d * scale;
        var window = new Window
        {
            Content = shell, Width = width, Height = 900, ShowInTaskbar = false,
            ShowActivated = false, WindowStyle = WindowStyle.None, Opacity = 0.01
        };
        try
        {
            window.Show();
            var overlay = (Grid)shell.FindName("CommandDirectoryOverlay")!;
            overlay.Visibility = Visibility.Visible;
            typeof(AcrylicProductionShellView).GetMethod("UpdateCommandDirectorySearchResults",
                BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(shell, null);
            Pump(window);
            var list = (ListBox)shell.FindName("CommandDirectoryResultsListBox")!;
            list.ScrollIntoView(item);
            Pump(window);
            var row = Find<FrameworkElement>(list, "CommandResultRow");
            var execute = Find<System.Windows.Controls.Button>(row, "CommandResultExecuteButton");
            var title = Find<TextBlock>(row, "CommandResultName");
            var metadata = Find<WrapPanel>(row, "CommandResultMetadata");
            var objectText = Find<TextBlock>(row, "CommandResultTarget");
            var shortcut = Find<TextBlock>(row, "CommandResultShortcut");
            var unavailable = Find<TextBlock>(row, "CommandResultUnavailable");
            var panel = (Border)shell.FindName("CommandDirectoryPanel")!;

            Assert.True(list.ActualHeight >= 80, $"results viewport collapsed: {list.ActualHeight:0.##}");
            Assert.True(row.ActualWidth <= list.ActualWidth + 1);
            Assert.True(title.ActualWidth > 20);
            Assert.Equal(14d * scale, title.FontSize);
            Assert.Equal(name, title.Text);
            Assert.Equal(TextTrimming.CharacterEllipsis, title.TextTrimming);
            Assert.Equal(target, objectText.Text);
            Assert.Equal("Ctrl+F", shortcut.Text);
            Assert.True(shortcut.ActualWidth + 1 >= TextWidth(shortcut), "shortcut was truncated");
            foreach (var text in Children<TextBlock>(metadata).Where(text => !string.IsNullOrEmpty(text.Text)))
            {
                Assert.True(text.ActualWidth + 1 >= TextWidth(text), $"metadata text clipped: {text.Text}");
                Assert.True(Bounds(text, metadata).Right <= metadata.ActualWidth + 1,
                    $"metadata escaped its parent: {text.Text}");
            }
            Assert.True(title.ActualHeight + 1 >= TextHeight(title));
            Assert.True(objectText.ActualHeight + 1 >= TextHeight(objectText));
            Assert.True(Bounds(title, row).Bottom <= Bounds(metadata, row).Top + 1);
            Assert.True(Bounds(metadata, row).Bottom <= Bounds(objectText, row).Top + 1);
            Assert.True(Bounds(objectText, row).Bottom <= Bounds(unavailable, row).Top + 1);
            Assert.True(Bounds(unavailable, row).Right <= row.ActualWidth + 1);
            Assert.True(Bounds(unavailable, row).Bottom <= row.ActualHeight + 1);
            Assert.True(unavailable.IsEnabled, "reason inherited the disabled action state");
            Assert.Null(Ancestor<ButtonBase>(unavailable));
            Assert.Equal(reason, unavailable.Text);
            Assert.Contains(name, Assert.IsType<string>(execute.ToolTip));
            Assert.Contains(target, AutomationProperties.GetHelpText(execute));
            Assert.Contains(reason, AutomationProperties.GetHelpText(execute));
            Assert.Contains(target, AutomationProperties.GetName(execute));
            Assert.Contains("Ctrl+F", AutomationProperties.GetName(execute));
            Assert.True(ToolTipService.GetShowOnDisabled(execute));
            Assert.False(execute.IsEnabled);
            Assert.False(item.TryExecute());
            Assert.Equal(0, executions);
            Assert.True(VirtualizingPanel.GetIsVirtualizing(list));
            Assert.Equal(VirtualizationMode.Recycling, VirtualizingPanel.GetVirtualizationMode(list));
            Assert.Equal(ScrollUnit.Pixel, VirtualizingPanel.GetScrollUnit(list));

            Capture(shell, $"{theme}-{width:0}-{scale * 100:0}-top");
            var scroller = Children<ScrollViewer>(list).First();
            scroller.ScrollToEnd();
            Pump(window);
            var presenter = Children<ScrollContentPresenter>(scroller).First();
            Assert.True(Bounds(unavailable, presenter).Bottom <= presenter.ActualHeight + 1,
                "pixel scrolling could not bring the complete reason to the viewport end");
            var context = (ScrollViewer)shell.FindName("CommandDirectoryContextScrollViewer")!;
            Assert.True(context.Focusable && KeyboardNavigation.GetIsTabStop(context));
            Assert.True(context.ActualHeight <= 140);
            context.ScrollToEnd();
            Pump(window);
            Assert.True(context.VerticalOffset <= context.ScrollableHeight + 1);
            Capture(shell, $"{theme}-{width:0}-{scale * 100:0}-bottom");
            return $"{theme} window={width:0}, text={scale * 100:0}%, panel={panel.ActualWidth:0.##}, "
                + $"list={list.ActualWidth:0.##}x{list.ActualHeight:0.##}, row={row.ActualHeight:0.##}, "
                + $"titleWidth={title.ActualWidth:0.##}, reasonHeight={unavailable.ActualHeight:0.##}, "
                + $"scroll={scroller.VerticalOffset:0.##}/{scroller.ScrollableHeight:0.##}";
        }
        finally { window.Close(); }
    }

    private static void Capture(FrameworkElement view, string name)
    {
        var outputRoot = Environment.GetEnvironmentVariable("GSC_COMMAND_DIRECTORY_EVIDENCE_ROOT");
        if (string.IsNullOrWhiteSpace(outputRoot)) return;
        var path = Path.GetFullPath(outputRoot!);
        var allowed = Path.GetFullPath(Path.Combine(TestRepositoryContext.Root, "artifacts")) + Path.DirectorySeparatorChar;
        Assert.StartsWith(allowed, path, StringComparison.OrdinalIgnoreCase);
        Directory.CreateDirectory(path);
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(view.ActualWidth),
            (int)Math.Ceiling(view.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(view);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(Path.Combine(path, name + ".png"));
        encoder.Save(stream);
    }

    private static FormattedText Measure(TextBlock text)
        => new FormattedText(text.Text, CultureInfo.CurrentUICulture, text.FlowDirection,
            new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch),
            text.FontSize, text.Foreground, VisualTreeHelper.GetDpi(text).PixelsPerDip);
    private static double TextWidth(TextBlock text) => Measure(text).WidthIncludingTrailingWhitespace;
    private static double TextHeight(TextBlock text) => Measure(text).Height;
    private static Rect Bounds(FrameworkElement child, Visual ancestor)
        => child.TransformToAncestor(ancestor).TransformBounds(new Rect(0, 0, child.ActualWidth, child.ActualHeight));
    private static T Find<T>(DependencyObject root, string name) where T : FrameworkElement
        => Children<T>(root).Single(element => element.Name == name);
    private static IEnumerable<T> Children<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T typed) yield return typed;
            foreach (var descendant in Children<T>(child)) yield return descendant;
        }
    }
    private static T? Ancestor<T>(DependencyObject element) where T : DependencyObject
    {
        for (var parent = VisualTreeHelper.GetParent(element); parent != null; parent = VisualTreeHelper.GetParent(parent))
            if (parent is T typed) return typed;
        return null;
    }
    private static void SetField(object target, string name, object value)
        => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);
    private static void Pump(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(window.UpdateLayout));
        window.UpdateLayout();
    }
}
