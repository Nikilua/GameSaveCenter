using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R08PageSwitchBehaviorTests
{
    private readonly ITestOutputHelper output;

    public R08PageSwitchBehaviorTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void CachedProductionPagesPreserveBindingSelectionAndScrollAcrossNavigation()
    {
        Exception? exception = null;
        var samples = new List<NavigationSample>();

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var shell = new AcrylicProductionShellView();
                typeof(AcrylicProductionShellView)
                    .GetMethod("CreatePages", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(shell, null);

                var viewModel = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
                typeof(DashboardViewModel)
                    .GetField("gamePicker", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(viewModel, new GamePickerViewModel());
                typeof(DashboardViewModel)
                    .GetField("navigationHistory", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(viewModel, new WorkspaceNavigationStack());
                typeof(AcrylicProductionShellView)
                    .GetField("viewModel", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(shell, viewModel);

                // Keep page construction real while preventing an uninitialized VM
                // from starting any business subscriptions in the page Loaded hooks.
                foreach (var page in shell.WorkspaceViews)
                    page.DataContext = new object();

                var taskPage = shell.GetWorkspaceView<TaskCenterView>(WorkspaceKind.Tasks)!;
                var taskGrid = (DataGrid)typeof(TaskCenterView)
                    .GetField("TaskGrid", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(taskPage)!;
                var tasks = Enumerable.Range(0, 96)
                    .Select(index => new TaskStatusDto
                    {
                        TaskId = "r08-05-task-" + index,
                        TaskType = "Validation",
                        GameName = "合成游戏 " + index,
                        State = TaskState.Succeeded,
                        Message = "合成任务完成"
                    })
                    .ToList();
                taskGrid.ItemsSource = tasks;

                window = new Window
                {
                    Content = shell,
                    Width = 1366,
                    Height = 900,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };

                shell.NavigateTo(WorkspaceKind.Tasks);
                window.Show();
                FlushLayout(window);
                taskGrid.SelectedItem = tasks[55];
                FlushLayout(window);

                var gridScrollViewer = FindVisualChild<ScrollViewer>(taskGrid);
                if (gridScrollViewer == null || gridScrollViewer.ScrollableHeight <= 0)
                    throw new InvalidOperationException("The synthetic task grid did not expose a finite scroll viewport.");

                var targetOffset = Math.Min(10d, gridScrollViewer.ScrollableHeight);
                gridScrollViewer.ScrollToVerticalOffset(targetOffset);
                FlushLayout(window);
                var offsetBefore = gridScrollViewer.VerticalOffset;
                var selectedBefore = taskGrid.SelectedItem;
                var dataContextBefore = taskPage.DataContext;
                var taskPageReference = taskPage;
                var measureBeforeSwitch = shell.MeasurePassesForAudit;
                var arrangeBeforeSwitch = shell.ArrangePassesForAudit;

                shell.NavigateTo(WorkspaceKind.Media);
                FlushLayout(window);
                var measureAfterMedia = shell.MeasurePassesForAudit;
                var arrangeAfterMedia = shell.ArrangePassesForAudit;

                shell.NavigateTo(WorkspaceKind.Tasks);
                FlushLayout(window);
                var measureAfterTasks = shell.MeasurePassesForAudit;
                var arrangeAfterTasks = shell.ArrangePassesForAudit;
                var offsetAfter = gridScrollViewer.VerticalOffset;

                Assert.Same(taskPageReference, shell.GetWorkspaceView<TaskCenterView>(WorkspaceKind.Tasks));
                Assert.Same(taskPageReference, ((ContentControl)shell.PageHostForAudit).Content);
                Assert.Same(dataContextBefore, taskPage.DataContext);
                Assert.Same(tasks, taskGrid.ItemsSource);
                Assert.Same(selectedBefore, taskGrid.SelectedItem);
                Assert.InRange(Math.Abs(offsetAfter - offsetBefore), 0, 0.1);
                Assert.True(measureAfterMedia > measureBeforeSwitch, "Replacing the active page should cause a measurable host layout pass.");
                Assert.True(arrangeAfterMedia > arrangeBeforeSwitch, "Replacing the active page should cause a measurable host arrange pass.");
                Assert.True(measureAfterTasks > measureAfterMedia, "Returning to the cached page should complete a measurable host layout pass.");
                Assert.True(arrangeAfterTasks > arrangeAfterMedia, "Returning to the cached page should complete a measurable host arrange pass.");

                var measureBeforeSamePage = shell.MeasurePassesForAudit;
                var arrangeBeforeSamePage = shell.ArrangePassesForAudit;
                shell.NavigateTo(WorkspaceKind.Tasks);
                FlushLayout(window);
                Assert.Equal(measureBeforeSamePage, shell.MeasurePassesForAudit);
                Assert.Equal(arrangeBeforeSamePage, shell.ArrangePassesForAudit);

                var pageHost = (FrameworkElement)shell.PageHostForAudit;
                Assert.Equal(1, pageHost.Opacity);
                Assert.Null(pageHost.Effect);
                Assert.True(pageHost.RenderTransform == null || pageHost.RenderTransform == Transform.Identity);

                samples.Add(new NavigationSample(
                    offsetBefore,
                    offsetAfter,
                    measureAfterMedia - measureBeforeSwitch,
                    arrangeAfterMedia - arrangeBeforeSwitch,
                    measureAfterTasks - measureAfterMedia,
                    arrangeAfterTasks - arrangeAfterMedia,
                    taskPageReference == shell.GetWorkspaceView<TaskCenterView>(WorkspaceKind.Tasks),
                    ReferenceEquals(selectedBefore, taskGrid.SelectedItem)));
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
        Assert.Single(samples);
        output.WriteLine(samples[0].ToString());
    }

    [Fact]
    public void ProductionPageHostDoesNotDeclareFullPageBlurOrEntranceAnimation()
    {
        var root = TestRepositoryContext.Root;
        var source = System.IO.File.ReadAllText(System.IO.Path.Combine(
            root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml.cs"));
        var markup = System.IO.File.ReadAllText(System.IO.Path.Combine(
            root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));

        Assert.Contains("var contentChanged = !ReferenceEquals(PageHost.Content, page);", source);
        Assert.Contains("if (contentChanged)", source);
        Assert.Contains("PageHost.Content = page;", source);
        Assert.DoesNotContain("AnimateEntrance(PageHost", source);
        Assert.DoesNotContain("BlurEffect", markup);
        Assert.DoesNotContain("PageHost.Effect", source);
    }

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.UpdateLayout();
    }

    private static T? FindVisualChild<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) return match;
            var descendant = FindVisualChild<T>(child);
            if (descendant != null) return descendant;
        }
        return null;
    }

    private sealed class NavigationSample
    {
        public NavigationSample(
            double offsetBefore,
            double offsetAfter,
            int measureToMedia,
            int arrangeToMedia,
            int measureBack,
            int arrangeBack,
            bool cachedPage,
            bool selectionPreserved)
        {
            OffsetBefore = offsetBefore;
            OffsetAfter = offsetAfter;
            MeasureToMedia = measureToMedia;
            ArrangeToMedia = arrangeToMedia;
            MeasureBack = measureBack;
            ArrangeBack = arrangeBack;
            CachedPage = cachedPage;
            SelectionPreserved = selectionPreserved;
        }

        public double OffsetBefore { get; }
        public double OffsetAfter { get; }
        public int MeasureToMedia { get; }
        public int ArrangeToMedia { get; }
        public int MeasureBack { get; }
        public int ArrangeBack { get; }
        public bool CachedPage { get; }
        public bool SelectionPreserved { get; }

        public override string ToString()
            => $"offset={OffsetBefore:0.###}->{OffsetAfter:0.###}; "
                + $"to-media measure/arrange={MeasureToMedia}/{ArrangeToMedia}; "
                + $"back measure/arrange={MeasureBack}/{ArrangeBack}; "
                + $"cached={CachedPage}; selection={SelectionPreserved}";
    }
}
