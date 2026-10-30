using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;

namespace GameSaveCenter.RenderHarness;

internal static class U07PairProbe
{
    public static int Run(string[] args)
    {
        if (args.Length < 6)
        {
            Console.Error.WriteLine("usage: u07pairprobe <output> <dialog|tooltip|menu> <light|dark> <width> <height>");
            return 2;
        }

        var outputRoot = Path.GetFullPath(args[1]);
        var scenario = args[2];
        var theme = args[3];
        var width = double.Parse(args[4], System.Globalization.CultureInfo.InvariantCulture);
        var height = double.Parse(args[5], System.Globalization.CultureInfo.InvariantCulture);
        var mode = string.Equals(theme, "light", StringComparison.OrdinalIgnoreCase)
            ? GameSaveCenterThemeMode.Light
            : GameSaveCenterThemeMode.Dark;
        var sourceRoot = Environment.GetEnvironmentVariable("GSC_SOURCE_ROOT");
        if (string.IsNullOrWhiteSpace(sourceRoot) || !Directory.Exists(sourceRoot))
            throw new DirectoryNotFoundException("GSC_SOURCE_ROOT must identify the archived historical source tree.");

        Directory.CreateDirectory(outputRoot);
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var application = Application.Current ?? new Application();
                if (!application.Resources.Contains("BaseTextBlockStyle"))
                    application.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
                if (scenario == "dialog")
                    CaptureDialog(outputRoot, sourceRoot, theme, mode, width, height);
                else if (scenario == "tooltip")
                    CaptureTooltip(outputRoot, theme, mode, width, height);
                else if (scenario == "menu")
                    CaptureColumnMenu(outputRoot, theme, mode, width, height);
                else
                    throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown U07 capture scenario.");
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
        {
            Console.Error.WriteLine(failure);
            return 1;
        }
        return 0;
    }

    private static void CaptureDialog(string outputRoot, string sourceRoot, string theme, GameSaveCenterThemeMode mode, double width, double height)
    {
        var sourcePath = Path.Combine(sourceRoot, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml");
        var document = XDocument.Load(sourcePath, LoadOptions.PreserveWhitespace);
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        var dialog = document.Descendants().Single(element => (string)element.Attribute(x + "Name") == "DialogOverlay");
        dialog.SetAttributeValue("Visibility", "Visible");
        foreach (var click in dialog.DescendantsAndSelf().Attributes().Where(attribute => attribute.Name.LocalName == "Click").ToArray())
            click.Remove();

        SetNamedAttribute(dialog, x, "DialogTitleText", "Text", "安全恢复确认 · 静态视觉样张");
        var message = "游戏：山海行者（离线样例）\n存档版本：2026-10-28 09:00 · 已校验\n范围：仅检查所选版本与当前设备\n影响：恢复前会保留当前状态供回滚，并按已显示的安全顺序关闭目标游戏与 Playnite。\n说明：本图为静态 WPF 样张，不会执行恢复、删除或云端写入。\n\n" +
            string.Join("\n\n", Enumerable.Repeat("预检查记录显示样例数据只用于确认对象、范围和后果。正式操作仍由真实确认请求、取消令牌和恢复安全策略控制。", 9));
        SetNamedAttribute(dialog, x, "DialogMessageText", "Text", message);
        SetNamedAttribute(dialog, x, "DialogCopyButton", "Visibility", "Visible");
        SetNamedAttribute(dialog, x, "DialogCopyButton", "Content", "复制详情");
        SetNamedAttribute(dialog, x, "DialogNeverButton", "Visibility", "Visible");
        SetNamedAttribute(dialog, x, "DialogNeverButton", "Content", "保留此设置");
        SetNamedAttribute(dialog, x, "DialogLaterButton", "Visibility", "Visible");
        SetNamedAttribute(dialog, x, "DialogLaterButton", "Content", "以后再说");
        SetNamedAttribute(dialog, x, "DialogCancelButton", "Content", "取消");
        SetNamedAttribute(dialog, x, "DialogConfirmButton", "Content", "确认恢复所选版本并保留当前设备的其他存档与云端同步设置");

        var resourceSources = new[]
        {
            "DesignTokens.xaml", "WpfUiProduction.xaml", "Redesign.xaml", "AcrylicReferenceTokens.xaml",
            "AcrylicReferenceColorsDark.xaml", "AcrylicReferenceControls.xaml"
        };
        var dictionaries = new XElement(presentation + "ResourceDictionary.MergedDictionaries",
            resourceSources.Select(file => new XElement(presentation + "ResourceDictionary",
                new XAttribute("Source", "/GameSaveCenter.Playnite;component/Themes/" + file))));
        var resources = new XElement(presentation + "UserControl.Resources",
            new XElement(presentation + "ResourceDictionary", dictionaries));
        var wrapper = new XElement(presentation + "UserControl",
            new XAttribute(XNamespace.Xmlns + "x", x.NamespaceName),
            new XAttribute("Width", width), new XAttribute("Height", height),
            resources, dialog);
        var wrapperText = new XDocument(wrapper).ToString(SaveOptions.DisableFormatting);
        var dialogLayer = (UserControl)XamlReader.Parse(wrapperText);
        ApplyThemePalette(dialogLayer, mode);

        var page = new SaveCenterView { DataContext = new FakeDashboardData() };
        ApplyThemePalette(page, mode);
        ApplyThemeResponsive(page, width, height);
        var backdrop = new Grid();
        backdrop.Children.Add(page);
        backdrop.Children.Add(dialogLayer);
        var window = CreateWindow(backdrop, width, height);
        try
        {
            window.Show();
            DrainDispatcher();
            window.UpdateLayout();
            dialogLayer.UpdateLayout();
            var overlay = FindNamed<Grid>(dialogLayer, "DialogOverlay");
            var card = FindNamed<Border>(dialogLayer, "DialogCard");
            var text = FindNamed<TextBlock>(dialogLayer, "DialogMessageText");
            var scroll = LogicalTreeHelper.GetParent(text) as ScrollViewer;
            var contentGrid = card.Child as Grid;
            if (contentGrid == null) throw new InvalidOperationException("Dialog card content grid is missing.");
            var actions = contentGrid.Children.OfType<Panel>().Last();
            card.Opacity = 1;
            overlay.Visibility = Visibility.Visible;
            overlay.UpdateLayout();
            window.UpdateLayout();
            DrainDispatcher();

            var file = Path.Combine(outputRoot, "dialog-" + theme + "-" + width.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + "x" + height.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + ".png");
            SavePng(window, file);
            var buttonLines = actions.Children.OfType<Button>().Where(button => button.Visibility == Visibility.Visible)
                .Select(button => "button=" + button.Content + ";margin=" + button.Margin + ";bounds=" + GetBounds(button, actions));
            WriteReport(outputRoot, "dialog", theme, width, height,
                new[]
                {
                    "dialogSource=src/GameSaveCenter.Playnite/Views/DashboardView.xaml",
                    "title=" + FindNamed<TextBlock>(dialogLayer, "DialogTitleText").Text,
                    "actionPanel=" + actions.GetType().Name,
                    "card=" + card.ActualWidth + "x" + card.ActualHeight,
                    "messageScrollPresent=" + (scroll != null),
                    "messageViewport=" + (scroll == null ? "not-scrolled" : scroll.ActualWidth + "x" + scroll.ActualHeight),
                    "messageTextHeight=" + text.ActualHeight,
                    "scrollVerticalVisibility=" + (scroll == null ? "not-applicable" : scroll.VerticalScrollBarVisibility.ToString()),
                    "sample=static-only; restore/delete/cloud-write=not-invoked",
                    "image=" + Path.GetFileName(file)
                }.Concat(buttonLines));
        }
        finally { window.Close(); }
    }

    private static void CaptureTooltip(string outputRoot, string theme, GameSaveCenterThemeMode mode, double width, double height)
    {
        var view = new GameSaveCenterSettingsView { DataContext = new GameSaveCenterSettings() };
        ApplyThemePalette(view, mode);
        ApplyThemeResponsive(view, width, height);
        var owner = FindNamed<TextBox>(view, "SettingsSearchTextBox");
        var content = owner.ToolTip as string;
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("The production SettingsSearchTextBox has no tooltip text.");
        var tooltip = new ToolTip
        {
            Content = content,
            Style = (Style)view.FindResource(typeof(ToolTip)),
            PlacementTarget = owner,
            Placement = ToolTipService.GetPlacement(owner)
        };
        owner.ToolTip = tooltip;
        var window = CreateWindow(view, width, height);
        try
        {
            window.Show();
            DrainDispatcher();
            window.UpdateLayout();
            owner.UpdateLayout();
            tooltip.Placement = ToolTipService.GetPlacement(owner);
            tooltip.IsOpen = true;
            DrainDispatcher();
            window.UpdateLayout();
            tooltip.UpdateLayout();
            var stem = "tooltip-" + theme + "-" + width.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + "x" + height.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
            var hostFile = Path.Combine(outputRoot, stem + "-settings.png");
            var popupFile = Path.Combine(outputRoot, stem + "-popup.png");
            SavePng(window, hostFile);
            SavePng(tooltip, popupFile);
            var ownerPoint = owner.PointToScreen(new Point(0, 0));
            var popupPoint = tooltip.PointToScreen(new Point(0, 0));
            WriteReport(outputRoot, "tooltip", theme, width, height, new[]
            {
                "owner=SettingsSearchTextBox",
                "ownerPlacement=" + ToolTipService.GetPlacement(owner),
                "popupPlacement=" + tooltip.Placement,
                "tooltipSize=" + tooltip.ActualWidth + "x" + tooltip.ActualHeight,
                "ownerScreenOrigin=" + ownerPoint,
                "popupScreenOrigin=" + popupPoint,
                "tooltipText=" + content,
                "settingsHostImage=" + Path.GetFileName(hostFile),
                "tooltipPopupImage=" + Path.GetFileName(popupFile)
            });
        }
        finally
        {
            tooltip.IsOpen = false;
            window.Close();
        }
    }

    private static void CaptureColumnMenu(string outputRoot, string theme, GameSaveCenterThemeMode mode, double width, double height)
    {
        var view = new TaskCenterView { DataContext = new FakeDashboardData() };
        ApplyThemePalette(view, mode);
        ApplyThemeResponsive(view, width, height);
        var grid = FindNamed<DataGrid>(view, "TaskGrid");
        var button = FindNamed<Button>(view, "TaskGridColumnVisibilityButton");
        var keys = new[] { "local-time", "task", "stage", "game", "state", "progress", "detail" };
        if (grid.Columns.Count != keys.Length)
            throw new InvalidOperationException("TaskGrid column identity changed: expected " + keys.Length + ", found " + grid.Columns.Count + ".");
        using (var controller = new DataGridColumnVisibilityController(
            grid, button, "tasks", keys, new[] { "local-time", "task", "state" }, new GameSaveCenterSettings(), () => { }))
        {
            var window = CreateWindow(view, width, height);
            try
            {
                window.Show();
                DrainDispatcher();
                window.UpdateLayout();
                controller.Menu.IsOpen = true;
                DrainDispatcher();
                controller.Menu.UpdateLayout();
                var required = controller.Menu.Items.OfType<MenuItem>()
                    .First(item => AutomationProperties.GetName(item) == "时间（必要列）");
                var helpText = AutomationProperties.GetHelpText(required);
                var tooltipText = Convert.ToString(required.ToolTip);
                ToolTip? tooltip = null;
                if (!string.IsNullOrWhiteSpace(tooltipText))
                {
                    tooltip = new ToolTip
                    {
                        Content = tooltipText,
                        Style = (Style)view.FindResource(typeof(ToolTip)),
                        PlacementTarget = required,
                        Placement = PlacementMode.Right,
                        HorizontalOffset = 4
                    };
                    required.ToolTip = tooltip;
                    tooltip.IsOpen = true;
                    DrainDispatcher();
                    controller.Menu.UpdateLayout();
                    tooltip.UpdateLayout();
                }
                var stem = "menu-" + theme + "-" + width.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + "x" + height.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
                var hostFile = Path.Combine(outputRoot, stem + "-task-page.png");
                var menuFile = Path.Combine(outputRoot, stem + "-column-menu.png");
                var tooltipFile = Path.Combine(outputRoot, stem + "-required-column-tip.png");
                SavePng(window, hostFile);
                SavePng(controller.Menu, menuFile);
                if (tooltip != null) SavePng(tooltip, tooltipFile);
                WriteReport(outputRoot, "menu", theme, width, height, new[]
                {
                    "page=TaskCenterView with FakeDashboardData",
                    "menuItems=" + string.Join(" | ", controller.Menu.Items.OfType<MenuItem>().Select(item => AutomationProperties.GetName(item))),
                    "requiredItem=" + AutomationProperties.GetName(required),
                    "requiredEnabled=" + required.IsEnabled,
                    "showOnDisabled=" + ToolTipService.GetShowOnDisabled(required),
                    "helpText=" + helpText,
                    "tooltipText=" + (tooltipText ?? "none"),
                    "menuSize=" + controller.Menu.ActualWidth + "x" + controller.Menu.ActualHeight,
                    "taskPageImage=" + Path.GetFileName(hostFile),
                    "menuPopupImage=" + Path.GetFileName(menuFile),
                    "requiredColumnTooltipImage=" + (tooltip == null ? "not-captured; this snapshot has no mouse tooltip" : Path.GetFileName(tooltipFile))
                });
                if (tooltip != null) tooltip.IsOpen = false;
                controller.Menu.IsOpen = false;
            }
            finally { window.Close(); }
        }
    }

    private static Window CreateWindow(UIElement content, double width, double height)
        => new Window
        {
            Content = content,
            Width = width,
            Height = height,
            Left = 30,
            Top = 30,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            ShowActivated = false,
            Background = Brushes.Transparent
        };

    private static void ApplyThemePalette(UserControl view, GameSaveCenterThemeMode mode)
    {
        if (view is GameSaveCenterSettingsView settings)
        {
            settings.ApplyThemeForAudit(mode);
            return;
        }
        var palette = AdaptiveThemePaletteFactory.Create(view, false, 50, mode);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(view.Resources, palette, false, false);
    }

    private static void ApplyThemeResponsive(UserControl view, double width, double height)
    {
        var method = view.GetType().GetMethod("ApplyResponsiveLayout", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (method != null) method.Invoke(view, new object[] { width, height });
    }

    private static T FindNamed<T>(FrameworkElement root, string name) where T : FrameworkElement
    {
        var match = root.FindName(name) as T;
        if (match == null) throw new InvalidOperationException("Could not find production element " + name + ".");
        return match;
    }

    private static void SetNamedAttribute(XElement root, XNamespace xaml, string name, string attribute, string value)
    {
        var element = root.DescendantsAndSelf().Single(candidate => (string)candidate.Attribute(xaml + "Name") == name);
        element.SetAttributeValue(attribute, value);
    }

    private static string GetBounds(FrameworkElement element, FrameworkElement ancestor)
    {
        var bounds = element.TransformToAncestor(ancestor).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
        return bounds.X + "," + bounds.Y + "," + bounds.Width + "," + bounds.Height;
    }

    private static void DrainDispatcher()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    private static void WriteReport(string outputRoot, string scenario, string theme, double width, double height, IEnumerable<string> details)
    {
        var values = new[]
        {
            "scenario=" + scenario,
            "theme=" + theme,
            "logicalSizeDip=" + width + "x" + height,
                "hostBaseTextBlockStyle=neutral-render-harness-fallback",
            "sampleUtc=" + (Environment.GetEnvironmentVariable("GSC_RENDER_SAMPLE_UTC") ?? "not-applicable")
        }.Concat(details);
        var size = width.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + "x" + height.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
        File.WriteAllLines(Path.Combine(outputRoot, scenario + "-" + theme + "-" + size + "-measurements.txt"), values);
    }

    private static void SavePng(Visual visual, string path)
    {
        var element = visual as FrameworkElement;
        var width = element == null ? 0 : element.ActualWidth;
        var height = element == null ? 0 : element.ActualHeight;
        if (width <= 0 || height <= 0)
            throw new InvalidOperationException("Cannot capture an empty WPF visual: " + path + " (" + width + "x" + height + ").");
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(width), (int)Math.Ceiling(height), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using (var stream = File.Create(path)) encoder.Save(stream);
    }
}
