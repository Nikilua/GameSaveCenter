using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Threading;
using Microsoft.Win32;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Playnite.Infrastructure;
using Playnite.SDK;

namespace GameSaveCenter.Playnite.ViewModels
{
    /// <summary>Apple-inspired dashboard state; all file operations remain in the Worker.</summary>
    public sealed partial class DashboardViewModel : ObservableObject
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly GameSaveCenterPlugin plugin;
        private readonly GamePickerViewModel gamePicker;
        private readonly PlayniteGameIconProvider gameIconProvider;
        private readonly SynchronizationContext? uiSynchronizationContext = SynchronizationContext.Current;
        private readonly Dictionary<string, TaskState> knownTaskStates = new Dictionary<string, TaskState>(StringComparer.OrdinalIgnoreCase);
        private readonly TaskIndexedCollection taskIndex = new TaskIndexedCollection();
        private readonly DateTime dashboardOpenedUtc = DateTime.UtcNow;
        private bool isBusy;
        private bool isBackgroundRefreshing;
        private bool isCancellingTask;
        private bool taskSnapshotInitialized;
        private bool initialSelectionApplied;
        private string? pendingAutoSelectPlayniteId;
        private string? lastStartedPlayniteId;
        private ImageSource selectedGameIcon = null!;
        private long lastTaskEventSequence;
        private CancellationTokenSource? taskEventSubscription;
        private CancellationTokenSource? gamePickerPersistenceCancellation;
        private CancellationTokenSource? detailsLoadCancellation;
        private CancellationTokenSource? initialSynchronizationCancellation;
        private long deferredUiWorkGeneration;
        private long detailsLoadGeneration;
        private Task? taskEventListener;
        private bool commandRefreshScheduled;
        private readonly DebouncedRefresh taskSearchRefresh;
        private readonly DebouncedRefresh mediaSearchRefresh;
        private DateTime lastFullDashboardRefreshUtc=DateTime.MinValue;
        private string statusMessage = "准备就绪";
        private BackupVersionDto selectedBackup = null!;
        private DashboardSnapshotDto snapshot = new DashboardSnapshotDto();
        private EnvironmentCheckReportDto environmentCheck = new EnvironmentCheckReportDto();
        private bool environmentCheckLoaded;
        private RecentProtectionSummary recentProtection = new RecentProtectionSummary(30, 0, 0, 0, 0, Array.Empty<RecentProtectionItem>());
        private SavePathCandidateDto selectedCandidate = null!;
        private string backupComment = string.Empty;
        private bool lockSelectedBackup;
        private MediaItemDto selectedMedia = null!;
        private MediaStorageSummaryDto mediaSummary = new MediaStorageSummaryDto();
        private string mediaComment = string.Empty;
        private bool mediaFavorite;
        private string mediaSearchText = string.Empty;
        private string mediaFilter = "全部";
        private GameStatusDto mediaTargetGame = null!;
        private MediaItemDto selectedInboxMedia = null!;
        private GameStatusDto inboxTargetGame = null!;
        private TaskStatusDto selectedTask = null!;
        private ValidationFindingDto selectedFinding = null!;
        private WorkerSettingsSnapshotDto effectiveSettings = new WorkerSettingsSnapshotDto();
        private string diagnosticSummary = "诊断信息尚未加载。";
        private string diffSummary = string.Empty;
        private string retentionSummary = string.Empty;
        private BackupPolicyTemplateDto selectedPolicyTemplate = null!;
        private BackupPolicyTemplateDto policyTemplateDraft = new BackupPolicyTemplateDto();
        private string policyTemplateNameDraft = string.Empty;
        private bool policyTemplatesLoaded;
        private BackupDiffDto? lastBackupDiff;
        private RetentionPreviewDto? lastRetentionPreview;
        private bool suppressSelectionLoad;
        private string gameSearchText = string.Empty;
        private string gameStatusFilter = "全部";
        private string gameSortMode = "名称";
        private int filteredGameCount;
        private WorkspaceKind currentWorkspace = WorkspaceKind.Overview;
        private LayoutMode layoutMode = LayoutMode.Standard;
        private GameToolDto selectedGameTool = null!;
        private GameToolVersionDto selectedGameToolVersion = null!;
        private TrainerCatalogItemDto selectedTrainerCatalogItem = null!;
        private TrainerReleaseDto selectedTrainerRelease = null!;
        private string trainerSearchText = string.Empty;
        private bool showTrainerLibrary;
        private string taskStatusFilter = "全部";
        private string taskGameFilter = "全部";
        private string taskTypeFilter = "全部";
        private string taskSearchText = string.Empty;
        private string deviceStateMessage = "尚未刷新多设备状态。该功能只比较摘要，绝不自动恢复或覆盖存档。";
        private DeviceConflictStatusDto selectedDeviceComparison = null!;
        private string deviceDecision = "稍后处理";
        private string deviceDecisionComment = string.Empty;
        private RemoteBackupStageResultDto? stagedRemoteBackup;
        private string stagedRemoteBackupStatus = "尚未下载远端存档。下载只会写入本机隔离区，不会覆盖当前存档。";
        private string processMappingExecutable = string.Empty;
        private GameStatusDto processMappingTargetGame = null!;
        private ProcessMappingDto selectedProcessMapping = null!;
        private GameToolEntryCandidateDto selectedImportEntryCandidate = null!;
        private string pendingGameToolImportSource = string.Empty;
        private GameToolType pendingGameToolImportType = GameToolType.Trainer;
        private bool hasPendingGameToolEntrySelection;
        private readonly RecentProtectionAssessmentService recentProtectionAssessment = new RecentProtectionAssessmentService();

        public DashboardViewModel(GameSaveCenterPlugin plugin)
        {
            this.plugin = plugin;
            if (!plugin.Settings.OnboardingCompleted) currentWorkspace = WorkspaceKind.Maintenance;
            gamePicker = new GamePickerViewModel();
            gamePicker.ApplyPersistedState(plugin.Settings.GamePickerSearchText, plugin.Settings.GamePickerStatusFilter, plugin.Settings.GamePickerPlatformFilter, plugin.Settings.GamePickerSortMode);
            gamePicker.StateChanged += OnGamePickerStateChanged;
            gamePicker.PropertyChanged += OnGamePickerPropertyChanged;
            gameIconProvider = new PlayniteGameIconProvider(plugin.PlayniteApi);
            plugin.PlayniteGameStarted += OnPlayniteGameStarted;
            gameSearchText = gamePicker.SearchText;
            gameStatusFilter = gamePicker.StatusFilter;
            gameSortMode = gamePicker.SortMode;
            GamesView = CollectionViewSource.GetDefaultView(Games);
            GamesView.Filter = FilterGame;
            TasksView = CollectionViewSource.GetDefaultView(Tasks);
            TasksView.Filter = FilterTask;
            MediaView = CollectionViewSource.GetDefaultView(Media);
            MediaView.Filter = FilterMedia;
            taskSearchRefresh = new DebouncedRefresh(() => ApplyOnUi(RefreshTasksView), TimeSpan.FromMilliseconds(180));
            mediaSearchRefresh = new DebouncedRefresh(() => ApplyOnUi(RefreshMediaView), TimeSpan.FromMilliseconds(180));
            ApplyGameSort();
            RefreshCommand = new RelayCommand(_ => Run(RefreshAsync), _ => !IsBusy);
            BackupSelectedCommand = new RelayCommand(_ => Run(BackupSelectedAsync), _ => !IsBusy && SelectedGame != null && SelectedGame.LudusaviMatched && Snapshot.LudusaviAvailable);
            BackupAllCommand = new RelayCommand(_ => Run(BackupAllAsync), _ => !IsBusy && Snapshot.LudusaviAvailable && Games.Any(x => x.LudusaviMatched));
            SyncMediaCommand = new RelayCommand(_ => Run(SyncMediaAsync), _ => !IsBusy);
            DetectPathsCommand = new RelayCommand(_ => Run(DetectPathsAsync), _ => !IsBusy && SelectedGame != null);
            ValidateCommand = new RelayCommand(_ => Run(ValidateAsync), _ => !IsBusy && SelectedGame != null && SelectedGame.LudusaviMatched);
            RestoreCommand = new RelayCommand(_ => Run(RestoreAsync), _ => !IsBusy && SelectedGame != null && SelectedBackup != null && Snapshot.LudusaviAvailable);
            ValidateRestoreReadinessCommand = new RelayCommand(_ => Run(ValidateRestoreReadinessAsync), _ => !IsBusy && SelectedGame != null && SelectedBackup != null);
            UndoRestoreCommand = new RelayCommand(_ => Run(UndoRestoreAsync), _ => !IsBusy && SelectedGame != null && Backups.Any(x => x.IsPreRestore));
            LoadDetailsCommand = new RelayCommand(_ => Run(() => LoadDetailsAsync(true)), _ => !IsBusy && SelectedGame != null);
            SavePolicyCommand = new RelayCommand(_ => Run(SavePolicyAsync), _ => !IsBusy && SelectedGame != null);
            CreatePolicyTemplateCommand = new RelayCommand(_ => CreatePolicyTemplate(), _ => !IsBusy);
            SavePolicyTemplateCommand = new RelayCommand(_ => Run(SavePolicyTemplateAsync), _ => !IsBusy && PolicyTemplateDraft != null && !PolicyTemplateDraft.IsBuiltIn && !string.IsNullOrWhiteSpace(PolicyTemplateNameDraft));
            ApplyPolicyTemplateCommand = new RelayCommand(_ => Run(ApplyPolicyTemplateAsync), _ => !IsBusy && SelectedGame != null && SelectedPolicyTemplate != null && !string.IsNullOrWhiteSpace(SelectedPolicyTemplate.TemplateId));
            DeletePolicyTemplateCommand = new RelayCommand(_ => Run(DeletePolicyTemplateAsync), _ => !IsBusy && PolicyTemplateDraft != null && !PolicyTemplateDraft.IsBuiltIn && !string.IsNullOrWhiteSpace(PolicyTemplateDraft.TemplateId));
            UpdateBackupMetadataCommand = new RelayCommand(_ => Run(UpdateBackupMetadataAsync), _ => !IsBusy && SelectedGame != null && SelectedBackup != null);
            CompareBackupCommand = new RelayCommand(_ => Run(CompareBackupAsync), _ => !IsBusy && SelectedGame != null && SelectedBackup != null && Backups.IndexOf(SelectedBackup) >= 0 && Backups.IndexOf(SelectedBackup) + 1 < Backups.Count);
            PreviewRetentionCommand = new RelayCommand(_ => Run(PreviewRetentionAsync), _ => !IsBusy && SelectedGame != null && Backups.Count > 0);
            AddMediaSourceCommand = new RelayCommand(_ => Run(AddMediaSourceAsync), _ => !IsBusy && SelectedGame != null);
            UpdateMediaSourceCommand = new RelayCommand(value => Run(() => UpdateMediaSourceAsync(value as MediaSourceRuleDto)), _ => !IsBusy);
            DeleteMediaSourceCommand = new RelayCommand(value => Run(() => DeleteMediaSourceAsync(value as MediaSourceRuleDto)), _ => !IsBusy);
            AcceptCandidateCommand = new RelayCommand(_ => Run(AcceptCandidateAsync), _ => !IsBusy && SelectedGame != null && SelectedCandidate != null && !string.Equals(SelectedCandidate.Status, "Accepted", StringComparison.OrdinalIgnoreCase));
            RejectCandidateCommand = new RelayCommand(_ => Run(RejectCandidateAsync), _ => !IsBusy && SelectedGame != null && SelectedCandidate != null && !string.Equals(SelectedCandidate.Status, "Accepted", StringComparison.OrdinalIgnoreCase));
            ReassignMediaCommand = new RelayCommand(_ => Run(ReassignMediaAsync), _ => !IsBusy && SelectedMedia != null && MediaTargetGame != null);
            UpdateMediaMetadataCommand = new RelayCommand(_ => Run(UpdateMediaMetadataAsync), _ => !IsBusy && SelectedMedia != null);
            FavoriteSelectedMediaCommand = new RelayCommand(value => Run(() => UpdateMediaMetadataBatchAsync(value, true, false)), _ => !IsBusy);
            UnfavoriteSelectedMediaCommand = new RelayCommand(value => Run(() => UpdateMediaMetadataBatchAsync(value, false, false)), _ => !IsBusy);
            CommentSelectedMediaCommand = new RelayCommand(value => Run(() => UpdateMediaMetadataBatchAsync(value, null, true)), _ => !IsBusy);
            OpenSelectedMediaCommand = new RelayCommand(_ => RunLocal(OpenSelectedMedia), _ => SelectedMedia != null && !string.IsNullOrWhiteSpace(SelectedMedia.ArchivePath));
            RevealSelectedMediaCommand = new RelayCommand(_ => RunLocal(() => OpenPath(SelectedMedia.ArchivePath)), _ => SelectedMedia != null && !string.IsNullOrWhiteSpace(SelectedMedia.ArchivePath));
            AssignInboxMediaCommand = new RelayCommand(_ => Run(AssignInboxMediaAsync), _ => !IsBusy && SelectedInboxMedia != null && InboxTargetGame != null);
            IgnoreInboxMediaCommand = new RelayCommand(_ => Run(IgnoreInboxMediaAsync), _ => !IsBusy && SelectedInboxMedia != null);
            CancelTaskCommand = new RelayCommand(_ => _ = CancelSelectedTaskAsync(), _ => SelectedTask != null && SelectedTask.CanCancel && !IsCancellingTask);
            RetryTaskCommand = new RelayCommand(_ => Run(RetrySelectedTaskAsync), _ => !IsBusy && CanRetrySelectedTask());
            CopyTaskErrorCommand = new RelayCommand(_ => RunLocal(CopySelectedTaskError), _ => SelectedTask != null && !string.IsNullOrWhiteSpace(SelectedTask.DetailMessage));
            OpenAttentionCenterCommand = new RelayCommand(_ => OpenAttentionCenter());
            OpenAttentionFindingCommand = new RelayCommand(value => OpenAttentionFinding(value as ValidationFindingDto));
            OpenProtectionGamesCommand = new RelayCommand(_ => OpenProtectionGames());
            OpenProtectionItemCommand = new RelayCommand(value => OpenProtectionItem(value as RecentProtectionItem));
            RefreshDiagnosticsCommand = new RelayCommand(_ => Run(RefreshDiagnosticsAsync), _ => !IsBusy);
            RunEnvironmentCheckCommand = new RelayCommand(_ => Run(RunEnvironmentCheckAsync), _ => !IsBusy);
            SkipOnboardingCommand = new RelayCommand(_ => SkipOnboarding(), _ => !IsBusy && IsOnboardingPending);
            CompleteOnboardingCommand = new RelayCommand(_ => CompleteOnboarding(), _ => !IsBusy && IsOnboardingPending && EnvironmentCheck.IsReady && EnvironmentCheck.CheckedUtc != default(DateTime));
            OnboardingTestBackupCommand = new RelayCommand(_ => Run(BackupSelectedAsync), _ => !IsBusy && IsOnboardingPending && SelectedGame != null && SelectedGame.LudusaviMatched && Snapshot.LudusaviAvailable);
            SyncDeviceStatesCommand = new RelayCommand(_ => Run(SyncDeviceStatesAsync), _ => !IsBusy);
            SaveDeviceDecisionCommand = new RelayCommand(_ => Run(SaveDeviceDecisionAsync), _ => !IsBusy && SelectedDeviceComparison != null);
            StageRemoteBackupCommand = new RelayCommand(_ => Run(StageRemoteBackupAsync), _ => !IsBusy && SelectedDeviceComparison != null && !string.IsNullOrWhiteSpace(SelectedDeviceComparison.RemoteBackupId));
            RestoreStagedRemoteBackupCommand = new RelayCommand(_ => Run(RestoreStagedRemoteBackupAsync), _ => !IsBusy && StagedRemoteBackup != null && StagedRemoteBackup.Verified);
            SaveProcessMappingCommand = new RelayCommand(_ => Run(SaveProcessMappingAsync), _ => !IsBusy && !string.IsNullOrWhiteSpace(ProcessMappingExecutable) && ProcessMappingTargetGame != null);
            DeleteProcessMappingCommand = new RelayCommand(_ => Run(DeleteProcessMappingAsync), _ => !IsBusy && SelectedProcessMapping != null);
            CopyDiagnosticsCommand = new RelayCommand(_ => RunLocal(CopyDiagnostics), _ => !string.IsNullOrWhiteSpace(DiagnosticSummary));
            OpenDataDirectoryCommand = new RelayCommand(_ => RunLocal(() => OpenPath(EffectiveSettings.DataDirectory)), _ => !string.IsNullOrWhiteSpace(EffectiveSettings.DataDirectory));
            OpenBackupDirectoryCommand = new RelayCommand(_ => RunLocal(() => OpenPath(EffectiveSettings.LudusaviBackupDirectory)), _ => !string.IsNullOrWhiteSpace(EffectiveSettings.LudusaviBackupDirectory));
            OpenMediaDirectoryCommand = new RelayCommand(_ => RunLocal(() => OpenPath(EffectiveSettings.MediaArchiveDirectory)), _ => !string.IsNullOrWhiteSpace(EffectiveSettings.MediaArchiveDirectory));
            OpenWorkerLogCommand = new RelayCommand(_ => RunLocal(OpenWorkerLog));
            ImportTrainerCommand = new RelayCommand(_ => Run(() => ImportGameToolAsync(GameToolType.Trainer)), _ => !IsBusy && SelectedGame != null);
            ImportCheatTableCommand = new RelayCommand(_ => Run(() => ImportGameToolAsync(GameToolType.CheatTable)), _ => !IsBusy && SelectedGame != null);
            ImportCustomLaunchItemCommand = new RelayCommand(_ => Run(ImportCustomLaunchItemAsync), _ => !IsBusy && SelectedGame != null);
            ImportToolFolderCommand = new RelayCommand(_ => Run(ImportGameToolFolderAsync), _ => !IsBusy && SelectedGame != null);
            ConfirmGameToolImportCommand = new RelayCommand(_ => Run(ConfirmGameToolImportAsync), _ => !IsBusy && HasPendingGameToolEntrySelection && SelectedImportEntryCandidate != null);
            CancelGameToolImportCommand = new RelayCommand(_ => ClearPendingGameToolImport(), _ => HasPendingGameToolEntrySelection);
            SaveGameToolCommand = new RelayCommand(_ => Run(SaveSelectedGameToolAsync), _ => !IsBusy && SelectedGameTool != null);
            LaunchGameToolCommand = new RelayCommand(_ => Run(LaunchSelectedGameToolAsync), _ => !IsBusy && SelectedGameTool != null && SelectedGameTool.ActiveVersion.IsAvailable);
            OpenGameToolDirectoryCommand = new RelayCommand(_ => Run(OpenSelectedGameToolDirectoryAsync), _ => !IsBusy && SelectedGameTool != null);
            DeleteGameToolCommand = new RelayCommand(_ => Run(DeleteSelectedGameToolAsync), _ => !IsBusy && SelectedGameTool != null);
            RelocateGameToolCommand = new RelayCommand(_ => Run(RelocateSelectedGameToolAsync), _ => !IsBusy && SelectedGameTool != null && SelectedGameTool.IsExternalReference);
            SyncTrainerCatalogCommand = new RelayCommand(_ => Run(SyncTrainerCatalogAsync), _ => !IsBusy);
            SearchTrainerCatalogCommand = new RelayCommand(_ => Run(SearchTrainerCatalogAsync), _ => !IsBusy);
            LoadTrainerReleasesCommand = new RelayCommand(_ => Run(LoadTrainerReleasesAsync), _ => !IsBusy && SelectedTrainerCatalogItem != null);
            DownloadTrainerCommand = new RelayCommand(_ => Run(DownloadTrainerAsync), _ => !IsBusy && SelectedGame != null && SelectedTrainerRelease != null);
            // Initial rendering is cache-first and must not pass through RunAsync: that helper
            // waits for Worker startup before doing anything and marks the whole dashboard busy.
            // On a large Playnite library this made opening the panel look hung even though the
            // durable snapshot was already available.  Initialization now renders independently
            // and lets the existing background synchronization establish the Worker connection.
            Observe(InitializeAsync());
        }

        public BatchObservableCollection<GameStatusDto> Games { get; } = new BatchObservableCollection<GameStatusDto>();
        /// <summary>Shared global picker state. The dashboard keeps the legacy bindings below for compatibility.</summary>
        public GamePickerViewModel GamePicker => gamePicker;
        public BatchObservableCollection<TaskStatusDto> Tasks { get; } = new BatchObservableCollection<TaskStatusDto>();
        public BatchObservableCollection<TaskStatusDto> OverviewTasks { get; } = new BatchObservableCollection<TaskStatusDto>();
        public ObservableCollection<string> TaskGameFilterOptions { get; } = new ObservableCollection<string> { "全部" };
        public ObservableCollection<string> TaskTypeFilterOptions { get; } = new ObservableCollection<string> { "全部" };
        public BatchObservableCollection<ValidationFindingDto> Findings { get; } = new BatchObservableCollection<ValidationFindingDto>();
        /// <summary>Small overview projection so a warning count always has a visible reason.</summary>
        public BatchObservableCollection<ValidationFindingDto> AttentionFindings { get; } = new BatchObservableCollection<ValidationFindingDto>();
        public BatchObservableCollection<DeviceConflictStatusDto> DeviceComparisons { get; } = new BatchObservableCollection<DeviceConflictStatusDto>();
        public IReadOnlyList<string> DeviceDecisionOptions { get; } = new[] { "稍后处理", "保留两者", "以本机为准", "以远端为准" };
        public BatchObservableCollection<ProcessMappingDto> ProcessMappings { get; } = new BatchObservableCollection<ProcessMappingDto>();
        public BatchObservableCollection<BackupVersionDto> Backups { get; } = new BatchObservableCollection<BackupVersionDto>();
        public BatchObservableCollection<BackupPolicyTemplateDto> PolicyTemplates { get; } = new BatchObservableCollection<BackupPolicyTemplateDto>();
        public BatchObservableCollection<MediaItemDto> Media { get; } = new BatchObservableCollection<MediaItemDto>();
        private BatchObservableCollection<MediaItemDto> unassignedMedia = new BatchObservableCollection<MediaItemDto>();
        public BatchObservableCollection<MediaItemDto> UnassignedMedia
        {
            get => unassignedMedia;
            private set => SetValue(ref unassignedMedia, value);
        }
        public BatchObservableCollection<AuditLogEntryDto> Audit { get; } = new BatchObservableCollection<AuditLogEntryDto>();
        public BatchObservableCollection<SavePathCandidateDto> SaveCandidates { get; } = new BatchObservableCollection<SavePathCandidateDto>();
        public BatchObservableCollection<MediaSourceRuleDto> MediaSources { get; } = new BatchObservableCollection<MediaSourceRuleDto>();
        public BatchObservableCollection<GameToolDto> GameTools { get; } = new BatchObservableCollection<GameToolDto>();
        public BatchObservableCollection<GameToolEntryCandidateDto> ImportEntryCandidates { get; } = new BatchObservableCollection<GameToolEntryCandidateDto>();
        public BatchObservableCollection<TrainerCatalogItemDto> TrainerCatalogResults { get; } = new BatchObservableCollection<TrainerCatalogItemDto>();
        public BatchObservableCollection<TrainerReleaseDto> TrainerReleases { get; } = new BatchObservableCollection<TrainerReleaseDto>();
        public ICollectionView GamesView { get; }
        public ICollectionView TasksView { get; }
        public ICollectionView MediaView { get; }
        public IReadOnlyList<string> MediaFilterOptions { get; } = new[] { "全部", "截图", "录像", "收藏" };
        public IReadOnlyList<string> GameStatusFilterOptions { get; } = new[] { "全部", "已就绪", "未匹配", "运行中", "需关注", "有历史" };
        public IReadOnlyList<string> GameSortOptions { get; } = new[] { "名称", "运行优先", "匹配优先", "最近备份" };

        public DashboardSnapshotDto Snapshot { get => snapshot; private set => SetValue(ref snapshot, value); }
        public EnvironmentCheckReportDto EnvironmentCheck { get => environmentCheck; private set { SetValue(ref environmentCheck, value ?? new EnvironmentCheckReportDto()); RaiseCommandStates(); } }
        public bool IsOnboardingPending => !plugin.Settings.OnboardingCompleted;
        public string OnboardingTitle => IsOnboardingPending ? "首次使用：准备环境" : "环境检查";
        public string OnboardingDescription => IsOnboardingPending
            ? "先确认 Worker、目录、SQLite 与备份工具可用。所有检查都是非破坏性的；你可以跳过，之后随时在维护中心重新运行。"
            : "重新运行非破坏性环境检查，确认备份链路仍然可用。";
        public RecentProtectionSummary RecentProtection { get => recentProtection; private set => SetValue(ref recentProtection, value); }
        public WorkerSettingsSnapshotDto EffectiveSettings
        {
            get => effectiveSettings;
            private set
            {
                SetValue(ref effectiveSettings, value);
                RaiseCommandStates();
            }
        }
        public bool IsBusy
        {
            get => isBusy;
            private set
            {
                SetValue(ref isBusy, value);
                RaiseCommandStates();
            }
        }
        public bool IsBackgroundRefreshing { get => isBackgroundRefreshing; private set => SetValue(ref isBackgroundRefreshing, value); }
        public bool IsCancellingTask
        {
            get => isCancellingTask;
            private set
            {
                SetValue(ref isCancellingTask, value);
                RaiseCommandStates();
            }
        }
        public string StatusMessage { get => statusMessage; private set => SetValue(ref statusMessage, value); }
        public string DiagnosticSummary { get => diagnosticSummary; private set => SetValue(ref diagnosticSummary, value); }
        public string GameSearchText
        {
            get => gameSearchText;
            set
            {
                SetValue(ref gameSearchText, value ?? string.Empty);
                if (!string.Equals(gamePicker.SearchText, gameSearchText, StringComparison.Ordinal)) gamePicker.SearchText = gameSearchText;
                RefreshGameView();
            }
        }
        public string GameStatusFilter
        {
            get => gameStatusFilter;
            set
            {
                SetValue(ref gameStatusFilter, string.IsNullOrWhiteSpace(value) ? "全部" : value);
                var pickerFilter = gameStatusFilter == "全部" ? "全部" : gameStatusFilter;
                if (!string.Equals(gamePicker.StatusFilter, pickerFilter, StringComparison.Ordinal)) gamePicker.StatusFilter = pickerFilter;
                RefreshGameView();
            }
        }
        public string GameSortMode
        {
            get => gameSortMode;
            set
            {
                SetValue(ref gameSortMode, string.IsNullOrWhiteSpace(value) ? "名称" : value);
                if (!string.Equals(gamePicker.SortMode, gameSortMode, StringComparison.Ordinal)) gamePicker.SortMode = gameSortMode;
                ApplyGameSort();
                RefreshGameView();
            }
        }
        public IReadOnlyList<string> TaskStatusFilterOptions { get; } = new[] { "全部", "运行中", "等待中", "失败", "已完成" };
        public int RunningTaskCount => Tasks.Count(task => task.State == TaskState.Running);
        public int RetryableTaskCount => Tasks.Count(CanRetryTask);
        public int CompletedTaskCount => Tasks.Count(task => task.State == TaskState.Succeeded);
        public string TaskSearchText
        {
            get => taskSearchText;
            set
            {
                SetValue(ref taskSearchText, value ?? string.Empty);
                taskSearchRefresh.Schedule(value);
            }
        }
        public string TaskStatusFilter
        {
            get => taskStatusFilter;
            set
            {
                SetValue(ref taskStatusFilter, string.IsNullOrWhiteSpace(value) ? "全部" : value);
                TasksView.Refresh();
            }
        }
        public string TaskGameFilter
        {
            get => taskGameFilter;
            set
            {
                SetValue(ref taskGameFilter, string.IsNullOrWhiteSpace(value) ? "全部" : value);
                TasksView.Refresh();
            }
        }
        public string TaskTypeFilter
        {
            get => taskTypeFilter;
            set
            {
                SetValue(ref taskTypeFilter, string.IsNullOrWhiteSpace(value) ? "全部" : value);
                TasksView.Refresh();
            }
        }
        public int FilteredGameCount { get => filteredGameCount; private set => SetValue(ref filteredGameCount, value); }
        public WorkspaceKind CurrentWorkspace { get => currentWorkspace; set => SetValue(ref currentWorkspace, value); }
        public LayoutMode LayoutMode { get => layoutMode; set => SetValue(ref layoutMode, value); }
        public bool ShowTrainerLibrary { get => showTrainerLibrary; set => SetValue(ref showTrainerLibrary, value); }
        public string TrainerSearchText { get => trainerSearchText; set => SetValue(ref trainerSearchText, value ?? string.Empty); }
        public GameToolDto SelectedGameTool
        {
            get => selectedGameTool;
            set
            {
                SetValue(ref selectedGameTool,value);
                SelectedGameToolVersion=value==null?null!:value.ActiveVersion;
                RaiseCommandStates();
            }
        }
        public GameToolVersionDto SelectedGameToolVersion
        {
            get => selectedGameToolVersion;
            set
            {
                SetValue(ref selectedGameToolVersion,value);
                if(selectedGameTool!=null&&value!=null)selectedGameTool.ActiveVersionId=value.VersionId;
                RaiseCommandStates();
            }
        }
        public TrainerCatalogItemDto SelectedTrainerCatalogItem
        {
            get => selectedTrainerCatalogItem;
            set { SetValue(ref selectedTrainerCatalogItem,value); TrainerReleases.Clear(); SelectedTrainerRelease=null!; RaiseCommandStates(); }
        }
        public TrainerReleaseDto SelectedTrainerRelease
        {
            get => selectedTrainerRelease;
            set { SetValue(ref selectedTrainerRelease,value); RaiseCommandStates(); }
        }
        public GameStatusDto SelectedGame
        {
            get => gamePicker.SelectedGame!;
            set => gamePicker.SelectGame(value);
        }
        public ImageSource SelectedGameIcon
        {
            get => selectedGameIcon;
            private set => SetValue(ref selectedGameIcon, value);
        }
        public BackupVersionDto SelectedBackup
        {
            get => selectedBackup;
            set
            {
                SetValue(ref selectedBackup, value);
                if (value != null)
                {
                    BackupComment = value.Comment;
                    LockSelectedBackup = value.IsLocked;
                }
                RaiseCommandStates();
            }
        }
        public SavePathCandidateDto SelectedCandidate
        {
            get => selectedCandidate;
            set
            {
                SetValue(ref selectedCandidate, value);
                RaiseCommandStates();
            }
        }
        public TaskStatusDto SelectedTask
        {
            get => selectedTask;
            set
            {
                SetValue(ref selectedTask, value);
                RaiseCommandStates();
            }
        }
        public ValidationFindingDto SelectedFinding
        {
            get => selectedFinding;
            set => SetValue(ref selectedFinding, value);
        }
        public string BackupComment { get => backupComment; set => SetValue(ref backupComment, value); }
        public bool LockSelectedBackup { get => lockSelectedBackup; set => SetValue(ref lockSelectedBackup, value); }
        public MediaItemDto SelectedMedia
        {
            get => selectedMedia;
            set
            {
                SetValue(ref selectedMedia, value);
                MediaComment=value?.Comment??string.Empty;
                MediaFavorite=value?.IsFavorite??false;
                RaiseCommandStates();
            }
        }
        public MediaStorageSummaryDto MediaSummary { get => mediaSummary; private set => SetValue(ref mediaSummary,value??new MediaStorageSummaryDto()); }
        public string MediaComment { get => mediaComment; set => SetValue(ref mediaComment,value??string.Empty); }
        public bool MediaFavorite { get => mediaFavorite; set => SetValue(ref mediaFavorite,value); }
        public string MediaSearchText
        {
            get => mediaSearchText;
            set
            {
                SetValue(ref mediaSearchText,value??string.Empty);
                mediaSearchRefresh.Schedule(value);
            }
        }
        public string MediaFilter
        {
            get => mediaFilter;
            set { SetValue(ref mediaFilter,string.IsNullOrWhiteSpace(value)?"全部":value); MediaView.Refresh(); }
        }
        public GameStatusDto MediaTargetGame
        {
            get => mediaTargetGame;
            set
            {
                SetValue(ref mediaTargetGame, value);
                RaiseCommandStates();
            }
        }
        public MediaItemDto SelectedInboxMedia
        {
            get => selectedInboxMedia;
            set
            {
                SetValue(ref selectedInboxMedia, value);
                RaiseCommandStates();
            }
        }
        public GameStatusDto InboxTargetGame
        {
            get => inboxTargetGame;
            set
            {
                SetValue(ref inboxTargetGame, value);
                RaiseCommandStates();
            }
        }
        public string DiffSummary { get => diffSummary; private set => SetValue(ref diffSummary, value); }
        public string RetentionSummary { get => retentionSummary; private set => SetValue(ref retentionSummary, value); }
        public BackupDiffDto? LastBackupDiff { get => lastBackupDiff; private set => SetValue(ref lastBackupDiff, value); }
        public RetentionPreviewDto? LastRetentionPreview { get => lastRetentionPreview; private set => SetValue(ref lastRetentionPreview, value); }
        public BackupPolicyTemplateDto SelectedPolicyTemplate
        {
            get => selectedPolicyTemplate;
            set
            {
                if (ReferenceEquals(selectedPolicyTemplate, value)) return;
                SetValue(ref selectedPolicyTemplate, value);
                PolicyTemplateDraft = value == null
                    ? new BackupPolicyTemplateDto()
                    : GameSaveCenter.Core.Services.BackupPolicyTemplateCatalog.Clone(value);
                PolicyTemplateNameDraft = PolicyTemplateDraft.Name;
                RaiseCommandStates();
            }
        }
        public BackupPolicyTemplateDto PolicyTemplateDraft
        {
            get => policyTemplateDraft;
            private set
            {
                SetValue(ref policyTemplateDraft, value ?? new BackupPolicyTemplateDto());
                OnPropertyChanged(nameof(CanEditPolicyTemplate));
            }
        }
        public bool CanEditPolicyTemplate => PolicyTemplateDraft != null && !PolicyTemplateDraft.IsBuiltIn;
        public string PolicyTemplateNameDraft
        {
            get => policyTemplateNameDraft;
            set { SetValue(ref policyTemplateNameDraft, value ?? string.Empty); RaiseCommandStates(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand BackupSelectedCommand { get; }
        public ICommand BackupAllCommand { get; }
        public ICommand SyncMediaCommand { get; }
        public ICommand DetectPathsCommand { get; }
        public ICommand ValidateCommand { get; }
        public ICommand RestoreCommand { get; }
        public ICommand ValidateRestoreReadinessCommand { get; }
        public ICommand UndoRestoreCommand { get; }
        public ICommand LoadDetailsCommand { get; }
        public ICommand SavePolicyCommand { get; }
        public ICommand CreatePolicyTemplateCommand { get; }
        public ICommand SavePolicyTemplateCommand { get; }
        public ICommand ApplyPolicyTemplateCommand { get; }
        public ICommand DeletePolicyTemplateCommand { get; }
        public ICommand UpdateBackupMetadataCommand { get; }
        public ICommand CompareBackupCommand { get; }
        public ICommand PreviewRetentionCommand { get; }
        public ICommand AddMediaSourceCommand { get; }
        public ICommand UpdateMediaSourceCommand { get; }
        public ICommand DeleteMediaSourceCommand { get; }
        public ICommand AcceptCandidateCommand { get; }
        public ICommand RejectCandidateCommand { get; }
        public ICommand ReassignMediaCommand { get; }
        public ICommand UpdateMediaMetadataCommand { get; }
        public ICommand FavoriteSelectedMediaCommand { get; }
        public ICommand UnfavoriteSelectedMediaCommand { get; }
        public ICommand CommentSelectedMediaCommand { get; }
        public ICommand OpenSelectedMediaCommand { get; }
        public ICommand RevealSelectedMediaCommand { get; }
        public ICommand AssignInboxMediaCommand { get; }
        public ICommand IgnoreInboxMediaCommand { get; }
        public ICommand CancelTaskCommand { get; }
        public ICommand RetryTaskCommand { get; }
        public ICommand CopyTaskErrorCommand { get; }
        public ICommand OpenAttentionCenterCommand { get; }
        public ICommand OpenAttentionFindingCommand { get; }
        public ICommand OpenProtectionGamesCommand { get; }
        public ICommand OpenProtectionItemCommand { get; }
        public ICommand RefreshDiagnosticsCommand { get; }
        public ICommand RunEnvironmentCheckCommand { get; }
        public ICommand SkipOnboardingCommand { get; }
        public ICommand CompleteOnboardingCommand { get; }
        public ICommand OnboardingTestBackupCommand { get; }
        public ICommand SyncDeviceStatesCommand { get; }
        public ICommand SaveDeviceDecisionCommand { get; }
        public ICommand StageRemoteBackupCommand { get; }
        public ICommand RestoreStagedRemoteBackupCommand { get; }
        public ICommand SaveProcessMappingCommand { get; }
        public ICommand DeleteProcessMappingCommand { get; }
        public ICommand CopyDiagnosticsCommand { get; }
        public ICommand OpenDataDirectoryCommand { get; }
        public ICommand OpenBackupDirectoryCommand { get; }
        public ICommand OpenMediaDirectoryCommand { get; }
        public ICommand OpenWorkerLogCommand { get; }
        public string DeviceStateMessage { get => deviceStateMessage; private set => SetValue(ref deviceStateMessage,value); }
        public DeviceConflictStatusDto SelectedDeviceComparison
        {
            get=>selectedDeviceComparison;
            set
            {
                SetValue(ref selectedDeviceComparison,value);
                DeviceDecision=value?.DecisionDisplay switch
                {
                    "保留两者"=>"保留两者","记录为优先本机"=>"以本机为准","记录为优先远端"=>"以远端为准",_=>"稍后处理"
                };
                DeviceDecisionComment=value?.DecisionComment??string.Empty;
                if(StagedRemoteBackup!=null&&(value==null||
                   !string.Equals(StagedRemoteBackup.PlayniteId,value.PlayniteId,StringComparison.OrdinalIgnoreCase)||
                   !string.Equals(StagedRemoteBackup.RemoteDevice,value.RemoteDevice,StringComparison.OrdinalIgnoreCase)||
                   !string.Equals(StagedRemoteBackup.BackupId,value.RemoteBackupId,StringComparison.OrdinalIgnoreCase)))
                    StagedRemoteBackup=null;
                RaiseCommandStates();
            }
        }
        public RemoteBackupStageResultDto? StagedRemoteBackup
        {
            get=>stagedRemoteBackup;
            private set
            {
                SetValue(ref stagedRemoteBackup,value);
                StagedRemoteBackupStatus=value==null
                    ?"尚未下载远端存档。下载只会写入本机隔离区，不会覆盖当前存档。"
                    :$"已校验：{value.GameName} / {value.RemoteDevice} / {value.BackupId}；{value.ExpiresUtc.ToLocalTime():yyyy-MM-dd HH:mm} 前有效。";
                RaiseCommandStates();
            }
        }
        public string StagedRemoteBackupStatus
        {
            get=>stagedRemoteBackupStatus;
            private set=>SetValue(ref stagedRemoteBackupStatus,value);
        }
        public string DeviceDecision { get=>deviceDecision; set=>SetValue(ref deviceDecision,value??"稍后处理"); }
        public string DeviceDecisionComment { get=>deviceDecisionComment; set=>SetValue(ref deviceDecisionComment,value??string.Empty); }
        public string ProcessMappingExecutable { get => processMappingExecutable; set { SetValue(ref processMappingExecutable,value??string.Empty); RaiseCommandStates(); } }
        public GameStatusDto ProcessMappingTargetGame { get => processMappingTargetGame; set { SetValue(ref processMappingTargetGame,value); RaiseCommandStates(); } }
        public ProcessMappingDto SelectedProcessMapping { get => selectedProcessMapping; set { SetValue(ref selectedProcessMapping,value); RaiseCommandStates(); } }
        public GameToolEntryCandidateDto SelectedImportEntryCandidate
        {
            get => selectedImportEntryCandidate;
            set { SetValue(ref selectedImportEntryCandidate,value); RaiseCommandStates(); }
        }
        public bool HasPendingGameToolEntrySelection
        {
            get => hasPendingGameToolEntrySelection;
            private set { SetValue(ref hasPendingGameToolEntrySelection,value); RaiseCommandStates(); }
        }
        public ICommand ImportTrainerCommand { get; }
        public ICommand ImportCheatTableCommand { get; }
        public ICommand ImportCustomLaunchItemCommand { get; }
        public ICommand ImportToolFolderCommand { get; }
        public ICommand ConfirmGameToolImportCommand { get; }
        public ICommand CancelGameToolImportCommand { get; }
        public ICommand SaveGameToolCommand { get; }
        public ICommand LaunchGameToolCommand { get; }
        public ICommand OpenGameToolDirectoryCommand { get; }
        public ICommand DeleteGameToolCommand { get; }
        public ICommand RelocateGameToolCommand { get; }
        public ICommand SyncTrainerCatalogCommand { get; }
        public ICommand SearchTrainerCatalogCommand { get; }
        public ICommand LoadTrainerReleasesCommand { get; }
        public ICommand DownloadTrainerCommand { get; }

        public Task RefreshAsync()
        {
            // A manual refresh is explicit user intent. Do not leave the idle, delayed
            // large-library startup sync queued behind it; the explicit operation becomes
            // the single source of truth for this refresh cycle.
            CancelInitialSynchronization();
            return RefreshCoreAsync(true);
        }

        /// <summary>Turns the overview warning count into a route to its concrete reasons.</summary>
        private void OpenAttentionCenter()
        {
            var finding=Findings.FirstOrDefault(x=>x.Severity>=FindingSeverity.Warning);
            if(finding==null)
            {
                StatusMessage="当前没有需要处理的关注项。";
                return;
            }

            SelectedFinding=finding;
            CurrentWorkspace=WorkspaceKind.Maintenance;
            AttentionCenterRequested?.Invoke(this,EventArgs.Empty);
        }

        private void OpenAttentionFinding(ValidationFindingDto? finding)
        {
            if (finding == null) { OpenAttentionCenter(); return; }
            SelectedFinding = finding;
            CurrentWorkspace = WorkspaceKind.Maintenance;
            AttentionCenterRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OpenProtectionGames()
        {
            var first = RecentProtection.Items.FirstOrDefault();
            if (first == null)
            {
                StatusMessage = "最近保护窗口内没有需要处理的游戏。";
                return;
            }

            OpenProtectionItem(first);
        }

        private void OpenProtectionItem(RecentProtectionItem? item)
        {
            if (item == null) return;
            var game = Games.FirstOrDefault(candidate => string.Equals(candidate.PlayniteId, item.PlayniteId, StringComparison.OrdinalIgnoreCase));
            if (game == null)
            {
                StatusMessage = "该游戏已不在当前快照中，请先刷新面板。";
                return;
            }

            SelectedGame = game;
            StatusMessage = $"已选择“{game.Name}”，请确认后再执行备份或校验。";
        }

        public event EventHandler? AttentionCenterRequested;

        /// <summary>
        /// Enables the optional Worker event connection while the WPF dashboard is visible.
        /// Normal task polling remains active as the durable fallback after a Worker restart,
        /// missed event, or temporarily unavailable event pipe.
        /// </summary>
        public void StartTaskEventSubscription()
        {
            if (taskEventSubscription != null) return;
            taskEventSubscription = new CancellationTokenSource();
            var token = taskEventSubscription.Token;
            taskEventListener = ListenForTaskEventsWhenReadyAsync(token);
        }

        private async Task ListenForTaskEventsWhenReadyAsync(CancellationToken token)
        {
            try
            {
                // A large Playnite profile can take a few seconds to start the Worker and open
                // SQLite. Do not spin an event-pipe reconnect loop while the host is importing
                // hundreds of games; the durable snapshot poll remains the fallback and the
                // event stream becomes useful once the dashboard has settled.
                if (plugin.IsLargeLibraryForUi)
                    await Task.Delay(TimeSpan.FromSeconds(60), token).ConfigureAwait(false);
                await plugin.ListenForTaskEventsAsync(ApplyTaskEventAsync, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // Unloading the dashboard intentionally cancels the delayed listener. Observe
                // the cancellation here so the fire-and-forget task cannot surface as a
                // Dispatcher/Playnite unhandled exception.
            }
        }

        public void StopTaskEventSubscription()
        {
            var subscription = taskEventSubscription;
            taskEventSubscription = null;
            if (subscription == null) return;
            try { subscription.Cancel(); }
            finally { subscription.Dispose(); }
            taskEventListener = null;
        }

        public void CancelDeferredUiWork()
        {
            plugin.PlayniteGameStarted -= OnPlayniteGameStarted;
            gamePicker.CancelPendingRefresh();
            taskSearchRefresh.Cancel();
            mediaSearchRefresh.Cancel();
            CancelDetailsLoad();
            CancelInitialSynchronization();
            var persistence = gamePickerPersistenceCancellation;
            gamePickerPersistenceCancellation = null;
            if (persistence == null) return;
            try { persistence.Cancel(); }
            finally { persistence.Dispose(); }
        }

        private void CancelInitialSynchronization()
        {
            Interlocked.Increment(ref deferredUiWorkGeneration);
            var pending = initialSynchronizationCancellation;
            initialSynchronizationCancellation = null;
            if (pending == null) return;
            // The delayed task owns disposal in its finally block. Disposing the source here
            // races with Task.Delay's cancellation registration on the WPF dispatcher and can
            // turn an intentional unload into an ObjectDisposedException.
            pending.Cancel();
        }

        private async Task ApplyTaskEventAsync(TaskChangeEventDto change)
        {
            if (change == null || change.Task == null) return;
            ApplyOnUi(() =>
            {
                taskIndex.Merge(Tasks, change.Task);
                Replace(OverviewTasks, Tasks.OrderByDescending(x => x.CreatedUtc).Take(8), SnapshotComparers.Task);
                knownTaskStates[change.Task.TaskId] = change.Task.State;
                taskSnapshotInitialized = true;
                if (SelectedTask == null || string.Equals(SelectedTask.TaskId, change.Task.TaskId, StringComparison.OrdinalIgnoreCase))
                    SelectedTask = Tasks.FirstOrDefault(x => string.Equals(x.TaskId, change.Task.TaskId, StringComparison.OrdinalIgnoreCase));
                RaiseCommandStates();
            });

            if (change.Task.State == TaskState.Succeeded || change.Task.State == TaskState.Failed || change.Task.State == TaskState.Cancelled)
            {
                // A terminal event can change backup/media counts and findings. Request the normal
                // cached snapshot refresh; the event itself only updates the task rows immediately.
                await RequestBackgroundRefreshAsync();
            }
        }

        private async Task InitializeAsync()
        {
            // Render durable SQLite state first. The library synchronization runs separately, so
            // opening the panel never waits for a large Playnite library to be rematched.
            try
            {
                // A first dashboard paint must not wait the full IPC default timeout when the
                // Worker is still being started. Fail fast to the cache/empty state and let the
                // scheduled background refresh retry once the pipe is ready.
                await RefreshCoreAsync(false, TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                // The Worker may still be starting. Keep the shell usable and let the scheduled
                // synchronization retry the cache read once the pipe is ready.
                StatusMessage = "正在启动后台服务，稍后刷新本地状态…";
                Logger.Debug(ex, "GameSaveCenter dashboard cache read deferred until Worker startup completes.");
            }
            // For 500+ game profiles the dashboard is intentionally cache-first. Do not keep a
            // delayed task alive merely to discover that the plugin will skip the automatic full
            // catalog rematch; an explicit Refresh command remains available to opt in.
            if (plugin.IsVeryLargeLibraryForUi)
            {
                ApplyOnUi(() => StatusMessage = "已显示本地缓存；大型目录默认不自动全量匹配，可按需点击刷新。\n游戏启动时仍会按需更新当前游戏。");
                // The dashboard is allowed to open while the Worker is still coming up. A
                // first cache read can therefore time out even though the Worker becomes
                // healthy moments later. Retry only the aggregate SQLite snapshot a few times;
                // never turn this recovery path into a catalog synchronization or Ludusavi
                // scan. This keeps the first paint responsive without leaving a newly created
                // 900+ library permanently blank until the user clicks Refresh.
                var cacheGeneration = Interlocked.Read(ref deferredUiWorkGeneration);
                _ = RefreshLargeLibraryCacheWhenWorkerReadyAsync(cacheGeneration);
                return;
            }
            // A large library already has durable game summaries in SQLite in the normal
            // case. Starting 100+ Ludusavi lookups while the user is opening the panel makes
            // Playnite appear frozen and competes with other Ludusavi integrations. Keep the
            // shell immediately usable and release the background catalog sync only after a
            // quiet period. An empty cache uses a shorter delay so first-run installations
            // still begin indexing without making the sidebar activation synchronous.
            var largeLibraryDelay = plugin.IsLargeLibraryForUi
                ? (Games.Count > 0 ? TimeSpan.FromSeconds(60) : TimeSpan.FromSeconds(10))
                : TimeSpan.Zero;
            var generation = Interlocked.Read(ref deferredUiWorkGeneration);
            _ = RefreshAfterSynchronizationAsync(largeLibraryDelay, generation);
        }

        private async Task RefreshAfterSynchronizationAsync(TimeSpan delay, long generation)
        {
            if (generation != Interlocked.Read(ref deferredUiWorkGeneration)) return;
            var cancellation = new CancellationTokenSource();
            initialSynchronizationCancellation = cancellation;
            try
            {
                if (generation != Interlocked.Read(ref deferredUiWorkGeneration)) return;
                if (delay > TimeSpan.Zero)
                {
                    ApplyOnUi(() => StatusMessage = Games.Count > 0
                        ? "已显示本地缓存；大型目录同步将在空闲时进行。"
                        : "已打开工作区；大型目录同步将在后台开始。\n首次索引可能需要一些时间。\n");
                    await Task.Delay(delay, cancellation.Token).ConfigureAwait(false);
                }

                // The startup hook may already be synchronizing a large Playnite library. Join
                // that task instead of marking another full refresh as pending when the user
                // opens the sidebar. The cache-first snapshot is already rendered above.
                await plugin.SynchronizeFromDashboardAsync();
                await RefreshDashboardAsync(false, false);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                // The view was unloaded or the user requested an explicit refresh. The
                // cancellation is intentional and must not surface as a Playnite exception.
            }
            catch (Exception ex)
            {
                ApplyOnUi(() => StatusMessage = "已显示本地缓存；后台同步暂不可用：" + ex.Message);
            }
            finally
            {
                if (ReferenceEquals(initialSynchronizationCancellation, cancellation))
                    initialSynchronizationCancellation = null;
                cancellation.Dispose();
            }
        }

        private async Task RefreshLargeLibraryCacheWhenWorkerReadyAsync(long generation)
        {
            var cancellation = new CancellationTokenSource();
            initialSynchronizationCancellation = cancellation;
            try
            {
                for (var attempt = 0; attempt < 4; attempt++)
                {
                    if (generation != Interlocked.Read(ref deferredUiWorkGeneration)) return;
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(attempt == 0 ? 1 : 2), cancellation.Token).ConfigureAwait(false);
                        if (generation != Interlocked.Read(ref deferredUiWorkGeneration)) return;
                        await RefreshDashboardAsync(false, false, TimeSpan.FromSeconds(3));
                        ApplyOnUi(() => StatusMessage = "已显示本地缓存；大型目录默认不自动全量匹配，可按需点击刷新。\n游戏启动时仍会按需更新当前游戏。");
                        return;
                    }
                    catch (OperationCanceledException) when (cancellation.IsCancellationRequested || generation != Interlocked.Read(ref deferredUiWorkGeneration))
                    {
                        // Dashboard unload intentionally cancels the retry loop.
                        return;
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug(ex, "Large-library cache snapshot is not ready yet; retrying without catalog synchronization.");
                    }
                }
            }
            finally
            {
                if (ReferenceEquals(initialSynchronizationCancellation, cancellation))
                    initialSynchronizationCancellation = null;
                cancellation.Dispose();
            }
        }

        /// <summary>Lightweight polling entry used by the view timer. It remains active while a manual task is running.</summary>
        public async Task RequestBackgroundRefreshAsync()
        {
            var started = false;
            try
            {
                ApplyOnUi(() =>
                {
                    if (!plugin.Settings.EnableDashboardAutoRefresh || IsBackgroundRefreshing) return;
                    IsBackgroundRefreshing = true;
                    started = true;
                });
                if (!started) return;
                await plugin.EnsureWorkerAsync();
                var taskChanges=await plugin.RequestAsync<TaskChangeFeedDto>(MessageTypes.GetTaskChanges,
                    new TaskChangeRequestDto{AfterSequence=lastTaskEventSequence,Limit=100});
                lastTaskEventSequence=taskChanges.LatestSequence;
                var refreshDashboard=taskChanges.ResetRequired||taskChanges.Changes.Count>0||
                    DateTime.UtcNow-lastFullDashboardRefreshUtc>=TimeSpan.FromMinutes(1);
                if(!refreshDashboard)return;
                var refreshDetails = await RefreshDashboardAsync(false, true);
                var loadDetails = false;
                ApplyOnUi(() =>
                {
                    loadDetails = refreshDetails && !IsBusy && SelectedGame != null;
                });
                if (loadDetails) await LoadDetailsAsync();
            }
            catch (Exception ex)
            {
                ApplyOnUi(() => StatusMessage = "自动刷新暂不可用：" + ex.Message);
            }
            finally
            {
                if (started) ApplyOnUi(() => IsBackgroundRefreshing = false);
            }
        }

        private async Task RefreshCoreAsync(bool synchronize, TimeSpan? snapshotTimeout = null)
        {
            StatusMessage = synchronize ? "正在同步设置与游戏库…" : "正在读取本地状态…";
            await RefreshDashboardAsync(synchronize, false, snapshotTimeout);
            if (!policyTemplatesLoaded) await LoadPolicyTemplatesAsync();
            if (CurrentWorkspace == WorkspaceKind.Media) await LoadInboxAsync();
            if (CurrentWorkspace == WorkspaceKind.Maintenance) await LoadDiagnosticsAsync();
            if (SelectedGame != null && IsGameScopedWorkspace(CurrentWorkspace)) await LoadDetailsAsync();
            else ClearSelectedGameDetails();
        }

        private async Task<bool> RefreshDashboardAsync(bool synchronize, bool notifyTaskChanges, TimeSpan? snapshotTimeout = null)
        {
            if (synchronize) await plugin.SynchronizeAsync();
            var fetchTimer = Stopwatch.StartNew();
            var data = await plugin.RequestAsync<DashboardSnapshotDto>(MessageTypes.GetDashboard, new { }, snapshotTimeout);
            fetchTimer.Stop();
            var notifications = new List<TaskStatusDto>();
            var selectedTaskCompleted = false;
            var applyTimer = Stopwatch.StartNew();
            ApplyOnUi(() =>
            {
                var selectedGameId = SelectedGame?.PlayniteId;
                var selectedTaskId = SelectedTask?.TaskId;
                var mediaTargetId = MediaTargetGame?.PlayniteId;
                if (taskSnapshotInitialized)
                {
                    foreach (var task in data.RecentTasks)
                    {
                        var changed = !knownTaskStates.TryGetValue(task.TaskId, out var oldState) || oldState != task.State;
                        var terminal = task.State == TaskState.Succeeded || task.State == TaskState.Failed || task.State == TaskState.Cancelled;
                        if (notifyTaskChanges && !IsBusy && changed && terminal && task.CreatedUtc >= dashboardOpenedUtc.AddSeconds(-5))
                            notifications.Add(task);
                        if (changed && terminal && !string.IsNullOrWhiteSpace(selectedGameId) && string.Equals(task.GameId, selectedGameId, StringComparison.OrdinalIgnoreCase))
                            selectedTaskCompleted = true;
                    }
                }
                knownTaskStates.Clear();
                foreach (var task in data.RecentTasks) knownTaskStates[task.TaskId] = task.State;
                taskSnapshotInitialized = true;

                Snapshot = data;
                suppressSelectionLoad = true;
                try
                {
                    var gamesChanged = Replace(Games, data.Games, SnapshotComparers.Game);
                    var pickerChanged = gamePicker.SetItems(Games, selectedGameId ?? plugin.Settings.GamePickerSelectedGameId);
                    if (gamesChanged || pickerChanged)
                        RefreshGameView(false);
                    SelectedGame = gamePicker.SelectedGame
                        ?? Games.FirstOrDefault(x => x.PlayniteId == selectedGameId && GamesView.Contains(x))
                        ?? GamesView.Cast<GameStatusDto>().FirstOrDefault();
                    MediaTargetGame = Games.FirstOrDefault(x => string.Equals(x.PlayniteId, mediaTargetId, StringComparison.OrdinalIgnoreCase))
                                      ?? SelectedGame
                                      ?? Games.FirstOrDefault();
                    TryApplyPendingAutoSelection();
                    ApplyInitialSelectionIfNeeded();
                    RefreshSelectedGameIcon();
                }
                finally { suppressSelectionLoad = false; }
                // Cache-first snapshots can be older than the current wall clock. The protection
                // window is a user-facing "recent" promise, so anchor it to now rather than to
                // the snapshot's generation time when Worker data is temporarily unavailable.
                RecentProtection = recentProtectionAssessment.Assess(data.Games, plugin.Settings.RecentProtectionWindowDays, DateTime.UtcNow);
                var tasksChanged = Replace(Tasks, data.RecentTasks, SnapshotComparers.Task);
                if (tasksChanged) taskIndex.Rebuild(Tasks);
                OnPropertyChanged(nameof(RunningTaskCount));
                OnPropertyChanged(nameof(RetryableTaskCount));
                OnPropertyChanged(nameof(CompletedTaskCount));
                Replace(OverviewTasks, data.RecentTasks.Take(8), SnapshotComparers.Task);
                RebuildTaskFilters();
                SelectedTask = Tasks.FirstOrDefault(x => x.TaskId == selectedTaskId) ?? Tasks.FirstOrDefault();
                Replace(Findings, data.Findings, SnapshotComparers.Finding);
                Replace(AttentionFindings, data.Findings.Where(x => x.Severity >= FindingSeverity.Warning).Take(4), SnapshotComparers.Finding);
                Replace(Audit, data.RecentAudit, SnapshotComparers.Audit);
                StatusMessage = data.WorkerHealthy
                    ? data.LudusaviAvailable ? "Worker 与 Ludusavi 均正常" : "Worker 正常，Ludusavi 尚未配置"
                    : "Worker 不可用";
            });
            applyTimer.Stop();
            Logger.Debug($"[PERF] DashboardSnapshot fetch={fetchTimer.ElapsedMilliseconds}ms apply={applyTimer.ElapsedMilliseconds}ms games={data.Games.Count} tasks={data.RecentTasks.Count} findings={data.Findings.Count}");
            foreach (var task in notifications) plugin.ShowTaskNotification(task);
            lastFullDashboardRefreshUtc=DateTime.UtcNow;
            return selectedTaskCompleted;
        }

        private async Task LoadDiagnosticsAsync()
        {
            var settings = await plugin.RequestAsync<WorkerSettingsSnapshotDto>(MessageTypes.GetSettings, new { });
            var mappings = await plugin.RequestAsync<List<ProcessMappingDto>>(MessageTypes.ListProcessMappings, new { });
            ApplyOnUi(() =>
            {
                EffectiveSettings = settings;
                DiagnosticSummary = BuildDiagnosticSummary(settings);
                Replace(ProcessMappings,mappings, SnapshotComparers.ProcessMapping);
                if(ProcessMappingTargetGame==null) ProcessMappingTargetGame=SelectedGame??Games.FirstOrDefault();
            });
            if (IsOnboardingPending && !environmentCheckLoaded)
                await RunEnvironmentCheckAsync();
        }

        private async Task RunEnvironmentCheckAsync()
        {
            var report = await plugin.RequestAsync<EnvironmentCheckReportDto>(MessageTypes.CheckEnvironment, new EnvironmentCheckRequestDto(), TimeSpan.FromMinutes(3));
            ApplyOnUi(() =>
            {
                EnvironmentCheck = report;
                environmentCheckLoaded = true;
                StatusMessage = report.Summary;
            });
        }

        private void SkipOnboarding()
        {
            plugin.Settings.OnboardingCompleted = true;
            plugin.SavePluginSettings(plugin.Settings);
            OnPropertyChanged(nameof(IsOnboardingPending));
            OnPropertyChanged(nameof(OnboardingTitle));
            OnPropertyChanged(nameof(OnboardingDescription));
            StatusMessage = "已跳过首次环境检查；之后可在维护中心重新运行。";
            RaiseCommandStates();
        }

        private void CompleteOnboarding()
        {
            if (!EnvironmentCheck.IsReady || EnvironmentCheck.CheckedUtc == default(DateTime))
            {
                StatusMessage = "请先运行环境检查；失败项处理后才能完成设置。";
                return;
            }
            plugin.Settings.OnboardingCompleted = true;
            plugin.SavePluginSettings(plugin.Settings);
            OnPropertyChanged(nameof(IsOnboardingPending));
            OnPropertyChanged(nameof(OnboardingTitle));
            OnPropertyChanged(nameof(OnboardingDescription));
            StatusMessage = "环境检查已完成。测试备份仍需由你明确点击执行。";
            RaiseCommandStates();
        }

        private async Task SaveProcessMappingAsync()
        {
            var saved=await plugin.RequestAsync<ProcessMappingDto>(MessageTypes.SaveProcessMapping,new ProcessMappingDto{ExecutableName=ProcessMappingExecutable,PlayniteId=ProcessMappingTargetGame.PlayniteId});
            await LoadDiagnosticsAsync();ProcessMappingExecutable=string.Empty;StatusMessage=$"已将 {saved.ExecutableName} 绑定到 {saved.GameName}";
        }

        private async Task DeleteProcessMappingAsync()
        {
            await plugin.RequestAsync<object>(MessageTypes.DeleteProcessMapping,new ProcessMappingDto{ExecutableName=SelectedProcessMapping.ExecutableName});
            await LoadDiagnosticsAsync();StatusMessage="已删除进程映射";
        }

        private async Task RefreshDiagnosticsAsync()
        {
            await RefreshDashboardAsync(false, false);
            await LoadDiagnosticsAsync();
            StatusMessage = "诊断信息已更新";
        }

        private async Task SyncDeviceStatesAsync()
        {
            var result=await plugin.RequestAsync<DeviceStateSyncResultDto>(MessageTypes.SyncDeviceStates,new { },TimeSpan.FromMinutes(5));
            ApplyOnUi(()=>
            {
                Replace(DeviceComparisons,result.Comparisons, SnapshotComparers.DeviceComparison);
                DeviceStateMessage=result.StatusMessage;
            });
            StatusMessage=result.StatusMessage;
        }

        private async Task SaveDeviceDecisionAsync()
        {
            var selected=SelectedDeviceComparison??throw new InvalidOperationException("请先选择设备比较记录。");
            var code=DeviceDecision switch{"保留两者"=>"KeepBoth","以本机为准"=>"PreferLocal","以远端为准"=>"PreferRemote",_=>"Defer"};
            var saved=await plugin.RequestAsync<DeviceConflictDecisionDto>(MessageTypes.SaveDeviceConflictDecision,new DeviceConflictDecisionDto
            {
                PlayniteId=selected.PlayniteId,RemoteDevice=selected.RemoteDevice,
                LocalBackupId=selected.LocalBackupId,RemoteBackupId=selected.RemoteBackupId,
                Decision=code,Comment=DeviceDecisionComment
            });
            selected.Decision=saved.Decision;selected.DecisionComment=saved.Comment;selected.DecidedUtc=saved.DecidedUtc;
            var index=DeviceComparisons.IndexOf(selected);
            if(index>=0)DeviceComparisons[index]=selected;
            SelectedDeviceComparison=selected;
            ConfirmSuccess("已记录人工决策；未下载、恢复、删除或覆盖任何存档");
        }

        private async Task StageRemoteBackupAsync()
        {
            var selected=SelectedDeviceComparison??throw new InvalidOperationException("请先选择包含远端备份的设备记录。");
            if(!await plugin.ConfirmAsync(
                   "下载远端备份到隔离区",
                   $"将从设备“{selected.RemoteDevice}”下载完整 Ludusavi 备份库，并在本机隔离区校验版本“{selected.RemoteBackupId}”。\n\n此步骤不会恢复或覆盖当前存档，但下载量可能较大。是否继续？",
                   "下载并校验",
                   "取消"))return;
            var staged=await plugin.RequestAsync<RemoteBackupStageResultDto>(MessageTypes.StageRemoteBackup,
                new RemoteBackupStageRequestDto
                {
                    PlayniteId=selected.PlayniteId,RemoteDevice=selected.RemoteDevice,BackupId=selected.RemoteBackupId
                },TimeSpan.FromHours(3));
            StagedRemoteBackup=staged;
            ConfirmSuccess(staged.StatusMessage);
        }

        private async Task RestoreStagedRemoteBackupAsync()
        {
            var staged=StagedRemoteBackup??throw new InvalidOperationException("请先下载并校验远端备份。");
            if(!await plugin.ConfirmAsync(
                   "从已校验的远端备份恢复",
                   $"即将恢复“{staged.GameName}”在设备“{staged.RemoteDevice}”上的版本“{staged.BackupId}”。\n\n恢复前会创建并锁定本机当前存档的 PreRestore 快照。请确认游戏、启动器和 MOD 管理器均已关闭。",
                   "创建快照并恢复",
                   "取消"))return;
            var task=await plugin.RequestAsync<TaskStatusDto>(MessageTypes.RestoreRemoteBackup,
                new RemoteRestoreRequestDto
                {
                    StagingId=staged.StagingId,ConfirmedCurrentSnapshot=true,ConfirmedGameClosed=true,
                    UserComment="Playnite remote restore wizard"
                },TimeSpan.FromMinutes(45));
            await RefreshCoreAsync(false);
            NotifyTaskResults(new[]{task});
        }

        private async Task LoadDetailsAsync(bool forceBackupHistory = false, CancellationToken cancellationToken = default(CancellationToken), long expectedGeneration = 0, string? expectedGameId = null)
        {
            if (SelectedGame == null) return;
            var id = SelectedGame.PlayniteId;
            if (!string.IsNullOrWhiteSpace(expectedGameId)
                && !string.Equals(expectedGameId, id, StringComparison.OrdinalIgnoreCase)) return;
            switch (CurrentWorkspace)
            {
                case WorkspaceKind.Saves:
                {
                    var backupsTask = plugin.RequestAsync<BackupVersionDto[]>(MessageTypes.ListBackups, new GameQueryDto { PlayniteId = id, Limit = 500, ForceRefresh = forceBackupHistory });
                    var candidatesTask = plugin.RequestAsync<SavePathCandidateDto[]>(MessageTypes.ListSaveCandidates, new GameQueryDto { PlayniteId = id });
                    await Task.WhenAll(backupsTask, candidatesTask);
                    if (!IsCurrentDetailsLoad(id, cancellationToken, expectedGeneration)) return;
                    ApplyOnUi(() =>
                    {
                        if (!IsCurrentDetailsLoad(id, cancellationToken, expectedGeneration)) return;
                        Replace(Backups, backupsTask.Result, SnapshotComparers.Backup);
                        Replace(SaveCandidates, candidatesTask.Result, SnapshotComparers.SaveCandidate);
                        SelectedBackup = Backups.FirstOrDefault();
                        SelectedCandidate = SaveCandidates.FirstOrDefault(x => string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                                            ?? SaveCandidates.FirstOrDefault();
                        RaiseCommandStates();
                    });
                    break;
                }
                case WorkspaceKind.Media:
                {
                    var mediaLoadTimer = Stopwatch.StartNew();
                    var mediaTask = plugin.RequestAsync<MediaItemDto[]>(MessageTypes.ListMedia, new GameQueryDto { PlayniteId = id, Limit = 1000 });
                    var sourcesTask = plugin.RequestAsync<MediaSourceRuleDto[]>(MessageTypes.ListMediaSources, new GameQueryDto { PlayniteId = id });
                    var summaryTask = plugin.RequestAsync<MediaStorageSummaryDto>(MessageTypes.GetMediaSummary, new GameQueryDto { PlayniteId = id });
                    await Task.WhenAll(mediaTask, sourcesTask, summaryTask);
                    mediaLoadTimer.Stop();
                    if (!IsCurrentDetailsLoad(id, cancellationToken, expectedGeneration)) return;
                    var mediaApplyTimer = Stopwatch.StartNew();
                    ApplyOnUi(() =>
                    {
                        if (!IsCurrentDetailsLoad(id, cancellationToken, expectedGeneration)) return;
                        if (Replace(Media, mediaTask.Result, SnapshotComparers.Media))
                            MediaView.Refresh();
                        Replace(MediaSources, sourcesTask.Result, SnapshotComparers.MediaSource);
                        MediaSummary=summaryTask.Result;
                        SelectedMedia=Media.FirstOrDefault();
                        MediaTargetGame = Games.FirstOrDefault(x => string.Equals(x.PlayniteId, MediaTargetGame?.PlayniteId, StringComparison.OrdinalIgnoreCase))
                                          ?? SelectedGame
                                          ?? Games.FirstOrDefault();
                        RaiseCommandStates();
                    });
                    mediaApplyTimer.Stop();
                    Logger.Debug($"[PERF] MediaDetails load={mediaLoadTimer.ElapsedMilliseconds}ms apply={mediaApplyTimer.ElapsedMilliseconds}ms media={mediaTask.Result.Length} sources={sourcesTask.Result.Length}");
                    break;
                }
                case WorkspaceKind.Trainers:
                {
                    var gameTools = await plugin.RequestAsync<GameToolDto[]>(MessageTypes.ListGameTools, new GameQueryDto { PlayniteId = id });
                    if (!IsCurrentDetailsLoad(id, cancellationToken, expectedGeneration)) return;
                    ApplyOnUi(() =>
                    {
                        if (!IsCurrentDetailsLoad(id, cancellationToken, expectedGeneration)) return;
                        var selectedToolId = SelectedGameTool?.ToolId;
                        Replace(GameTools, gameTools, SnapshotComparers.GameTool);
                        SelectedGameTool = GameTools.FirstOrDefault(x => string.Equals(x.ToolId, selectedToolId, StringComparison.OrdinalIgnoreCase))
                                           ?? GameTools.FirstOrDefault();
                        RaiseCommandStates();
                    });
                    break;
                }
            }
        }

        private bool IsCurrentDetailsLoad(string playniteId, CancellationToken cancellationToken, long expectedGeneration)
            => !cancellationToken.IsCancellationRequested
               && (expectedGeneration == 0 || expectedGeneration == Interlocked.Read(ref detailsLoadGeneration))
               && IsSelectedGame(playniteId);

        public void RequestWorkspaceLoad()
        {
            if (CurrentWorkspace == WorkspaceKind.Media) Run(LoadMediaWorkspaceAsync);
            else if (CurrentWorkspace == WorkspaceKind.Maintenance) Run(LoadDiagnosticsAsync);
            else if (IsGameScopedWorkspace(CurrentWorkspace)) Run(() => LoadDetailsAsync());
        }

        private bool IsSelectedGame(string playniteId)
            => SelectedGame != null && string.Equals(SelectedGame.PlayniteId, playniteId, StringComparison.OrdinalIgnoreCase);

        private static bool IsGameScopedWorkspace(WorkspaceKind workspace)
            => workspace == WorkspaceKind.Saves || workspace == WorkspaceKind.Trainers || workspace == WorkspaceKind.Media;

        private async Task BackupSelectedAsync()
        {
            var tasks = await plugin.RequestAsync<TaskStatusDto[]>(MessageTypes.BackupGame, new BackupRequestDto { PlayniteIds = { SelectedGame.PlayniteId }, Force = true, Reason = "Manual" }, TimeSpan.FromMinutes(15));
            NotifyTaskResults(tasks);
            await RefreshCoreAsync(false);
            await LoadDetailsAsync();
            StatusMessage = Backups.Count > 0
                ? $"备份完成，已读取 {Backups.Count} 个历史版本"
                : "备份完成，但历史索引仍为空；请打开诊断页查看 Ludusavi 输出。";
        }

        private async Task ImportGameToolAsync(GameToolType type)
        {
            var dialog=new OpenFileDialog
            {
                Title=type==GameToolType.CheatTable?"导入 Cheat Table":"导入修改器（EXE 或 ZIP）",
                Filter=type==GameToolType.CheatTable?"Cheat Engine Table (*.ct)|*.ct|所有文件 (*.*)|*.*":"修改器 (*.exe;*.zip)|*.exe;*.zip|所有文件 (*.*)|*.*",
                Multiselect=false,CheckFileExists=true
            };
            if(dialog.ShowDialog()!=true)return;
            await PrepareGameToolImportAsync(dialog.FileName,type);
        }

        private async Task ImportGameToolFolderAsync()
        {
            var folder=plugin.PlayniteApi.Dialogs.SelectFolder();
            if(string.IsNullOrWhiteSpace(folder))return;
            await PrepareGameToolImportAsync(folder,GameToolType.Trainer);
        }

        private async Task ImportCustomLaunchItemAsync()
        {
            var dialog=new OpenFileDialog
            {
                Title="添加自定义启动项",
                Filter="可执行文件 (*.exe)|*.exe|快捷方式 (*.lnk)|*.lnk|脚本 (*.bat;*.cmd;*.ps1)|*.bat;*.cmd;*.ps1|所有文件 (*.*)|*.*",
                Multiselect=false,CheckFileExists=true
            };
            if(dialog.ShowDialog()!=true)return;
            await ExecuteGameToolImportAsync(dialog.FileName,GameToolType.CustomExecutable,Path.GetFileName(dialog.FileName),false);
        }

        private async Task PrepareGameToolImportAsync(string source,GameToolType type)
        {
            var inspection=await plugin.RequestAsync<GameToolImportInspectionDto>(MessageTypes.InspectGameToolImport,
                new InspectGameToolImportRequestDto{SourcePath=source,ToolType=type},TimeSpan.FromMinutes(2));
            if(inspection.Candidates.Count==1)
            {
                await ExecuteGameToolImportAsync(inspection.SourcePath,type,inspection.Candidates[0].RelativePath);
                return;
            }
            pendingGameToolImportSource=inspection.SourcePath;
            pendingGameToolImportType=type;
            Replace(ImportEntryCandidates,inspection.Candidates);
            SelectedImportEntryCandidate=ImportEntryCandidates.FirstOrDefault();
            HasPendingGameToolEntrySelection=true;
            StatusMessage=$"检测到 {ImportEntryCandidates.Count} 个可执行文件，请选择主程序";
        }

        private Task ConfirmGameToolImportAsync()
        {
            if(!HasPendingGameToolEntrySelection||SelectedImportEntryCandidate==null)
                throw new InvalidOperationException("请先选择修改器主程序。");
            return ExecuteGameToolImportAsync(pendingGameToolImportSource,pendingGameToolImportType,SelectedImportEntryCandidate.RelativePath);
        }

        private async Task ExecuteGameToolImportAsync(string source,GameToolType type,string entryFileName,bool copyIntoLibrary=true)
        {
            var imported=await plugin.RequestAsync<GameToolDto>(MessageTypes.ImportGameTool,new ImportGameToolRequestDto
            {
                PlayniteId=SelectedGame.PlayniteId,ToolType=type,SourcePath=source,EntryFileName=entryFileName,CopyIntoLibrary=copyIntoLibrary
            },TimeSpan.FromMinutes(5));
            ClearPendingGameToolImport();
            await LoadDetailsAsync();SelectedGameTool=GameTools.FirstOrDefault(x=>x.ToolId==imported.ToolId)??GameTools.FirstOrDefault();
            var message=type switch
            {
                GameToolType.CheatTable=>"Cheat Table 已导入，自动启动保持关闭",
                GameToolType.CustomExecutable=>"自定义启动项已添加，外部路径引用，不复制文件",
                _=>"修改器已导入，自动启动保持关闭"
            };
            ConfirmSuccess(message);
        }

        private void ClearPendingGameToolImport()
        {
            pendingGameToolImportSource=string.Empty;
            ImportEntryCandidates.Clear();
            SelectedImportEntryCandidate=null!;
            HasPendingGameToolEntrySelection=false;
            StatusMessage="已取消本次导入";
        }

        private async Task SaveSelectedGameToolAsync()
        {
            var tool=SelectedGameTool;
            var closeWasUnsafe=tool.CloseOnGameExit&&!tool.CanTrackProcess;
            var adminWasUnsupported=tool.RequiresAdmin&&tool.LaunchKind==GameToolLaunchKind.ShellDocument;
            var closeOnExit=tool.CanTrackProcess&&tool.CloseOnGameExit;
            var requiresAdmin=tool.LaunchKind==GameToolLaunchKind.ShellDocument?false:tool.RequiresAdmin;
            await plugin.RequestAsync<object>(MessageTypes.UpdateGameTool,new UpdateGameToolRequestDto
            {
                ToolId=tool.ToolId,Enabled=tool.Enabled,AutoStart=tool.AutoStart,LaunchTiming=tool.LaunchTiming,
                LaunchDelaySeconds=Math.Max(0,Math.Min(300,tool.LaunchDelaySeconds)),CloseOnGameExit=closeOnExit,
                RequiresAdmin=requiresAdmin,ActiveVersionId=tool.ActiveVersionId,
                DisplayName=tool.DisplayName?.Trim()??string.Empty,
                WorkingDirectory=tool.ActiveVersion.WorkingDirectory??string.Empty,
                Arguments=tool.ActiveVersion.Arguments??string.Empty
            });
            await LoadDetailsAsync();
            ConfirmSuccess(closeWasUnsafe
                ? "设置已保存；该类型无法可靠跟踪进程，已自动关闭“随游戏退出关闭”"
                : adminWasUnsupported
                    ? "设置已保存；系统默认程序打开的类型不支持管理员运行"
                    : "游戏工具设置已保存");
        }

        private async Task RelocateSelectedGameToolAsync()
        {
            var tool=SelectedGameTool;
            var dialog=new OpenFileDialog
            {
                Title="重新定位外部启动项",
                Filter="可执行文件 (*.exe)|*.exe|快捷方式 (*.lnk)|*.lnk|脚本 (*.bat;*.cmd;*.ps1)|*.bat;*.cmd;*.ps1|所有文件 (*.*)|*.*",
                Multiselect=false,CheckFileExists=true
            };
            if(dialog.ShowDialog()!=true)return;
            var relocated=await plugin.RequestAsync<GameToolDto>(MessageTypes.RelocateGameTool,
                new RelocateGameToolRequestDto{ToolId=tool.ToolId,SourcePath=dialog.FileName},TimeSpan.FromMinutes(2));
            await LoadDetailsAsync();
            SelectedGameTool=GameTools.FirstOrDefault(x=>x.ToolId==relocated.ToolId)??GameTools.FirstOrDefault();
            ConfirmSuccess("已重新定位外部启动项："+relocated.DisplayName);
        }

        private async Task LaunchSelectedGameToolAsync()
        {
            await plugin.RequestAsync<object>(MessageTypes.LaunchGameTool,new GameToolCommandRequestDto{ToolId=SelectedGameTool.ToolId});
            ConfirmSuccess("已启动 "+SelectedGameTool.DisplayName);
        }

        private async Task OpenSelectedGameToolDirectoryAsync()
        {
            await plugin.RequestAsync<object>(MessageTypes.OpenGameToolDirectory,new GameToolCommandRequestDto{ToolId=SelectedGameTool.ToolId});
        }

        private async Task DeleteSelectedGameToolAsync()
        {
            var name = SelectedGameTool.DisplayName;
            if (!await plugin.ConfirmAsync(
                    "解除修改器绑定",
                    $"确认解除“{name}”与当前游戏的绑定？\n\n本地文件会保留，不会被删除。",
                    "解除绑定",
                    "取消")) return;
            await plugin.RequestAsync<object>(MessageTypes.DeleteGameTool,new GameToolCommandRequestDto{ToolId=SelectedGameTool.ToolId});
            await LoadDetailsAsync();ConfirmSuccess("已解除绑定并保留文件："+name);
        }

        private async Task SyncTrainerCatalogAsync()
        {
            var result=await plugin.RequestAsync<TrainerCatalogSyncResultDto>(MessageTypes.SyncTrainerCatalog,new{},TimeSpan.FromMinutes(2));
            ConfirmSuccess(result.Message);
            if(!string.IsNullOrWhiteSpace(TrainerSearchText))await SearchTrainerCatalogAsync();
        }

        private async Task SearchTrainerCatalogAsync()
        {
            var query=string.IsNullOrWhiteSpace(TrainerSearchText)?SelectedGame?.Name??string.Empty:TrainerSearchText.Trim();
            var results=await plugin.RequestAsync<TrainerCatalogItemDto[]>(MessageTypes.SearchTrainerCatalog,new TrainerCatalogQueryDto{Query=query,Limit=60},TimeSpan.FromMinutes(2));
            ApplyOnUi(()=>
            {
                Replace(TrainerCatalogResults,results);
                SelectedTrainerCatalogItem=TrainerCatalogResults.FirstOrDefault();
                StatusMessage=results.Length==0?"没有找到匹配的 FLiNG 修改器":"找到 "+results.Length+" 个 FLiNG 结果";
            });
            // A search result is only useful when its downloadable releases are immediately visible.
            // Keep the explicit button for retrying a failed release lookup, but load the first result
            // automatically and load again whenever the user selects another catalogue entry in the view.
            if (results.Length > 0) await LoadTrainerReleasesAsync();
        }

        private async Task LoadTrainerReleasesAsync()
        {
            var releases=await plugin.RequestAsync<TrainerReleaseDto[]>(MessageTypes.GetTrainerReleases,
                new TrainerCatalogQueryDto{CatalogId=SelectedTrainerCatalogItem.CatalogId},TimeSpan.FromMinutes(2));
            ApplyOnUi(()=>
            {
                Replace(TrainerReleases,releases);SelectedTrainerRelease=TrainerReleases.FirstOrDefault();
                StatusMessage=releases.Length==0?"没有可下载版本":"已加载 "+releases.Length+" 个版本";
            });
        }

        private async Task DownloadTrainerAsync()
        {
            var task=await plugin.RequestAsync<TaskStatusDto>(MessageTypes.DownloadTrainer,new DownloadTrainerRequestDto
            {PlayniteId=SelectedGame.PlayniteId,CatalogId=SelectedTrainerCatalogItem.CatalogId,ReleaseId=SelectedTrainerRelease.ReleaseId},TimeSpan.FromMinutes(10));
            NotifyTaskResults(new[]{task});await LoadDetailsAsync();ShowTrainerLibrary=false;
            StatusMessage="FLiNG 修改器已下载并绑定，自动启动保持关闭";
        }

        private async Task BackupAllAsync()
        {
            var tasks = await plugin.RequestAsync<TaskStatusDto[]>(MessageTypes.BackupAll, new BackupRequestDto { Force = true, Reason = "ManualAll" }, TimeSpan.FromMinutes(45));
            await RefreshCoreAsync(false);
            NotifyTaskResults(tasks);
        }

        private async Task DetectPathsAsync()
        {
            var candidates = await plugin.RequestAsync<SavePathCandidateDto[]>(MessageTypes.DetectSavePaths, new DetectionRequestDto { PlayniteId = SelectedGame.PlayniteId }, TimeSpan.FromMinutes(20));
            ConfirmSuccess(candidates.Length == 0 ? "未发现新的高可信存档路径候选" : $"发现 {candidates.Length} 个高可信存档路径候选");
            await LoadDetailsAsync();
        }

        private async Task ValidateAsync()
        {
            await plugin.RequestAsync<object>(MessageTypes.ValidateGame, new ValidateGameRequestDto { PlayniteId = SelectedGame.PlayniteId });
            await RefreshCoreAsync(false);
            ConfirmSuccess($"{SelectedGame.Name} 的存档校验已完成");
        }

        private async Task ValidateRestoreReadinessAsync()
        {
            var selectedId = SelectedBackup.BackupId;
            var result = await plugin.RequestAsync<RestoreReadinessDto>(MessageTypes.ValidateRestoreReadiness,
                new RestoreReadinessRequestDto { PlayniteId = SelectedGame.PlayniteId, BackupId = selectedId },
                TimeSpan.FromMinutes(15));
            await LoadDetailsAsync(true);
            ConfirmSuccess($"{SelectedGame.Name} / {selectedId}：{result.StatusDisplay}。{result.Summary}");
        }

        private async Task SavePolicyAsync()
        {
            await plugin.RequestAsync<object>(MessageTypes.UpdateGamePolicy, new GamePolicyUpdateDto { PlayniteId = SelectedGame.PlayniteId, Policy = SelectedGame.Policy });
            ConfirmSuccess($"已保存 {SelectedGame.Name} 的游戏策略");
        }

        private void CreatePolicyTemplate()
        {
            var selected = SelectedPolicyTemplate;
            var source = selected?.Policy ?? SelectedGame?.Policy ?? new BackupPolicyDto();
            var name = selected == null || string.IsNullOrWhiteSpace(selected.Name)
                ? "新策略模板"
                : selected.Name + "（副本）";
            SelectedPolicyTemplate = null!;
            PolicyTemplateDraft = new BackupPolicyTemplateDto
            {
                Name = name,
                Policy = GameSaveCenter.Core.Services.BackupPolicyTemplateCatalog.ClonePolicy(source)
            };
            PolicyTemplateNameDraft = PolicyTemplateDraft.Name;
            OnPropertyChanged(nameof(PolicyTemplateDraft));
            OnPropertyChanged(nameof(CanEditPolicyTemplate));
            RaiseCommandStates();
        }

        private async Task LoadPolicyTemplatesAsync()
        {
            var templates = await plugin.RequestAsync<BackupPolicyTemplateDto[]>(MessageTypes.ListPolicyTemplates, new { });
            var selectedId = SelectedPolicyTemplate?.TemplateId;
            ApplyOnUi(() =>
            {
                PolicyTemplates.ReplaceAll(templates ?? Array.Empty<BackupPolicyTemplateDto>(),
                    (left, right) => string.Equals(left.TemplateId, right.TemplateId, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(left.Name, right.Name, StringComparison.Ordinal)
                        && left.IsBuiltIn == right.IsBuiltIn);
                policyTemplatesLoaded = true;
                SelectedPolicyTemplate = PolicyTemplates.FirstOrDefault(x => string.Equals(x.TemplateId, selectedId, StringComparison.OrdinalIgnoreCase))
                    ?? PolicyTemplates.FirstOrDefault();
            });
        }

        private async Task SavePolicyTemplateAsync()
        {
            var draft = GameSaveCenter.Core.Services.BackupPolicyTemplateCatalog.Clone(PolicyTemplateDraft);
            draft.Name = PolicyTemplateNameDraft.Trim();
            var saved = await plugin.RequestAsync<BackupPolicyTemplateDto>(MessageTypes.SavePolicyTemplate,
                new PolicyTemplateSaveDto { Template = draft });
            policyTemplatesLoaded = false;
            await LoadPolicyTemplatesAsync();
            SelectedPolicyTemplate = PolicyTemplates.FirstOrDefault(x => string.Equals(x.TemplateId, saved.TemplateId, StringComparison.OrdinalIgnoreCase));
            ConfirmSuccess($"已保存策略模板“{saved.Name}”");
        }

        private async Task ApplyPolicyTemplateAsync()
        {
            await plugin.RequestAsync<object>(MessageTypes.ApplyPolicyTemplate,
                new ApplyPolicyTemplateDto { PlayniteId = SelectedGame.PlayniteId, TemplateId = SelectedPolicyTemplate.TemplateId });
            await RefreshDashboardAsync(false, false);
            ConfirmSuccess($"已将策略模板“{SelectedPolicyTemplate.Name}”复制到 {SelectedGame.Name}；后续修改模板不会影响该游戏");
        }

        private async Task DeletePolicyTemplateAsync()
        {
            var name = PolicyTemplateDraft.Name;
            await plugin.RequestAsync<object>(MessageTypes.DeletePolicyTemplate,
                new PolicyTemplateDeleteDto { TemplateId = PolicyTemplateDraft.TemplateId });
            policyTemplatesLoaded = false;
            await LoadPolicyTemplatesAsync();
            ConfirmSuccess($"已删除自定义策略模板“{name}”");
        }

        private async Task UpdateBackupMetadataAsync()
        {
            await plugin.RequestAsync<object>(MessageTypes.UpdateBackupMetadata, new BackupMetadataUpdateDto { PlayniteId = SelectedGame.PlayniteId, BackupId = SelectedBackup.BackupId, Comment = BackupComment, Locked = LockSelectedBackup });
            await LoadDetailsAsync();
            ConfirmSuccess("备份备注与锁定状态已保存");
        }

        private async Task CompareBackupAsync()
        {
            var index = Backups.IndexOf(SelectedBackup);
            if (index < 0 || index + 1 >= Backups.Count) { DiffSummary = "没有可比较的上一个版本。"; return; }
            var diff = await plugin.RequestAsync<BackupDiffDto>(MessageTypes.CompareBackups, new BackupCompareRequestDto { PlayniteId = SelectedGame.PlayniteId, LeftBackupId = Backups[index + 1].BackupId, RightBackupId = SelectedBackup.BackupId });
            LastBackupDiff = diff;
            DiffSummary = diff.Summary;
        }

        private async Task PreviewRetentionAsync()
        {
            var preview = await plugin.RequestAsync<RetentionPreviewDto>(MessageTypes.PreviewRetention, new GameQueryDto { PlayniteId = SelectedGame.PlayniteId });
            LastRetentionPreview = preview;
            RetentionSummary = preview.Summary;
        }

        private async Task AcceptCandidateAsync()
        {
            await plugin.RequestAsync<object>(MessageTypes.AcceptSavePath, new AcceptSavePathRequestDto { PlayniteId = SelectedGame.PlayniteId, Path = SelectedCandidate.Path, IncludeSubdirectories = true });
            ConfirmSuccess("已生成 Ludusavi 自定义规则草案");
            await LoadDetailsAsync();
        }

        private async Task RejectCandidateAsync()
        {
            await plugin.RequestAsync<object>(MessageTypes.RejectSavePath, new AcceptSavePathRequestDto { PlayniteId = SelectedGame.PlayniteId, Path = SelectedCandidate.Path });
            ConfirmSuccess("已忽略该存档路径候选");
            await LoadDetailsAsync();
        }

        private async Task RestoreAsync()
        {
            if (!await plugin.ConfirmAsync(
                    "GameSaveCenter 安全恢复",
                    "恢复前会先创建并锁定当前存档的 PreRestore 快照。请确认游戏、启动器和 MOD 管理器均已关闭。\n\n继续恢复选中的历史版本？",
                    "开始安全恢复",
                    "取消")) return;
            var task = await plugin.RequestAsync<TaskStatusDto>(MessageTypes.RestoreExecute, new RestoreRequestDto
            {
                PlayniteId = SelectedGame.PlayniteId,
                BackupId = SelectedBackup.BackupId,
                ConfirmedCurrentSnapshot = true,
                ConfirmedGameClosed = true,
                UserComment = "Playnite restore wizard"
            }, TimeSpan.FromMinutes(30));
            await RefreshCoreAsync(false);
            NotifyTaskResults(new[] { task });
        }

        private async Task UndoRestoreAsync()
        {
            if (!await plugin.ConfirmAsync(
                    "撤销恢复",
                    "撤销将恢复最近的 PreRestore 快照，并且仍会先保存当前状态。确认继续？",
                    "撤销恢复",
                    "取消")) return;
            var task = await plugin.RequestAsync<TaskStatusDto>(MessageTypes.UndoRestore, new GameQueryDto { PlayniteId = SelectedGame.PlayniteId }, TimeSpan.FromMinutes(30));
            await RefreshCoreAsync(false);
            NotifyTaskResults(new[] { task });
        }

        private bool CanRetrySelectedTask()
        {
            return CanRetryTask(SelectedTask);
        }

        private static bool CanRetryTask(TaskStatusDto? task)
        {
            if (task == null) return false;
            if (task.State != TaskState.Failed && task.State != TaskState.Cancelled) return false;
            if (string.Equals(task.TaskType, "MediaInbox", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.IsNullOrWhiteSpace(task.GameId)) return false;
            if (string.Equals(task.TaskType, "CloudUpload", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(task.ErrorCode, "RCLONE_COPY_FAILED", StringComparison.OrdinalIgnoreCase)) return true;
            return string.Equals(task.TaskType, "Backup", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(task.TaskType, "MediaSync", StringComparison.OrdinalIgnoreCase);
        }

        private async Task RetrySelectedTaskAsync()
        {
            var task = SelectedTask ?? throw new InvalidOperationException("请先选择失败或已取消的任务。");
            if (string.Equals(task.TaskType, "CloudUpload", StringComparison.OrdinalIgnoreCase)
                || string.Equals(task.ErrorCode, "RCLONE_COPY_FAILED", StringComparison.OrdinalIgnoreCase))
            {
                var result=await plugin.RequestAsync<TaskStatusDto>(
                    MessageTypes.RetryCloudUpload,
                    new GameQueryDto{PlayniteId=task.GameId},
                    TimeSpan.FromHours(2));
                NotifyTaskResults(new[]{result});
            }
            else if (string.Equals(task.TaskType, "Backup", StringComparison.OrdinalIgnoreCase))
            {
                var result = await plugin.RequestAsync<TaskStatusDto[]>(
                    MessageTypes.BackupGame,
                    new BackupRequestDto { PlayniteIds = { task.GameId }, Force = true, Reason = "Retry" },
                    TimeSpan.FromMinutes(15));
                NotifyTaskResults(result);
            }
            else if (string.Equals(task.TaskType, "MediaSync", StringComparison.OrdinalIgnoreCase))
            {
                var request = new MediaSyncRequestDto { UploadAfterSync = plugin.Settings.EnableCloudUpload };
                request.PlayniteIds.Add(task.GameId);
                var result = await plugin.RequestAsync<TaskStatusDto[]>(MessageTypes.SyncMedia, request, TimeSpan.FromMinutes(60));
                NotifyTaskResults(result);
            }
            else if (string.Equals(task.TaskType, "MediaInbox", StringComparison.OrdinalIgnoreCase))
            {
                var result = await plugin.RequestAsync<TaskStatusDto[]>(MessageTypes.SyncMedia, new MediaSyncRequestDto
                {
                    IncludeUnassignedInbox = true,
                    SharedOnly = true,
                    UploadAfterSync = plugin.Settings.EnableCloudUpload
                }, TimeSpan.FromMinutes(60));
                NotifyTaskResults(result);
            }
            else
            {
                throw new NotSupportedException("该任务类型暂不支持安全重试。");
            }
            await RefreshCoreAsync(false);
            StatusMessage = "重试任务已完成";
        }

        private void CopySelectedTaskError()
        {
            if (SelectedTask == null) return;
            var text = $"{SelectedTask.GameName} · {SelectedTask.TaskType}\r\n{SelectedTask.DetailMessage}\r\n任务 ID：{SelectedTask.TaskId}";
            Clipboard.SetText(text);
            StatusMessage = "任务详情已复制";
        }

        private async Task CancelSelectedTaskAsync()
        {
            try
            {
                if (SelectedTask == null || !SelectedTask.CanCancel || IsCancellingTask) return;
                var taskId = SelectedTask.TaskId;
                if (!await plugin.ConfirmAsync(
                    "取消后台任务",
                    $"取消“{SelectedTask.GameName} · {SelectedTask.TaskType}”任务？\n\n取消请求会在当前文件操作的安全边界生效。",
                    "取消任务",
                    "保留任务",
                    true)) return;
                IsCancellingTask = true;
                await plugin.EnsureWorkerAsync();
                var response = await plugin.RequestAsync<CancelTaskResultDto>(MessageTypes.CancelTask, new CancelTaskRequestDto { TaskId = taskId });
                StatusMessage = response.Cancelled ? "已发送取消请求" : "任务已经结束或无法取消";
                plugin.ShowInfo(StatusMessage);
                await RefreshDashboardAsync(false, false);
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "操作已取消";
            }
            catch (Exception ex)
            {
                ReportDashboardFailure(ex, true);
            }
            finally
            {
                IsCancellingTask = false;
            }
        }

        private void CopyDiagnostics()
        {
            Clipboard.SetText(DiagnosticSummary ?? string.Empty);
            StatusMessage = "诊断信息已复制到剪贴板";
            plugin.ShowInfo("GameSaveCenter 诊断信息已复制");
        }

        private string BuildDiagnosticSummary(WorkerSettingsSnapshotDto settings)
        {
            var builder = new StringBuilder();
            builder.AppendLine("GameSaveCenter 诊断摘要");
            builder.AppendLine("生成时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss zzz"));
            builder.AppendLine("插件版本：" + (typeof(DashboardViewModel).Assembly.GetName().Version?.ToString() ?? "dev"));
            builder.AppendLine("Worker：" + (Snapshot.WorkerHealthy ? "正常" : "不可用") + " / " + Snapshot.WorkerVersion);
            builder.AppendLine("Ludusavi：" + (Snapshot.LudusaviAvailable ? "可用" : "不可用") + " / " + Snapshot.LudusaviVersion);
            builder.AppendLine("Ludusavi 路径：" + EmptyAsUnset(settings.LudusaviExecutable));
            builder.AppendLine("存档目录：" + EmptyAsUnset(settings.LudusaviBackupDirectory));
            builder.AppendLine("媒体目录：" + EmptyAsUnset(settings.MediaArchiveDirectory));
            builder.AppendLine("数据目录：" + EmptyAsUnset(settings.DataDirectory));
            builder.AppendLine($"备份策略：{settings.BackupFormat} / {settings.Compression} {settings.CompressionLevel} / 完整 {settings.FullBackupLimit} / 差异 {settings.DifferentialBackupLimit}");
            builder.AppendLine("会话存档候选：" + (settings.EnableSessionSavePathDetection ? "启用" : "关闭"));
            builder.AppendLine("Rclone：" + (Snapshot.RcloneAvailable ? "可用" : "不可用") + " / 远端 " + (settings.RcloneDestinationConfigured ? "已配置" : "未配置"));
            builder.AppendLine($"游戏：管理 {Snapshot.ManagedGames} / 匹配 {Snapshot.MatchedGames} / 运行 {Snapshot.RunningGames} / 警告 {Snapshot.WarningGames}");
            builder.AppendLine();
            builder.AppendLine("最近失败任务：");
            var failed = Tasks.Where(x => x.State == TaskState.Failed).Take(10).ToList();
            if (failed.Count == 0) builder.AppendLine("- 无");
            foreach (var task in failed)
                builder.AppendLine($"- {task.CreatedLocal:yyyy-MM-dd HH:mm:ss} | {task.TaskType} | {task.GameName} | {task.DetailMessage}");
            return builder.ToString().TrimEnd();
        }

        private void OpenWorkerLog()
        {
            var log = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameSaveCenter", "Logs", "worker-launch.log");
            OpenPath(log);
        }

        private static void OpenPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new InvalidOperationException("路径尚未配置。");
            var expanded = Environment.ExpandEnvironmentVariables(path);
            if (File.Exists(expanded))
            {
                Process.Start("explorer.exe", "/select,\"" + expanded + "\"");
                return;
            }
            if (Directory.Exists(expanded))
            {
                Process.Start("explorer.exe", "\"" + expanded + "\"");
                return;
            }
            var parent = Path.GetDirectoryName(expanded);
            if (!string.IsNullOrWhiteSpace(parent) && Directory.Exists(parent))
            {
                Process.Start("explorer.exe", "\"" + parent + "\"");
                return;
            }
            throw new DirectoryNotFoundException(expanded);
        }

        private void Run(Func<Task> action)
        {
            Observe(RunAsync(action));
        }

        private async Task RunAsync(Func<Task> action)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                await plugin.EnsureWorkerAsync();
                await action();
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "操作已取消";
            }
            catch (NotifiedTaskException ex)
            {
                ReportDashboardFailure(ex, !plugin.Settings.EnableTaskNotifications);
            }
            catch (Exception ex)
            {
                ReportDashboardFailure(ex, true);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void RunLocal(Action action)
        {
            try { action(); }
            catch (Exception ex)
            {
                ReportDashboardFailure(ex, true);
            }
        }

        private void ReportDashboardFailure(Exception error, bool showNotification)
        {
            StatusMessage = error.Message;
            if (!showNotification)
            {
                Logger.Error(error, "GameSaveCenter dashboard command failed.");
                return;
            }

            try
            {
                plugin.ShowError(error.Message);
            }
            catch (Exception notificationError)
            {
                // A notification failure must not turn a recoverable command failure into a
                // Playnite Dispatcher crash. Preserve the original failure in the plugin log.
                Logger.Error(error, "GameSaveCenter dashboard command failed and could not be presented.");
                Logger.Error(notificationError, "GameSaveCenter failed to present dashboard command error.");
            }
        }

        private static void Observe(Task operation)
        {
            _ = operation.ContinueWith(
                task => Logger.Error(task.Exception, "GameSaveCenter dashboard command faulted outside its error boundary."),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        private void NotifyTaskResults(IEnumerable<TaskStatusDto> tasks)
        {
            var completed = tasks?.ToList() ?? new List<TaskStatusDto>();
            foreach (var task in completed) plugin.ShowTaskNotification(task);
            var failed = completed.FirstOrDefault(x => x.State == TaskState.Failed);
            if (failed != null) throw new NotifiedTaskException(failed.DetailMessage);
            var cancelled = completed.FirstOrDefault(x => x.State == TaskState.Cancelled);
            if (cancelled != null) throw new NotifiedTaskException(string.IsNullOrWhiteSpace(cancelled.Message) ? "任务已取消" : cancelled.Message);
        }

        private void ConfirmSuccess(string message)
        {
            StatusMessage = message;
            plugin.ShowInfo(message);
        }

        private sealed class NotifiedTaskException : InvalidOperationException
        {
            public NotifiedTaskException(string message) : base(message) { }
        }

        private bool FilterGame(object item)
        {
            var game = item as GameStatusDto;
            if (game == null) return false;

            var query = (GameSearchText ?? string.Empty).Trim();
            if (query.Length > 0)
            {
                var matched = Contains(game.Name, query)
                    || Contains(game.LudusaviName, query)
                    || Contains(game.PlatformDisplay, query)
                    || Contains(game.HealthStateDisplay, query);
                if (!matched) return false;
            }

            switch (GameStatusFilter)
            {
                case "已就绪":
                    return game.LudusaviMatched && !IsAttention(game);
                case "未匹配":
                    return !game.LudusaviMatched;
                case "运行中":
                    return game.IsRunning;
                case "需关注":
                    return IsAttention(game);
                case "有历史":
                    return game.BackupVersionCount > 0;
                default:
                    return true;
            }
        }

        private bool FilterTask(object item)
        {
            var task = item as TaskStatusDto;
            if (task == null) return false;
            var search = TaskSearchText.Trim();
            if (search.Length > 0
                && !ContainsTaskSearchValue(task.TaskId, search)
                && !ContainsTaskSearchValue(task.TaskTypeDisplay, search)
                && !ContainsTaskSearchValue(task.GameName, search)
                && !ContainsTaskSearchValue(task.DetailMessage, search)
                && !ContainsTaskSearchValue(task.ErrorMessage, search)) return false;
            if (TaskGameFilter != "全部" && !string.Equals(task.GameName, TaskGameFilter, StringComparison.OrdinalIgnoreCase)) return false;
            if (TaskTypeFilter != "全部" && !string.Equals(task.TaskTypeDisplay, TaskTypeFilter, StringComparison.OrdinalIgnoreCase)) return false;
            return TaskStatusFilter switch
            {
                "运行中" => task.State == TaskState.Running,
                "等待中" => task.State == TaskState.Queued || task.State == TaskState.WaitingForUser,
                "失败" => task.State == TaskState.Failed,
                "已完成" => task.State == TaskState.Succeeded || task.State == TaskState.Cancelled,
                _ => true
            };
        }

        private static bool ContainsTaskSearchValue(string? value, string search)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value!.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void RefreshTasksView()
        {
            var timer = Stopwatch.StartNew();
            TasksView.Refresh();
            timer.Stop();
            Logger.Debug($"[PERF] TaskSearch refresh={timer.ElapsedMilliseconds}ms tasks={Tasks.Count}");
        }

        private void RefreshMediaView()
        {
            var timer = Stopwatch.StartNew();
            MediaView.Refresh();
            timer.Stop();
            Logger.Debug($"[PERF] MediaSearch refresh={timer.ElapsedMilliseconds}ms media={Media.Count}");
        }

        private void RebuildTaskFilters()
        {
            // Full snapshots arrive on every refresh even when RecentTasks is unchanged.
            // Skip the O(n log n) game/type option rebuild when the visible task order and
            // identity are identical; user filter changes still refresh TasksView directly.
            var fingerprint = ComputeTaskFilterFingerprint(Tasks);
            if (fingerprint == lastTaskFilterFingerprint)
                return;
            lastTaskFilterFingerprint = fingerprint;

            var selectedGame = TaskGameFilter;
            var selectedType = TaskTypeFilter;
            TaskFilterOptionsSync.Sync(TaskGameFilterOptions, Tasks.Select(x => x.GameName));
            TaskFilterOptionsSync.Sync(TaskTypeFilterOptions, Tasks.Select(x => x.TaskTypeDisplay));

            // Only touch the selection when it actually disappeared; the incremental sync
            // above never Clear()s the option collections, so an existing selection survives.
            if (string.IsNullOrEmpty(selectedGame) || !TaskGameFilterOptions.Contains(selectedGame))
                TaskGameFilter = "全部";
            if (string.IsNullOrEmpty(selectedType) || !TaskTypeFilterOptions.Contains(selectedType))
                TaskTypeFilter = "全部";
            TasksView.Refresh();
        }

        private long lastTaskFilterFingerprint;

        private static long ComputeTaskFilterFingerprint(IEnumerable<TaskStatusDto> tasks)
        {
            unchecked
            {
                long hash = 17;
                var count = 0;
                foreach (var task in tasks)
                {
                    count++;
                    hash = hash * 31 + (task.TaskId?.GetHashCode() ?? 0);
                }
                return hash * 31 + count;
            }
        }

        private void ApplyGameSort()
        {
            if (GamesView == null) return;
            GamesView.SortDescriptions.Clear();
            switch (GameSortMode)
            {
                case "运行优先":
                    GamesView.SortDescriptions.Add(new SortDescription(nameof(GameStatusDto.IsRunning), ListSortDirection.Descending));
                    break;
                case "匹配优先":
                    GamesView.SortDescriptions.Add(new SortDescription(nameof(GameStatusDto.LudusaviMatched), ListSortDirection.Descending));
                    break;
                case "最近备份":
                    GamesView.SortDescriptions.Add(new SortDescription(nameof(GameStatusDto.LastBackupUtc), ListSortDirection.Descending));
                    break;
            }
            GamesView.SortDescriptions.Add(new SortDescription(nameof(GameStatusDto.Name), ListSortDirection.Ascending));
        }

        private void RefreshGameView(bool keepSelection = true)
        {
            if (GamesView == null) return;
            GamesView.Refresh();
            FilteredGameCount = GamesView.Cast<object>().Count();
            if (!keepSelection || SelectedGame == null || GamesView.Contains(SelectedGame)) return;

            suppressSelectionLoad = true;
            try { SelectedGame = GamesView.Cast<GameStatusDto>().FirstOrDefault(); }
            finally { suppressSelectionLoad = false; }
            CancelDetailsLoad();
            if (SelectedGame != null) Run(() => LoadDetailsAsync());
            else ClearSelectedGameDetails();
        }

        private void OnGamePickerStateChanged(object? sender, EventArgs e)
        {
            // Keep the preference in the plugin settings, but debounce disk writes so fast
            // keyboard search never causes one settings write per keystroke.
            plugin.Settings.GamePickerSearchText = gamePicker.SearchText;
            plugin.Settings.GamePickerStatusFilter = gamePicker.StatusFilter;
            plugin.Settings.GamePickerPlatformFilter = gamePicker.PlatformFilter;
            plugin.Settings.GamePickerSortMode = gamePicker.SortMode;
            plugin.Settings.GamePickerSelectedGameId = gamePicker.SelectedGame?.PlayniteId ?? string.Empty;
            gamePickerPersistenceCancellation?.Cancel();
            gamePickerPersistenceCancellation?.Dispose();
            gamePickerPersistenceCancellation = new CancellationTokenSource();
            var token = gamePickerPersistenceCancellation.Token;
            _ = PersistGamePickerStateAsync(token);
        }

        private void OnGamePickerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // SelectedItem raises both SelectedItem and SelectedGame notifications. Respond once
            // to the source notification so a rapid keyboard/mouse selection does not enqueue
            // duplicate IPC requests for the same game.
            if (!string.Equals(e.PropertyName, nameof(GamePickerViewModel.SelectedItem), StringComparison.Ordinal)) return;
            var selected = gamePicker.SelectedGame;
            OnPropertyChanged(nameof(SelectedGame));
            if (!suppressSelectionLoad) RefreshSelectedGameIcon();
            RaiseCommandStates();
            if (suppressSelectionLoad) return;
            ClearSelectedGameDetails();
            CancelDetailsLoad();
            if (selected != null && IsGameScopedWorkspace(CurrentWorkspace))
                Observe(LoadSelectionDetailsAsync(selected.PlayniteId));
        }

        private void OnPlayniteGameStarted(Guid playniteId)
        {
            ApplyOnUi(() =>
            {
                lastStartedPlayniteId = playniteId.ToString("D");
                pendingAutoSelectPlayniteId = playniteId.ToString("D");
                TryApplyPendingAutoSelection();
            });
        }

        private void TryApplyPendingAutoSelection()
        {
            if (pendingAutoSelectPlayniteId == null) return;
            var game = Games.FirstOrDefault(x =>
                string.Equals(x.PlayniteId, pendingAutoSelectPlayniteId, StringComparison.OrdinalIgnoreCase));
            if (game == null) return;
            pendingAutoSelectPlayniteId = null;
            initialSelectionApplied = true;
            gamePicker.SelectGame(game);
        }

        private void ApplyInitialSelectionIfNeeded()
        {
            if (initialSelectionApplied) return;
            initialSelectionApplied = true;
            var selected = GameSelectionResolver.ResolveInitial(
                Games,
                plugin.Settings.GamePickerSelectedGameId,
                lastStartedPlayniteId);
            if (selected != null)
            {
                gamePicker.SelectGame(selected);
            }
        }

        private void RefreshSelectedGameIcon()
        {
            ImageSource? icon = null;
            var selected = gamePicker.SelectedGame;
            if (selected != null && Guid.TryParse(selected.PlayniteId, out var playniteId))
                icon = gameIconProvider.Load(playniteId);
            SelectedGameIcon = icon!;
        }

        /// <summary>
        /// Loads only the currently selected game's detail surface. The IPC client does not
        /// expose cancellation for an already-written named-pipe request, so a generation token
        /// is used as a second safety boundary: stale responses are ignored and never overwrite
        /// the newly selected game's UI. This path deliberately does not toggle IsBusy, allowing
        /// a fast picker sequence to converge on the latest selection while a normal command is
        /// still running.
        /// </summary>
        private async Task LoadSelectionDetailsAsync(string playniteId)
        {
            var next = new CancellationTokenSource();
            var previous = Interlocked.Exchange(ref detailsLoadCancellation, next);
            previous?.Cancel();
            previous?.Dispose();
            var generation = Interlocked.Increment(ref detailsLoadGeneration);
            try
            {
                await LoadDetailsAsync(false, next.Token, generation, playniteId).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            finally
            {
                if (ReferenceEquals(Interlocked.CompareExchange(ref detailsLoadCancellation, null, next), next))
                    next.Dispose();
            }
        }

        private void CancelDetailsLoad()
        {
            Interlocked.Increment(ref detailsLoadGeneration);
            var cancellation = Interlocked.Exchange(ref detailsLoadCancellation, null);
            if (cancellation == null) return;
            try { cancellation.Cancel(); }
            finally { cancellation.Dispose(); }
        }

        private async Task PersistGamePickerStateAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(500, token).ConfigureAwait(false);
                if (token.IsCancellationRequested) return;
                if (uiSynchronizationContext == null)
                {
                    SaveGamePickerSettings();
                    return;
                }
                uiSynchronizationContext.Post(_ =>
                {
                    if (!token.IsCancellationRequested) SaveGamePickerSettings();
                }, null);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Logger.Error(ex, "GameSaveCenter could not persist global game picker state."); }
        }

        private void SaveGamePickerSettings()
        {
            try { plugin.SavePluginSettings(plugin.Settings); }
            catch (Exception ex) { Logger.Error(ex, "GameSaveCenter could not save global game picker settings on the UI dispatcher."); }
        }

        private static bool Contains(string value, string query)
            => !string.IsNullOrWhiteSpace(value) && value.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0;

        private static bool IsAttention(GameStatusDto game)
            => string.Equals(game.HealthState, "Attention", StringComparison.OrdinalIgnoreCase)
               || string.Equals(game.HealthState, "Risk", StringComparison.OrdinalIgnoreCase)
               || string.Equals(game.HealthState, "Warning", StringComparison.OrdinalIgnoreCase)
               || string.Equals(game.HealthState, "LudusaviUnavailable", StringComparison.OrdinalIgnoreCase);

        private void ClearSelectedGameDetails()
        {
            ApplyOnUi(() =>
            {
                Backups.Clear();
                Media.Clear();
                MediaSources.Clear();
                SaveCandidates.Clear();
                GameTools.Clear();
                SelectedGameTool = null!;
                SelectedGameToolVersion = null!;
                SelectedBackup = null!;
                SelectedCandidate = null!;
                SelectedMedia = null!;
                MediaSummary = new MediaStorageSummaryDto();
            });
        }

        private void RaiseCommandStates()
        {
            if (commandRefreshScheduled) return;
            commandRefreshScheduled = true;
            try
            {
                plugin.PlayniteApi.MainView.UIDispatcher.BeginInvoke(
                    DispatcherPriority.DataBind,
                    new Action(RaiseCommandStatesCore));
            }
            catch (Exception)
            {
                // A closing Playnite dispatcher must not leave commands stale; fall back to
                // an immediate refresh so the view can still evaluate CanExecute locally.
                commandRefreshScheduled = false;
                RaiseCommandStatesCore();
            }
        }

        private void RaiseCommandStatesCore()
        {
            commandRefreshScheduled = false;
            foreach (var command in new[]
            {
                RefreshCommand, BackupSelectedCommand, BackupAllCommand, SyncMediaCommand,
                DetectPathsCommand, ValidateCommand, RestoreCommand,
                ValidateRestoreReadinessCommand, UndoRestoreCommand, LoadDetailsCommand, SavePolicyCommand,
                CreatePolicyTemplateCommand, SavePolicyTemplateCommand, ApplyPolicyTemplateCommand, DeletePolicyTemplateCommand,
                UpdateBackupMetadataCommand, CompareBackupCommand, PreviewRetentionCommand,
                AddMediaSourceCommand, AcceptCandidateCommand, RejectCandidateCommand, ReassignMediaCommand,
                UpdateMediaMetadataCommand,OpenSelectedMediaCommand,RevealSelectedMediaCommand,
                AssignInboxMediaCommand, IgnoreInboxMediaCommand,
                CancelTaskCommand, RetryTaskCommand, CopyTaskErrorCommand, RefreshDiagnosticsCommand, SyncDeviceStatesCommand, SaveDeviceDecisionCommand,
                StageRemoteBackupCommand,RestoreStagedRemoteBackupCommand,CopyDiagnosticsCommand,
                SaveProcessMappingCommand,DeleteProcessMappingCommand,RunEnvironmentCheckCommand,SkipOnboardingCommand,CompleteOnboardingCommand,OnboardingTestBackupCommand,
                OpenDataDirectoryCommand, OpenBackupDirectoryCommand, OpenMediaDirectoryCommand, OpenWorkerLogCommand
                ,ImportTrainerCommand,ImportCheatTableCommand,ImportCustomLaunchItemCommand,ImportToolFolderCommand,SaveGameToolCommand,LaunchGameToolCommand,
                ConfirmGameToolImportCommand,CancelGameToolImportCommand,
                OpenGameToolDirectoryCommand,DeleteGameToolCommand,RelocateGameToolCommand,SyncTrainerCatalogCommand,SearchTrainerCatalogCommand,
                LoadTrainerReleasesCommand,DownloadTrainerCommand
            }.OfType<RelayCommand>())
            {
                command.RaiseCanExecuteChanged();
            }
        }

        private void ApplyOnUi(Action action)
        {
            if (action == null) return;
            var dispatcher = plugin.PlayniteApi.MainView.UIDispatcher;
            if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished) return;
            try
            {
                if (dispatcher.CheckAccess())
                {
                    action();
                    return;
                }

                // Task event listeners may be on a Worker continuation. Retain synchronous
                // ordering for collection/filter updates, but never invoke a closing Playnite UI.
                dispatcher.Invoke(action, DispatcherPriority.DataBind);
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException) && !(ex is StackOverflowException))
            {
                Logger.Error(ex, "GameSaveCenter skipped a Dashboard UI collection update because the callback failed or the dispatcher is unavailable.");
            }
        }
        private static bool Replace<T>(ObservableCollection<T> target, IEnumerable<T> source, Func<T, T, bool>? areSame = null)
        {
            if (target is BatchObservableCollection<T> batch)
            {
                return batch.ReplaceAll(source, areSame);
            }

            var incoming = (source ?? Enumerable.Empty<T>()).ToList();
            var existing = target.ToList();
            if (existing.SequenceEqual(incoming))
                return false;

            // Avoid Clear()+Add for large virtualized DataGrids. Replacing the backing
            // collection in one Reset keeps WPF's item extent and recycled row range in sync.
            target.Clear();
            foreach (var item in incoming)
                target.Add(item);
            return true;
        }
        private static string EmptyAsUnset(string value) => string.IsNullOrWhiteSpace(value) ? "（未配置）" : value;
    }
}
