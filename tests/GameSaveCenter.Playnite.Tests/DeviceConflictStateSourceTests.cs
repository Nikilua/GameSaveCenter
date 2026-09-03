using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class DeviceConflictStateSourceTests
{
    [Fact]
    public void ConflictAndMediaSelectionsUseViewModelStateWithoutStaticIndexOverwrite()
    {
        var root = FindRepositoryRoot();
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));

        Assert.DoesNotContain("SelectedIndex=\"0\" ItemsSource=\"{Binding DeviceDecisionOptions}", maintenance);
        Assert.Contains("ItemsSource=\"{Binding DeviceDecisionOptions}\" SelectedItem=\"{Binding DeviceDecision, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, TargetNullValue=稍后处理, FallbackValue=稍后处理}\"", maintenance);
        Assert.Contains("Text=\"{Binding StagedRemoteBackupStatus, Mode=OneWay}\"", maintenance);
        Assert.Contains("AutomationProperties.Name=\"远端备份隔离状态\"", maintenance);

        Assert.DoesNotContain("SelectedIndex=\"0\" ItemsSource=\"{Binding MediaFilterOptions}", media);
        Assert.Contains("ItemsSource=\"{Binding MediaFilterOptions}\" SelectedItem=\"{Binding MediaFilter, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, TargetNullValue=全部, FallbackValue=全部}\"", media);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
