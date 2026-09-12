using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class TaskCenterViewResponsiveTests
    {
        [Fact]
        public void ShortTaskWindowTightensSummaryWithoutDroppingTableFloor()
        {
            Exception? exception = null;
            double shortSummaryMinHeight = 0;
            double regularSummaryMinHeight = 0;
            double shortSummaryTopPadding = 0;
            double regularSummaryTopPadding = 0;
            double taskGridMinHeight = 0;

            var thread = new Thread(() =>
            {
                try
                {
                    var view = new TaskCenterView();
                    var viewType = typeof(TaskCenterView);
                    var summary = (Border)viewType.GetField("TaskSummaryPanel", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                    var grid = (DataGrid)viewType.GetField("TaskGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;

                    view.ApplyResponsiveLayout(744, 680);
                    shortSummaryMinHeight = summary.MinHeight;
                    shortSummaryTopPadding = summary.Padding.Top;
                    taskGridMinHeight = grid.MinHeight;

                    view.ApplyResponsiveLayout(744, 720);
                    regularSummaryMinHeight = summary.MinHeight;
                    regularSummaryTopPadding = summary.Padding.Top;
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
            Assert.Equal(52, shortSummaryMinHeight);
            Assert.Equal(5, shortSummaryTopPadding);
            Assert.Equal(84, regularSummaryMinHeight);
            Assert.Equal(14, regularSummaryTopPadding);
            Assert.Equal(236, taskGridMinHeight);
        }

        [Fact]
        public void TaskStateSurfaceSeparatesFilterEmptyAndStaleDataRecovery()
        {
            var root = FindRepositoryRoot();
            var view = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
            var state = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.TaskPageState.cs"));
            var dashboard = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

            Assert.Contains("Value=\"FilterEmpty\"", view);
            Assert.Contains("Value=\"Loading\"", view);
            Assert.Contains("Value=\"Error\"", view);
            Assert.Contains("TaskStaleDataBanner", view);
            Assert.Contains("RetryCommand=\"{Binding ClearTaskFiltersCommand}\"", view);
            Assert.Contains("RetryCommand=\"{Binding RefreshCommand}\"", view);
            Assert.Contains("TaskPageLastUpdatedDisplay", state);
            Assert.Contains("TaskPageHasItems", state);
            Assert.Contains("TaskPageLoadFailed", state);
            Assert.Contains("TaskPageStatusSummary", state);
            Assert.Contains("TaskHasActiveFilters", dashboard);
            Assert.Contains("TaskCompactCloseDetailsButton", view);
        }

        [Fact]
        public void FailedTaskDetailsPutUserReasonBeforeCollapsedTechnicalDetails()
        {
            var root = FindRepositoryRoot();
            var view = System.IO.File.ReadAllText(System.IO.Path.Combine(
                root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));

            var failure = view.IndexOf("x:Name=\"TaskInspectorErrorCard\"", StringComparison.Ordinal);
            var technical = view.IndexOf("x:Name=\"TaskTechnicalDetailsExpander\"", StringComparison.Ordinal);
            Assert.True(failure >= 0 && technical > failure);
            Assert.Contains("Header=\"技术详情\" IsExpanded=\"False\"", view);
            Assert.Contains("Text=\"{Binding SelectedTask.ErrorMessage, Mode=OneWay}\"", view);
            Assert.Contains("Text=\"{Binding SelectedTask.ErrorCode, Mode=OneWay}\"", view);
        }

        [Fact]
        public void FilterEmptyStateUsesDistinctNoResultsLabel()
        {
            var root = FindRepositoryRoot();
            var redesign = System.IO.File.ReadAllText(System.IO.Path.Combine(
                root, "src", "GameSaveCenter.Playnite", "Themes", "Redesign.xaml"));

            Assert.Contains("<Trigger Property=\"State\" Value=\"FilterEmpty\">", redesign);
            Assert.Contains("Property=\"Text\" Value=\"无结果\"", redesign);
        }

        [Fact]
        public void CopyTaskDiagnosticIsEnabledForAnyAvailableFailureField()
        {
            var root = FindRepositoryRoot();
            var dashboard = System.IO.File.ReadAllText(System.IO.Path.Combine(
                root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

            Assert.Contains("CopyTaskErrorCommand = new RelayCommand(", dashboard);
            Assert.Contains("!string.IsNullOrWhiteSpace(SelectedTask.ErrorMessage)", dashboard);
            Assert.Contains("!string.IsNullOrWhiteSpace(SelectedTask.ErrorCode)", dashboard);
            Assert.Contains("!string.IsNullOrWhiteSpace(SelectedTask.DetailMessage)", dashboard);
        }

        [Fact]
        public void CompactTaskFiltersKeepCommonControlsInOneRowAndMoveSecondaryFiltersIntoDisclosure()
        {
            Exception? exception = null;
            var compactMainHasType = true;
            var compactMainHasScope = true;
            var compactMainHasRange = true;
            var compactMoreHasType = false;
            var compactMoreHasScope = false;
            var compactMoreHasRange = false;
            var compactSearchSpan = 0;
            var wideMainHasType = false;
            var wideMainHasScope = false;
            var wideMainHasRange = false;

            var thread = new Thread(() =>
            {
                try
                {
                    var view = new TaskCenterView();
                    var viewType = typeof(TaskCenterView);
                    var filters = (Grid)viewType.GetField("TaskFiltersPanel", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                    var more = (StackPanel)viewType.GetField("TaskMoreFiltersHost", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                    var typeLabel = (TextBlock)viewType.GetField("TaskTypeFilterLabel", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                    var typeCombo = (ComboBox)viewType.GetField("TaskTypeFilterComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                    var scopeCombo = (ComboBox)viewType.GetField("TaskHistoryScopeComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                    var rangeCombo = (ComboBox)viewType.GetField("TaskHistoryRangeComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                    var search = (Grid)viewType.GetField("TaskSearchBoxHost", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;

                    view.ApplyResponsiveLayout(700, 640);
                    compactMainHasType = filters.Children.Contains(typeLabel) || filters.Children.Contains(typeCombo);
                    compactMainHasScope = filters.Children.Contains(scopeCombo);
                    compactMainHasRange = filters.Children.Contains(rangeCombo);
                    compactMoreHasType = more.Children.Contains(typeLabel) && more.Children.Contains(typeCombo);
                    compactMoreHasScope = more.Children.Contains(scopeCombo);
                    compactMoreHasRange = more.Children.Contains(rangeCombo);
                    compactSearchSpan = Grid.GetColumnSpan(search);

                    view.ApplyResponsiveLayout(1280, 720);
                    wideMainHasType = filters.Children.Contains(typeLabel) && filters.Children.Contains(typeCombo);
                    wideMainHasScope = filters.Children.Contains(scopeCombo);
                    wideMainHasRange = filters.Children.Contains(rangeCombo);
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
            Assert.False(compactMainHasType);
            Assert.False(compactMainHasScope);
            Assert.False(compactMainHasRange);
            Assert.True(compactMoreHasType);
            Assert.True(compactMoreHasScope);
            Assert.True(compactMoreHasRange);
            Assert.Equal(4, compactSearchSpan);
            Assert.True(wideMainHasType);
            Assert.True(wideMainHasScope);
            Assert.True(wideMainHasRange);
        }

        [Fact]
        public void CompactTaskDetailsKeepsReadableTableFloor()
        {
            Exception? exception = null;
            var compactTableMinHeight = 0d;
            var compactInspectorMaxHeight = 0d;
            var queueDetailsVisibility = Visibility.Visible;
            var inspectorCloseVisibility = Visibility.Collapsed;

            var thread = new Thread(() =>
            {
                try
                {
                    var view = new TaskCenterView();
                    var viewType = typeof(TaskCenterView);
                    var grid = (DataGrid)viewType.GetField("TaskGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                    var button = (Button)viewType.GetField("TaskCompactDetailsButton", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                    var closeButton = (Button)viewType.GetField("TaskCompactCloseDetailsButton", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                    var selected = new object();
                    grid.ItemsSource = new[] { selected };
                    grid.SelectedItem = selected;

                    view.ApplyResponsiveLayout(715, 577);
                    viewType.GetMethod("OnTaskCompactDetailsClick", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .Invoke(view, new object[] { button, new RoutedEventArgs() });
                    compactTableMinHeight = grid.MinHeight;
                    compactInspectorMaxHeight = view.TaskDetailScrollViewerElement.MaxHeight;
                    queueDetailsVisibility = button.Visibility;
                    inspectorCloseVisibility = closeButton.Visibility;
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
            Assert.Equal(180, compactTableMinHeight);
            Assert.Equal(160, compactInspectorMaxHeight);
            Assert.Equal(Visibility.Collapsed, queueDetailsVisibility);
            Assert.Equal(Visibility.Visible, inspectorCloseVisibility);
        }

        private static string FindRepositoryRoot()
        {
            var directory = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !System.IO.File.Exists(System.IO.Path.Combine(directory.FullName, "GameSaveCenter.sln")))
                directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        }
    }
}
