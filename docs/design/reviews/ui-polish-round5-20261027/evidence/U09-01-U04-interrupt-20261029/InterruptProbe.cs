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
    private static int RunInterruptEntry(string output)
    {
        var result = 1;
        var thread = new Thread(() =>
        {
            try { result = RunInterrupt(output); }
            catch (Exception error) { Console.Error.WriteLine(error); }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start(); thread.Join(); return result;
    }

    private static int RunInterrupt(string output)
    {
        Directory.CreateDirectory(output);
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
        var rows = new StringBuilder("theme,phase,elapsed_ms,expanded,enabled,angle,animated,same_header,same_path,same_transform\n");
        foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
        {
            var production = new MaintenanceView();
            ApplyThemePalette(production, theme, false, true);
            var root = new Grid { Width = 700, Height = 220, Resources = production.Resources, Background = CreateHarnessBackground(production), IsHitTestVisible = false };
            var card = new Expander { Header = "共享披露卡 · 中断与对象复用", Style = (Style)production.Resources["GscDisclosureCard"], Margin = new Thickness(20), VerticalAlignment = VerticalAlignment.Top,
                Content = new TextBlock { Text = "只读夹具，不执行恢复、删除或云端写入。", Margin = new Thickness(12) } };
            root.Children.Add(card);
            AdaptiveThemePaletteFactory.SetMotionEnabledActive(root, true);
            AdaptiveThemePaletteFactory.SetHighContrastActive(root, false);
            var window = new Window { Content = root, Width = 720, Height = 260, WindowStyle = WindowStyle.None, ShowInTaskbar = false, ShowActivated = false, Opacity = .01 };
            AutomationProperties.SetName(window, "U04披露卡历史中断探针");
            try
            {
                window.Show(); root.UpdateLayout(); InterruptPump(200);
                var header = (ToggleButton)card.Template.FindName("HeaderSite", card);
                header.ApplyTemplate();
                var path = FindVisualChildren<System.Windows.Shapes.Path>(header).First(x => x.Name == "Chevron");
                var rotation = (RotateTransform)path.RenderTransform;
                var clock = Stopwatch.StartNew();
                void Flush()
                {
                    root.Dispatcher.Invoke(DispatcherPriority.Loaded, new Action(() => { }));
                    root.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
                    root.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                    root.UpdateLayout();
                }
                void Record(string phase, bool image)
                {
                    Flush();
                    var currentHeader = (ToggleButton)card.Template.FindName("HeaderSite", card);
                    var currentPath = FindVisualChildren<System.Windows.Shapes.Path>(currentHeader).First(x => x.Name == "Chevron");
                    var currentTransform = (RotateTransform)currentPath.RenderTransform;
                    rows.AppendLine(theme + "," + phase + "," + clock.Elapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture)
                        + "," + card.IsExpanded + "," + card.IsEnabled + "," + currentTransform.Angle.ToString("F6", CultureInfo.InvariantCulture)
                        + "," + DependencyPropertyHelper.GetValueSource(currentTransform, RotateTransform.AngleProperty).IsAnimated
                        + "," + ReferenceEquals(header, currentHeader) + "," + ReferenceEquals(path, currentPath) + "," + ReferenceEquals(rotation, currentTransform));
                    if (image) SavePng(root, Path.Combine(output, theme + "-" + phase + ".png"));
                }
                Record("initial", true);
                // Observe a real calculated frame; no Seek or synthetic transform value.
                var frame = new DispatcherFrame();
                var deadline = new DispatcherTimer(DispatcherPriority.Send) { Interval = TimeSpan.FromMilliseconds(500) };
                var observed = false;
                EventHandler rendering = (_, _) =>
                {
                    if (rotation.Angle > -85 && rotation.Angle < -5) { observed = true; frame.Continue = false; }
                };
                deadline.Tick += (_, _) => { deadline.Stop(); frame.Continue = false; };
                CompositionTarget.Rendering += rendering;
                try { clock.Restart(); card.IsExpanded = true; deadline.Start(); Dispatcher.PushFrame(frame); }
                finally { CompositionTarget.Rendering -= rendering; deadline.Stop(); }
                if (!observed) throw new InvalidOperationException("No intermediate disclosure frame observed.");
                Record("before-disable", false);
                card.IsEnabled = false;
                Record("disabled-immediate", true);
                InterruptPump(250);
                Record("disabled-settled", true);
                card.IsEnabled = true; InterruptPump(250);
                Record("reenabled", true);
                AdaptiveThemePaletteFactory.SetMotionEnabledActive(root, false);
                ApplyThemePalette(production, theme, false, false);
                Flush();
                Record("reduced-theme-applied", false);
                // Theme/template reapplication is outside the 100-toggle reuse interval.
                header = (ToggleButton)card.Template.FindName("HeaderSite", card);
                path = FindVisualChildren<System.Windows.Shapes.Path>(header).First(x => x.Name == "Chevron");
                rotation = (RotateTransform)path.RenderTransform;
                Record("reduced-baseline", false);
                for (var i = 0; i < 100; i++) { card.IsExpanded = i % 2 == 0; Flush(); }
                card.IsExpanded = true; Flush();
                Record("reduced-after100", true);
            }
            finally { window.Close(); InterruptPump(80); }
        }
        File.WriteAllText(Path.Combine(output, "timeline.csv"), rows.ToString());
        app.Shutdown(); return 0;
    }

    private static void InterruptPump(double milliseconds)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Send) { Interval = TimeSpan.FromMilliseconds(milliseconds) };
        timer.Tick += (_, _) => { timer.Stop(); frame.Continue = false; };
        timer.Start(); Dispatcher.PushFrame(frame);
    }
}
