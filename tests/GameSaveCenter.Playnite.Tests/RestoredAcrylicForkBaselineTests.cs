using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class RestoredAcrylicForkBaselineTests
{
    [Fact]
    public void OverviewUsesTheRestoredDemoSectionsInsteadOfTheDiscardedWorkbench()
    {
        var overview = ReadSource("Views", "OverviewView.xaml");

        Assert.Contains("最近任务", overview);
        Assert.Contains("全局活动", overview);
        Assert.Contains("风险与提醒", overview);
        Assert.Contains("OverviewActivityList", overview);
        Assert.Contains("OverviewActivityTimelineList", overview);
        Assert.DoesNotContain("今日工作台", overview);
        Assert.DoesNotContain("最近活动", overview);
    }

    [Fact]
    public void SaveCenterUsesTheRestoredBackupPolicySections()
    {
        var save = ReadSource("Views", "SaveCenterView.xaml");

        Assert.Contains("备份策略", save);
        Assert.Contains("备份自动化", save);
        Assert.Contains("保留与云端", save);
        Assert.Contains("策略模板", save);
    }

    [Fact]
    public void RestoredPagesKeepTheProductionMediaAndTaskEntryPoints()
    {
        var media = ReadSource("Views", "MediaCenterView.xaml");
        var tasks = ReadSource("Views", "TaskCenterView.xaml");

        Assert.Contains("当前游戏媒体", media);
        Assert.Contains("待归类", media);
        Assert.Contains("任务总数", tasks);
        Assert.Contains("任务队列", tasks);
    }

    [Fact]
    public void MediaUsesDemoMetricCardsAndTheSharedPurpleSegmentedTabs()
    {
        var media = ReadSource("Views", "MediaCenterView.xaml");

        Assert.Contains("x:Name=\"MediaSummaryPanel\" Grid.Row=\"0\" Columns=\"4\"", media);
        Assert.Contains("Style=\"{StaticResource MediaTabControl}\"", media);
        Assert.Contains("Header=\"当前游戏媒体\"", media);
        Assert.Contains("Style=\"{DynamicResource GscRedesignMetricBorder}\"", media);
        Assert.DoesNotContain("x:Key=\"MediaModeStrip\"", media);
        Assert.DoesNotContain("x:Key=\"MediaModeRadio\"", media);
    }

    [Fact]
    public void OverviewAttentionFindingsUseASeparateBoundedProjectScrollSurface()
    {
        var overview = ReadSource("Views", "OverviewView.xaml");

        Assert.Contains("x:Name=\"OverviewAttentionScrollViewer\"", overview);
        Assert.Contains("Style=\"{DynamicResource GscPageScrollViewer}\"", overview);
        Assert.Contains("MaxHeight=\"190\"", overview);
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", overview);
        Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", overview);
        Assert.Contains("ItemsSource=\"{Binding AttentionFindings}\"", overview);
        Assert.Contains("x:Name=\"OverviewProtectionItemsScrollViewer\"", overview);
        Assert.Contains("ItemsSource=\"{Binding RecentProtection.Items}\"", overview);
    }

    [Fact]
    public void OverviewRiskRailUsesABoundedScrollSurfaceWithoutMovingItsActions()
    {
        var overview = ReadSource("Views", "OverviewView.xaml");

        Assert.Contains("x:Name=\"OverviewRiskViewport\"", overview);
        Assert.Contains("MaxHeight=\"330\"", overview);
        Assert.Contains("AutomationProperties.Name=\"风险与提醒列表\"", overview);
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", overview);
        Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", overview);
        Assert.Contains("x:Name=\"OverviewRiskScrollViewer\"", overview);
        Assert.Contains("Command=\"{Binding OpenAttentionCenterCommand}\"", overview);
    }

    private static string ReadSource(params string[] relativePath)
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new DirectoryNotFoundException("无法定位 GameSaveCenter 仓库根目录。");
        }

        return File.ReadAllText(Path.Combine(new[] { directory.FullName, "src", "GameSaveCenter.Playnite" }.Concat(relativePath).ToArray()));
    }
}
