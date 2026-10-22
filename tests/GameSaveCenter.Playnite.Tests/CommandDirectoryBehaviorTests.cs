using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class CommandDirectoryBehaviorTests
{
    [Fact]
    public void DirectoryRoutesExistingNavigationAndExplainsMissingGameOrTaskContext()
    {
        var dashboard = CreateDashboard();
        var navigation = new RecordingCommand();
        var taskNavigation = new RecordingCommand();
        SetField(dashboard, "<OpenSelectedTaskGameCommand>k__BackingField", taskNavigation);
        var items = CommandDirectoryCatalog.Create(
            dashboard,
            navigation,
            new RecordingCommand(),
            new RecordingCommand(),
            new RecordingCommand());

        Assert.Equal(items.Count, items.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count());

        var tasks = Assert.Single(items, item => item.Id == "workspace.tasks");
        Assert.Equal(CommandDirectoryScope.Workspace, tasks.Scope);
        Assert.Contains("Tasks", tasks.Aliases, StringComparer.OrdinalIgnoreCase);
        Assert.True(tasks.TryExecute());
        Assert.Equal(1, navigation.ExecutionCount);
        Assert.Equal(WorkspaceKind.Tasks, navigation.LastParameter);

        var saves = Assert.Single(items, item => item.Id == "workspace.saves");
        Assert.Equal(CommandDirectoryScope.CurrentGame, saves.Scope);
        Assert.False(saves.CanExecute(null));
        Assert.Equal("请先选择当前游戏，再打开此工作区。", saves.UnavailableReason);
        Assert.False(saves.TryExecute());
        Assert.Equal(1, navigation.ExecutionCount);

        SetField(dashboard, "currentWorkspace", WorkspaceKind.Tasks);
        var openTaskGame = Assert.Single(items, item => item.Id == "task.open-associated-game");
        Assert.Equal(CommandDirectoryScope.SelectedTask, openTaskGame.Scope);
        Assert.False(openTaskGame.CanExecute(null));
        Assert.Equal("请先选择一个任务。", openTaskGame.UnavailableReason);
        Assert.False(openTaskGame.TryExecute());
        Assert.Equal(0, taskNavigation.ExecutionCount);
    }

    [Fact]
    public void DirectoryItemRechecksAvailabilityAndForwardsTheBoundParameterOnce()
    {
        var canNavigate = false;
        var command = new RecordingCommand();
        var item = new CommandDirectoryItem(
            "workspace.tasks",
            "查看任务",
            new[] { "Tasks", "tasks", "任务列表" },
            "页面导航",
            CommandDirectoryScope.Workspace,
            command,
            WorkspaceKind.Tasks,
            additionalCanExecute: () => canNavigate,
            unavailableReason: () => "当前工作区不可用。");

        Assert.Equal(new[] { "Tasks", "任务列表" }, item.Aliases);
        Assert.False(item.TryExecute());
        Assert.Equal("当前工作区不可用。", item.UnavailableReason);
        Assert.Equal(0, command.ExecutionCount);

        canNavigate = true;
        Assert.True(item.TryExecute());
        Assert.Equal(1, command.ExecutionCount);
        Assert.Equal(WorkspaceKind.Tasks, command.LastParameter);
    }

    [Fact]
    public void ContextSnapshotShowsCapturedTargetsAndRequiresReselectionWhenTheyChangeOrDisappear()
    {
        var dashboard = CreateDashboard();
        SetField(dashboard, "currentWorkspace", WorkspaceKind.Tasks);
        var firstGame = new GameStatusDto { PlayniteId = "game-a", Name = "游戏甲" };
        var secondGame = new GameStatusDto { PlayniteId = "game-b", Name = "游戏乙" };
        dashboard.Games.Add(firstGame);
        dashboard.Games.Add(secondGame);
        var gamePicker = (GamePickerViewModel)GetField(dashboard, "gamePicker")!;
        SetField(gamePicker, "selectedItem", new GamePickerItem(firstGame));

        var firstTask = new TaskStatusDto
        {
            TaskId = "task-a",
            TaskType = "Backup",
            GameId = firstGame.PlayniteId,
            GameName = firstGame.Name,
        };
        var secondTask = new TaskStatusDto
        {
            TaskId = "task-b",
            TaskType = "MediaSync",
            GameId = secondGame.PlayniteId,
            GameName = secondGame.Name,
        };
        dashboard.Tasks.Add(firstTask);
        dashboard.Tasks.Add(secondTask);
        SetField(dashboard, "selectedTask", firstTask);

        var navigation = new RecordingCommand();
        var taskNavigation = new RecordingCommand();
        SetField(dashboard, "<OpenSelectedTaskGameCommand>k__BackingField", taskNavigation);
        CommandDirectoryContextSnapshot? snapshot = CommandDirectoryContextSnapshot.Capture(dashboard);
        var items = CommandDirectoryCatalog.Create(
            dashboard,
            navigation,
            new RecordingCommand(),
            new RecordingCommand(),
            new RecordingCommand(),
            () => snapshot);
        var saves = Assert.Single(items, item => item.Id == "workspace.saves");
        var openTaskGame = Assert.Single(items, item => item.Id == "task.open-associated-game");

        Assert.Contains("存档中心", saves.ContextPreview);
        Assert.Contains("游戏甲", saves.ContextPreview);
        Assert.Contains("Backup", openTaskGame.ContextPreview);
        Assert.Contains("游戏甲", openTaskGame.ContextPreview);
        Assert.True(saves.CanExecute(null));
        Assert.True(openTaskGame.CanExecute(null));

        SetField(gamePicker, "selectedItem", new GamePickerItem(secondGame));
        Assert.False(saves.CanExecute(null));
        Assert.Contains("游戏选择已变化", saves.UnavailableReason);
        Assert.False(saves.TryExecute());
        Assert.Equal(0, navigation.ExecutionCount);

        SetField(gamePicker, "selectedItem", new GamePickerItem(firstGame));
        SetField(dashboard, "selectedTask", secondTask);
        Assert.False(openTaskGame.CanExecute(null));
        Assert.Contains("任务已变化", openTaskGame.UnavailableReason);
        Assert.False(openTaskGame.TryExecute());
        Assert.Equal(0, taskNavigation.ExecutionCount);

        SetField(dashboard, "selectedTask", firstTask);
        dashboard.Tasks.Remove(firstTask);
        Assert.False(openTaskGame.CanExecute(null));
        Assert.Contains("任务已不在", openTaskGame.UnavailableReason);
        Assert.False(openTaskGame.TryExecute());
        Assert.Equal(0, taskNavigation.ExecutionCount);

        snapshot = CommandDirectoryContextSnapshot.Capture(dashboard);
        Assert.True(saves.TryExecute());
        Assert.Equal(1, navigation.ExecutionCount);
        Assert.Equal(WorkspaceKind.Saves, navigation.LastParameter);
    }

    private static DashboardViewModel CreateDashboard()
    {
        var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
        SetField(dashboard, "gamePicker", (GamePickerViewModel)FormatterServices.GetUninitializedObject(typeof(GamePickerViewModel)));
        SetField(dashboard, "<Games>k__BackingField", new BatchObservableCollection<GameSaveCenter.Contracts.GameStatusDto>());
        SetField(dashboard, "<Tasks>k__BackingField", new BatchObservableCollection<TaskStatusDto>());
        SetField(dashboard, "currentWorkspace", WorkspaceKind.Overview);
        SetField(dashboard, "isBusy", false);
        SetField(dashboard, "<OpenSelectedTaskGameCommand>k__BackingField", new RecordingCommand());
        return dashboard;
    }

    private static void SetField(object target, string name, object? value)
    {
        var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }

    private static object? GetField(object target, string name)
    {
        var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return field!.GetValue(target);
    }

    private sealed class RecordingCommand : ICommand
    {
        public int ExecutionCount { get; private set; }
        public object? LastParameter { get; private set; }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter)
        {
            ExecutionCount++;
            LastParameter = parameter;
        }
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
