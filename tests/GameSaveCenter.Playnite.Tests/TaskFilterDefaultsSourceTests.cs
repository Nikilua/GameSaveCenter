using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class TaskFilterDefaultsSourceTests
    {
        [Fact]
        public void TaskFiltersDefaultToAllAndUseStableIncrementalSync()
        {
            var repositoryRoot = FindRepositoryRoot();
            var viewModelPath = Path.Combine(repositoryRoot, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs");
            var viewModel = File.ReadAllText(viewModelPath);
            Assert.Contains("private string taskStatusFilter = \"全部\";", viewModel);
            Assert.Contains("private string taskGameFilter = \"全部\";", viewModel);
            Assert.Contains("private string taskTypeFilter = \"全部\";", viewModel);
            Assert.Contains("TaskFilterOptionsSync.Sync(TaskGameFilterOptions", viewModel);
            Assert.Contains("TaskFilterOptionsSync.Sync(TaskTypeFilterOptions", viewModel);
            Assert.DoesNotContain("Replace(TaskGameFilterOptions", viewModel);
            Assert.DoesNotContain("taskGameFilter = string.Empty", viewModel);

            var viewDirectory = Path.Combine(repositoryRoot, "src", "GameSaveCenter.Playnite", "Views");
            var taskCode = File.ReadAllText(Path.Combine(viewDirectory, "TaskCenterView.xaml.cs"));
            var taskText = File.ReadAllText(Path.Combine(viewDirectory, "TaskCenterView.xaml"));
            Assert.DoesNotContain("filterRestoreTimer", taskCode);
            Assert.DoesNotContain("OnTaskFilterSelectionChanged", taskCode);
            Assert.DoesNotContain("EnsureTaskFilterDefaults", taskCode);
            Assert.DoesNotContain("StartFilterRestoreTimer", taskCode);
            Assert.DoesNotContain("SelectionChanged=\"OnTaskFilterSelectionChanged\"", taskText);
            Assert.DoesNotContain("SelectedIndex=\"0\"", taskText);
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
                directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        }
    }
}
