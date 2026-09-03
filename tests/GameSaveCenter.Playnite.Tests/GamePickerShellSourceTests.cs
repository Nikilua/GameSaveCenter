using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class GamePickerShellSourceTests
    {
        [Fact]
        public void ProductionPickerUsesRoundedSharedItemChromeAndRestoresFilterDefaults()
        {
            var root = FindRepositoryRoot();
            var xaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));
            var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml.cs"));

            Assert.Contains("BasedOn=\"{StaticResource {x:Type ListBoxItem}}\"", xaml);
            Assert.Contains("SelectedIndex=\"0\"", xaml);
            Assert.Contains("TargetNullValue=全部", xaml);
            Assert.Contains("UiFilterSelection.Synchronize(GamePickerStatusComboBox", code);
            Assert.Contains("UiFilterSelection.Synchronize(GamePickerPlatformComboBox", code);
            Assert.Contains("UiFilterSelection.Synchronize(GamePickerSortComboBox", code);
            Assert.Contains("PlatformFilterOptions.CollectionChanged += OnGamePickerPlatformOptionsChanged", code);
            Assert.Contains("DispatcherPriority.Loaded", code);
            Assert.Contains("pickerFilterRestorePending", code);
            Assert.Contains("Loaded=\"OnGamePickerFilterLoaded\"", xaml);
        }

        [Fact]
        public void GamePickerStatusAndSortSelectionsComeFromSharedViewModel()
        {
            var root = FindRepositoryRoot();
            var production = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));
            var dashboard = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));

            Assert.DoesNotContain("ItemsSource=\"{Binding GamePicker.StatusFilterOptions}\" SelectedIndex=\"0\"", production);
            Assert.DoesNotContain("ItemsSource=\"{Binding GamePicker.SortOptions}\" SelectedIndex=\"0\"", production);
            Assert.DoesNotContain("GamePickerStatusComboBox\" Style=\"{StaticResource GscWpfUiFilterComboBox}\" SelectedIndex=\"0\"", dashboard);
            Assert.DoesNotContain("GamePickerSortComboBox\" Style=\"{StaticResource GscWpfUiFilterComboBox}\" SelectedIndex=\"0\"", dashboard);
        }

        [Fact]
        public void DashboardPickerClipsItsRoundedSelectionSurface()
        {
            var root = FindRepositoryRoot();
            var xaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));

            Assert.Contains("CornerRadius=\"11\" Padding=\"11,9\" ClipToBounds=\"True\"", xaml);
        }

        [Fact]
        public void RefreshAndResizePathsCoalesceExpensiveVisualWork()
        {
            var root = FindRepositoryRoot();
            var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
            var dashboardCode = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));
            var shellCode = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml.cs"));
            var backgroundProvider = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "PlayniteGameBackgroundProvider.cs"));

            Assert.Contains("EnsureSelectedGameBackgroundLoaded();", dashboardCode);
            Assert.Contains("var selectedGameChanged", viewModel);
            Assert.Contains("if (selectedGameChanged)", viewModel);
            Assert.Contains("new Int32Rect(px, py, 1, 1)", backgroundProvider);
            Assert.DoesNotContain("var pixels = new byte[stride * height]", backgroundProvider);
            Assert.Contains("private bool responsiveLayoutPending;", shellCode);
            Assert.Contains("DispatcherPriority.Render", shellCode);
            Assert.Contains("if (!IsLoaded) return;", shellCode);
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
                directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("无法定位仓库根目录");
        }
    }
}
