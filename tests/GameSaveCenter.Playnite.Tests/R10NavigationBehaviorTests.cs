using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R10NavigationBehaviorTests
{
    [Fact]
    public void RouteStackRestoresGameTabsFiltersSelectionAndScrollInLifoOrder()
    {
        var stack = new WorkspaceNavigationStack();
        var alert = new WorkspaceNavigationSnapshot(
            WorkspaceKind.Maintenance,
            "game-alert",
            2,
            1,
            0,
            "",
            "全部",
            "全部",
            "全部",
            "最近任务",
            "全部时间",
            "",
            "",
            "",
            -1,
            "game-alert\u001fSAVE_PATH_MISSING\u001f存档路径不存在",
            0,
            144,
            "返回告警",
            "来源：维护中心 · 存档路径不存在");
        var tasks = new WorkspaceNavigationSnapshot(
            WorkspaceKind.Tasks,
            "game-task",
            0,
            1,
            0,
            "失败",
            "失败",
            "全部",
            "存档备份",
            "全部历史",
            "近30天",
            "game-task",
            "示例游戏",
            "task-2",
            7,
            "",
            288,
            0,
            "返回任务",
            "来源：任务中心 · 示例游戏");

        stack.Push(alert);
        stack.Push(tasks);

        Assert.True(stack.TryPop(out var taskReturn));
        Assert.Equal(WorkspaceKind.Tasks, taskReturn.Workspace);
        Assert.Equal("game-task", taskReturn.SelectedGameId);
        Assert.Equal(1, taskReturn.MediaTabIndex);
        Assert.Equal("失败", taskReturn.TaskSearchText);
        Assert.Equal("存档备份", taskReturn.TaskTypeFilter);
        Assert.Equal("task-2", taskReturn.SelectedTaskId);
        Assert.Equal(7, taskReturn.SelectedTaskIndex);
        Assert.Equal(288, taskReturn.TaskScrollOffset);

        Assert.True(stack.TryPop(out var alertReturn));
        Assert.Equal(WorkspaceKind.Maintenance, alertReturn.Workspace);
        Assert.Equal(0, alertReturn.MaintenanceTabIndex);
        Assert.Equal("game-alert\u001fSAVE_PATH_MISSING\u001f存档路径不存在", alertReturn.SelectedFindingKey);
        Assert.Equal(144, alertReturn.MaintenanceFindingsScrollOffset);
        Assert.False(stack.TryPop(out _));
    }

    [Fact]
    public void SnapshotRejectsInvalidScrollWithoutChangingRouteIdentity()
    {
        var snapshot = new WorkspaceNavigationSnapshot(
            WorkspaceKind.Tasks,
            "game-1",
            0,
            0,
            0,
            "",
            "全部",
            "全部",
            "全部",
            "最近任务",
            "全部时间",
            "",
            "",
            "task-1",
            0,
            "",
            double.NaN,
            double.PositiveInfinity,
            "返回任务",
            "来源：任务中心");

        Assert.Equal("game-1", snapshot.SelectedGameId);
        Assert.Equal("task-1", snapshot.SelectedTaskId);
        Assert.Equal(0, snapshot.TaskScrollOffset);
        Assert.Equal(0, snapshot.MaintenanceFindingsScrollOffset);
    }
}
