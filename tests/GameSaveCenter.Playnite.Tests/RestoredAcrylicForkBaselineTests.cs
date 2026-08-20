using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class RestoredAcrylicForkBaselineTests
{
    [Fact]
    public void OverviewKeepsTheDemoSectionsAndTheCurrentWorkbenchActions()
    {
        var overview = ReadSource("Views", "OverviewView.xaml");

        Assert.Contains("最近任务", overview);
        Assert.Contains("全局活动", overview);
        Assert.Contains("风险与提醒", overview);
        Assert.Contains("OverviewActivityList", overview);
        Assert.Contains("OverviewActivityTimelineList", overview);
        Assert.Contains("x:Name=\"OverviewActivityColumn\"", overview);
        Assert.Contains("Grid.RowSpan=\"2\"", overview);
        Assert.Contains("今日工作台", overview);
        Assert.DoesNotContain("最近活动", overview);
    }

    [Fact]
    public void ProductionSidebarDoesNotExposeAPlaceholderSettingsNavigationItem()
    {
        var shell = ReadSource("Views", "AcrylicProductionShellView.xaml");

        Assert.DoesNotContain("Text=\"设置\"", shell);
        Assert.Contains("NavMaintenance", shell);
    }

    [Fact]
    public void SaveCenterUsesTheRestoredBackupPolicySections()
    {
        var save = ReadSource("Views", "SaveCenterView.xaml");

        Assert.Contains("x:Name=\"SaveHistorySummaryCard\"", save);
        Assert.Contains("Text=\"历史版本\"", save);
        Assert.Contains("Text=\"{Binding Backups.Count, Mode=OneWay, StringFormat={}{0} 个版本}\"", save);
        Assert.Contains("Command=\"{Binding DetectPathsCommand}\"", save);
        Assert.Contains("Command=\"{Binding ValidateCommand}\"", save);
        Assert.Contains("Command=\"{Binding LoadDetailsCommand}\"", save);
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
    public void TaskSearchHintAndInputShareOneStretchableSurface()
    {
        var tasks = ReadSource("Views", "TaskCenterView.xaml");
        var taskCode = ReadSource("Views", "TaskCenterView.xaml.cs");

        Assert.Contains("x:Name=\"TaskSearchBoxHost\"", tasks);
        Assert.Contains("Text=\"搜索任务…\"", tasks);
        Assert.Contains("x:Name=\"TaskSearchTextBox\"", tasks);
        Assert.Contains("HorizontalAlignment=\"Stretch\"", tasks);
        Assert.Contains("TaskSearchBoxHost.MinWidth = 420", taskCode);
        Assert.DoesNotContain("x:Name=\"TaskSearchLabel\"", tasks);
    }

    [Fact]
    public void TaskCenterKeepsTheDemoQueueAndInspectorReadingOrder()
    {
        var tasks = ReadSource("Views", "TaskCenterView.xaml");

        Assert.Contains("x:Name=\"TaskSummaryPanel\"", tasks);
        Assert.Contains("x:Name=\"TaskFilterBar\"", tasks);
        Assert.Contains("x:Name=\"TaskMoreFiltersExpander\"", tasks);
        Assert.Contains("x:Name=\"TaskQueuePanel\"", tasks);
        Assert.Contains("x:Name=\"TaskGrid\"", tasks);
        Assert.Contains("x:Name=\"TaskDetailScrollViewer\"", tasks);
        Assert.Contains("ItemsSource=\"{Binding TasksView}\"", tasks);
        Assert.Contains("Command=\"{Binding RetryTaskCommand}\"", tasks);
        Assert.Contains("Command=\"{Binding CancelTaskCommand}\"", tasks);
        Assert.Contains("EnableRowVirtualization\" Value=\"True\"", tasks);
        Assert.Contains("EnableColumnVirtualization\" Value=\"True\"", tasks);
        Assert.Contains("ScrollViewer.CanContentScroll\" Value=\"True\"", tasks);
    }

    [Fact]
    public void ExtractedWorkspaceTablesShareOneExplicitDemoGridContract()
    {
        var redesign = ReadSource("Themes", "Redesign.xaml");
        Assert.Contains("x:Key=\"GscRedesignWorkspaceDataGrid\"", redesign);
        Assert.Contains("VirtualizingPanel.VirtualizationMode\" Value=\"Recycling\"", redesign);
        Assert.Contains("CanUserResizeColumns\" Value=\"True\"", redesign);
        Assert.Contains("CanUserSortColumns\" Value=\"True\"", redesign);
        Assert.Contains("ScrollViewer.VerticalScrollBarVisibility\" Value=\"Auto\"", redesign);

        foreach (var page in new[] { "SaveCenterView.xaml", "MediaCenterView.xaml", "MaintenanceView.xaml", "TaskCenterView.xaml" })
        {
            var source = ReadSource("Views", page);
            Assert.Contains("BasedOn=\"{StaticResource GscRedesignWorkspaceDataGrid}\"", source);
        }
    }

    [Fact]
    public void DashboardFeedbackUsesTheSharedDemoToastAndDialogSurfaces()
    {
        var redesign = ReadSource("Themes", "Redesign.xaml");
        var dashboard = ReadSource("Views", "DashboardView.xaml");
        var dashboardCode = ReadSource("Views", "DashboardView.xaml.cs");

        Assert.Contains("x:Key=\"GscRedesignFeedbackToastCard\"", redesign);
        Assert.Contains("x:Key=\"GscRedesignFeedbackDialogCard\"", redesign);
        Assert.Contains("x:Key=\"GscRedesignFeedbackDialogOverlay\"", redesign);
        Assert.Contains("Style=\"{StaticResource GscRedesignFeedbackDialogOverlay}\"", dashboard);
        Assert.Contains("Style=\"{StaticResource GscRedesignFeedbackDialogCard}\"", dashboard);
        Assert.Contains("Style=\"{StaticResource GscRedesignFeedbackDialogTitle}\"", dashboard);
        Assert.Contains("Style=\"{StaticResource GscRedesignFeedbackDialogMessage}\"", dashboard);
        Assert.Contains("Style = (Style)Resources[\"GscRedesignFeedbackToastCard\"]", dashboardCode);
        Assert.Contains("Style = (Style)Resources[\"GscRedesignFeedbackToastTitle\"]", dashboardCode);
        Assert.Contains("Style = (Style)Resources[\"GscRedesignFeedbackToastMessage\"]", dashboardCode);
        Assert.DoesNotContain("<Style x:Key=\"GscButtonBase\"", dashboard);
        Assert.DoesNotContain("<Style x:Key=\"GscPrimaryButton\"", dashboard);
        Assert.Contains("UiNotificationRequested", dashboardCode);
        Assert.Contains("UiConfirmationRequested", dashboardCode);
        Assert.Contains("UiChoiceRequested", dashboardCode);
    }

    [Fact]
    public void WorkspaceDiagnosticTextUsesTheSharedCascadiaMonoFallback()
    {
        var tokens = ReadSource("Themes", "DesignTokens.xaml");
        var maintenance = ReadSource("Views", "MaintenanceView.xaml");

        Assert.Contains("x:Key=\"GscCodeFontFamily\">Cascadia Mono, Consolas, Microsoft YaHei UI", tokens);
        Assert.Contains("FontFamily=\"{DynamicResource GscCodeFontFamily}\"", maintenance);
    }

    [Fact]
    public void MediaUsesDemoMetricCardsAndTheFullWorkspaceTabs()
    {
        var media = ReadSource("Views", "MediaCenterView.xaml");

        Assert.Contains("x:Name=\"MediaSummaryPanel\" Grid.Row=\"0\" Columns=\"4\"", media);
        Assert.Contains("x:Name=\"MediaTabControl\"", media);
        Assert.Contains("Style=\"{StaticResource MediaTabControl}\"", media);
        Assert.Contains("x:Name=\"MediaInboxInspectorScrollViewer\"", media);
        Assert.Contains("x:Name=\"MediaInboxPreviewPanel\"", media);
        Assert.Contains("SelectedInboxMedia", media);
        Assert.Contains("Header=\"当前游戏媒体\"", media);
        Assert.Contains("Style=\"{DynamicResource GscRedesignMetricBorder}\"", media);
        Assert.DoesNotContain("x:Name=\"MediaSummaryTabStrip\"", media);
        Assert.DoesNotContain("x:Name=\"MediaTabStrip\"", media);
    }

    [Fact]
    public void TrainerCenterUsesDemoSegmentedNavigationWithoutDroppingRealEntryPoints()
    {
        var trainer = ReadSource("Views", "TrainerCenterView.xaml");
        var trainerCode = ReadSource("Views", "TrainerCenterView.xaml.cs");

        Assert.DoesNotContain("<TabControl", trainer);
        Assert.Contains("x:Name=\"TrainerSegmentTabs\"", trainer);
        Assert.Contains("Style=\"{StaticResource LabSegmented}\"", trainer);
        Assert.Contains("SelectionChanged=\"OnTrainerSegmentChanged\"", trainer);
        Assert.Contains("修改器 · CT 表 · 自定义启动项 · 拖拽导入", trainer);
        Assert.Contains("x:Name=\"PanelTools\"", trainer);
        Assert.Contains("x:Name=\"PanelImport\"", trainer);
        Assert.Contains("x:Name=\"PanelCatalog\"", trainer);
        Assert.Contains("x:Name=\"PanelReleases\"", trainer);
        Assert.Contains("ImportTrainerCommand", trainer);
        Assert.Contains("ImportToolFolderCommand", trainer);
        Assert.Contains("ImportCheatTableCommand", trainer);
        Assert.Contains("ImportCustomLaunchItemCommand", trainer);
        Assert.Contains("SearchTrainerCatalogCommand", trainer);
        Assert.Contains("LoadTrainerReleasesCommand", trainer);
        Assert.Contains("DownloadTrainerCommand", trainer);
        Assert.Contains("PanelTools.Visibility", trainerCode);
        Assert.Contains("PanelImport.Visibility", trainerCode);
        Assert.Contains("PanelCatalog.Visibility", trainerCode);
        Assert.Contains("PanelReleases.Visibility", trainerCode);
        Assert.Contains("VirtualizingPanel.VirtualizationMode=\"Recycling\"", trainer);
        Assert.Contains("Property=\"ScrollViewer.VerticalScrollBarVisibility\" Value=\"Auto\"", trainer);
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
        Assert.Contains("MaxHeight=\"420\"", overview);
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
