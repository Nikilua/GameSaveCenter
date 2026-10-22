using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class CommandDirectoryReviewBehaviorTests
{
    [Theory]
    [InlineData("恢复", "review.restore", WorkspaceKind.Saves)]
    [InlineData("cleanup", "review.retention", WorkspaceKind.Maintenance)]
    public void EnterOnARegisteredReviewOnlyNavigatesOnceAndKeepsTheProtectedVersion(
        string query, string expectedId, WorkspaceKind expectedWorkspace)
    {
        RunSta(() =>
        {
            var fixture = new ReviewFixture();
            var shell = new AcrylicProductionShellView();
            SetField(shell, "viewModel", fixture.Dashboard);
            SetField(shell, "commandDirectoryItems", fixture.Items);
            var window = new Window
            {
                Content = shell, Width = 900, Height = 640,
                ShowInTaskbar = false, ShowActivated = true,
                WindowStyle = WindowStyle.None, Opacity = 0.01
            };
            try
            {
                window.Show();
                var overlay = (Grid)shell.FindName("CommandDirectoryOverlay")!;
                var search = (TextBox)shell.FindName("CommandDirectorySearchTextBox")!;
                var results = (ListBox)shell.FindName("CommandDirectoryResultsListBox")!;
                overlay.Visibility = Visibility.Visible;
                search.Text = query;
                typeof(AcrylicProductionShellView).GetMethod("UpdateCommandDirectorySearchResults",
                    BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(shell, null);
                window.UpdateLayout();
                Keyboard.Focus(search);

                var selected = Assert.IsType<CommandDirectoryItem>(Assert.Single(results.Items.Cast<object>()));
                Assert.Equal(expectedId, selected.Id);
                Assert.Contains("仅打开审阅页", selected.ContextPreview);
                Assert.Equal(expectedId, ((CommandDirectoryItem)results.SelectedItem).Id);
                var enter = new KeyEventArgs(Keyboard.PrimaryDevice,
                    PresentationSource.FromVisual(window)!, 0, Key.Enter)
                {
                    RoutedEvent = Keyboard.PreviewKeyDownEvent
                };
                search.RaiseEvent(enter);
                window.Dispatcher.Invoke(DispatcherPriority.Input, new Action(() => { }));

                Assert.True(enter.Handled);
                Assert.Equal(Visibility.Collapsed, overlay.Visibility);
                Assert.Equal(1, fixture.Navigation.ExecutionCount);
                Assert.Equal(expectedWorkspace, fixture.Navigation.LastParameter);
                Assert.Equal(expectedWorkspace == WorkspaceKind.Saves ? 0 : 2, fixture.Dashboard.SaveTabIndex);
                Assert.Equal(expectedWorkspace == WorkspaceKind.Maintenance ? 3 : 1, fixture.Dashboard.MaintenanceTabIndex);
                fixture.AssertNoDangerousActionOrSelectionMutation();
            }
            finally { window.Close(); }
        });
    }

    [Fact]
    public void RestoreReviewRejectsChangedRemovedAndMissingCapturedGamesBeforeChangingTheTab()
    {
        RunSta(() =>
        {
            var fixture = new ReviewFixture();
            var restore = fixture.Items.Single(item => item.Id == "review.restore");
            var original = fixture.Dashboard.SelectedGame!;
            Assert.Contains(original.Name, restore.ContextPreview);

            var other = new GameStatusDto { PlayniteId = "synthetic-other", Name = "其他合成游戏" };
            fixture.Dashboard.Games.Add(other);
            SetField(fixture.Picker, "selectedItem", new GamePickerItem(other));
            Assert.False(restore.TryExecute());
            Assert.Contains("选择已变化", restore.UnavailableReason);
            Assert.Contains(original.Name, restore.ContextPreview);
            Assert.DoesNotContain(other.Name, restore.ContextPreview);

            fixture.Dashboard.Games.Remove(original);
            Assert.False(restore.TryExecute());
            Assert.Contains("不在当前游戏列表", restore.UnavailableReason);

            SetField(fixture.Picker, "selectedItem", null);
            fixture.Snapshot = CommandDirectoryContextSnapshot.Capture(fixture.Dashboard);
            Assert.False(restore.TryExecute());
            Assert.Contains("没有选择", restore.UnavailableReason);
            Assert.Equal(2, fixture.Dashboard.SaveTabIndex);
            Assert.Equal(0, fixture.Navigation.ExecutionCount);
            fixture.AssertNoDangerousActionOrSelectionMutation();
        });
    }

    [Fact]
    public void BothReviewRoutesRecheckNavigationAvailabilityBeforeChangingTabs()
    {
        RunSta(() =>
        {
            var fixture = new ReviewFixture();
            var restore = fixture.Items.Single(item => item.Id == "review.restore");
            var cleanup = fixture.Items.Single(item => item.Id == "review.retention");
            fixture.Navigation.Allowed = false;
            Assert.False(restore.TryExecute());
            Assert.False(cleanup.TryExecute());
            restore.Execute(null);
            cleanup.Execute(null);
            Assert.NotEmpty(restore.UnavailableReason);
            Assert.NotEmpty(cleanup.UnavailableReason);
            Assert.Equal(2, fixture.Dashboard.SaveTabIndex);
            Assert.Equal(1, fixture.Dashboard.MaintenanceTabIndex);
            Assert.Equal(0, fixture.Navigation.ExecutionCount);

            fixture.Navigation.Allowed = true;
            Assert.True(cleanup.TryExecute());
            Assert.Equal(1, fixture.Navigation.ExecutionCount);
            Assert.Equal(WorkspaceKind.Maintenance, fixture.Navigation.LastParameter);
            Assert.Equal(3, fixture.Dashboard.MaintenanceTabIndex);
            Assert.Contains("预览并确认", cleanup.ContextPreview);
            fixture.AssertNoDangerousActionOrSelectionMutation();
        });
    }

    private sealed class ReviewFixture
    {
        public ReviewFixture()
        {
            Dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
            Picker = (GamePickerViewModel)FormatterServices.GetUninitializedObject(typeof(GamePickerViewModel));
            SetField(Dashboard, "gamePicker", Picker);
            SetField(Dashboard, "<Games>k__BackingField", new BatchObservableCollection<GameStatusDto>());
            SetField(Dashboard, "<Tasks>k__BackingField", new BatchObservableCollection<TaskStatusDto>());
            SetField(Dashboard, "currentWorkspace", WorkspaceKind.Overview);
            SetField(Dashboard, "saveTabIndex", 2);
            SetField(Dashboard, "maintenanceTabIndex", 1);
            SetField(Dashboard, "<OpenSelectedTaskGameCommand>k__BackingField", new RecordingCommand());
            foreach (var name in new[] { "RestoreCommand", "ApplyRetentionSimulationCommand", "PreviewRetentionCommand", "RefreshRetentionSimulationCommand" })
                SetField(Dashboard, "<" + name + ">k__BackingField", UnsafeActions);
            var game = new GameStatusDto { PlayniteId = "synthetic-review-game", Name = "审阅合成游戏" };
            Dashboard.Games.Add(game);
            SetField(Picker, "selectedItem", new GamePickerItem(game));
            SetField(Dashboard, "selectedBackup", ProtectedVersion);
            Snapshot = CommandDirectoryContextSnapshot.Capture(Dashboard);
            Items = CommandDirectoryCatalog.Create(Dashboard, Navigation, new RecordingCommand(),
                new RecordingCommand(), new RecordingCommand(), () => Snapshot);
        }

        public DashboardViewModel Dashboard { get; }
        public GamePickerViewModel Picker { get; }
        public CommandDirectoryContextSnapshot? Snapshot { get; set; }
        public System.Collections.Generic.IReadOnlyList<CommandDirectoryItem> Items { get; }
        public RecordingCommand Navigation { get; } = new RecordingCommand();
        private RecordingCommand UnsafeActions { get; } = new RecordingCommand();
        private BackupVersionDto ProtectedVersion { get; } = new BackupVersionDto
        {
            BackupId = "synthetic-protected-version", IsLocked = true, IsPreRestore = true
        };

        public void AssertNoDangerousActionOrSelectionMutation()
        {
            Assert.Equal(0, UnsafeActions.ExecutionCount);
            Assert.Same(ProtectedVersion, Dashboard.SelectedBackup);
            Assert.True(ProtectedVersion.IsLocked);
            Assert.True(ProtectedVersion.IsPreRestore);
        }
    }

    private sealed class RecordingCommand : ICommand
    {
        public bool Allowed { get; set; } = true;
        public int ExecutionCount { get; private set; }
        public object? LastParameter { get; private set; }
        public bool CanExecute(object? parameter) => Allowed;
        public void Execute(object? parameter) { ExecutionCount++; LastParameter = parameter; }
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }

    private static void SetField(object target, string name, object? value)
    {
        var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception error) { failure = error; }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)), "Review navigation WPF fixture did not finish.");
        if (failure != null) throw new InvalidOperationException("Review navigation fixture failed.", failure);
    }
}
