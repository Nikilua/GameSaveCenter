using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Newtonsoft.Json;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R11SaveWpf")]
public sealed class R12RestorePlanDraftBehaviorTests
{
    [Fact]
    public void DraftSettingsRoundTripKeepsOnlyBoundedPathFreePlanningEvidence()
    {
        var settings = new GameSaveCenterSettings
        {
            RestorePlanDrafts = new List<RestorePlanDraftDefinition>
            {
                new()
                {
                    DraftId = Guid.NewGuid().ToString("N"),
                    CreatedUtc = new DateTime(2026, 10, 19, 1, 0, 0, DateTimeKind.Utc),
                    PlayniteId = "game-1",
                    GameName = "Demo Game",
                    TargetBackupId = "backup-2",
                    TargetCreatedUtc = new DateTime(2026, 10, 18, 1, 0, 0, DateTimeKind.Utc),
                    BaselineBackupId = "backup-1",
                    MappingFileCount = 3,
                    MappingTotalBytes = 1024,
                    MappingUnknownSizeCount = 1,
                    MappingPreviewId = Guid.NewGuid().ToString("N"),
                    MappingPreviewedUtc = new DateTime(2026, 10, 19, 0, 55, 0, DateTimeKind.Utc),
                    MappingFingerprintVersion = 1,
                    MappingFingerprint = new string('A', 64),
                    MappingFingerprintKeyId = Guid.NewGuid().ToString("N"),
                    ReadinessStatus = "Ready",
                    ReadinessCheckedUtc = new DateTime(2026, 10, 18, 23, 0, 0, DateTimeKind.Utc),
                    ReadinessBackupId = "backup-2"
                }
            }
        };

        var json = JsonConvert.SerializeObject(settings);
        var restored = JsonConvert.DeserializeObject<GameSaveCenterSettings>(json)!;

        var draft = Assert.Single(restored.RestorePlanDrafts);
        Assert.True(draft.IsReadOnly);
        Assert.Equal("backup-1", draft.BaselineBackupId);
        Assert.Equal("Ready", draft.ReadinessStatus);
        Assert.Contains("3 个文件", draft.MappingSummary);
        Assert.DoesNotContain("ArchivePath", json, StringComparison.Ordinal);
        Assert.DoesNotContain("save.dat", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Credential", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Authorization", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RestoreCommand", json, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(new string('A', 64), draft.MappingFingerprint);
    }

    [Fact]
    public void MappingFingerprintKeyIsDpapiProtectedAndSurvivesSettingsRoundTrip()
    {
        var settings = new GameSaveCenterSettings();
        string originalKey;
        string originalKeyId;
        using (var material = RestorePlanMappingFingerprintKeyStore.GetOrCreate(settings, () => { }))
        {
            originalKey = material.KeyBase64;
            originalKeyId = material.KeyId;
        }

        var json = JsonConvert.SerializeObject(settings);
        Assert.DoesNotContain(originalKey, json, StringComparison.Ordinal);

        var restored = JsonConvert.DeserializeObject<GameSaveCenterSettings>(json)!;
        using var reopened = RestorePlanMappingFingerprintKeyStore.GetOrCreate(restored, () => throw new InvalidOperationException("A valid protected key must not be rewritten."));
        Assert.Equal(originalKey, reopened.KeyBase64);
        Assert.Equal(originalKeyId, reopened.KeyId);
    }

    [Fact]
    public void ComparisonDistinguishesDifferentMappingsForSameVersionAndKeepsUnknownCountsUnknown()
    {
        var from = ComparisonDraft("backup-2", new string('A', 64), 1);
        var to = ComparisonDraft("backup-2", new string('B', 64), 0);

        var different = RestorePlanDraftComparison.Create(from, to);

        Assert.Equal(RestorePlanMappingComparisonState.Different, different.MappingState);
        Assert.Contains("同一目标版本", different.SourceDisplay, StringComparison.Ordinal);
        Assert.Contains("映射摘要不同", different.MappingDisplay, StringComparison.Ordinal);
        Assert.Contains("文件数 3 → 3（0）", different.EstimatedImpactDisplay, StringComparison.Ordinal);
        Assert.Contains("已知大小小计", different.EstimatedImpactDisplay, StringComparison.Ordinal);
        Assert.Contains("受影响路径数未知", different.UnknownsDisplay, StringComparison.Ordinal);
        Assert.Contains("A 1 / B 0", different.UnknownsDisplay, StringComparison.Ordinal);

        to.MappingFingerprint = string.Empty;
        to.MappingFingerprintVersion = 0;
        to.MappingFingerprintKeyId = string.Empty;
        to.MappingUnknownSizeCount = null;
        var unknown = RestorePlanDraftComparison.Create(from, to);
        Assert.Equal(RestorePlanMappingComparisonState.Unknown, unknown.MappingState);
        Assert.Contains("未知", unknown.MappingDisplay, StringComparison.Ordinal);
        Assert.Contains("B 未记录", unknown.UnknownsDisplay, StringComparison.Ordinal);
    }

    [Fact]
    public void SaveCenterExposesKeyboardOperableAtoBComparisonSelectorsAndResult()
    {
        RunSta(() =>
        {
            var state = new RestorePlanPageState(includeSecondDraft: true);
            var view = new SaveCenterView { DataContext = state, Width = 900, Height = 900 };
            var window = new Window { Width = 900, Height = 900, Content = view, ShowInTaskbar = false, WindowStyle = WindowStyle.None };
            try
            {
                window.Show();
                PumpLayout(window);
                view.ApplyResponsiveLayout(window.ActualWidth, window.ActualHeight);
                PumpLayout(window);

                var from = Assert.IsType<ComboBox>(view.FindName("RestorePlanCompareFromSelector"));
                var to = Assert.IsType<ComboBox>(view.FindName("RestorePlanCompareToSelector"));
                Assert.Same(state.RestorePlanDrafts, from.ItemsSource);
                Assert.Same(state.RestorePlanDrafts, to.ItemsSource);
                Assert.Equal("恢复方案比较起始方案 A", AutomationProperties.GetName(from));
                Assert.Equal("恢复方案比较目标方案 B", AutomationProperties.GetName(to));

                from.SelectedItem = state.Draft;
                to.SelectedItem = state.SecondDraft;
                PumpLayout(window);
                Assert.Same(state.Draft, state.SelectedRestorePlanDraftFrom);
                Assert.Same(state.SecondDraft, state.SelectedRestorePlanDraftTo);
                var compare = Assert.Single(FindVisualChildren<ButtonBase>(view), button => AutomationProperties.GetName(button) == "比较恢复方案 A 到 B");
                Assert.Same(state.CompareCommand, compare.Command);
                Assert.True(compare.IsEnabled);
                compare.Command.Execute(null);
                PumpLayout(window);

                var resultCard = Assert.IsType<Border>(view.FindName("RestorePlanDraftComparisonCard"));
                Assert.NotNull(state.RestorePlanDraftComparisonResult);
                Assert.Equal(Visibility.Visible, resultCard.Visibility);
                Assert.Equal(RestorePlanMappingComparisonState.Different, state.RestorePlanDraftComparisonResult!.MappingState);
                var labels = FindVisualChildren<TextBlock>(resultCard).Select(text => text.Text).ToArray();
                Assert.Contains(labels, text => text.IndexOf("A → B", StringComparison.Ordinal) >= 0);
                Assert.Contains(labels, text => text.IndexOf("映射摘要不同", StringComparison.Ordinal) >= 0);
                Assert.Contains(labels, text => text.IndexOf("受影响路径数未知", StringComparison.Ordinal) >= 0);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static RestorePlanDraftDefinition ComparisonDraft(string backupId, string fingerprint, int unknownSizeCount)
        => new()
        {
            DraftId = Guid.NewGuid().ToString("N"),
            PlayniteId = "game-1",
            GameName = "Demo Game",
            TargetBackupId = backupId,
            TargetCreatedUtc = DateTime.UtcNow.AddDays(-1),
            BaselineBackupId = "backup-1",
            MappingFileCount = 3,
            MappingTotalBytes = 1024,
            MappingUnknownSizeCount = unknownSizeCount,
            MappingPreviewedUtc = DateTime.UtcNow.AddMinutes(-5),
            MappingFingerprintVersion = 1,
            MappingFingerprint = fingerprint,
            MappingFingerprintKeyId = "7921d802aedb490ab9935cd8f2241968",
            ReadinessStatus = "Ready",
            ReadinessCheckedUtc = DateTime.UtcNow.AddMinutes(-10),
            ReadinessBackupId = backupId
        };

    [Fact]
    public void DraftCreationReferencesTheMatchingMappingAndReadinessCheck()
    {
        var game = new GameStatusDto { PlayniteId = "game-1", Name = "Demo Game" };
        var backup = new BackupVersionDto
        {
            PlayniteId = game.PlayniteId,
            BackupId = "backup-2",
            ParentBackupId = "backup-1",
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            RestoreReadiness = new RestoreReadinessDto
            {
                BackupVersionId = "backup-2",
                Status = RestoreReadinessStatus.Ready,
                CheckedUtc = DateTime.UtcNow.AddMinutes(-10)
            }
        };
        var mapping = new RestoreMappingPreviewDto
        {
            Success = true,
            PreviewId = "preview-1",
            PlayniteId = game.PlayniteId,
            BackupId = backup.BackupId,
            PreviewedUtc = DateTime.UtcNow,
            FileCount = 2,
            TotalBytes = 384
        };

        var draft = RestorePlanDraftDefinition.Create(game, backup, mapping);

        Assert.Equal("backup-1", draft.BaselineBackupId);
        Assert.Equal("preview-1", draft.MappingPreviewId);
        Assert.Equal("backup-2", draft.ReadinessBackupId);
        Assert.Equal("Ready", draft.ReadinessStatus);
        Assert.Contains("PreRestore", draft.ProtectionRequirements);
        Assert.Throws<InvalidOperationException>(() => RestorePlanDraftDefinition.Create(
            game,
            backup,
            new RestoreMappingPreviewDto { Success = true, PlayniteId = "other-game", BackupId = "backup-2" }));
    }

    [Fact]
    public void SavedDraftCollectionIsBoundedAndDeduplicated()
    {
        var duplicateId = Guid.NewGuid().ToString("N");
        var drafts = Enumerable.Range(0, RestorePlanDraftDefinition.MaximumSavedDrafts + 2)
            .Select(index => new RestorePlanDraftDefinition
            {
                DraftId = index < 2 ? duplicateId : Guid.NewGuid().ToString("N"),
                PlayniteId = "game-1",
                TargetBackupId = "backup-" + index,
                MappingFileCount = -1,
                MappingTotalBytes = -1
            })
            .ToList();

        var normalized = RestorePlanDraftDefinition.NormalizeMany(drafts);

        Assert.Equal(RestorePlanDraftDefinition.MaximumSavedDrafts, normalized.Count);
        Assert.Equal(normalized.Count, normalized.Select(item => item.DraftId).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(normalized, item =>
        {
            Assert.True(item.IsReadOnly);
            Assert.Equal(0, item.MappingFileCount);
            Assert.Equal(0, item.MappingTotalBytes);
        });
    }

    [Fact]
    public void SaveCenterRendersDraftEvidenceAndBindsOnlyPreviewSaveAndDeleteCommands()
    {
        RunSta(() =>
        {
            var state = new RestorePlanPageState();
            var view = new SaveCenterView { DataContext = state, Width = 900, Height = 900 };
            var window = new Window { Width = 900, Height = 900, Content = view, ShowInTaskbar = false, WindowStyle = WindowStyle.None };
            try
            {
                window.Show();
                PumpLayout(window);
                view.ApplyResponsiveLayout(window.ActualWidth, window.ActualHeight);
                PumpLayout(window);

                var detailsButton = Assert.IsAssignableFrom<ButtonBase>(view.FindName("SaveHistoryCompactDetailsButton"));
                Assert.Equal(Visibility.Visible, detailsButton.Visibility);
                detailsButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                PumpLayout(window);

                var previewCard = Assert.IsType<Border>(view.FindName("SaveRestorePlanMappingPreviewCard"));
                Assert.Equal(Visibility.Visible, previewCard.Visibility);
                var cardText = FindVisualChildren<TextBlock>(view).Select(text => text.Text).ToArray();
                Assert.Contains(cardText, text => text.IndexOf("backup-2", StringComparison.Ordinal) >= 0);
                Assert.Contains(cardText, text => text.IndexOf("基线版本：backup-1", StringComparison.Ordinal) >= 0);
                Assert.Contains(cardText, text => text.IndexOf("映射预览：2 个文件", StringComparison.Ordinal) >= 0);
                Assert.Contains(cardText, text => text.IndexOf("可恢复性结果：Ready", StringComparison.Ordinal) >= 0);
                Assert.Contains(cardText, text => text.IndexOf("PreRestore", StringComparison.Ordinal) >= 0);
                Assert.DoesNotContain(cardText, text => text.IndexOf("private-slot.sav", StringComparison.OrdinalIgnoreCase) >= 0);

                var buttons = FindVisualChildren<ButtonBase>(view).ToArray();
                var previewButton = Assert.Single(buttons, button => AutomationProperties.GetName(button) == "只读预览恢复映射");
                var saveButton = Assert.Single(buttons, button => AutomationProperties.GetName(button) == "保存只读恢复方案草稿");
                var deleteButton = Assert.Single(buttons, button => AutomationProperties.GetName(button) == "删除只读恢复方案草稿");
                Assert.Same(state.PreviewCommand, previewButton.Command);
                Assert.Same(state.SaveCommand, saveButton.Command);
                Assert.Same(state.DeleteCommand, deleteButton.Command);
                Assert.Same(state.Draft, deleteButton.CommandParameter);
                Assert.True(previewButton.IsEnabled);
                Assert.True(saveButton.IsEnabled);
                Assert.True(deleteButton.IsEnabled);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void RunSta(Action action)
    {
        Exception? error = null;
        var thread = new System.Threading.Thread(() =>
        {
            try { action(); }
            catch (Exception exception) { error = exception; }
        });
        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (error != null) throw error;
    }

    private static void PumpLayout(Window window)
    {
        window.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() => { }));
        window.UpdateLayout();
        window.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() => { }));
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, index);
            if (child is T typed) yield return typed;
            foreach (var nested in FindVisualChildren<T>(child)) yield return nested;
        }
    }

    private sealed class RestorePlanPageState : INotifyPropertyChanged
    {
        private RestorePlanDraftDefinition? selectedRestorePlanDraftFrom;
        private RestorePlanDraftDefinition? selectedRestorePlanDraftTo;
        private RestorePlanDraftComparison? restorePlanDraftComparisonResult;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<BackupVersionDto> Backups { get; } = new();
        public int SaveTabIndex { get; set; }
        public GameStatusDto? SelectedGame { get; set; } = new() { PlayniteId = "game-1", Name = "Demo Game" };
        public BackupVersionDto SelectedBackup { get; set; } = new()
        {
            BackupId = "backup-2",
            ParentBackupId = "backup-1",
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            RestoreReadiness = new RestoreReadinessDto
            {
                BackupVersionId = "backup-2",
                Status = RestoreReadinessStatus.Ready,
                CheckedUtc = DateTime.UtcNow.AddMinutes(-10)
            }
        };
        public string SelectedBackupLineageSummary => "父版本 backup-1";
        public IReadOnlyList<string> SelectedBackupLineageItems { get; } = new[] { "父版本 backup-1" };
        public string RestoreAvailabilityHint => string.Empty;
        public bool RestoreAvailabilityNeedsMaintenance => false;
        public string RestoreWorkflowSummary => "恢复流程等待检查。";
        public IReadOnlyList<RestoreWorkflowStepState> RestoreWorkflowSteps { get; } = RestoreWorkflowProgress.Build(
            new BackupVersionDto { BackupId = "backup-2" }, false, false, false, null, string.Empty, string.Empty);
        public string RestorePlanDraftStatus => "只读映射预览已生成。";
        public RestoreMappingPreviewDto RestorePlanMappingPreview { get; } = new()
        {
            Success = true,
            PreviewId = "preview-1",
            PlayniteId = "game-1",
            BackupId = "backup-2",
            PreviewedUtc = DateTime.UtcNow,
            FileCount = 2,
            TotalBytes = 384,
            Summary = "映射预览：2 个文件 · 384 B；只读核对，不写入当前存档。"
        };
        public RestorePlanDraftDefinition Draft { get; } = new()
        {
            DraftId = "draft-1",
            CreatedUtc = DateTime.UtcNow,
            PlayniteId = "game-1",
            GameName = "Demo Game",
            TargetBackupId = "backup-2",
            TargetCreatedUtc = DateTime.UtcNow.AddDays(-1),
            BaselineBackupId = "backup-1",
            MappingFileCount = 2,
            MappingTotalBytes = 384,
            MappingPreviewId = "preview-1",
            MappingPreviewedUtc = DateTime.UtcNow,
            MappingFingerprintVersion = 1,
            MappingFingerprint = new string('A', 64),
            MappingFingerprintKeyId = "7921d802aedb490ab9935cd8f2241968",
            ReadinessStatus = "Ready",
            ReadinessCheckedUtc = DateTime.UtcNow.AddMinutes(-10),
            ReadinessBackupId = "backup-2"
        };
        public ObservableCollection<RestorePlanDraftDefinition> RestorePlanDrafts { get; } = new();
        public ICommand PreviewCommand { get; } = new TrackingCommand();
        public ICommand SaveCommand { get; } = new TrackingCommand();
        public ICommand DeleteCommand { get; } = new TrackingCommand();
        public RestorePlanDraftDefinition? SelectedRestorePlanDraftFrom
        {
            get => selectedRestorePlanDraftFrom;
            set { selectedRestorePlanDraftFrom = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedRestorePlanDraftFrom))); }
        }
        public RestorePlanDraftDefinition? SelectedRestorePlanDraftTo
        {
            get => selectedRestorePlanDraftTo;
            set { selectedRestorePlanDraftTo = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedRestorePlanDraftTo))); }
        }
        public RestorePlanDraftComparison? RestorePlanDraftComparisonResult
        {
            get => restorePlanDraftComparisonResult;
            private set { restorePlanDraftComparisonResult = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RestorePlanDraftComparisonResult))); }
        }
        public ICommand CompareCommand { get; }
        public ICommand CompareRestorePlanDraftsCommand => CompareCommand;
        public ICommand PreviewRestorePlanMappingCommand => PreviewCommand;
        public ICommand SaveRestorePlanDraftCommand => SaveCommand;
        public ICommand DeleteRestorePlanDraftCommand => DeleteCommand;
        public ICommand ValidateRestoreReadinessCommand { get; } = new TrackingCommand();
        public ICommand RestoreCommand { get; } = new TrackingCommand();
        public ICommand UndoRestoreCommand { get; } = new TrackingCommand();

        public RestorePlanPageState()
            : this(includeSecondDraft: false)
        {
        }

        public RestorePlanPageState(bool includeSecondDraft)
        {
            Backups.Add(SelectedBackup);
            RestorePlanDrafts.Add(Draft);
            SecondDraft = ComparisonDraft("backup-2", new string('B', 64), 0);
            if (includeSecondDraft) RestorePlanDrafts.Add(SecondDraft);
            CompareCommand = new TrackingCommand(() =>
            {
                if (SelectedRestorePlanDraftFrom != null && SelectedRestorePlanDraftTo != null)
                    RestorePlanDraftComparisonResult = RestorePlanDraftComparison.Create(SelectedRestorePlanDraftFrom, SelectedRestorePlanDraftTo);
            });
        }

        public RestorePlanDraftDefinition SecondDraft { get; }
    }

    private sealed class TrackingCommand : System.Windows.Input.ICommand
    {
        private readonly Action? execute;
        public TrackingCommand(Action? execute = null) => this.execute = execute;
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute?.Invoke();
    }
}
