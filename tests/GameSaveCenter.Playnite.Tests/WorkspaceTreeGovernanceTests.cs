using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WorkspaceTreeGovernanceTests
{
    [Fact]
    public void ProductionPageHostIsTheOnlyVisibleWorkspaceHost()
    {
        var dashboard = Read("src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml");
        var shell = Read("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml");
        var shellCode = Read("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml.cs");

        var legacyStart = dashboard.IndexOf("x:Name=\"DashboardDemoShell\"", StringComparison.Ordinal);
        Assert.True(legacyStart >= 0);
        Assert.Contains("Visibility=\"Collapsed\"", dashboard.Substring(legacyStart, 500));
        Assert.Contains("x:Name=\"MainShell\"", dashboard);
        Assert.Contains("Opacity=\"0\" Visibility=\"Collapsed\"", dashboard);
        Assert.Contains("x:Name=\"ProductionShellView\"", dashboard);
        Assert.Contains("x:Name=\"PageHost\"", shell);
        Assert.Contains("PageHost.Content = page", shellCode);
        Assert.Contains("public UserControl? GetWorkspaceView(WorkspaceKind workspace)", shellCode);
        Assert.Contains("public T? GetWorkspaceView<T>(WorkspaceKind workspace)", shellCode);
    }

    [Fact]
    public void DashboardCompatibilityCodeUsesProductionWorkspaceRegistry()
    {
        var dashboardCode = Read("src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs");

        Assert.Contains("ProductionShellView.GetWorkspaceView<MaintenanceView>(WorkspaceKind.Maintenance)", dashboardCode);
        Assert.Contains("ProductionShellView.GetWorkspaceView<TaskCenterView>(WorkspaceKind.Tasks)", dashboardCode);
        Assert.Contains("ProductionShellView.GetWorkspaceView<TrainerCenterView>(WorkspaceKind.Trainers)", dashboardCode);
        Assert.Contains("ProductionShellView.GetWorkspaceView<MediaCenterView>(WorkspaceKind.Media)", dashboardCode);
        Assert.Contains("ProductionShellView.ApplyResponsiveLayout(width, height)", dashboardCode);
        Assert.DoesNotContain("MaintenanceWorkspaceView.FindingsGridElement", dashboardCode);
        Assert.DoesNotContain("TaskWorkspaceView.TaskDetailCardElement", dashboardCode);
        Assert.DoesNotContain("TaskWorkspaceView.ApplyResponsiveLayout", dashboardCode);
        Assert.DoesNotContain("SaveWorkspaceView.ApplyResponsiveLayout", dashboardCode);
        Assert.DoesNotContain("TrainerWorkspaceView.ApplyResponsiveLayout", dashboardCode);
        Assert.DoesNotContain("MaintenanceWorkspaceView.ApplyResponsiveLayout", dashboardCode);
    }

    [Fact]
    public void LegacyWorkspaceTreeRemainsExplicitlyCompatibilityOnly()
    {
        var dashboard = Read("src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml");
        var dashboardCode = Read("src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs");

        Assert.Contains("<views:OverviewView x:Name=\"OverviewWorkspaceView\"/>", dashboard);
        Assert.Contains("<views:TaskCenterView x:Name=\"TaskWorkspaceView\"/>", dashboard);
        Assert.Contains("GetLegacyCompatibilityWorkspaceViews", dashboardCode);
        Assert.Contains("The visible production PageHost is the only responsive page coordinator", dashboardCode);
        Assert.Contains("The collapsed Dashboard tree remains as a compatibility surface", dashboardCode);
    }

    private static string Read(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        if (directory == null)
            throw new InvalidOperationException("无法定位仓库根目录");
        return File.ReadAllText(Path.Combine(new[] { directory.FullName }.Concat(segments).ToArray()));
    }
}
