using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class OvernightV4SaveFormTests
    {
        [LegacyProductionUiBaselineFact]
        public void SaveBackupAutomationFormHasLabelsUnitsAndHelpers()
        {
            var root = FindRepositoryRoot();
            var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
            var tokens = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml"));

            Assert.Contains("x:Name=\"SaveBackupAutomationCard\"", save);
            Assert.Contains("Text=\"游玩中周期备份间隔\" Style=\"{StaticResource GscFormFieldLabel}\"", save);
            Assert.Contains("在游戏运行期间，按该间隔自动创建备份", save);
            Assert.Contains("GscNumericFieldInput", save);
            Assert.Contains("AutomationProperties.Name=\"游玩中周期备份间隔，分钟\"", save);
            Assert.Contains("Text=\"最近版本保留小时\"", save);
            Assert.Contains("Text=\"每日保留天数\"", save);
            Assert.Contains("Text=\"每周保留周数\"", save);
            Assert.Contains("Margin=\"24,10,0,0\"", save);
            Assert.Contains("<Setter Property=\"TextAlignment\" Value=\"Center\"/>", tokens);
            Assert.Contains("<Setter Property=\"VerticalContentAlignment\" Value=\"Center\"/>", tokens);
            Assert.Contains("VerticalContentAlignment=\"{TemplateBinding VerticalContentAlignment}\"", tokens);
            Assert.Contains("<Setter Property=\"Padding\" Value=\"8,4\"/>", tokens);
        }

        [Fact]
        public void SharedWorkspaceBreakpointsKeepSearchAndHistoryEssentialsReadable()
        {
            var root = FindRepositoryRoot();
            var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml.cs"));
            var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));

            Assert.Contains("var narrowHistory = width < 1240;", save);
            Assert.Contains("<ColumnDefinition Width=\"Auto\" MinWidth=\"300\"/>", media);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"160\"/>", media);
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
