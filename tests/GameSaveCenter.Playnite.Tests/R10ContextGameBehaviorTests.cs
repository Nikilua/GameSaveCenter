using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R10ContextGameBehaviorTests
{
    [Fact]
    public void PickerLocatesDuplicateDisplayNamesByStablePlayniteId()
    {
        using var picker = new GamePickerViewModel();
        var first = Game("playnite-a", "同名游戏");
        var second = Game("playnite-b", "同名游戏");

        picker.SetItems(new[] { first, second });
        picker.SelectGame(new GameStatusDto { PlayniteId = "playnite-b", Name = "同名游戏" });

        Assert.NotNull(picker.SelectedGame);
        Assert.Equal("playnite-b", picker.SelectedGame!.PlayniteId);
        Assert.Same(second, picker.Items.Single(item => item.PlayniteId == "playnite-b").Game);
    }

    [Fact]
    public void PickerDoesNotFallBackToDuplicateDisplayNameWhenStableIdIsMissing()
    {
        using var picker = new GamePickerViewModel();
        picker.SetItems(new[]
        {
            Game("playnite-a", "同名游戏"),
            Game("playnite-b", "同名游戏")
        });

        picker.SelectGame(Game("removed-game", "同名游戏"));

        Assert.Null(picker.SelectedGame);
    }

    [Fact]
    public void TaskGameCommandNavigatesToTheExactIdWhenDisplayNamesCollide()
    {
        var first = Game("playnite-a", "同名游戏");
        var second = Game("playnite-b", "同名游戏");
        var task = new TaskStatusDto { TaskId = "task-1", GameId = "playnite-b", GameName = "同名游戏" };
        var workspaceLoads = new List<WorkspaceKind>();
        var dashboard = CreateTaskNavigationDashboard(new[] { first, second }, first, task, workspaceLoads);

        Assert.True(dashboard.OpenSelectedTaskGameCommand.CanExecute(null));
        dashboard.OpenSelectedTaskGameCommand.Execute(null);

        Assert.Equal("playnite-b", dashboard.SelectedGame!.PlayniteId);
        Assert.Same(second, dashboard.SelectedGame);
        Assert.Equal(WorkspaceKind.Saves, dashboard.CurrentWorkspace);
        Assert.True(dashboard.HasNavigationReturnTarget);
        Assert.Equal("返回任务", dashboard.NavigationReturnLabel);
        Assert.Equal(new[] { WorkspaceKind.Saves }, workspaceLoads);
    }

    [Fact]
    public void TaskGameCommandKeepsCurrentSelectionWhenTheStableIdIsMissing()
    {
        var current = Game("playnite-a", "同名游戏");
        var task = new TaskStatusDto { TaskId = "task-removed", GameId = "removed-game", GameName = "同名游戏" };
        var workspaceLoads = new List<WorkspaceKind>();
        var dashboard = CreateTaskNavigationDashboard(new[] { current }, current, task, workspaceLoads);

        Assert.True(dashboard.OpenSelectedTaskGameCommand.CanExecute(null));
        dashboard.OpenSelectedTaskGameCommand.Execute(null);

        Assert.Same(current, dashboard.SelectedGame);
        Assert.Equal(WorkspaceKind.Tasks, dashboard.CurrentWorkspace);
        Assert.False(dashboard.HasNavigationReturnTarget);
        Assert.Contains("未切换到其他游戏", dashboard.StatusMessage);
        Assert.Contains("请先刷新游戏库", dashboard.StatusMessage);
        Assert.Empty(workspaceLoads);
    }

    [Fact]
    public void NotificationTaskContextResolvesOnlyTheExactStableTaskId()
    {
        var first = new TaskStatusDto { TaskId = "task-a", GameName = "同名游戏" };
        var second = new TaskStatusDto { TaskId = "task-b", GameName = "同名游戏" };
        var tasks = new[] { first, second };

        Assert.Same(second, TaskSourceNavigationResolver.ResolveExactTask(" task-b ", tasks));
        Assert.Null(TaskSourceNavigationResolver.ResolveExactTask("removed-task", tasks));
        Assert.Null(TaskSourceNavigationResolver.ResolveExactTask("", tasks));
    }

    [Fact]
    public void TaskAndMediaDetailsExposeStableGameIdentityRoutes()
    {
        var root = TestRepositoryContext.Root;
        var navigation = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Navigation.cs"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Media.cs"));
        var shell = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));

        Assert.Contains("SelectedTask.GameId", navigation);
        Assert.Contains("SelectedGame.PlayniteId", media);
        Assert.Contains("Text=\"{Binding SelectedGame.Name", shell);
        Assert.Contains("ItemsSource=\"{Binding GamePicker.ItemsView}\"", shell);
    }

    private static GameStatusDto Game(string id, string name)
        => new GameStatusDto
        {
            PlayniteId = id,
            Name = name,
            IsInstalled = true,
            LudusaviMatched = true,
            HealthState = "Ready"
        };

    private static DashboardViewModel CreateTaskNavigationDashboard(
        GameStatusDto[] games,
        GameStatusDto selectedGame,
        TaskStatusDto selectedTask,
        List<WorkspaceKind> workspaceLoads)
    {
        var plugin = (GameSaveCenterPlugin)FormatterServices.GetUninitializedObject(typeof(GameSaveCenterPlugin));
        plugin.SessionLastWorkspace = WorkspaceKind.Tasks;
        var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
        SetField(dashboard, "plugin", plugin);

        var picker = new GamePickerViewModel();
        picker.SetItems(games);
        picker.SelectGame(selectedGame);
        SetField(dashboard, "gamePicker", picker);
        SetField(dashboard, "navigationHistory", new WorkspaceNavigationStack());
        SetField(dashboard, "currentWorkspace", WorkspaceKind.Tasks);
        SetField(dashboard, "selectedTask", selectedTask);
        SetField(dashboard, "restoringRecentAccess", true);
        SetField(dashboard, "taskSearchText", string.Empty);
        SetField(dashboard, "taskStatusFilter", "全部");
        SetField(dashboard, "taskGameFilter", "全部");
        SetField(dashboard, "taskTypeFilter", "全部");
        SetField(dashboard, "taskHistoryScope", "最近任务");
        SetField(dashboard, "taskHistoryRange", "全部时间");
        SetField(dashboard, "taskNavigationGameId", string.Empty);
        SetField(dashboard, "taskNavigationGameName", string.Empty);

        var gameItems = new BatchObservableCollection<GameStatusDto>();
        foreach (var game in games) gameItems.Add(game);
        SetAutoPropertyBackingField(dashboard, nameof(DashboardViewModel.Games), gameItems);
        var taskItems = new BatchObservableCollection<TaskStatusDto>();
        taskItems.Add(selectedTask);
        SetAutoPropertyBackingField(dashboard, nameof(DashboardViewModel.Tasks), taskItems);
        dashboard.WorkspaceLoadSchedulerOverrideForAudit = workspace => workspaceLoads.Add(workspace);
        typeof(DashboardViewModel)
            .GetMethod("OnNavigationStateInitialize", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(dashboard, null);
        return dashboard;
    }

    private static void SetAutoPropertyBackingField(object target, string propertyName, object value)
        => SetField(target, $"<{propertyName}>k__BackingField", value);

    private static void SetField(object target, string fieldName, object? value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }
}
