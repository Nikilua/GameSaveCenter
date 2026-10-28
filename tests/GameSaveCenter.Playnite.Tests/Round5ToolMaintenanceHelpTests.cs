using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class Round5ToolMaintenanceHelpTests
{
    [Fact]
    public void DisabledTrackingAndDiagnosticCopyExplainTheirActualScope()
    {
        var root = TestRepositoryContext.Root;
        var trainer = XDocument.Load(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));
        var trainerText = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));
        var toggle = trainer.Descendants().Single(e => (string?)e.Attribute("AutomationProperties.Name") == "退出游戏后关闭所选工具");
        Assert.Equal("{Binding SelectedGameTool.CanTrackProcess}", (string?)toggle.Attribute("IsEnabled"));
        Assert.Equal("True", (string?)toggle.Attribute("ToolTipService.ShowOnDisabled"));
        Assert.Contains("脚本和系统默认程序无法安全关闭", (string?)toggle.Attribute("AutomationProperties.HelpText"));
        Assert.Contains("{Binding SourceDisplay, Mode=OneWay}", trainerText);
        Assert.Contains("{Binding FileStateDisplay}", trainerText);
        Assert.Contains("{Binding AutoStartDisplay, Mode=OneWay}", trainerText);
        Assert.Contains("IsBusy=\"{Binding IsLaunchingGameTool}\"", trainerText);
        Assert.Contains("正在启动所选工具…", trainerText);
        Assert.Contains("AutomationProperties.HelpText=\"启动请求正在处理；完成后显示操作结果。\"", trainerText);

        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var launchStart = viewModel.IndexOf("private async Task LaunchSelectedGameToolAsync()", StringComparison.Ordinal);
        var launchEnd = viewModel.IndexOf("private async Task OpenSelectedGameToolDirectoryAsync()", launchStart, StringComparison.Ordinal);
        Assert.True(launchStart >= 0 && launchEnd > launchStart);
        var launchMethod = viewModel.Substring(launchStart, launchEnd - launchStart);
        Assert.Contains("IsLaunchingGameTool = true", launchMethod);
        Assert.Contains("finally", launchMethod);
        Assert.Contains("IsLaunchingGameTool = false", launchMethod);

        var maintenance = XDocument.Load(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var copy = maintenance.Descendants().Single(e => (string?)e.Attribute("AutomationProperties.Name") == "复制诊断");
        Assert.Contains("CopyDiagnosticsCommand", (string?)copy.Attribute("Command"));
        Assert.Contains("不仅限于当前所选问题", (string?)copy.Attribute("AutomationProperties.HelpText"));
        var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        Assert.Contains("CopyTextWithRetryAsync(DiagnosticSummary ?? string.Empty", code, StringComparison.Ordinal);
    }

    [Fact]
    public void TrainerLaunchProgressIsVisibleBusyAndHasAutomationDescription()
    {
        Exception? failure = null;
        var initialStateIsQuiet = false;
        var launchStateIsVisible = false;
        var launchStateIsBusy = false;
        var launchStateHasAutomationDescription = false;
        var returnedToQuiet = false;
        var launchBusyBindingStatus = "unread";
        var launchButtonDataContext = "unread";

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var state = new LaunchProgressFixture();
                var selectedTool = new object();
                state.GameTools.Add(selectedTool);
                state.SelectedGameTool = selectedTool;
                var view = new TrainerCenterView { DataContext = state };
                var toolList = (ListBox)view.FindName("TrainerToolsList")!;
                toolList.ItemsSource = state.GameTools;
                toolList.SelectedItem = selectedTool;
                var launch = (GameSaveCenter.Playnite.Controls.Button)view.FindName("TrainerLaunchGameToolButton")!;
                var status = (TextBlock)view.FindName("TrainerLaunchStatusText")!;
                window = new Window { Content = view, Width = 1180, Height = 760, ShowInTaskbar = false };
                window.Show();
                view.ApplyResponsiveLayout(1180, 760);
                window.UpdateLayout();
                launch.GetBindingExpression(GameSaveCenter.Playnite.Controls.Button.IsBusyProperty)?.UpdateTarget();
                initialStateIsQuiet = !launch.IsBusy && status.Visibility == Visibility.Collapsed;

                state.IsLaunchingGameTool = true;
                window.UpdateLayout();
                var busyBinding = launch.GetBindingExpression(GameSaveCenter.Playnite.Controls.Button.IsBusyProperty);
                busyBinding?.UpdateTarget();
                Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.DataBind);
                launchStateIsVisible = status.Visibility == Visibility.Visible && status.IsVisible
                    && status.Text == "正在启动所选工具…";
                launchStateIsBusy = launch.IsBusy;
                launchBusyBindingStatus = busyBinding?.Status.ToString() ?? "no binding";
                launchButtonDataContext = launch.DataContext?.GetType().Name ?? "null DataContext";
                if (!launchStateIsBusy)
                    throw new InvalidOperationException($"IsBusy={launch.IsBusy}; binding={launchBusyBindingStatus}; source={busyBinding?.ResolvedSource?.GetType().Name}/{busyBinding?.ResolvedSourcePropertyName}; dataContext={launchButtonDataContext}.");
                launchStateHasAutomationDescription = AutomationProperties.GetName(status) == "工具启动状态"
                    && AutomationProperties.GetHelpText(status) == "启动请求正在处理；完成后显示操作结果。";

                state.IsLaunchingGameTool = false;
                window.UpdateLayout();
                launch.GetBindingExpression(GameSaveCenter.Playnite.Controls.Button.IsBusyProperty)?.UpdateTarget();
                Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.DataBind);
                returnedToQuiet = !launch.IsBusy && status.Visibility == Visibility.Collapsed;
            }
            catch (Exception caught)
            {
                failure = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.True(initialStateIsQuiet && launchStateIsVisible && launchStateIsBusy
            && launchStateHasAutomationDescription && returnedToQuiet,
            $"quiet={initialStateIsQuiet}, visible={launchStateIsVisible}, busy={launchStateIsBusy}, automation={launchStateHasAutomationDescription}, reset={returnedToQuiet}, binding={launchBusyBindingStatus}, dataContext={launchButtonDataContext}.");
    }

    [Fact]
    public void MaintenanceSummaryAndCopyActionRemainReachableInShortWindow()
    {
        Exception? failure = null;
        var scrollsToBottom = false;
        var summaryIsReadable = false;
        var summaryRetainsItsOwnOverflow = false;
        var copyActionIsReachable = false;
        var copyCommandExecutionCount = 0;
        var selectedMainTab = string.Empty;
        var selectedDiagnosticsTab = string.Empty;
        var overviewIsVisible = false;
        var actionCardIsVisible = false;
        var summaryCardIsVisible = false;
        var summaryText = string.Empty;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var longSummary = string.Join(Environment.NewLine,
                    Enumerable.Range(0, 48).Select(index => $"诊断行 {index + 1}: retained technical detail remains available."));
                var copyCommand = new CommandProbe();
                var view = new MaintenanceView
                {
                    DataContext = new MaintenanceSummaryFixture(longSummary, copyCommand)
                };
                var mainTabs = (TabControl)view.FindName("MaintenanceTabControl")!;
                var diagnosticsTabs = (TabControl)view.FindName("MaintenanceDiagnosticsSubTabs")!;
                var overviewScroll = (ScrollViewer)view.FindName("MaintenanceDiagnosticsOverviewScrollSurface")!;
                var actionCard = (Border)view.FindName("MaintenanceDiagnosticsActionCard")!;
                var summaryCard = (Border)view.FindName("MaintenanceDiagnosticSummaryGrid")!;
                mainTabs.SelectedIndex = 0;
                diagnosticsTabs.SelectedIndex = 1;

                window = new Window { Content = view, Width = 1160, Height = 520, ShowInTaskbar = false };
                window.Show();
                view.ApplyResponsiveLayout(1160, 520);
                window.UpdateLayout();
                Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                scrollsToBottom = overviewScroll.ScrollableHeight > 0;

                overviewScroll.ScrollToHome();
                window.UpdateLayout();
                selectedMainTab = (mainTabs.SelectedItem as TabItem)?.Header?.ToString() ?? "null";
                selectedDiagnosticsTab = (diagnosticsTabs.SelectedItem as TabItem)?.Header?.ToString() ?? "null";
                overviewIsVisible = overviewScroll.IsVisible;
                actionCardIsVisible = actionCard.IsVisible;
                var copyButton = FindVisualChildren<GameSaveCenter.Playnite.Controls.Button>(actionCard)
                    .Single(button => Equals(button.Content, "复制诊断"));
                copyActionIsReachable = copyButton.IsVisible && copyButton.Command == copyCommand;
                copyButton.Command.Execute(null);
                copyCommandExecutionCount = copyCommand.ExecutionCount;
                if (copyCommandExecutionCount != 1)
                    throw new InvalidOperationException($"Bound command execution count was {copyCommandExecutionCount}.");

                overviewScroll.ScrollToEnd();
                window.UpdateLayout();
                Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                var summaryBox = FindVisualChildren<TextBox>(summaryCard).Single();
                summaryCardIsVisible = summaryCard.IsVisible;
                summaryText = summaryBox.Text;
                summaryIsReadable = summaryCard.IsVisible && summaryBox.IsVisible && summaryBox.Text == longSummary;
                var summaryScroller = FindVisualChildren<ScrollViewer>(summaryBox).Single();
                summaryRetainsItsOwnOverflow = summaryBox.VerticalScrollBarVisibility == ScrollBarVisibility.Auto
                    && summaryBox.HorizontalScrollBarVisibility == ScrollBarVisibility.Auto
                    && summaryScroller.ScrollableHeight > 0;
            }
            catch (Exception caught)
            {
                failure = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.True(scrollsToBottom && copyActionIsReachable && copyCommandExecutionCount == 1
            && summaryIsReadable && summaryRetainsItsOwnOverflow,
            $"scrolls={scrollsToBottom}, copyVisible={copyActionIsReachable}, copies={copyCommandExecutionCount}, summary={summaryIsReadable}, summaryOverflow={summaryRetainsItsOwnOverflow}, mainTab={selectedMainTab}, diagnosticsTab={selectedDiagnosticsTab}, overviewVisible={overviewIsVisible}, actionCardVisible={actionCardIsVisible}, summaryCardVisible={summaryCardIsVisible}, summaryLength={summaryText.Length}.");
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent is T match)
            yield return match;

        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            foreach (var child in FindVisualChildren<T>(System.Windows.Media.VisualTreeHelper.GetChild(parent, index)))
                yield return child;
        }
    }

    private sealed class LaunchProgressFixture : INotifyPropertyChanged
    {
        private bool isLaunchingGameTool;
        public ObservableCollection<object> GameTools { get; } = new ObservableCollection<object>();
        public object? SelectedGameTool { get; set; }

        public bool IsLaunchingGameTool
        {
            get => isLaunchingGameTool;
            set
            {
                if (isLaunchingGameTool == value) return;
                isLaunchingGameTool = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLaunchingGameTool)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class MaintenanceSummaryFixture
    {
        public MaintenanceSummaryFixture(string diagnosticSummary, ICommand copyDiagnosticsCommand)
        {
            DiagnosticSummary = diagnosticSummary;
            CopyDiagnosticsCommand = copyDiagnosticsCommand;
        }

        public string DiagnosticSummary { get; }
        public ICommand CopyDiagnosticsCommand { get; }
        public int MaintenanceTabIndex { get; set; }
    }

    private sealed class CommandProbe : ICommand
    {
        public int ExecutionCount { get; private set; }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => ExecutionCount++;
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }
}
