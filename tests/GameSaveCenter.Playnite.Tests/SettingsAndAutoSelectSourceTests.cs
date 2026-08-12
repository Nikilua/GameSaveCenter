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
            Assert.Contains("tab.MinHeight = compact ? 44 : 72;", code);
            Assert.Contains("SettingsSectionTabs.TabStripPlacement = compact ? Dock.Top : Dock.Left;", code);
            Assert.Contains("SettingsSectionTabs.SelectionChanged += OnSettingsTabSelectionChanged;", code);
            Assert.Contains("selected.BringIntoView()", code);
            Assert.Contains("tab.Width = compact ? double.NaN : 232;", code);
            Assert.Contains("x:Name=\"SettingsScroller\"", redesign);
            Assert.DoesNotContain("x:Name=\"SettingsScroller\"", xaml);
            Assert.Contains("x:Name=\"SettingsHeaderScroller\"", redesign);
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

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
                directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        }
    }
}
