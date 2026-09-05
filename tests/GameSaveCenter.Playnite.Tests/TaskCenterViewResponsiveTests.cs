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

                    view.ApplyResponsiveLayout(744, 700);
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
            Assert.Equal(64, shortSummaryMinHeight);
            Assert.Equal(8, shortSummaryTopPadding);
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
