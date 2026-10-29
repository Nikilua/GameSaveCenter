using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;

namespace GameSaveCenter.RenderHarness;

public static class U07HistoricalPairProbe
{
    private static readonly (string Name, GameSaveCenterThemeMode Mode)[] Themes =
    {
        ("light", GameSaveCenterThemeMode.Light),
        ("dark", GameSaveCenterThemeMode.Dark)
    };

    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Length != 2 || (args[0] != "settings" && args[0] != "task"))
        {
            Console.Error.WriteLine("Usage: U07HistoricalPairProbe <settings|task> <output-directory>");
            return 2;
        }

        var kind = args[0];
        var outputRoot = Path.GetFullPath(args[1]);
        Directory.CreateDirectory(outputRoot);
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
        var checks = new System.Collections.Generic.List<string>();
        var errors = new System.Collections.Generic.List<string>();

        try
        {
            foreach (var (themeName, themeMode) in Themes)
            {
                if (kind == "settings")
                    CaptureSettings(themeName, themeMode, outputRoot, checks, errors);
                else
                    CaptureTask(themeName, themeMode, outputRoot, checks, errors);
            }

            File.WriteAllLines(Path.Combine(outputRoot, "checks.txt"), checks.Concat(errors.Select(error => "PROBLEM " + error)));
            foreach (var line in checks)
                Console.WriteLine(line);
            foreach (var error in errors)
                Console.Error.WriteLine("PROBLEM " + error);
            return errors.Count == 0 ? 0 : 1;
        }
        catch (Exception exception)
        {
            File.WriteAllText(Path.Combine(outputRoot, "exception.txt"), exception.ToString());
            Console.Error.WriteLine(exception);
            return 1;
        }
        finally
        {
            app.Shutdown();
        }
    }

    private static void CaptureSettings(
        string themeName,
        GameSaveCenterThemeMode themeMode,
        string outputRoot,
        System.Collections.Generic.List<string> checks,
        System.Collections.Generic.List<string> errors)
    {
        const int width = 1600;
        const int height = 900;
        var fixtureRoot = Path.Combine(outputRoot, "fixture-" + themeName);
        Directory.CreateDirectory(fixtureRoot);
        var worker = Path.Combine(fixtureRoot, "GameSaveCenter.Worker.exe");
        var ludusavi = Path.Combine(fixtureRoot, "ludusavi.exe");
        var rclone = Path.Combine(fixtureRoot, "rclone.exe");
        File.WriteAllText(worker, "render-only worker fixture");
        File.WriteAllText(ludusavi, "render-only backup fixture");
        File.WriteAllText(rclone, "render-only cloud fixture");
        var saves = Path.Combine(fixtureRoot, "Saves");
        var media = Path.Combine(fixtureRoot, "Media");
        Directory.CreateDirectory(saves);
        Directory.CreateDirectory(media);

        var settings = new GameSaveCenterSettings
        {
            ThemeMode = themeMode,
            WorkerExecutable = worker,
            LudusaviExecutable = ludusavi,
            RcloneExecutable = rclone,
            LudusaviBackupDirectory = saves,
            MediaArchiveDirectory = media,
            HealthInspectionEnabled = false,
            EnableUiAnimations = false
        };
        var view = new GameSaveCenterSettingsView { DataContext = settings };
        view.ApplyThemeForAudit(themeMode);
        var tabs = (ListBox?)view.FindName("SettingsSectionTabs");
        if (tabs == null)
            throw new InvalidOperationException("SettingsSectionTabs was not found.");
        tabs.SelectedIndex = 3;

        var host = CreateHost(view, width, height);
        var window = CreateWindow(host, width, height);
        window.Show();
        ApplyResponsive(view, width, height);
        FlushLayout(window);

        var fields = view.FindName("HealthInspectionIntervalFields") as FrameworkElement;
        var hint = view.FindName("HealthInspectionDependencyHint") as TextBlock;
        var scroller = view.FindName("SettingsScroller") as ScrollViewer;
        if (fields != null && scroller != null)
        {
            var fieldTop = fields.TransformToAncestor(scroller).Transform(new Point(0, 0)).Y;
            scroller.ScrollToVerticalOffset(Math.Max(0, scroller.VerticalOffset + fieldTop - 300));
        }
        FlushLayout(window);
        if (fields == null || scroller == null)
            errors.Add($"settings/{themeName} missing interval fields or settings scroller");
        else if (fields.IsEnabled)
            errors.Add($"settings/{themeName} expected disabled dependency fields");

        if (hint == null)
        {
            checks.Add($"settings/{themeName}: dependency hint absent; interval fields disabled");
        }
        else
        {
            var hintBounds = hint.TransformToAncestor(scroller!).TransformBounds(new Rect(0, 0, hint.ActualWidth, hint.ActualHeight));
            var viewportBounds = new Rect(0, 0, scroller!.ViewportWidth, scroller.ViewportHeight);
            var isVisible = hint.IsVisible && hint.ActualWidth > 0 && hint.ActualHeight > 0 && viewportBounds.Contains(hintBounds);
            checks.Add($"settings/{themeName}: dependency hint visible={isVisible}; fields enabled={fields?.IsEnabled}; bounds={hintBounds}; viewport={viewportBounds}; text={hint.Text}");
            if (!isVisible || hint.Text.IndexOf("关闭时保留当前数值", StringComparison.Ordinal) < 0)
                errors.Add($"settings/{themeName} dependency hint is not visible or has unexpected text");
        }

        SavePng(host, Path.Combine(outputRoot, $"settings-{themeName}-{width}x{height}.png"));
        window.Close();
    }

    private static void CaptureTask(
        string themeName,
        GameSaveCenterThemeMode themeMode,
        string outputRoot,
        System.Collections.Generic.List<string> checks,
        System.Collections.Generic.List<string> errors)
    {
        const int width = 1366;
        const int height = 768;
        const string guidance = "权限不足。下一步：检查云端账户对目标目录的权限；修复后先核对上次远端结果，再决定是否重试。";
        var data = new FakeDashboardData(18);
        var fixtureNowUtc = new DateTime(2026, 10, 28, 1, 30, 0, DateTimeKind.Utc);
        for (var index = 0; index < data.Tasks.Count; index++)
        {
            var sampleTask = data.Tasks[index];
            var createdUtc = fixtureNowUtc.AddMinutes(-30 * (index + 1));
            sampleTask.CreatedUtc = createdUtc;
            sampleTask.StartedUtc = createdUtc.AddMinutes(1);
            sampleTask.FinishedUtc = sampleTask.State == TaskState.Running
                ? null
                : sampleTask.StartedUtc.Value.AddMinutes(24);
            sampleTask.ElapsedSeconds = sampleTask.State == TaskState.Running ? 30 * 60 : 24 * 60;
        }
        var task = data.Tasks[3];
        task.TaskType = "CloudUpload";
        task.State = TaskState.Failed;
        task.ErrorCode = "RCLONE_PERMISSION_DENIED";
        task.ErrorMessage = "目标云端目录拒绝写入。";
        task.Message = "云端上传失败；本地版本已保留。";
        data.SelectedTask = task;
        data.SelectedTaskRetryGuidance = guidance;

        var view = new TaskCenterView { DataContext = data };
        var palette = AdaptiveThemePaletteFactory.Create(view, false, 50, themeMode);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(view.Resources, palette, glassEnabled: false, motionEnabled: false);
        view.ApplyResponsiveLayout(width, height);
        var host = CreateHost(view, width, height);
        var window = CreateWindow(host, width, height);
        window.Show();
        view.ApplyResponsiveLayout(width, height);
        FlushLayout(window);

        var frame = view.FindName("TaskDetailInspectorFrame") as Border;
        var detailScroll = view.FindName("TaskDetailScrollViewer") as ScrollViewer;
        detailScroll?.ScrollToTop();
        FlushLayout(window);
        if (frame == null || frame.Visibility != Visibility.Visible || frame.ActualWidth <= 0)
            errors.Add($"task/{themeName} failed task inspector did not arrange visibly");

        var card = view.FindName("TaskRetryGuidanceCard") as Border;
        var guidanceText = view.FindName("TaskRetryGuidanceText") as TextBlock;
        if (card == null)
        {
            checks.Add($"task/{themeName}: retry guidance card absent; failed task selected");
        }
        else
        {
            var visible = card.IsVisible && card.ActualWidth > 0 && card.ActualHeight > 0;
            var actualText = guidanceText?.Text ?? string.Empty;
            var helpText = guidanceText == null ? string.Empty : AutomationProperties.GetHelpText(guidanceText);
            checks.Add($"task/{themeName}: retry card visible={visible}; text={actualText}; UIA help matches={helpText == actualText}");
            if (!visible || actualText != guidance || helpText != guidance)
                errors.Add($"task/{themeName} retry guidance card visibility, text, or UIA help did not match");
        }

        SavePng(host, Path.Combine(outputRoot, $"task-{themeName}-{width}x{height}.png"));
        window.Close();
    }

    private static Grid CreateHost(UserControl view, int width, int height)
    {
        var host = new Grid
        {
            Width = width,
            Height = height,
            Background = view.TryFindResource("GscBackdropBrush") as Brush ?? new SolidColorBrush(Color.FromRgb(24, 30, 43)),
            ClipToBounds = true
        };
        host.Children.Add(view);
        return host;
    }

    private static Window CreateWindow(Grid host, int width, int height)
        => new()
        {
            Content = host,
            Width = width,
            Height = height,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -32000,
            Top = -32000
        };

    private static void ApplyResponsive(UserControl view, double width, double height)
    {
        var method = view.GetType().GetMethod(
            "ApplyResponsiveLayout",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        method?.Invoke(view, new object[] { width, height });
    }

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
        window.UpdateLayout();
    }

    private static void SavePng(Visual visual, string path)
    {
        var element = (FrameworkElement)visual;
        if (element.ActualWidth <= 0 || element.ActualHeight <= 0)
            throw new InvalidOperationException($"Cannot capture empty visual {element.ActualWidth}x{element.ActualHeight}.");
        var bitmap = new RenderTargetBitmap(
            (int)Math.Ceiling(element.ActualWidth),
            (int)Math.Ceiling(element.ActualHeight),
            96,
            96,
            PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}
