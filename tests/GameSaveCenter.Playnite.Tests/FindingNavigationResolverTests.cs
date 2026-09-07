using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class FindingNavigationResolverTests
{
    [Fact]
    public void GameFindingRoutesToSaveWorkspace()
    {
        var route = FindingNavigationResolver.Resolve(new ValidationFindingDto
        {
            PlayniteId = "game-1",
            Code = "SAVE_PATH_MISSING",
            Title = "存档路径不存在"
        });

        Assert.Equal(FindingNavigationKind.Save, route.Kind);
        Assert.Equal("进入存档路径确认", route.Text);
    }

    [Fact]
    public void CloudFindingRoutesToCloudQueueBeforeGameFallback()
    {
        var route = FindingNavigationResolver.Resolve(new ValidationFindingDto
        {
            PlayniteId = "game-1",
            Code = "RCLONE_CHECK_FAILED",
            Title = "远端校验失败"
        });

        Assert.Equal(FindingNavigationKind.CloudQueue, route.Kind);
        Assert.Equal("打开云队列", route.Text);
    }

    [Fact]
    public void TaskHintRoutesToFailedTasks()
    {
        var route = FindingNavigationResolver.Resolve(new ValidationFindingDto
        {
            PlayniteId = "game-1",
            Code = "OPERATION_WARNING",
            SuggestedAction = "在任务中心查看详情并重试。"
        });

        Assert.Equal(FindingNavigationKind.FailedTasks, route.Kind);
        Assert.Equal("查看失败任务", route.Text);
    }

    [Fact]
    public void FindingWithoutIdentityHasNoNavigation()
    {
        var route = FindingNavigationResolver.Resolve(new ValidationFindingDto
        {
            Code = "INFO_ONLY",
            Title = "状态提示"
        });

        Assert.Equal(FindingNavigationKind.None, route.Kind);
        Assert.False(route.IsAvailable);
    }
}
