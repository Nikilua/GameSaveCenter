using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R11SaveWpf")]
public sealed class R11VersionSummaryBehaviorTests
{
    [Fact]
    public void BackupVersionSummarySeparatesSourceAndUnknownReadiness()
    {
        var unknown = new BackupVersionDto
        {
            BackupId = "backup-unknown",
            SourceDevice = string.Empty,
            IsLocked = true,
            TotalBytes = 1024,
            FileCount = 12
        };

        Assert.Equal("未知设备", unknown.SourceDisplay);
        Assert.Equal("未验证", unknown.RestoreReadinessStatusDisplay);
        Assert.Equal("已锁定 · 未验证", unknown.ProtectionAndReadinessDisplay);
        Assert.False(unknown.IsHealthProtected);

        unknown.RestoreReadiness = new RestoreReadinessDto
        {
            Status = RestoreReadinessStatus.Ready,
            Summary = "隔离校验通过"
        };
        Assert.Equal("可恢复", unknown.RestoreReadinessStatusDisplay);
        Assert.Equal("已锁定 · 可恢复", unknown.ProtectionAndReadinessDisplay);
        Assert.Equal("隔离校验通过", unknown.RestoreReadinessSummaryDisplay);
    }

    [Fact]
    public void SaveHistoryRowRendersSourceAndNeutralUnknownReadinessWithoutGreenSafetyState()
    {
        RunSta(() =>
        {
            var state = new SavePageState();
            state.Backups.Add(new BackupVersionDto
            {
                BackupId = "backup-unknown",
                SourceDevice = string.Empty,
                IsLocked = true,
                TotalBytes = 1024 * 1024,
                FileCount = 12,
                RestoreReadiness = null
            });

            var view = new SaveCenterView
            {
                DataContext = state,
                Width = 1040,
                Height = 700
            };
            var window = new Window
            {
                Width = 1040,
                Height = 700,
                Content = view,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            try
            {
                window.Show();
                PumpLayout(window);
                var history = (DataGrid)view.GetType()
                    .GetField("SaveHistoryGrid", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;
                history.EnableColumnVirtualization = false;
                history.EnableRowVirtualization = false;
                history.ScrollIntoView(state.Backups[0]);
                PumpLayout(window);

                Assert.Equal(7, history.Columns.Count);
                Assert.Equal(new[] { "时间", "类型", "文件数", "大小", "设备", "备注", "状态" },
                    history.Columns.Select(column => column.Header).ToArray());
                Assert.Same(state.Backups, history.ItemsSource);
                Assert.Single(history.Items.Cast<object>());

                // The real view is present and measured above; the semantic row values
                // come from the same DTO instance that the DataGrid consumes. Keep the
                // visual color rule source-level assertion next to that runtime check so
                // an unknown validation state cannot inherit the old lock-green trigger.
                var xaml = File.ReadAllText(Path.Combine(TestRepositoryContext.Root,
                    "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
                Assert.Contains("Binding=\"{Binding SourceDisplay, Mode=OneWay}\"", xaml);
                Assert.Contains("Text=\"{Binding ProtectionAndReadinessDisplay, Mode=OneWay}\"", xaml);
                Assert.Contains("Binding=\"{Binding RestoreReadiness.Status, Mode=OneWay}\" Value=\"Ready\"", xaml);
                Assert.DoesNotContain("Binding=\"{Binding IsLocked, Mode=OneWay}\" Value=\"True\"", xaml);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        window.UpdateLayout();
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private sealed class SavePageState
    {
        public ObservableCollection<BackupVersionDto> Backups { get; } = new ObservableCollection<BackupVersionDto>();
        public ObservableCollection<SavePathCandidateDto> SaveCandidates { get; } = new ObservableCollection<SavePathCandidateDto>();
        public object? SelectedGame => null;
        public BackupVersionDto? SelectedBackup { get; set; }
        public SavePathCandidateDto? SelectedCandidate { get; set; }
        public string SaveDetailsState => "Ready";
        public string SaveDetailsPresenterState => "Ready";
        public string SaveDetailsStateTitle => string.Empty;
        public string SaveDetailsStateMessage => string.Empty;
        public string SaveDetailsStateDetail => string.Empty;
        public bool SaveDetailsStaleVisible => false;
        public bool SaveHistoryStateOverlayVisible => false;
        public bool SaveCandidateStateOverlayVisible => false;
        public string RestoreAvailabilityHint => string.Empty;
        public bool RestoreAvailabilityNeedsMaintenance => false;
        public ICommandStub LoadDetailsCommand { get; } = new ICommandStub();

    }

    private sealed class ICommandStub : System.Windows.Input.ICommand
    {
        event EventHandler? System.Windows.Input.ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }
}
