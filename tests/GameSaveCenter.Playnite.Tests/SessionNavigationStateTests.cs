using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class SessionNavigationStateTests
    {
        [Fact]
        public void NavigationRestoreIsSessionOnlyAndStartsAtOverview()
        {
            var root = FindRepositoryRoot();
            var plugin = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "GameSaveCenterPlugin.cs"));
            var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
            var dashboard = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));

            Assert.Contains("public WorkspaceKind? SessionLastWorkspace { get; set; }", plugin);
            Assert.Contains("currentWorkspace = plugin.SessionLastWorkspace ?? WorkspaceKind.Overview;", viewModel);
            Assert.Contains("plugin.SessionLastWorkspace = value;", viewModel);
            Assert.DoesNotContain("Enum.TryParse(plugin.Settings.LastWorkspace", viewModel);
            Assert.DoesNotContain("if (viewModel.IsOnboardingPending)\n                NavMaintenance.IsChecked = true;", dashboard);
            Assert.Contains("public ICommand OpenMaintenanceCommand { get; }", viewModel);
            Assert.Contains("private void OpenMaintenance()", viewModel);
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
