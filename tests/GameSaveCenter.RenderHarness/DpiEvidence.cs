using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;

namespace GameSaveCenter.RenderHarness;

public static partial class Program
{
    private static int RunDpiEvidence(string outputRoot, bool processRequestSucceeded, int processRequestError, bool mainThreadPmv2)
    {
        const int shellWidth = 1600;
        const int shellHeight = 900;
        var scales = new[] { 1.25, 1.5, 2.0 };
        var themes = new[]
        {
            (Name: "light", Mode: GameSaveCenterThemeMode.Light),
            (Name: "dark", Mode: GameSaveCenterThemeMode.Dark)
        };

        Directory.CreateDirectory(outputRoot);
        s_problems.Clear();
        var report = new StringBuilder();
        var geometry = new List<string>
        {
            "case\ttheme\tcontrol\tautomation_name\tbinding_path\tvisible\twidth_dip\theight_dip\tx_dip\ty_dip\twidth_px\theight_px\trender_scale\tscale_mode\twindow_dpi\twpf_dpi_scale\twindow_physical_rect\tstate"
        };
        var geometryPath = Path.Combine(outputRoot, "dpi-geometry.tsv");
        var reportPath = Path.Combine(outputRoot, "dpi-matrix-report.txt");
        var app = Application.Current ?? new Application();
        app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));

        var priorThreadContext = SetThreadDpiAwarenessContext(PerMonitorAwareV2Context);
        var threadRequestError = priorThreadContext == IntPtr.Zero ? Marshal.GetLastWin32Error() : 0;
        var uiThreadPmv2 = AreDpiAwarenessContextsEqual(GetThreadDpiAwarenessContext(), PerMonitorAwareV2Context);
        report.AppendLine("GameSaveCenter Round 6 C03 controlled WPF DPI matrix");
        report.AppendLine($"GeneratedUtc: {DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)}");
        report.AppendLine($"ProcessDpiAwarenessRequestSucceeded: {processRequestSucceeded}");
        report.AppendLine($"ProcessDpiAwarenessRequestWin32Error: {processRequestError}");
        report.AppendLine($"MainThreadWasPmv2BeforeUiThread: {mainThreadPmv2}");
        report.AppendLine($"UiThreadSetPmv2Succeeded: {priorThreadContext != IntPtr.Zero}");
        report.AppendLine($"UiThreadSetPmv2Win32Error: {threadRequestError}");
        report.AppendLine($"UiThreadIsPmv2: {uiThreadPmv2}");
        report.AppendLine($"ProcessId: {System.Diagnostics.Process.GetCurrentProcess().Id}");
        report.AppendLine($"RuntimeVersion: {Environment.Version}");
        report.AppendLine("MeasurementBoundary: real WPF Window and Popup HWNDs on the current desktop monitor; all page data are synthetic; no Playnite host, physical input, real profile, save, or cloud access.");
        report.AppendLine($"LayoutViewport: shell {shellWidth}x{shellHeight} DIP; page content size derived by the existing RenderHarness ContentSize policy.");
        report.AppendLine("DpiMatrixBoundary: window WPF geometry is measured once at the host's effective DPI; 125% and 200% screenshots are RenderTargetBitmap raster-scale simulations. A matrix row is marked current-monitor only when its render scale matches GetDpiForWindow and VisualTreeHelper.GetDpi.");
        report.AppendLine("CaptureStabilization: the harness pumps the WPF dispatcher through loaded/render priorities before capture; task inspector motion is disabled on the harness view only.");
        report.AppendLine("Controls: task inspector, overview priority CTA, policy-template game checkbox row, task-status ComboBox popup.");

        try
        {
            var (contentWidth, contentHeight) = ContentSize(shellWidth, shellHeight);
            report.AppendLine($"ContentViewport: {contentWidth.ToString("0.##", CultureInfo.InvariantCulture)}x{contentHeight.ToString("0.##", CultureInfo.InvariantCulture)} DIP");
            report.AppendLine($"SystemMetricMonitorCount: {GetSystemMetrics(SystemMetricMonitorCount)}");

            foreach (var theme in themes)
            {
                CaptureDpiCase(
                    app,
                    outputRoot,
                    geometry,
                    report,
                    scales,
                    contentWidth,
                    contentHeight,
                    theme.Name,
                    theme.Mode,
                    "task-details",
                    data => new TaskCenterView { DataContext = data },
                    (window, view, data) => PrepareTaskDetails(window, (TaskCenterView)view, data, contentWidth, contentHeight),
                    view => FindNamedElement<FrameworkElement>(view, "TaskDetailInspectorFrame"),
                    null);

                CaptureDpiCase(
                    app,
                    outputRoot,
                    geometry,
                    report,
                    scales,
                    contentWidth,
                    contentHeight,
                    theme.Name,
                    theme.Mode,
                    "overview-cta",
                    data => new OverviewView { DataContext = data },
                    null,
                    view => FindNamedElement<FrameworkElement>(view, "OverviewPriorityActionButton"),
                    null);

                CaptureDpiCase(
                    app,
                    outputRoot,
                    geometry,
                    report,
                    scales,
                    contentWidth,
                    contentHeight,
                    theme.Name,
                    theme.Mode,
                    "save-policy-checkbox",
                    data => new SaveCenterView { DataContext = data },
                    (window, view, data) => PreparePolicyCheckbox((SaveCenterView)view, window),
                    view => FindPolicyCheckboxRow(view),
                    null);

                CaptureDpiCase(
                    app,
                    outputRoot,
                    geometry,
                    report,
                    scales,
                    contentWidth,
                    contentHeight,
                    theme.Name,
                    theme.Mode,
                    "task-status-popup",
                    data => new TaskCenterView { DataContext = data },
                    (window, view, data) => PrepareTaskStatusPopup((TaskCenterView)view, window),
                    view => FindNamedElement<ComboBox>(view, "TaskStatusFilterComboBox"),
                    view => FindTaskStatusPopup((TaskCenterView)view));
            }

            File.WriteAllLines(geometryPath, geometry, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var status = s_problems.Count == 0 ? "DpiEvidence OK" : "DpiEvidence FAILED";
            report.AppendLine(status);
            foreach (var problem in s_problems)
                report.AppendLine("  PROBLEM " + problem);
            report.AppendLine($"GeometryTsv: {Path.GetFileName(geometryPath)}");
            File.WriteAllText(reportPath, report.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            Console.WriteLine(report.ToString());
            return s_problems.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            report.AppendLine("DpiEvidence FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllLines(geometryPath, geometry, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            File.WriteAllText(reportPath, report.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            Console.Error.WriteLine(report.ToString());
            return 1;
        }
    }

    private static void CaptureDpiCase(
        Application app,
        string outputRoot,
        IList<string> geometry,
        StringBuilder report,
        IReadOnlyList<double> renderScales,
        double contentWidth,
        double contentHeight,
        string themeName,
        GameSaveCenterThemeMode themeMode,
        string caseName,
        Func<FakeDashboardData, UserControl> createView,
        Action<Window, UserControl, FakeDashboardData>? prepare,
        Func<UserControl, FrameworkElement> selectTarget,
        Func<UserControl, FrameworkElement?>? selectPopupTarget)
    {
        var data = caseName.StartsWith("task-", StringComparison.Ordinal)
            ? FakeDashboardData.CreateTaskComparisonFixture()
            : new FakeDashboardData();
        var view = createView(data);
        ApplyThemePalette(view, themeMode, glassEnabled: false, motionEnabled: false);
        var host = new Grid
        {
            Width = contentWidth,
            Height = contentHeight,
            Background = CreateHarnessBackground(view),
            ClipToBounds = true
        };
        host.Children.Add(view);

        var window = new Window
        {
            Width = contentWidth,
            Height = contentHeight,
            Left = 16,
            Top = 16,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            ShowActivated = false,
            Content = host
        };
        Popup? ownedPopup = null;
        try
        {
            ApplyThemeResponsive(view, contentWidth, contentHeight);
            window.Show();
            window.UpdateLayout();
            ApplyThemeResponsive(view, window.ActualWidth, window.ActualHeight);
            window.UpdateLayout();
            prepare?.Invoke(window, view, data);
            window.UpdateLayout();
            PumpWpfVisualState(window);

            var target = selectTarget(view);
            target.BringIntoView();
            window.UpdateLayout();
            if (!target.IsVisible || target.ActualWidth <= 0 || target.ActualHeight <= 0)
                throw new InvalidOperationException($"{caseName}/{themeName}: target {target.Name} is not visible or measurable ({target.ActualWidth:0.##}x{target.ActualHeight:0.##} DIP, {target.Visibility}).");

            if (caseName == "task-status-popup")
            {
                var combo = (ComboBox)target;
                ownedPopup = combo.Template.FindName("PART_Popup", combo) as Popup
                    ?? FindVisualChildren<Popup>(combo).FirstOrDefault();
                if (ownedPopup == null || !ownedPopup.IsOpen)
                    throw new InvalidOperationException("Task status ComboBox did not expose an open PART_Popup.");
            }

            var windowHandle = new WindowInteropHelper(window).Handle;
            var windowDpi = windowHandle == IntPtr.Zero ? 0u : GetDpiForWindow(windowHandle);
            var wpfDpi = VisualTreeHelper.GetDpi(window);
            var windowRect = default(Win32Rect);
            var hasWindowRect = windowHandle != IntPtr.Zero && GetWindowRect(windowHandle, out windowRect);
            var actualScale = wpfDpi.DpiScaleX;
            var nativeScale = windowDpi / 96d;
            if (windowDpi > 0 && Math.Abs(actualScale - nativeScale) > 0.02)
                s_problems.Add($"{caseName}/{themeName}: WPF scale {actualScale:0.###} does not match GetDpiForWindow {windowDpi} ({nativeScale:0.###}).");
            if (!uiThreadIsPmv2ForCurrentThread())
                s_problems.Add($"{caseName}/{themeName}: WPF window was created without a PMv2 UI thread.");

            var monitor = windowHandle == IntPtr.Zero ? IntPtr.Zero : MonitorFromWindow(windowHandle, MonitorDefaultToNearest);
            var monitorInfo = new MonitorInfoEx
            {
                Size = Marshal.SizeOf(typeof(MonitorInfoEx)),
                Device = new string('\0', 32)
            };
            var monitorName = "unknown";
            var monitorRect = "unknown";
            var monitorDpiText = "unknown";
            if (monitor != IntPtr.Zero)
            {
                if (GetMonitorInfo(monitor, ref monitorInfo))
                {
                    monitorName = (monitorInfo.Device ?? string.Empty).TrimEnd('\0');
                    monitorRect = FormatRect(monitorInfo.Monitor);
                }
                var dpiResult = GetDpiForMonitor(monitor, MonitorEffectiveDpi, out var monitorDpiX, out var monitorDpiY);
                monitorDpiText = dpiResult == 0 ? $"{monitorDpiX}x{monitorDpiY}" : $"HRESULT:{dpiResult}";
            }

            report.AppendLine($"Case: {caseName}/{themeName}");
            report.AppendLine($"  WindowDpi: GetDpiForWindow={windowDpi}; WPF={wpfDpi.PixelsPerInchX:0.##}x{wpfDpi.PixelsPerInchY:0.##}; scale={actualScale:0.###}x{wpfDpi.DpiScaleY:0.###}");
            report.AppendLine($"  Monitor: {monitorName}; effectiveDpi={monitorDpiText}; bounds={monitorRect}; windowPhysicalRect={(hasWindowRect ? FormatRect(windowRect) : "unknown")}");
            report.AppendLine($"  SyntheticFixture: tasks={data.Tasks.Count}; games={data.Games.Count}; selectedTask={(data.SelectedTask == null ? "none" : data.SelectedTask.TaskId)}; input=programmatic setup only");

            var targetBounds = target.TransformToAncestor(host).TransformBounds(new Rect(new Point(0, 0), target.RenderSize));
            var clippedByHostBounds = targetBounds.Left < -0.5 || targetBounds.Top < -0.5
                || targetBounds.Right > host.ActualWidth + 0.5 || targetBounds.Bottom > host.ActualHeight + 0.5;
            if (clippedByHostBounds)
                s_problems.Add($"{caseName}/{themeName}: target extends outside its page host ({targetBounds} vs {host.ActualWidth:0.##}x{host.ActualHeight:0.##} DIP).");
            var automationName = AutomationProperties.GetName(target);
            var bindingPath = GetRelevantBindingPath(caseName, target);
            report.AppendLine($"  Target: name={target.Name}; automationName={automationName}; bindingPath={bindingPath}; bounds={targetBounds}; visible={target.IsVisible}; hostClipped={clippedByHostBounds}");
            report.AppendLine($"  State: {GetTargetState(caseName, target, data)}");

            var pageContextName = $"{caseName}-{themeName}-{(int)Math.Round(actualScale * 100)}pct-current-monitor-pagecontext.png";
            SaveDpiPng(host, Path.Combine(outputRoot, pageContextName), wpfDpi.PixelsPerInchX);
            report.AppendLine($"  PageContext: {pageContextName}; {host.ActualWidth:0.##}x{host.ActualHeight:0.##} DIP at {wpfDpi.PixelsPerInchX:0} DPI");

            foreach (var scale in renderScales)
            {
                var dpi = 96d * scale;
                var scaleMatchesCurrentWindow = Math.Abs(scale - actualScale) < 0.015 && windowDpi > 0;
                var scaleMode = scaleMatchesCurrentWindow ? "current-monitor-WPF" : "logical-raster-simulation";
                var safeAutomationName = string.IsNullOrWhiteSpace(automationName) ? "unnamed" : SanitizeFileName(automationName);
                var fileName = $"{caseName}-{themeName}-{(int)Math.Round(scale * 100)}pct-{scaleMode}-{safeAutomationName}.png";
                SaveDpiPng(target, Path.Combine(outputRoot, fileName), dpi);
                var widthPixels = (int)Math.Ceiling(target.ActualWidth * scale);
                var heightPixels = (int)Math.Ceiling(target.ActualHeight * scale);
                geometry.Add(string.Join("\t", new[]
                {
                    caseName,
                    themeName,
                    EscapeDpiTsv(target.Name),
                    EscapeDpiTsv(automationName),
                    EscapeDpiTsv(bindingPath),
                    target.IsVisible.ToString(),
                    target.ActualWidth.ToString("0.###", CultureInfo.InvariantCulture),
                    target.ActualHeight.ToString("0.###", CultureInfo.InvariantCulture),
                    targetBounds.X.ToString("0.###", CultureInfo.InvariantCulture),
                    targetBounds.Y.ToString("0.###", CultureInfo.InvariantCulture),
                    widthPixels.ToString(CultureInfo.InvariantCulture),
                    heightPixels.ToString(CultureInfo.InvariantCulture),
                    scale.ToString("0.##", CultureInfo.InvariantCulture),
                    scaleMode,
                    windowDpi.ToString(CultureInfo.InvariantCulture),
                    actualScale.ToString("0.###", CultureInfo.InvariantCulture),
                    hasWindowRect ? FormatRect(windowRect) : "unknown",
                    EscapeDpiTsv(GetTargetState(caseName, target, data))
                }));
                report.AppendLine($"  Raster: {fileName}; dpi={dpi:0}; size={widthPixels}x{heightPixels}px; mode={scaleMode}");
            }

            var popupTarget = selectPopupTarget?.Invoke(view);
            if (popupTarget != null)
            {
                if (!popupTarget.IsVisible || popupTarget.ActualWidth <= 0 || popupTarget.ActualHeight <= 0)
                    throw new InvalidOperationException($"{caseName}/{themeName}: popup child is not visible or measurable ({popupTarget.ActualWidth:0.##}x{popupTarget.ActualHeight:0.##} DIP).");
                var source = PresentationSource.FromVisual(popupTarget) as HwndSource;
                var popupHandle = source?.Handle ?? IntPtr.Zero;
                var popupDpi = popupHandle == IntPtr.Zero ? 0u : GetDpiForWindow(popupHandle);
                var popupRect = default(Win32Rect);
                var hasPopupRect = popupHandle != IntPtr.Zero && GetWindowRect(popupHandle, out popupRect);
                report.AppendLine($"  PopupHwnd: dpi={popupDpi}; physicalRect={(hasPopupRect ? FormatRect(popupRect) : "unknown")}; child={popupTarget.ActualWidth:0.##}x{popupTarget.ActualHeight:0.##} DIP");
                foreach (var scale in renderScales)
                {
                    var dpi = 96d * scale;
                    var scaleMatchesCurrentWindow = Math.Abs(scale - actualScale) < 0.015 && popupDpi > 0;
                    var scaleMode = scaleMatchesCurrentWindow ? "current-monitor-WPF" : "logical-raster-simulation";
                    var fileName = $"{caseName}-{themeName}-{(int)Math.Round(scale * 100)}pct-{scaleMode}-popup.png";
                    SaveDpiPng(popupTarget, Path.Combine(outputRoot, fileName), dpi);
                    var widthPixels = (int)Math.Ceiling(popupTarget.ActualWidth * scale);
                    var heightPixels = (int)Math.Ceiling(popupTarget.ActualHeight * scale);
                    geometry.Add(string.Join("\t", new[]
                    {
                        caseName + ".popup",
                        themeName,
                        EscapeDpiTsv(popupTarget.Name),
                        EscapeDpiTsv(AutomationProperties.GetName(popupTarget)),
                        "popup-child",
                        popupTarget.IsVisible.ToString(),
                        popupTarget.ActualWidth.ToString("0.###", CultureInfo.InvariantCulture),
                        popupTarget.ActualHeight.ToString("0.###", CultureInfo.InvariantCulture),
                        "0", "0",
                        widthPixels.ToString(CultureInfo.InvariantCulture),
                        heightPixels.ToString(CultureInfo.InvariantCulture),
                        scale.ToString("0.##", CultureInfo.InvariantCulture),
                        scaleMode,
                        popupDpi.ToString(CultureInfo.InvariantCulture),
                        (popupDpi / 96d).ToString("0.###", CultureInfo.InvariantCulture),
                        hasPopupRect ? FormatRect(popupRect) : "unknown",
                        "ComboBox.IsDropDownOpen=true; options remain unselected"
                    }));
                    report.AppendLine($"  PopupRaster: {fileName}; dpi={dpi:0}; size={widthPixels}x{heightPixels}px; mode={scaleMode}");
                }
            }

            report.AppendLine();
        }
        finally
        {
            if (ownedPopup != null)
                ownedPopup.IsOpen = false;
            window.Close();
        }
    }

    private static void PrepareTaskDetails(Window window, TaskCenterView view, FakeDashboardData data, double width, double height)
    {
        DisableTaskInspectorMotionForAllocationProbe(view);
        var grid = FindNamedElement<DataGrid>(view, "TaskGrid");
        if (grid.Items.Count != data.Tasks.Count || data.SelectedTask == null)
            throw new InvalidOperationException("Task DPI fixture has no selected synthetic task rows.");
        grid.SelectedItem = data.SelectedTask;
        grid.ScrollIntoView(data.SelectedTask);
        view.ApplyResponsiveLayout(width, height);
        window.UpdateLayout();
        var frame = FindNamedElement<FrameworkElement>(view, "TaskDetailInspectorFrame");
        if (frame.Visibility != Visibility.Visible)
        {
            var openButton = FindNamedElement<Button>(view, "TaskCompactDetailsButton");
            if (openButton.Visibility != Visibility.Visible)
                throw new InvalidOperationException("Task inspector is collapsed and the synthetic open-details control is unavailable.");
            openButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            window.UpdateLayout();
        }
        if (frame.Visibility != Visibility.Visible)
            throw new InvalidOperationException("Task inspector did not open for the selected synthetic task.");
    }

    private static void PreparePolicyCheckbox(SaveCenterView view, Window window)
    {
        var tabs = FindVisualChildren<TabControl>(view).FirstOrDefault()
            ?? throw new InvalidOperationException("Save page has no top-level TabControl.");
        if (tabs.Items.Count <= 2)
            throw new InvalidOperationException($"Save page expected policy tab index 2, found {tabs.Items.Count} tabs.");
        tabs.SelectedIndex = 2;
        window.UpdateLayout();
        var expander = FindVisualChildren<Expander>(view).FirstOrDefault(candidate => candidate.Header?.ToString() == "批量应用到游戏")
            ?? throw new InvalidOperationException("Policy-template batch expander was not found.");
        expander.IsExpanded = true;
        window.UpdateLayout();
        var targets = FindVisualChildren<ItemsControl>(expander)
            .FirstOrDefault(candidate => BindingOperations.GetBinding(candidate, ItemsControl.ItemsSourceProperty)?.Path?.Path == "PolicyTemplateBatchTargetsView")
            ?? throw new InvalidOperationException("Policy-template batch target ItemsControl was not found.");
        if (targets.Items.Count < 2)
            throw new InvalidOperationException($"DPI fixture expected at least two policy targets, found {targets.Items.Count}.");
        var scrollViewer = FindVisualChildren<ScrollViewer>(expander).FirstOrDefault();
        scrollViewer?.ScrollToTop();
        window.UpdateLayout();
    }

    private static FrameworkElement FindPolicyCheckboxRow(UserControl view)
    {
        var checkbox = FindVisualChildren<CheckBox>(view)
            .FirstOrDefault(candidate => AutomationProperties.GetName(candidate) == "Baldur's Gate 3")
            ?? throw new InvalidOperationException("Synthetic policy checkbox for Baldur's Gate 3 was not realized.");
        if (checkbox.IsChecked != false)
            throw new InvalidOperationException("DPI fixture unexpectedly selected a policy target; checkbox state must remain excluded.");
        var binding = BindingOperations.GetBinding(checkbox, ToggleButton.IsCheckedProperty);
        if (binding?.Path?.Path != "IsSelected")
            throw new InvalidOperationException($"Policy checkbox lost its IsSelected binding (path={binding?.Path?.Path ?? "none"}).");
        return (FrameworkElement?)FindVisualAncestor<Grid>(checkbox) ?? checkbox;
    }

    private static void PrepareTaskStatusPopup(TaskCenterView view, Window window)
    {
        var combo = FindNamedElement<ComboBox>(view, "TaskStatusFilterComboBox");
        if (combo.SelectedItem?.ToString() != "全部")
            throw new InvalidOperationException($"Task status ComboBox did not start with the stable fixture filter (selected={combo.SelectedItem}).");
        combo.IsDropDownOpen = true;
        window.UpdateLayout();
        window.Dispatcher.Invoke(new Action(window.UpdateLayout));
        var popup = combo.Template.FindName("PART_Popup", combo) as Popup
            ?? FindVisualChildren<Popup>(combo).FirstOrDefault();
        if (popup == null || !popup.IsOpen)
            throw new InvalidOperationException("Task status filter popup failed to open in the controlled WPF window.");
    }

    private static FrameworkElement? FindTaskStatusPopup(TaskCenterView view)
    {
        var combo = FindNamedElement<ComboBox>(view, "TaskStatusFilterComboBox");
        var popup = combo.Template.FindName("PART_Popup", combo) as Popup
            ?? FindVisualChildren<Popup>(combo).FirstOrDefault();
        return popup?.Child as FrameworkElement;
    }

    private static T FindNamedElement<T>(DependencyObject root, string name) where T : FrameworkElement
        => FindVisualChildren<T>(root).FirstOrDefault(candidate => candidate.Name == name)
            ?? throw new InvalidOperationException($"Could not find {typeof(T).Name} named {name}.");

    private static string GetRelevantBindingPath(string caseName, FrameworkElement target)
    {
        if (caseName == "overview-cta" && target is ButtonBase button)
            return BindingOperations.GetBinding(button, ButtonBase.CommandProperty)?.Path?.Path ?? "none";
        if (caseName == "save-policy-checkbox" && target is Grid row)
        {
            var checkbox = FindVisualChildren<CheckBox>(row).FirstOrDefault();
            return checkbox == null
                ? "checkbox-not-found"
                : BindingOperations.GetBinding(checkbox, ToggleButton.IsCheckedProperty)?.Path?.Path ?? "none";
        }
        if (caseName == "task-status-popup" && target is ComboBox combo)
            return BindingOperations.GetBinding(combo, ComboBox.SelectedItemProperty)?.Path?.Path ?? "none";
        if (caseName == "task-details")
        {
            var card = FindVisualChildren<FrameworkElement>(target).FirstOrDefault(candidate => candidate.Name == "TaskDetailCard");
            return card == null ? "selected-task-binding-not-found" : "SelectedTask.* (inspector card)";
        }
        return "none";
    }

    private static string GetTargetState(string caseName, FrameworkElement target, FakeDashboardData data)
    {
        if (caseName == "task-details")
            return $"selectedTask={data.SelectedTask?.TaskId ?? "none"}; detailFrame={target.Visibility}";
        if (caseName == "overview-cta" && target is ButtonBase button)
            return $"enabled={button.IsEnabled}; commandBound={BindingOperations.GetBinding(button, ButtonBase.CommandProperty)?.Path?.Path ?? "none"}";
        if (caseName == "save-policy-checkbox")
        {
            var checkbox = FindVisualChildren<CheckBox>(target).FirstOrDefault();
            return checkbox == null
                ? "checkbox=unavailable"
                : $"automationName={AutomationProperties.GetName(checkbox)}; isChecked={checkbox.IsChecked}; binding={BindingOperations.GetBinding(checkbox, ToggleButton.IsCheckedProperty)?.Path?.Path ?? "none"}; selectedCount={data.PolicyTemplateBatchSelectedCount}";
        }
        if (caseName == "task-status-popup" && target is ComboBox combo)
            return $"selected={combo.SelectedItem}; isDropDownOpen={combo.IsDropDownOpen}; command=none";
        return "unreported";
    }

    private static bool uiThreadIsPmv2ForCurrentThread()
        => AreDpiAwarenessContextsEqual(GetThreadDpiAwarenessContext(), PerMonitorAwareV2Context);

    private static void PumpWpfVisualState(Window window)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background, window.Dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(650)
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
        window.UpdateLayout();
    }

    private static string FormatRect(Win32Rect rect)
        => string.Join(",", rect.Left.ToString(CultureInfo.InvariantCulture), rect.Top.ToString(CultureInfo.InvariantCulture), rect.Right.ToString(CultureInfo.InvariantCulture), rect.Bottom.ToString(CultureInfo.InvariantCulture));

    private static string EscapeDpiTsv(string? value)
        => (value ?? string.Empty).Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n");

    private static string SanitizeFileName(string value)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
            value = value.Replace(invalid, '_');
        return value.Replace(' ', '-').Replace('/', '-').Replace('\\', '-');
    }

    private static void SaveDpiPng(Visual visual, string path, double dpi)
    {
        var element = visual as FrameworkElement;
        var width = element?.ActualWidth ?? 0;
        var height = element?.ActualHeight ?? 0;
        if (width <= 0 || height <= 0)
            throw new InvalidOperationException($"Cannot render {path}: empty size {width}x{height} DIP.");
        var bitmapWidth = (int)Math.Ceiling(width * dpi / 96d);
        var bitmapHeight = (int)Math.Ceiling(height * dpi / 96d);
        var bitmap = new RenderTargetBitmap(bitmapWidth, bitmapHeight, dpi, dpi, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using (var stream = File.Create(path))
            encoder.Save(stream);
        encoder.Frames.Clear();
    }
}
