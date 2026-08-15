using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class SettingsAndAutoSelectSourceTests
    {
        [Fact]
        public void SettingsHeaderIsNotClippedAndFiveCategoriesRemain()
        {
            var root = FindRepositoryRoot();
            var xaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
            var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml.cs"));
            var redesign = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "Redesign.xaml"));

            Assert.Contains("x:Name=\"SettingsHeader\" Style=\"{DynamicResource GscRedesignWorkspaceHeroCard}\" ClipToBounds=\"False\"", xaml);
            Assert.Contains("常规与目录", xaml);
            Assert.Contains("备份与恢复", xaml);
            Assert.Contains("外观与可访问性", xaml);
            Assert.Contains("自动化与媒体", xaml);
            Assert.Contains("RecentProtectionWindowDays", xaml);
            Assert.Contains("最近 7 天", xaml);
            Assert.Contains("最近 30 天", xaml);
            Assert.Contains("最近 90 天", xaml);
            Assert.Contains("设置迁移", xaml);
            Assert.Contains("tab.MinHeight = compact ? 50 : shortHeight ? 60 : 72;", code);
            Assert.Contains("SettingsSectionTabs.TabStripPlacement = compact ? Dock.Top : Dock.Left;", code);
            Assert.Contains("SettingsSectionTabs.SelectionChanged += OnSettingsTabSelectionChanged;", code);
            Assert.Contains("selected.BringIntoView()", code);
            Assert.Contains("tab.Width = compact ? double.NaN : 232;", code);
            Assert.Contains("x:Name=\"SettingsScroller\"", redesign);
            Assert.DoesNotContain("x:Name=\"SettingsScroller\"", xaml);
            Assert.Contains("x:Name=\"SettingsHeaderScroller\"", redesign);
            Assert.Contains("x:Name=\"SettingsHeaderItemsHost\"", redesign);
            Assert.Contains("x:Name=\"SettingsHeaderBottomSafetyZone\"", redesign);
            Assert.Contains("HorizontalScrollBarVisibility=\"Auto\"", redesign);
            Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", redesign);
        }

        [Fact]
        public void AutoSelectUsesEventsAndResolverWithoutPolling()
        {
            var root = FindRepositoryRoot();
            var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
            var plugin = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "GameSaveCenterPlugin.cs"));
            var resolver = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "GameSelectionResolver.cs"));
            var iconProvider = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "PlayniteGameIconProvider.cs"));
            var dashboard = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));
            var overview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));
            var saves = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
            var trainers = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));
            var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));

            Assert.Contains("internal event Action<Guid>? PlayniteGameStarted;", plugin);
            Assert.Contains("plugin.PlayniteGameStarted += OnPlayniteGameStarted;", viewModel);
            Assert.Contains("plugin.PlayniteGameStarted -= OnPlayniteGameStarted;", viewModel);
            Assert.Contains("GameSelectionResolver.ResolveInitial", viewModel);
            Assert.DoesNotContain("DispatcherTimer", resolver);
            Assert.DoesNotContain("Process.GetProcesses", resolver);
            Assert.DoesNotContain("HttpClient", iconProvider);
            Assert.DoesNotContain("WebClient", iconProvider);
            Assert.DoesNotContain("WebRequest", iconProvider);
            Assert.Contains("SelectedGameIcon", dashboard);
            Assert.Contains("SelectedGameIcon", overview);
            Assert.Contains("SelectedGameIcon", saves);
            Assert.Contains("SelectedGameIcon", trainers);
            Assert.Contains("SelectedGameIcon", media);
            Assert.Contains("SelectedGameHiddenByFilter", dashboard);
            var pickerStart = dashboard.IndexOf("ItemsSource=\"{Binding GamePicker.ItemsView}\"", StringComparison.Ordinal);
            var pickerEnd = dashboard.IndexOf("</ListBox>", pickerStart, StringComparison.Ordinal);
            Assert.True(pickerStart >= 0 && pickerEnd > pickerStart);
            Assert.DoesNotContain("SelectedGameIcon", dashboard.Substring(pickerStart, pickerEnd - pickerStart));
        }

        [Fact]
        public void OnboardingUsesMaintenanceWorkspaceAndExplicitCommands()
        {
            var root = FindRepositoryRoot();
            var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
            var dashboardCode = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));
            var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
            var overview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));
            var contracts = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "MessageTypes.cs"));

            Assert.Contains("currentWorkspace = plugin.SessionLastWorkspace ?? WorkspaceKind.Overview;", viewModel);
            Assert.Contains("MessageTypes.CheckEnvironment", viewModel);
            Assert.Contains("OnboardingTestBackupCommand", viewModel);
            Assert.Contains("OnboardingCompleted", viewModel);
            Assert.DoesNotContain("if (viewModel.IsOnboardingPending)\n                NavMaintenance.IsChecked = true;", dashboardCode);
            Assert.Contains("OpenMaintenanceCommand", viewModel);
            Assert.Contains("首次环境检查尚未完成", overview);
            Assert.Contains("EnvironmentCheckCard", maintenance);
            Assert.Contains("Command=\"{Binding RunEnvironmentCheckCommand}\"", maintenance);
            Assert.Contains("Command=\"{Binding OnboardingTestBackupCommand}\"", maintenance);
            Assert.Contains("CheckEnvironment", contracts);
        }

        [Fact]
        public void OnboardingTestBackupReusesProductionBackupPipeline()
        {
            var root = FindRepositoryRoot();
            var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
            var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

            Assert.Contains("OnboardingTestBackupCommand = new RelayCommand(_ => Run(BackupSelectedAsync)", viewModel);
            Assert.Contains("private async Task BackupSelectedAsync()", viewModel);
            Assert.Contains("MessageTypes.BackupGame", viewModel);
            Assert.Contains("Reason = \"Manual\"", viewModel);
            Assert.DoesNotContain("TestBackupService", viewModel);
            Assert.Contains("Command=\"{Binding OnboardingTestBackupCommand}\"", maintenance);
            Assert.Contains("若当前没有可用于测试的已识别存档游戏", maintenance);
        }

        [Fact]
        public void SafeModeNextStartRequestAndRecoveryButtonAreExposed()
        {
            var root = FindRepositoryRoot();
            var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
            var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
            var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

            Assert.Contains("SafeModeRequested", settings);
            Assert.Contains("下次以安全模式启动", settings);
            Assert.Contains("ExitSafeModeCommand", maintenance);
            Assert.Contains("ExitSafeModeCommand = new RelayCommand(_ => Run(ExitSafeModeAsync)", viewModel);
            Assert.Contains("SafeModeRequested && !safeModePromptShown", viewModel);
            Assert.Contains("连续启动失败", viewModel);
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
