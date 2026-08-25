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
            Assert.Contains("UiFilterSelection.RestoreDefault(GamePickerPlatformComboBox", code);
            Assert.Contains("PlatformFilterOptions.CollectionChanged += OnGamePickerPlatformOptionsChanged", code);
            Assert.Contains("DispatcherPriority.DataBind", code);
            Assert.Contains("DispatcherPriority.Loaded", code);
            Assert.Contains("Loaded=\"OnGamePickerFilterLoaded\"", xaml);
        }

        [Fact]
        public void DashboardPickerClipsItsRoundedSelectionSurface()
        {
            var root = FindRepositoryRoot();
            var xaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));

            Assert.Contains("CornerRadius=\"11\" Padding=\"11,9\" ClipToBounds=\"True\"", xaml);
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
