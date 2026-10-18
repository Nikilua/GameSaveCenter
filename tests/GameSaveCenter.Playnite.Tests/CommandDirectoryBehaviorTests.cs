using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Input;
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

    private static DashboardViewModel CreateDashboard()
    {
        var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
        SetField(dashboard, "gamePicker", (GamePickerViewModel)FormatterServices.GetUninitializedObject(typeof(GamePickerViewModel)));
        SetField(dashboard, "<Games>k__BackingField", new BatchObservableCollection<GameSaveCenter.Contracts.GameStatusDto>());
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
