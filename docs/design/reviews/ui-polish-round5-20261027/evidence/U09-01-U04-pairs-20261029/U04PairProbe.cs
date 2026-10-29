using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;

namespace GameSaveCenter.RenderHarness;

public static partial class Program
{
    private static int RunU04PairEntry(string output)
    {
        var exit = 1;
        var thread = new Thread(() =>
        {
            try { exit = RunU04Pair(output); }
            catch (Exception error) { Console.Error.WriteLine(error); }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        return exit;
    }

    private static int RunU04Pair(string output)
    {
        Directory.CreateDirectory(output);
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
        var rows = new StringBuilder("theme,motion,phase,target_ms,elapsed_ms,expanded,chevron_angle,first_indicator,second_indicator,second_indicator_animated\n");
        foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
        {
            var production = new MaintenanceView();
            ApplyThemePalette(production, theme, false, true);
            var root = new Grid { Width = 700, Height = 370, Resources = production.Resources, Background = CreateHarnessBackground(production), IsHitTestVisible = false };
            var stack = new StackPanel { Margin = new Thickness(20) };
            var disclosure = new Expander
            {
                Header = "共享披露卡 · 只读样本",
                Style = (Style)production.Resources["GscDisclosureCard"],
                Content = new TextBlock { Text = "仅观察展开与收起，不执行业务命令。", Margin = new Thickness(12) },
                Margin = new Thickness(0, 0, 0, 18)
            };
            var tabs = new TabControl
            {
                Style = (Style)production.Resources["GscRedesignWorkspaceTabControl"],
                ItemContainerStyle = (Style)production.Resources["GscRedesignWorkspaceTabItem"],
                Height = 160,
                Items =
                {
                    new TabItem { Header = "存档历史", Content = new TextBlock { Text = "同一控件样本 · 第一页" } },
                    new TabItem { Header = "候选路径", Content = new TextBlock { Text = "同一控件样本 · 第二页" } }
                }
            };
            stack.Children.Add(disclosure);
            stack.Children.Add(tabs);
            root.Children.Add(stack);
            AdaptiveThemePaletteFactory.SetMotionEnabledActive(root, true);
            AdaptiveThemePaletteFactory.SetHighContrastActive(root, false);
            var window = new Window { Content = root, Width = 720, Height = 410, WindowStyle = WindowStyle.None, ShowInTaskbar = false, ShowActivated = false, Opacity = .01 };
            AutomationProperties.SetName(window, "U04历史动效配对探针");
            try
            {
                window.Show();
                root.UpdateLayout();
                U04PairPump(200);
                var header = (ToggleButton)disclosure.Template.FindName("HeaderSite", disclosure);
                header.ApplyTemplate();
                var chevron = FindVisualChildren<System.Windows.Shapes.Path>(header).First(path => path.Name == "Chevron");
                var first = (TabItem)tabs.Items[0];
                var second = (TabItem)tabs.Items[1];
                first.ApplyTemplate(); second.ApplyTemplate();
                var firstIndicator = first.Template.FindName("SelectionIndicator", first) as Border;
                var secondIndicator = second.Template.FindName("SelectionIndicator", second) as Border;
                var clock = Stopwatch.StartNew();
                string Value(double value) => value.ToString("F6", CultureInfo.InvariantCulture);
                void Flush()
                {
                    root.Dispatcher.Invoke(DispatcherPriority.Loaded, new Action(() => { }));
                    root.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
                    root.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                    root.UpdateLayout();
                }
                void Settle(bool expanded, int selected)
                {
                    var timeout = Stopwatch.StartNew();
                    while (true)
                    {
                        U04PairPump(20); Flush();
                        var angle = (chevron.RenderTransform as RotateTransform)?.Angle ?? double.NaN;
                        var ready = Math.Abs(angle - (expanded ? 0 : -90)) < .001
                            && (firstIndicator == null || Math.Abs(firstIndicator.Opacity - (selected == 0 ? 1 : 0)) < .001)
                            && (secondIndicator == null || Math.Abs(secondIndicator.Opacity - (selected == 1 ? 1 : 0)) < .001);
                        if (ready) return;
                        if (timeout.Elapsed.TotalSeconds > 2) throw new InvalidOperationException("Control did not reach its expected endpoint.");
                    }
                }
                void Capture(string phase, int target, bool motion, bool save = true)
                {
                    while (target - clock.Elapsed.TotalMilliseconds > 0)
                        U04PairPump(Math.Max(1, target - clock.Elapsed.TotalMilliseconds));
                    Flush();
                    var elapsed = clock.Elapsed.TotalMilliseconds;
                    var angle = (chevron.RenderTransform as RotateTransform)?.Angle ?? double.NaN;
                    rows.AppendLine(theme + "," + motion + "," + phase + "," + target + "," + Value(elapsed) + "," + disclosure.IsExpanded + "," + Value(angle)
                        + "," + (firstIndicator == null ? "not-present" : Value(firstIndicator.Opacity))
                        + "," + (secondIndicator == null ? "not-present" : Value(secondIndicator.Opacity))
                        + "," + (secondIndicator != null && DependencyPropertyHelper.GetValueSource(secondIndicator, UIElement.OpacityProperty).IsAnimated));
                    if (save) SavePng(root, Path.Combine(output, theme + "-" + phase + "-" + target + ".png"));
                }
                Capture("initial", 0, true);
                clock.Restart(); disclosure.IsExpanded = true; tabs.SelectedIndex = 1;
                foreach (var ms in new[] { 0, 20, 40, 60, 80, 100, 120, 160, 240 }) Capture("enter", ms, true, false);
                Settle(true, 1);
                SavePng(root, Path.Combine(output, theme + "-enter-settled.png"));
                clock.Restart(); disclosure.IsExpanded = false; tabs.SelectedIndex = 0;
                foreach (var ms in new[] { 0, 20, 40, 60, 80, 100, 120, 160, 240 }) Capture("exit", ms, true, false);
                Settle(false, 0);
                SavePng(root, Path.Combine(output, theme + "-exit-settled.png"));
                AdaptiveThemePaletteFactory.SetMotionEnabledActive(root, false);
                ApplyThemePalette(production, theme, false, false);
                clock.Restart(); disclosure.IsExpanded = true; tabs.SelectedIndex = 1;
                Capture("reduced", 0, false);
                Capture("reduced", 160, false, false);
            }
            finally { window.Close(); U04PairPump(80); }
        }
        File.WriteAllText(Path.Combine(output, "timeline.csv"), rows.ToString());
        app.Shutdown();
        Console.WriteLine("U04 historical control pair captured; real production resources; no physical input or Playnite verification inferred.");
        return 0;
    }

    private static void U04PairPump(double milliseconds)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Send) { Interval = TimeSpan.FromMilliseconds(milliseconds) };
        timer.Tick += (_, _) => { timer.Stop(); frame.Continue = false; };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }
}
