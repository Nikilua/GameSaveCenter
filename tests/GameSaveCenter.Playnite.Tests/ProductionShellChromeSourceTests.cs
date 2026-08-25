using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class ProductionShellChromeSourceTests
{
    [Fact]
    public void ProductionFooterOwnsWorkerStatusAndSpansTheShell()
    {
        var shell = ReadSource("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml");

        Assert.Contains("x:Name=\"FooterSurface\" Grid.Row=\"1\" Grid.Column=\"0\" Grid.ColumnSpan=\"2\"", shell);
        Assert.Contains("Grid.Column=\"1\" x:Name=\"FooterStatusPanel\"", shell);
        Assert.Contains("{Binding Snapshot.WorkerHealthy}", shell);
        Assert.Contains("{Binding Snapshot.LudusaviAvailable}", shell);
        Assert.DoesNotContain("Text=\"生产版 · 真实数据由 Worker 提供\"", shell);
        Assert.DoesNotContain("Grid.Column=\"2\" Text=\"GameSaveCenter\"", shell);
        Assert.DoesNotContain("GscRedesignStatusCard", shell);
    }

    [Fact]
    public void ProductionSidebarCollapseIsASeparateSmallChromeAction()
    {
        var shell = ReadSource("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml");
        var shellCode = ReadSource("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml.cs");

        Assert.Contains("x:Name=\"SidebarColumn\" Width=\"236\"", shell);
        Assert.Contains("x:Name=\"SidebarContentLayer\"", shell);
        Assert.Contains("AcrylicSidebarBookmarkButton", shell);
        Assert.Contains("x:Name=\"SidebarCollapseButton\"", shell);
        Assert.Contains("VerticalAlignment=\"Bottom\"", shell);
        Assert.Contains("Margin=\"0,0,-1,16\"", shell);
        Assert.Contains("Click=\"OnSidebarCollapseClick\"", shell);
        Assert.Contains("AutomationProperties.Name=\"收起导航栏\"", shell);
        Assert.Contains("x:Name=\"SidebarProductionVersionText\"", shell);
        Assert.Contains("sidebarCollapsed = !sidebarCollapsed", shellCode);
        Assert.Contains("sidebarTransitionRunning", shellCode);
        Assert.Contains("DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(110))", shellCode);
        Assert.Contains("MotionEnabledProvider", shellCode);
        Assert.Contains("new GridLength(sidebarCollapsed ? 78 : 236)", shellCode);
        Assert.Contains("typeof(AcrylicProductionShellView).Assembly.GetName().Version", shellCode);
        Assert.Contains("展开导航栏", shellCode);
        Assert.Contains("ApplyPageLayout();", shellCode);
    }

    [Fact]
    public void SettingsViewRequestsAUsableDefaultWindowSize()
    {
        var settings = ReadSource("src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml");

        Assert.Contains("MinWidth=\"1180\" MinHeight=\"760\"", settings);
    }

    private static string ReadSource(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        if (directory == null)
            throw new InvalidOperationException("无法定位仓库根目录");
        return File.ReadAllText(Path.Combine(new[] { directory.FullName }.Concat(segments).ToArray()));
    }
}
