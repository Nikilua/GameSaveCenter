using System;
using System.Collections.Generic;
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

public sealed class CommandDirectoryRegisteredRouteTests
{
    public static IEnumerable<object[]> Routes()
    {
        yield return new object[] { "workspace.overview", "Overview", 0, WorkspaceKind.Overview };
        yield return new object[] { "workspace.saves", "Save history", 0, WorkspaceKind.Saves };
        yield return new object[] { "workspace.trainers", "Trainers", 0, WorkspaceKind.Trainers };
        yield return new object[] { "workspace.media", "Screenshots", 0, WorkspaceKind.Media };
        yield return new object[] { "workspace.tasks", "View tasks", 0, WorkspaceKind.Tasks };
        yield return new object[] { "workspace.notifications", "Unread notifications", 0, WorkspaceKind.Notifications };
        yield return new object[] { "workspace.maintenance", "Diagnostics", 0, WorkspaceKind.Maintenance };
        yield return new object[] { "settings.open", "Plugin settings", 1, WorkspaceKind.Tasks };
        yield return new object[] { "game-picker.open", "Switch game", 2, WorkspaceKind.Tasks };
        yield return new object[] { "workspace.focus-search", "Ctrl+F", 3, WorkspaceKind.Tasks };
        yield return new object[] { "task.open-associated-game", "Open related game", 4, WorkspaceKind.Tasks };
        yield return new object[] { "review.restore", "Review restore", 0, WorkspaceKind.Saves };
        yield return new object[] { "review.retention", "Review cleanup", 0, WorkspaceKind.Maintenance };
    }

    [Theory]
    [MemberData(nameof(Routes))]
    public void SearchExecutesOnlyTheRegisteredSelectedRouteOnce(
        string id, string alias, int commandIndex, WorkspaceKind target)
    {
        var fixture = new Fixture();
        var item = Assert.Single(CommandDirectorySearch.Filter(fixture.Items, alias.ToUpperInvariant()));
        Assert.Equal(id, item.Id);
        Assert.Same(fixture.Items.Single(entry => entry.Id == id), item);
        Assert.True(item.CanExecute(null));
        var executed = 0;
        item.Executed += (_, _) => executed++;
        Assert.True(item.TryExecute());
        Assert.Equal(1, executed);
        Assert.Equal(1, fixture.Commands.Sum(command => command.Count));
        Assert.Equal(1, fixture.Commands[commandIndex].Count);
        Assert.Equal(commandIndex == 0 ? (object)target : null, fixture.Commands[commandIndex].Parameter);
        Assert.Equal(id == "review.restore" ? 0 : 2, fixture.Dashboard.SaveTabIndex);
        Assert.Equal(id == "review.retention" ? 3 : 1, fixture.Dashboard.MaintenanceTabIndex);
        fixture.AssertProtectedState();
    }

    [Theory]
    [MemberData(nameof(Routes))]
    public void RevokedUnderlyingAvailabilityRejectsBothExecutionEntrypoints(
        string id, string alias, int commandIndex, WorkspaceKind target)
    {
        var fixture = new Fixture();
        var item = Assert.Single(CommandDirectorySearch.Filter(fixture.Items, alias));
        Assert.Equal(id, item.Id);
        Assert.True(item.IsAvailable);
        fixture.Commands[commandIndex].Allowed = false;
        var executed = 0;
        item.Executed += (_, _) => executed++;
        Assert.False(item.CanExecute(target));
        Assert.False(item.TryExecute());
        ((ICommand)item).Execute(target);
        Assert.Equal(0, executed);
        Assert.Equal(0, fixture.Commands.Sum(command => command.Count));
        Assert.False(string.IsNullOrWhiteSpace(item.UnavailableReason));
        Assert.Equal(2, fixture.Dashboard.SaveTabIndex);
        Assert.Equal(1, fixture.Dashboard.MaintenanceTabIndex);
        fixture.AssertProtectedState();
    }

    [Fact]
    public void ActualRegistryHasNoUntestedRouteAndPreservesTheDefaultEight()
    {
        var fixture = new Fixture();
        Assert.Equal(Routes().Select(row => (string)row[0]).OrderBy(id => id),
            fixture.Items.Select(item => item.Id).OrderBy(id => id));
        Assert.Equal(new[] { "workspace.overview", "workspace.saves", "workspace.trainers",
            "workspace.media", "workspace.tasks", "workspace.notifications", "workspace.maintenance", "settings.open" },
            CommandDirectorySearch.Filter(fixture.Items, " ").Select(item => item.Id));
        Assert.Empty(CommandDirectorySearch.Filter(fixture.Items, "不存在的合成命令"));
        Assert.Equal(0, fixture.Commands.Sum(command => command.Count));
    }

    [Theory]
    [InlineData("workspace.saves")]
    [InlineData("workspace.trainers")]
    [InlineData("workspace.media")]
    [InlineData("review.restore")]
    public void RemovedCapturedGameNeverFallsThroughToAnotherGame(string id)
    {
        var fixture = new Fixture();
        var item = fixture.Items.Single(entry => entry.Id == id);
        fixture.Dashboard.Games.Clear();
        Assert.False(item.TryExecute());
        ((ICommand)item).Execute(null);
        Assert.Contains("不在当前游戏列表", item.UnavailableReason);
        Assert.Equal(0, fixture.Commands.Sum(command => command.Count));
        fixture.AssertProtectedState();
    }

    [Theory]
    [InlineData("game-picker.open")]
    [InlineData("workspace.focus-search")]
    public void PageBoundCommandsRejectAChangedWorkspace(string id)
    {
        var fixture = new Fixture();
        SetField(fixture.Dashboard, "currentWorkspace", WorkspaceKind.Media);
        var item = fixture.Items.Single(entry => entry.Id == id);
        Assert.False(item.TryExecute());
        Assert.Contains("工作区已变化", item.UnavailableReason);
        Assert.Equal(0, fixture.Commands.Sum(command => command.Count));
    }

    [Theory]
    [InlineData("removed-task")]
    [InlineData("changed-game")]
    [InlineData("busy")]
    public void AssociatedGameRejectsStaleTaskAndBusyState(string mutation)
    {
        var fixture = new Fixture();
        if (mutation == "removed-task") fixture.Dashboard.Tasks.Clear();
        else if (mutation == "changed-game") fixture.Dashboard.SelectedTask!.GameId = "another-game";
        else SetField(fixture.Dashboard, "isBusy", true);
        var item = fixture.Items.Single(entry => entry.Id == "task.open-associated-game");
        Assert.False(item.TryExecute());
        Assert.False(string.IsNullOrWhiteSpace(item.UnavailableReason));
        Assert.Equal(0, fixture.Commands.Sum(command => command.Count));
        fixture.AssertProtectedState();
    }

    private sealed class Fixture
    {
        public Fixture()
        {
            Dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
            var picker = (GamePickerViewModel)FormatterServices.GetUninitializedObject(typeof(GamePickerViewModel));
            SetField(Dashboard, "gamePicker", picker);
            SetField(Dashboard, "<Games>k__BackingField", new BatchObservableCollection<GameStatusDto>());
            SetField(Dashboard, "<Tasks>k__BackingField", new BatchObservableCollection<TaskStatusDto>());
            SetField(Dashboard, "currentWorkspace", WorkspaceKind.Tasks);
            SetField(Dashboard, "saveTabIndex", 2);
            SetField(Dashboard, "maintenanceTabIndex", 1);
            SetField(Dashboard, "selectedBackup", ProtectedVersion);
            SetField(Dashboard, "<OpenSelectedTaskGameCommand>k__BackingField", Commands[4]);
            foreach (var name in new[] { "RestoreCommand", "ApplyRetentionSimulationCommand", "PreviewRetentionCommand", "RefreshRetentionSimulationCommand" })
                SetField(Dashboard, "<" + name + ">k__BackingField", Dangerous);
            var game = new GameStatusDto { PlayniteId = "registered-synthetic-game", Name = "登记命令合成游戏" };
            Dashboard.Games.Add(game);
            SetField(picker, "selectedItem", new GamePickerItem(game));
            var task = new TaskStatusDto { TaskId = "registered-synthetic-task", TaskType = "Backup", GameId = game.PlayniteId, GameName = game.Name };
            Dashboard.Tasks.Add(task);
            SetField(Dashboard, "selectedTask", task);
            var snapshot = CommandDirectoryContextSnapshot.Capture(Dashboard);
            Items = CommandDirectoryCatalog.Create(Dashboard, Commands[0], Commands[1], Commands[2], Commands[3], () => snapshot);
        }

        public DashboardViewModel Dashboard { get; }
        public RecordingCommand[] Commands { get; } = Enumerable.Range(0, 5).Select(_ => new RecordingCommand()).ToArray();
        public IReadOnlyList<CommandDirectoryItem> Items { get; }
        private RecordingCommand Dangerous { get; } = new RecordingCommand();
        private BackupVersionDto ProtectedVersion { get; } = new BackupVersionDto { BackupId = "protected-synthetic-version", IsLocked = true, IsPreRestore = true };

        public void AssertProtectedState()
        {
            Assert.Equal(0, Dangerous.Count);
            Assert.Same(ProtectedVersion, Dashboard.SelectedBackup);
            Assert.True(ProtectedVersion.IsLocked);
            Assert.True(ProtectedVersion.IsPreRestore);
        }
    }

    private sealed class RecordingCommand : ICommand
    {
        public bool Allowed { get; set; } = true;
        public int Count { get; private set; }
        public object? Parameter { get; private set; }
        public bool CanExecute(object? parameter) => Allowed;
        public void Execute(object? parameter) { Count++; Parameter = parameter; }
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }

    private static void SetField(object target, string name, object? value)
    {
        var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }
}
