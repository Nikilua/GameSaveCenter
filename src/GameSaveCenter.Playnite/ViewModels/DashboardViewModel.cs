using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using GameSaveCenter.Playnite.Services;
using GameSaveCenter.Playnite.Settings;
using Playnite.SDK;

namespace GameSaveCenter.Playnite.ViewModels
{
    /// <summary>Apple-inspired dashboard state; all file operations remain in the Worker.</summary>
    public sealed partial class DashboardViewModel : ObservableObject
    {
        partial void OnWorkspaceStateInitialize();
        partial void OnWorkspaceStateInputsChanged();

        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly GameSaveCenterPlugin plugin;
        private readonly GamePickerViewModel gamePicker;
        private readonly PlayniteGameIconProvider gameIconProvider;
        private readonly PlayniteGameBackgroundProvider gameBackgroundProvider;
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
        private ImageSource? selectedGameBackground;
        private Brush? selectedGameBackgroundAmbientBrush;
        private bool hasSelectedGameBackgroundAmbientMaterial;
        private long lastTaskEventSequence;
        private CancellationTokenSource? taskEventSubscription;
        private CancellationTokenSource? gamePickerPersistenceCancellation;
        private CancellationTokenSource? detailsLoadCancellation;
        private CancellationTokenSource? selectedGameBackgroundCancellation;
        private CancellationTokenSource? initialSynchronizationCancellation;
        private long deferredUiWorkGeneration;
        private long detailsLoadGeneration;
        private long selectedGameBackgroundGeneration;
        private bool selectedGameBackgroundPreferenceApplied;
        private Task? taskEventListener;
        private bool commandRefreshScheduled;
        private readonly DebouncedRefresh taskSearchRefresh;
        private readonly DebouncedRefresh mediaSearchRefresh;
        private readonly DebouncedRefresh uiStateSave;
        private DateTime lastFullDashboardRefreshUtc=DateTime.MinValue;
        private string? selectedGamePolicyId;
        private BackupPolicyDto? selectedGamePolicyBaseline;
        private string statusMessage = "准备就绪";
        private BackupVersionDto selectedBackup = null!;
        private DashboardSnapshotDto snapshot = new DashboardSnapshotDto();
        private EnvironmentCheckReportDto environmentCheck = new EnvironmentCheckReportDto();
        private bool environmentCheckLoaded;
        private bool safeModePromptShown;
        private RecentProtectionSummary recentProtection = new RecentProtectionSummary(30, 0, 0, 0, 0, Array.Empty<RecentProtectionItem>());
        private SavePathCandidateDto selectedCandidate = null!;
        private string backupComment = string.Empty;
        private bool lockSelectedBackup;
        private bool backupCommentDirty;
        private bool backupLockDirty;
        private MediaItemDto selectedMedia = null!;
        private MediaStorageSummaryDto mediaSummary = new MediaStorageSummaryDto();
        private string mediaComment = string.Empty;
        private bool mediaFavorite;
        private bool mediaCommentDirty;
        private bool mediaFavoriteDirty;
        private bool applyingEditorSelection;
        private string mediaSearchText = string.Empty;
        private string mediaFilter = "全部";
        private string mediaInboxMode = "待归类";
        private GameStatusDto mediaTargetGame = null!;
        private MediaItemDto selectedInboxMedia = null!;
        private GameStatusDto inboxTargetGame = null!;
        private const int MediaInboxBatchSize = 500;
        private TaskStatusDto selectedTask = null!;
        private ValidationFindingDto selectedFinding = null!;
        private WorkerSettingsSnapshotDto effectiveSettings = new WorkerSettingsSnapshotDto();
        private string diagnosticSummary = "诊断信息尚未加载。";
        private string integritySummary = "尚未运行完整性自检。";
        private string metadataBackupSummary = "尚未生成元数据灾备包。";
        private string metadataRestoreSummary = "尚未预览元数据恢复。";
        private string repositoryRebuildSummary = "尚未重建备份索引。";
        private string pathRemapOldRoot = string.Empty;
        private string pathRemapNewRoot = string.Empty;
        private string pathRemapSummary = "尚未执行路径迁移。";
        private string taskReconcileSummary = "尚未协调中断任务。";
        private StorageAnalysisDto storageAnalysis = new StorageAnalysisDto { Summary = "尚未分析备份存储。" };
        private RetentionSimulationPreviewDto retentionSimulation = new RetentionSimulationPreviewDto { Summary = "尚未生成全局保留预览。" };
        private LocalMirrorStatusDto localMirrorStatus = new LocalMirrorStatusDto { Message = "尚未检查本地镜像。" };
        private string diffSummary = "选择两个版本后，比较结果会显示在这里。";
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
        private bool isTrainerCatalogLoading;
        private bool isTrainerReleasesLoading;
        private long trainerReleaseLoadGeneration;
        private string? trainerReleaseLoadCatalogId;
        private string? pendingTrainerReleaseCatalogId;
        private long mediaInboxLoadGeneration;
        private string? pendingMediaInboxLoadMode;
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
        private readonly PlayniteGameStartedSubscription playniteGameStartedSubscription;

        public DashboardViewModel(GameSaveCenterPlugin plugin)
        {
            this.plugin = plugin;
            playniteGameStartedSubscription = new PlayniteGameStartedSubscription(
                callback => plugin.PlayniteGameStarted += callback,
                callback => plugin.PlayniteGameStarted -= callback,
                OnPlayniteGameStarted);
            currentWorkspace = plugin.SessionLastWorkspace ?? WorkspaceKind.Overview;
            gamePicker = new GamePickerViewModel();
            gamePicker.ApplyPersistedState(plugin.Settings.GamePickerSearchText, plugin.Settings.GamePickerStatusFilter, plugin.Settings.GamePickerPlatformFilter, plugin.Settings.GamePickerSortMode);
            gamePicker.StateChanged += OnGamePickerStateChanged;
            gamePicker.PropertyChanged += OnGamePickerPropertyChanged;
            OnWorkspaceStateInitialize();
            gameIconProvider = new PlayniteGameIconProvider(plugin.PlayniteApi);
            gameBackgroundProvider = new PlayniteGameBackgroundProvider(plugin.PlayniteApi);
            gameSearchText = gamePicker.SearchText;
            gameStatusFilter = gamePicker.StatusFilter;
            gameSortMode = gamePicker.SortMode;
            GamesView = CollectionViewSource.GetDefaultView(Games);
            GamesView.Filter = FilterGame;
            TasksView = CollectionViewSource.GetDefaultView(Tasks);
            TasksView.Filter = FilterTask;
            MediaView = CollectionViewSource.GetDefaultView(Media);
            MediaView.Filter = FilterMedia;
            MediaInboxItems = UnassignedMedia;
            taskSearchRefresh = new DebouncedRefresh(() => ApplyOnUi(RefreshTasksView), TimeSpan.FromMilliseconds(180));
            mediaSearchRefresh = new DebouncedRefresh(() => ApplyOnUi(RefreshMediaView), TimeSpan.FromMilliseconds(180));
            uiStateSave = new DebouncedRefresh(SaveUiStateSettings, TimeSpan.FromMilliseconds(500));
            taskStatusFilter = TaskStatusFilterOptions.Contains(plugin.Settings.TaskStatusFilterState) ? plugin.Settings.TaskStatusFilterState : "全部";
            taskGameFilter = "全部";
            taskTypeFilter = "全部";
            taskSearchText = plugin.Settings.TaskSearchTextState ?? string.Empty;
            mediaFilter = MediaFilterOptions.Contains(plugin.Settings.MediaFilterState) ? plugin.Settings.MediaFilterState : "全部";
            mediaSearchText = plugin.Settings.MediaSearchTextState ?? string.Empty;
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
            AssignInboxMediaCommand = new RelayCommand(_ => Run(AssignInboxMediaAsync), _ => !IsBusy && MediaInboxMode == "待归类" && SelectedInboxMedia != null && InboxTargetGame != null);
            IgnoreInboxMediaCommand = new RelayCommand(_ => Run(IgnoreInboxMediaAsync), _ => !IsBusy && MediaInboxMode == "待归类" && SelectedInboxMedia != null);
            AssignInboxMediaBatchCommand = new RelayCommand(value => Run(() => AssignInboxMediaBatchAsync(value)), value => !IsBusy && MediaInboxMode == "待归类" && InboxTargetGame != null && GetSelectedInboxMedia(value).Count > 0);
            IgnoreInboxMediaBatchCommand = new RelayCommand(value => Run(() => IgnoreInboxMediaBatchAsync(value)), value => !IsBusy && MediaInboxMode == "待归类" && GetSelectedInboxMedia(value).Count > 0);
            RestoreIgnoredMediaBatchCommand = new RelayCommand(value => Run(() => RestoreIgnoredMediaBatchAsync(value)), value => !IsBusy && MediaInboxMode == "已忽略" && GetSelectedInboxMedia(value).Count > 0);
            CancelTaskCommand = new RelayCommand(_ => _ = CancelSelectedTaskAsync(), _ => SelectedTask != null && SelectedTask.CanCancel && !IsCancellingTask);
            RetryTaskCommand = new RelayCommand(_ => Run(RetrySelectedTaskAsync), _ => !IsBusy && CanRetrySelectedTask());
            RetryAllTasksCommand = new RelayCommand(_ => Run(RetryAllTasksAsync), _ => !IsBusy && RetryableTaskCount > 0);
            CopyTaskErrorCommand = new RelayCommand(_ => Run(CopySelectedTaskErrorAsync), _ => SelectedTask != null && !string.IsNullOrWhiteSpace(SelectedTask.DetailMessage));
            OpenAttentionCenterCommand = new RelayCommand(_ => OpenAttentionCenter());
            OpenMaintenanceCommand = new RelayCommand(_ => OpenMaintenance());
            OpenAttentionFindingCommand = new RelayCommand(value => OpenAttentionFinding(value as ValidationFindingDto));
            OpenProtectionGamesCommand = new RelayCommand(_ => OpenProtectionGames());
            OpenProtectionItemCommand = new RelayCommand(value => OpenProtectionItem(value as RecentProtectionItem));
            ApplyRecommendedProtectionCommand = new RelayCommand(_ => Run(ApplyRecommendedProtectionAsync), _ => !IsBusy);
            RefreshDiagnosticsCommand = new RelayCommand(_ => Run(RefreshDiagnosticsAsync), _ => !IsBusy);
            RunIntegrityCheckCommand = new RelayCommand(_ => Run(RunIntegrityCheckAsync), _ => !IsBusy);
            CreateMetadataBackupCommand = new RelayCommand(_ => Run(CreateMetadataBackupAsync), _ => !IsBusy && !string.IsNullOrWhiteSpace(EffectiveSettings.DataDirectory));
            RestoreMetadataBackupCommand = new RelayCommand(_ => Run(RestoreMetadataBackupAsync), _ => !IsBusy && !string.IsNullOrWhiteSpace(EffectiveSettings.DataDirectory));
            RebuildRepositoryCommand = new RelayCommand(_ => Run(RebuildRepositoryAsync), _ => !IsBusy && Snapshot.LudusaviAvailable);
            RunPathRemapCommand = new RelayCommand(_ => Run(RunPathRemapAsync), _ => !IsBusy && !string.IsNullOrWhiteSpace(PathRemapOldRoot) && !string.IsNullOrWhiteSpace(PathRemapNewRoot));
            ReconcileTasksCommand = new RelayCommand(_ => Run(ReconcileTasksAsync), _ => !IsBusy);
            RefreshStorageAnalysisCommand = new RelayCommand(_ => Run(RefreshStorageAnalysisAsync), _ => !IsBusy);
            RefreshRetentionSimulationCommand = new RelayCommand(_ => Run(RefreshRetentionSimulationAsync), _ => !IsBusy);
            ApplyRetentionSimulationCommand = new RelayCommand(_ => Run(ApplyRetentionSimulationAsync), _ => !IsBusy && RetentionSimulation.DeleteCandidateCount > 0);
            RefreshLocalMirrorStatusCommand = new RelayCommand(_ => Run(RefreshLocalMirrorStatusAsync), _ => !IsBusy);
            SyncLocalMirrorCommand = new RelayCommand(_ => Run(SyncLocalMirrorAsync), _ => !IsBusy && EffectiveSettings.EnableLocalMirror);
            CopyMaintenanceReportCommand = new RelayCommand(_ => Run(CopyMaintenanceReportAsync), _ => !IsBusy);
            ExportMaintenanceReportCommand = new RelayCommand(_ => Run(ExportMaintenanceReportAsync), _ => !IsBusy);
            ExitSafeModeCommand = new RelayCommand(_ => Run(ExitSafeModeAsync), _ => !IsBusy);
            RunEnvironmentCheckCommand = new RelayCommand(_ => Run(() => RunEnvironmentCheckAsync(true)), _ => !IsBusy);
            SkipOnboardingCommand = new RelayCommand(_ => SkipOnboarding(), _ => !IsBusy && IsOnboardingPending);
            CompleteOnboardingCommand = new RelayCommand(_ => CompleteOnboarding(), _ => !IsBusy && IsOnboardingPending && EnvironmentCheck.IsReady && EnvironmentCheck.CheckedUtc != default(DateTime));
            OnboardingTestBackupCommand = new RelayCommand(_ => Run(BackupSelectedAsync), _ => !IsBusy && IsOnboardingPending && SelectedGame != null && SelectedGame.LudusaviMatched && Snapshot.LudusaviAvailable);
            SyncDeviceStatesCommand = new RelayCommand(_ => Run(SyncDeviceStatesAsync), _ => !IsBusy);
            SaveDeviceDecisionCommand = new RelayCommand(_ => Run(SaveDeviceDecisionAsync), _ => !IsBusy && SelectedDeviceComparison != null);
            StageRemoteBackupCommand = new RelayCommand(_ => Run(StageRemoteBackupAsync), _ => !IsBusy && SelectedDeviceComparison != null && !string.IsNullOrWhiteSpace(SelectedDeviceComparison.RemoteBackupId));
            RestoreStagedRemoteBackupCommand = new RelayCommand(_ => Run(RestoreStagedRemoteBackupAsync), _ => !IsBusy && StagedRemoteBackup != null && StagedRemoteBackup.Verified);
            SaveProcessMappingCommand = new RelayCommand(_ => Run(SaveProcessMappingAsync), _ => !IsBusy && !string.IsNullOrWhiteSpace(ProcessMappingExecutable) && ProcessMappingTargetGame != null);
            DeleteProcessMappingCommand = new RelayCommand(_ => Run(DeleteProcessMappingAsync), _ => !IsBusy && SelectedProcessMapping != null);
            CopyDiagnosticsCommand = new RelayCommand(_ => Run(CopyDiagnosticsAsync), _ => !string.IsNullOrWhiteSpace(DiagnosticSummary));
            CreateDiagnosticsPackageCommand = new RelayCommand(_ => Run(CreateDiagnosticsPackageAsync), _ => !IsBusy && !string.IsNullOrWhiteSpace(EffectiveSettings.DataDirectory));
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
            LoadTrainerReleasesCommand = new RelayCommand(value => RequestTrainerReleasesLoad(value as TrainerCatalogItemDto), _ => SelectedTrainerCatalogItem != null);
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
        public BatchObservableCollection<ActivityEntryDto> Activities { get; } = new BatchObservableCollection<ActivityEntryDto>();
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
        public IReadOnlyList<BackupAnomalyProtectionOption> BackupAnomalyProtectionOptions { get; } = new[]
        {
            new BackupAnomalyProtectionOption(BackupAnomalyProtectionLevel.Off, "关闭比较告警"),
            new BackupAnomalyProtectionOption(BackupAnomalyProtectionLevel.Normal, "标准保护"),
            new BackupAnomalyProtectionOption(BackupAnomalyProtectionLevel.Strict, "严格保护")
        };
        public BatchObservableCollection<MediaItemDto> Media { get; } = new BatchObservableCollection<MediaItemDto>();
        private BatchObservableCollection<MediaItemDto> unassignedMedia = new BatchObservableCollection<MediaItemDto>();
        public BatchObservableCollection<MediaItemDto> UnassignedMedia
        {
            get => unassignedMedia;
            private set => SetValue(ref unassignedMedia, value);
        }
        private BatchObservableCollection<MediaItemDto> ignoredMedia = new BatchObservableCollection<MediaItemDto>();
        public BatchObservableCollection<MediaItemDto> IgnoredMedia
        {
            get => ignoredMedia;
            private set => SetValue(ref ignoredMedia, value);
        }
        private BatchObservableCollection<MediaItemDto> mediaInboxItems = new BatchObservableCollection<MediaItemDto>();
        public BatchObservableCollection<MediaItemDto> MediaInboxItems
        {
            get => mediaInboxItems;
            private set => SetValue(ref mediaInboxItems, value);
        }
        public BatchObservableCollection<AuditLogEntryDto> Audit { get; } = new BatchObservableCollection<AuditLogEntryDto>();
        public BatchObservableCollection<SavePathCandidateDto> SaveCandidates { get; } = new BatchObservableCollection<SavePathCandidateDto>();
        public BatchObservableCollection<MediaSourceRuleDto> MediaSources { get; } = new BatchObservableCollection<MediaSourceRuleDto>();
        public BatchObservableCollection<GameToolDto> GameTools { get; } = new BatchObservableCollection<GameToolDto>();
        public BatchObservableCollection<GameToolEntryCandidateDto> ImportEntryCandidates { get; } = new BatchObservableCollection<GameToolEntryCandidateDto>();
        public BatchObservableCollection<TrainerCatalogItemDto> TrainerCatalogResults { get; } = new BatchObservableCollection<TrainerCatalogItemDto>();
        public BatchObservableCollection<TrainerReleaseDto> TrainerReleases { get; } = new BatchObservableCollection<TrainerReleaseDto>();
        public IReadOnlyList<GameToolRunningOption> GameToolIfAlreadyRunningOptions { get; } = new[]
        {
            new GameToolRunningOption(GameToolIfAlreadyRunning.Skip, "已有实例：跳过启动"),
            new GameToolRunningOption(GameToolIfAlreadyRunning.Restart, "已有实例：关闭后重启"),
            new GameToolRunningOption(GameToolIfAlreadyRunning.AllowAnotherInstance, "已有实例：允许多开")
        };
        public IReadOnlyList<GameToolRiskOption> GameToolRiskCategoryOptions { get; } = new[]
        {
            new GameToolRiskOption(GameToolRiskCategory.Unknown, "未分类（反作弊游戏需授权）"),
            new GameToolRiskOption(GameToolRiskCategory.GeneralUtility, "通用工具"),
            new GameToolRiskOption(GameToolRiskCategory.GameModification, "游戏修改工具")
        };
        public ICollectionView GamesView { get; }
        public ICollectionView TasksView { get; }
        public ICollectionView MediaView { get; }
        public IReadOnlyList<string> MediaFilterOptions { get; } = new[] { "全部", "截图", "录像", "收藏" };
        public IReadOnlyList<string> GameStatusFilterOptions { get; } = new[] { "全部", "已就绪", "未匹配", "运行中", "需关注", "有历史" };
        public IReadOnlyList<string> GameSortOptions { get; } = new[] { "名称", "运行优先", "匹配优先", "最近备份" };

        public DashboardSnapshotDto Snapshot
        {
            get => snapshot;
            private set
            {
                SetValue(ref snapshot, value);
                OnWorkspaceStateInputsChanged();
            }
        }
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
                OnWorkspaceStateInputsChanged();
                RaiseCommandStates();
            }
        }
        public bool IsBusy
        {
            get => isBusy;
            private set
            {
                SetValue(ref isBusy, value);
                OnWorkspaceStateInputsChanged();
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
        public string IntegritySummary { get => integritySummary; private set => SetValue(ref integritySummary, value); }
        public string MetadataBackupSummary { get => metadataBackupSummary; private set => SetValue(ref metadataBackupSummary, value); }
        public string MetadataRestoreSummary { get => metadataRestoreSummary; private set => SetValue(ref metadataRestoreSummary, value); }
        public string RepositoryRebuildSummary { get => repositoryRebuildSummary; private set => SetValue(ref repositoryRebuildSummary, value); }
        public string PathRemapOldRoot
        {
            get => pathRemapOldRoot;
            set
            {
                SetValue(ref pathRemapOldRoot, value ?? string.Empty);
                RaiseCommandStates();
            }
        }
        public string PathRemapNewRoot
        {
            get => pathRemapNewRoot;
            set
            {
                SetValue(ref pathRemapNewRoot, value ?? string.Empty);
                RaiseCommandStates();
            }
        }
        public string PathRemapSummary { get => pathRemapSummary; private set => SetValue(ref pathRemapSummary, value); }
        public string TaskReconcileSummary { get => taskReconcileSummary; private set => SetValue(ref taskReconcileSummary, value); }
        public StorageAnalysisDto StorageAnalysis { get => storageAnalysis; private set => SetValue(ref storageAnalysis, value ?? new StorageAnalysisDto()); }
        public RetentionSimulationPreviewDto RetentionSimulation { get => retentionSimulation; private set => SetValue(ref retentionSimulation, value ?? new RetentionSimulationPreviewDto()); }
        public LocalMirrorStatusDto LocalMirrorStatus { get => localMirrorStatus; private set => SetValue(ref localMirrorStatus, value ?? new LocalMirrorStatusDto()); }
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
                uiStateSave?.Schedule();
            }
        }
        public string TaskStatusFilter
        {
            get => taskStatusFilter;
            set
            {
                SetValue(ref taskStatusFilter, string.IsNullOrWhiteSpace(value) ? "全部" : value);
                TasksView.Refresh();
                uiStateSave?.Schedule();
            }
        }
        public string TaskGameFilter
        {
            get => taskGameFilter;
            set
            {
                SetValue(ref taskGameFilter, string.IsNullOrWhiteSpace(value) ? "全部" : value);
                TasksView.Refresh();
                uiStateSave?.Schedule();
            }
        }
        public string TaskTypeFilter
        {
            get => taskTypeFilter;
            set
            {
                SetValue(ref taskTypeFilter, string.IsNullOrWhiteSpace(value) ? "全部" : value);
                TasksView.Refresh();
                uiStateSave?.Schedule();
            }
        }
        public int FilteredGameCount { get => filteredGameCount; private set => SetValue(ref filteredGameCount, value); }
        public WorkspaceKind CurrentWorkspace
        {
            get => currentWorkspace;
            set
            {
                SetValue(ref currentWorkspace, value);
                plugin.SessionLastWorkspace = value;
                uiStateSave?.Schedule();
            }
        }
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
            set
            {
                var previousCatalogId = selectedTrainerCatalogItem == null ? null : selectedTrainerCatalogItem.CatalogId;
                var selectionChanged = !Equals(selectedTrainerCatalogItem, value);
                selectedTrainerCatalogItem = value!;
                if (selectionChanged)
                    OnPropertyChanged(nameof(SelectedTrainerCatalogItem));
                var currentCatalogId = value?.CatalogId;
                if (!string.Equals(previousCatalogId, currentCatalogId, StringComparison.OrdinalIgnoreCase))
                    Interlocked.Increment(ref trainerReleaseLoadGeneration);
                if (string.IsNullOrWhiteSpace(currentCatalogId))
                    pendingTrainerReleaseCatalogId = null;
                TrainerReleases.Clear();
                SelectedTrainerRelease = null!;
                RaiseCommandStates();
            }
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
        public ImageSource? SelectedGameBackground
        {
            get => selectedGameBackground;
            private set => SetValue(ref selectedGameBackground, value);
        }
        public Brush? SelectedGameBackgroundAmbientBrush
        {
            get => selectedGameBackgroundAmbientBrush;
            private set => SetValue(ref selectedGameBackgroundAmbientBrush, value);
        }
        public bool HasSelectedGameBackgroundAmbientMaterial
        {
            get => hasSelectedGameBackgroundAmbientMaterial;
            private set => SetValue(ref hasSelectedGameBackgroundAmbientMaterial, value);
        }

        /// <summary>
        /// Applies a changed visual preference without reloading the artwork when another
        /// appearance setting changes. Turning the preference off also releases the decoded
        /// image and cancels the in-flight provider operation immediately.
        /// </summary>
        public void ApplySelectedGameBackgroundPreference()
        {
            var enabled = plugin.Settings.FollowSelectedGameBackground;
            if (enabled == selectedGameBackgroundPreferenceApplied) return;
            RefreshSelectedGameBackground();
        }

        /// <summary>
        /// Restores a cancelled background load when the embedded dashboard is shown again.
        /// This is intentionally a one-shot safety check; normal dashboard polling must not
        /// restart the same decode while the selected game has not changed.
        /// </summary>
        public void EnsureSelectedGameBackgroundLoaded()
        {
            if (!plugin.Settings.FollowSelectedGameBackground
                || gamePicker.SelectedGame == null
                || SelectedGameBackground != null
                || HasSelectedGameBackgroundAmbientMaterial
                || selectedGameBackgroundCancellation != null)
                return;

            RefreshSelectedGameBackground();
        }
        public BackupVersionDto SelectedBackup
        {
            get => selectedBackup;
            set
            {
                var sameBackup = value != null
                    && string.Equals(selectedBackup?.BackupId, value.BackupId, StringComparison.OrdinalIgnoreCase);
                if (!ReferenceEquals(selectedBackup, value))
                {
                    selectedBackup = value!;
                    OnPropertyChanged(nameof(SelectedBackup));
                }
                SyncBackupEditor(value, sameBackup);
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
        public string BackupComment
        {
            get => backupComment;
            set
            {
                var normalized = value ?? string.Empty;
                if (!applyingEditorSelection && !string.Equals(backupComment, normalized, StringComparison.Ordinal))
                    backupCommentDirty = true;
                SetValue(ref backupComment, normalized);
            }
        }
        public bool LockSelectedBackup
        {
            get => lockSelectedBackup;
            set
            {
                if (!applyingEditorSelection && lockSelectedBackup != value)
                    backupLockDirty = true;
                SetValue(ref lockSelectedBackup, value);
            }
        }
        public MediaItemDto SelectedMedia
        {
            get => selectedMedia;
            set
            {
                var sameMedia = value != null
                    && string.Equals(selectedMedia?.MediaId, value.MediaId, StringComparison.OrdinalIgnoreCase);
                if (!ReferenceEquals(selectedMedia, value))
                {
                    selectedMedia = value!;
                    OnPropertyChanged(nameof(SelectedMedia));
                }
                SyncMediaEditor(value, sameMedia);
                RaiseCommandStates();
            }
        }
        public MediaStorageSummaryDto MediaSummary { get => mediaSummary; private set => SetValue(ref mediaSummary,value??new MediaStorageSummaryDto()); }
        public string MediaComment
        {
            get => mediaComment;
            set
            {
                var normalized = value ?? string.Empty;
                if (!applyingEditorSelection && !string.Equals(mediaComment, normalized, StringComparison.Ordinal))
                    mediaCommentDirty = true;
                SetValue(ref mediaComment, normalized);
            }
        }
        public bool MediaFavorite
        {
            get => mediaFavorite;
            set
            {
                if (!applyingEditorSelection && mediaFavorite != value)
                    mediaFavoriteDirty = true;
                SetValue(ref mediaFavorite, value);
            }
        }

        private void SyncBackupEditor(BackupVersionDto? value, bool preserveDirtyFields)
        {
            var applyComment = !preserveDirtyFields || !backupCommentDirty;
            var applyLock = !preserveDirtyFields || !backupLockDirty;
            applyingEditorSelection = true;
            try
            {
                if (applyComment) BackupComment = value?.Comment ?? string.Empty;
                if (applyLock) LockSelectedBackup = value?.IsLocked ?? false;
            }
            finally
            {
                applyingEditorSelection = false;
            }

            if (applyComment) backupCommentDirty = false;
            if (applyLock) backupLockDirty = false;
        }

        private void SyncMediaEditor(MediaItemDto? value, bool preserveDirtyFields)
        {
            var applyComment = !preserveDirtyFields || !mediaCommentDirty;
            var applyFavorite = !preserveDirtyFields || !mediaFavoriteDirty;
            applyingEditorSelection = true;
            try
            {
                if (applyComment) MediaComment = value?.Comment ?? string.Empty;
                if (applyFavorite) MediaFavorite = value?.IsFavorite ?? false;
            }
            finally
            {
                applyingEditorSelection = false;
            }

            if (applyComment) mediaCommentDirty = false;
            if (applyFavorite) mediaFavoriteDirty = false;
        }
        public string MediaSearchText
        {
            get => mediaSearchText;
            set
            {
                SetValue(ref mediaSearchText,value??string.Empty);
                mediaSearchRefresh.Schedule(value);
                uiStateSave?.Schedule();
            }
        }
        public string MediaFilter
        {
            get => mediaFilter;
            set
            {
                SetValue(ref mediaFilter,string.IsNullOrWhiteSpace(value)?"全部":value);
                MediaView.Refresh();
                uiStateSave?.Schedule();
            }
        }
        public IReadOnlyList<string> MediaInboxModeOptions { get; } = new[] { "待归类", "已忽略" };
        public string MediaInboxMode
        {
            get => mediaInboxMode;
            set
            {
                var normalized = string.Equals(value, "已忽略", StringComparison.Ordinal) ? "已忽略" : "待归类";
                if (string.Equals(mediaInboxMode, normalized, StringComparison.Ordinal)) return;
                SetValue(ref mediaInboxMode, normalized);
                OnPropertyChanged(nameof(MediaInboxTitle));
                OnPropertyChanged(nameof(MediaInboxEmptyText));
                ApplyMediaInboxMode();
                pendingMediaInboxLoadMode = normalized;
                Interlocked.Increment(ref mediaInboxLoadGeneration);
                StartQueuedMediaInboxLoad();
                RaiseCommandStates();
            }
        }
        public string MediaInboxTitle => MediaInboxMode == "已忽略" ? "已忽略媒体" : "待归类媒体";
        public string MediaInboxEmptyText => MediaInboxMode == "已忽略" ? "当前没有已忽略的媒体。" : "当前没有等待归类的媒体。";
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
        public ICommand AssignInboxMediaBatchCommand { get; }
        public ICommand IgnoreInboxMediaBatchCommand { get; }
        public ICommand RestoreIgnoredMediaBatchCommand { get; }
        public ICommand CancelTaskCommand { get; }
        public ICommand RetryTaskCommand { get; }
        public ICommand RetryAllTasksCommand { get; }
        public ICommand CopyTaskErrorCommand { get; }
        public ICommand OpenAttentionCenterCommand { get; }
        public ICommand OpenMaintenanceCommand { get; }
        public ICommand OpenAttentionFindingCommand { get; }
        public ICommand OpenProtectionGamesCommand { get; }
        public ICommand OpenProtectionItemCommand { get; }
        public ICommand ApplyRecommendedProtectionCommand { get; }
        public ICommand RefreshDiagnosticsCommand { get; }
        public ICommand RunIntegrityCheckCommand { get; }
        public ICommand CreateMetadataBackupCommand { get; }
        public ICommand RestoreMetadataBackupCommand { get; }
        public ICommand RebuildRepositoryCommand { get; }
        public ICommand RunPathRemapCommand { get; }
        public ICommand ReconcileTasksCommand { get; }
        public ICommand RefreshStorageAnalysisCommand { get; }
        public ICommand RefreshRetentionSimulationCommand { get; }
        public ICommand ApplyRetentionSimulationCommand { get; }
        public ICommand RefreshLocalMirrorStatusCommand { get; }
        public ICommand SyncLocalMirrorCommand { get; }
        public ICommand CopyMaintenanceReportCommand { get; }
        public ICommand ExportMaintenanceReportCommand { get; }
        public ICommand ExitSafeModeCommand { get; }
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
        public ICommand CreateDiagnosticsPackageCommand { get; }
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

        /// <summary>
        /// Synchronizes the current Playnite runtime flags whenever this page becomes visible.
        /// This intentionally does not use the Worker session snapshot: the Worker may have
        /// started after a game was already running and deliberately treats its first process
        /// scan as a baseline. Page activation can therefore select the live Playnite game
        /// without creating a duplicate session or changing backup automation.
        /// </summary>
        public void SelectCurrentlyRunningGameOnViewActivation()
        {
            if (Games.Count == 0) return;

            var runningIds = plugin.TryGetCurrentlyRunningPlayniteGameIds();
            if (runningIds == null) return;
            var runningIdSet = new HashSet<Guid>(runningIds);
            var runningGames = new List<GameStatusDto>();
            var stateChanged = false;

            foreach (var game in Games)
            {
                var isRunning = Guid.TryParse(game.PlayniteId, out var playniteId)
                    && runningIdSet.Contains(playniteId);
                if (game.IsRunning != isRunning)
                {
                    game.IsRunning = isRunning;
                    stateChanged = true;
                }
                if (isRunning) runningGames.Add(game);
            }

            if (stateChanged)
            {
                Snapshot.RunningGames = runningGames.Count;
                OnPropertyChanged(nameof(Snapshot));
                gamePicker.RefreshGameStates();
                RefreshGameView(false);
                OnPropertyChanged(nameof(SelectedGame));
                RaiseCommandStates();
            }

            // No running game means the user's remembered selection remains authoritative.
            // When one is running, use the same deterministic priority as initial open.
            if (runningGames.Count == 0) return;
            var selected = GameSelectionResolver.ResolveInitial(
                Games,
                plugin.Settings.GamePickerSelectedGameId,
                lastStartedPlayniteId);
            if (selected == null || !selected.IsRunning) return;

            pendingAutoSelectPlayniteId = null;
            initialSelectionApplied = true;
            gamePicker.SelectGame(selected);
        }

        /// <summary>Turns the overview warning count into a route to its concrete reasons.</summary>
        private void OpenMaintenance()
        {
            CurrentWorkspace = WorkspaceKind.Maintenance;
            AttentionCenterRequested?.Invoke(this, EventArgs.Empty);
        }

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
            var first = RecentProtection.AttentionItems.FirstOrDefault();
            if (first == null)
            {
                StatusMessage = "最近保护窗口内没有需要处理的游戏。";
                return;
            }

            OpenProtectionItem(first);
        }

        private async Task ApplyRecommendedProtectionAsync()
        {
            var selectedItems = ProtectionRecommendationPreview.Select(RecentProtection.Items);
            var selected = selectedItems.Select(x => x.PlayniteId).ToList();
            if(selected.Count==0)
            {
                StatusMessage="请先选择需要启用自动保护的游戏。";
                return;
            }

            var confirmed = await plugin.ConfirmAsync(
                "确认启用自动保护",
                ProtectionRecommendationPreview.Build(selectedItems),
                "确认应用",
                "取消").ConfigureAwait(true);
            if (!confirmed)
            {
                StatusMessage = "已取消批量启用自动保护。";
                return;
            }

            await plugin.RequestAsync<object>(MessageTypes.ApplyRecommendedProtection,new ApplyRecommendedProtectionDto{PlayniteIds=selected});
            foreach(var item in RecentProtection.Items.Where(x=>selected.Contains(x.PlayniteId,StringComparer.OrdinalIgnoreCase))) item.IsSelected=false;
            ConfirmSuccess($"已为 {selected.Count} 个游戏启用推荐自动保护策略");
            await RefreshAsync();
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

        /// <summary>Starts the Playnite game-started subscription once for the visible Dashboard.</summary>
        public void StartPlayniteGameStartedSubscription() => playniteGameStartedSubscription.Start();

        /// <summary>Stops the Playnite game-started subscription for an unloaded Dashboard.</summary>
        public void StopPlayniteGameStartedSubscription() => playniteGameStartedSubscription.Stop();

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
            gamePicker.CancelPendingRefresh();
            taskSearchRefresh.Cancel();
            mediaSearchRefresh.Cancel();
            Interlocked.Increment(ref mediaInboxLoadGeneration);
            pendingMediaInboxLoadMode = null;
            CancelDetailsLoad();
            CancelSelectedGameBackgroundLoad();
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
            try { pending.Cancel(); }
            catch (ObjectDisposedException)
            {
                // The retry/synchronization task completed and released the source between
                // the field exchange above and this cancellation call. The unload is already
                // safe; there is no remaining work to cancel.
            }
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
            // A large library remains cache-first for painting, but the descriptor sync itself
            // is safe to start in the background: GameCatalogService persists the changed
            // Playnite rows quickly and moves the expensive Ludusavi matching into a throttled
            // Worker queue. This is important on a second machine, where the local cache may
            // be empty or may not contain a game that Steam/Playnite already knows about.
            var generation = Interlocked.Read(ref deferredUiWorkGeneration);
            _ = RefreshAfterSynchronizationAsync(TimeSpan.Zero, generation);
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
            if (CurrentWorkspace == WorkspaceKind.Media)
            {
                await LoadInboxAsync();
                if (MediaInboxMode == "已忽略") await LoadIgnoredMediaAsync();
            }
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
                var selectedGamePolicyDraft = CaptureSelectedGamePolicyDraft(selectedGameId);
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
                    var displayGames = selectedGamePolicyDraft == null
                        ? data.Games
                        : data.Games.Select(game => string.Equals(game.PlayniteId, selectedGameId, StringComparison.OrdinalIgnoreCase)
                            ? CloneGameWithPolicy(game, selectedGamePolicyDraft)
                            : game).ToList();
                    var gamesChanged = Replace(Games, displayGames, SnapshotComparers.Game);
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
                    SelectCurrentlyRunningGameOnViewActivation();
                    ApplyInitialSelectionIfNeeded();
                    var selectedGameChanged = !string.Equals(
                        selectedGameId,
                        SelectedGame?.PlayniteId,
                        StringComparison.OrdinalIgnoreCase);
                    if (selectedGameChanged)
                    {
                        RefreshSelectedGameIcon();
                        RefreshSelectedGameBackground();
                    }
                    if (selectedGamePolicyDraft == null)
                        UpdateSelectedGamePolicyBaseline(SelectedGame);
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
                Replace(Activities, data.RecentActivities.Take(12), SnapshotComparers.Activity);
                RebuildTaskFilters();
                SelectedTask = Tasks.FirstOrDefault(x => x.TaskId == selectedTaskId) ?? Tasks.FirstOrDefault();
                var previousFindingPlayniteId = SelectedFinding?.PlayniteId;
                var previousFindingCode = SelectedFinding?.Code;
                var previousFindingTitle = SelectedFinding?.Title;
                Replace(Findings, data.Findings, SnapshotComparers.Finding);
                SelectedFinding = Findings.FirstOrDefault(x =>
                        !string.IsNullOrWhiteSpace(previousFindingCode)
                        && string.Equals(x.PlayniteId, previousFindingPlayniteId, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(x.Code, previousFindingCode, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(x.Title, previousFindingTitle, StringComparison.Ordinal));
                // OverviewView gives this projection its own finite viewport. Keep the
                // complete attention set here so additional findings scroll inside the
                // card instead of being silently discarded before the view can render them.
                Replace(AttentionFindings, data.Findings.Where(x => x.Severity >= FindingSeverity.Warning), SnapshotComparers.Finding);
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
            if (settings.SafeModeRequested && !safeModePromptShown)
            {
                safeModePromptShown = true;
                var useSafe = await plugin.ConfirmAsync(
                    "安全模式",
                    "GameSaveCenter 最近连续启动失败。是否使用安全模式打开？",
                    "使用安全模式",
                    "暂不");
                plugin.Settings.SafeModeEnabled = useSafe;
                plugin.Settings.SafeModeRequested = false;
                plugin.SavePluginSettings(plugin.Settings);
                await plugin.RequestAsync<object>(MessageTypes.UpdateSettings, plugin.Settings.ToWorkerSettings());
                await LoadDiagnosticsAsync();
            }
            if (IsOnboardingPending && !environmentCheckLoaded)
                await RunEnvironmentCheckAsync(false);
        }

        private BackupPolicyDto? CaptureSelectedGamePolicyDraft(string? selectedGameId)
        {
            if (string.IsNullOrWhiteSpace(selectedGameId)
                || selectedGamePolicyBaseline == null
                || !string.Equals(selectedGamePolicyId, selectedGameId, StringComparison.OrdinalIgnoreCase))
                return null;

            var selected = gamePicker.SelectedGame;
            if (selected == null || SnapshotComparers.Policy(selected.Policy, selectedGamePolicyBaseline))
                return null;

            return GameSaveCenter.Core.Services.BackupPolicyTemplateCatalog.ClonePolicy(selected.Policy);
        }

        private void UpdateSelectedGamePolicyBaseline(GameStatusDto? game)
        {
            selectedGamePolicyId = game?.PlayniteId;
            selectedGamePolicyBaseline = game == null
                ? null
                : GameSaveCenter.Core.Services.BackupPolicyTemplateCatalog.ClonePolicy(game.Policy);
        }

        private static GameStatusDto CloneGameWithPolicy(GameStatusDto source, BackupPolicyDto policy)
        {
            return new GameStatusDto
            {
                PlayniteId = source.PlayniteId,
                Name = source.Name,
                Platform = source.Platform,
                IsInstalled = source.IsInstalled,
                LastPlayedUtc = source.LastPlayedUtc,
                IsRunning = source.IsRunning,
                LudusaviMatched = source.LudusaviMatched,
                LudusaviName = source.LudusaviName,
                LastBackupUtc = source.LastBackupUtc,
                BackupVersionCount = source.BackupVersionCount,
                LastMediaSyncUtc = source.LastMediaSyncUtc,
                MediaCount = source.MediaCount,
                CloudState = source.CloudState,
                HealthState = source.HealthState,
                HealthSummary = source.HealthSummary,
                HealthReasons = source.HealthReasons == null ? new List<string>() : new List<string>(source.HealthReasons),
                LatestRestoreReadinessStatus = source.LatestRestoreReadinessStatus,
                Policy = GameSaveCenter.Core.Services.BackupPolicyTemplateCatalog.ClonePolicy(policy)
            };
        }

        private async Task ExitSafeModeAsync()
        {
            plugin.Settings.SafeModeEnabled = false;
            plugin.Settings.SafeModeRequested = false;
            plugin.SavePluginSettings(plugin.Settings);
            await plugin.RequestAsync<object>(MessageTypes.UpdateSettings, plugin.Settings.ToWorkerSettings());
            await LoadDiagnosticsAsync();
            StatusMessage = "已恢复正常模式";
        }

        private async Task RunEnvironmentCheckAsync(bool includeFullProbes)
        {
            var report = await plugin.RequestAsync<EnvironmentCheckReportDto>(
                MessageTypes.CheckEnvironment,
                new EnvironmentCheckRequestDto
                {
                    IncludeRemoteProbe = includeFullProbes,
                    IncludeBackupProbe = includeFullProbes
                },
                TimeSpan.FromMinutes(3));
            ApplyOnUi(() =>
            {
                EnvironmentCheck = report;
                environmentCheckLoaded = true;
                StatusMessage = report.Summary;
            });
        }

        private async Task RunIntegrityCheckAsync()
        {
            var result = await plugin.RequestAsync<IntegrityCheckResultDto>(MessageTypes.CheckIntegrity, new { }, TimeSpan.FromMinutes(3));
            ApplyOnUi(() =>
            {
                IntegritySummary = $"完整性自检：{result.StateDisplay}（错误 {result.ErrorCount} / 警告 {result.WarningCount}）\n{result.Summary}";
                StatusMessage = result.Summary;
            });
        }

        private async Task CreateMetadataBackupAsync()
        {
            var result = await plugin.RequestAsync<MetadataBackupResultDto>(
                MessageTypes.CreateMetadataBackup,
                new MetadataBackupCreateRequestDto { PluginSettingsJson = plugin.Settings.ExportPortableJson() },
                TimeSpan.FromMinutes(5));
            ApplyOnUi(() =>
            {
                MetadataBackupSummary = $"元数据灾备包已生成：{result.PackagePath}（{result.PackageBytes / 1024d / 1024d:0.#} MiB）" +
                    (result.PluginSettingsIncluded ? "，已包含 Playnite 插件设置。" : string.Empty);
                StatusMessage = result.Summary;
            });
        }

        private async Task RestoreMetadataBackupAsync()
        {
            var dialog = new OpenFileDialog
            {
                Title = "选择元数据灾备包",
                Filter = "ZIP 灾备包 (*.zip)|*.zip",
                CheckFileExists = true
            };
            if (dialog.ShowDialog() != true) return;

            var preRestorePluginJson = plugin.Settings.ExportPortableJson();
            var preview = await plugin.RequestAsync<MetadataRestorePreviewDto>(
                MessageTypes.PreviewMetadataRestore,
                new MetadataRestoreRequestDto { PackagePath = dialog.FileName },
                TimeSpan.FromMinutes(2));
            ApplyOnUi(() => MetadataRestoreSummary = preview.Summary);
            if (!preview.Valid)
            {
                plugin.ShowInfo(preview.Summary);
                return;
            }

            if (!string.IsNullOrWhiteSpace(preview.PluginSettingsJson))
            {
                try
                {
                    GameSaveCenterSettings.ValidatePortableJson(preview.PluginSettingsJson);
                }
                catch (Exception ex)
                {
                    ApplyOnUi(() => MetadataRestoreSummary = "插件设置校验失败，已取消恢复：" + ex.Message);
                    plugin.ShowInfo(MetadataRestoreSummary);
                    return;
                }
            }

            var confirmed = await plugin.ConfirmAsync(
                "恢复元数据",
                preview.Summary + "\n\n恢复会替换数据库与 Worker/Playnite 设置，并保留恢复前副本。是否继续？",
                "恢复",
                "取消",
                isDangerous: true);
            if (!confirmed) return;

            var result = await plugin.RequestAsync<MetadataRestoreResultDto>(
                MessageTypes.ExecuteMetadataRestore,
                new MetadataRestoreRequestDto { PackagePath = dialog.FileName, Confirmed = true },
                TimeSpan.FromMinutes(10));
            if (!string.IsNullOrWhiteSpace(result.PluginSettingsJson))
            {
                var importReport = await MetadataRestoreCoordinator.ApplyPluginSettingsAsync(
                    plugin.Settings,
                    () => plugin.SavePluginSettings(plugin.Settings),
                    () => plugin.NotifyVisualSettingsChanged(),
                    () => plugin.ApplySettingsAndAwaitAsync(),
                    result.PluginSettingsJson,
                    preRestorePluginJson,
                    () => plugin.RequestAsync<MetadataRestoreRollbackResultDto>(
                        MessageTypes.RollbackMetadataRestore,
                        new MetadataRestoreRollbackRequestDto
                        {
                            PreRestorePath = result.PreRestorePath,
                            Confirmed = true
                        },
                        TimeSpan.FromMinutes(10)));
                ApplyOnUi(() => MetadataRestoreSummary = result.Summary + "\n" + importReport.Summary);
            }
            else
            {
                ApplyOnUi(() => MetadataRestoreSummary = result.Summary);
            }
            StatusMessage = result.Summary;
            await LoadDiagnosticsAsync();
        }

        private async Task RebuildRepositoryAsync()
        {
            var preview = await plugin.RequestAsync<RepositoryRebuildPreviewDto>(
                MessageTypes.PreviewRepositoryRebuild,
                new { },
                TimeSpan.FromMinutes(10));
            ApplyOnUi(() => RepositoryRebuildSummary = preview.Summary);
            var confirmed = await plugin.ConfirmAsync(
                "确认重建备份索引",
                preview.Summary + "\n\n仅重建 SQLite 索引，不会删除或上传归档。是否继续？",
                "重建索引",
                "取消");
            if (!confirmed) return;

            var result = await plugin.RequestAsync<RepositoryRebuildResultDto>(
                MessageTypes.RebuildRepository,
                new RepositoryRebuildRequestDto { Confirmed = true },
                TimeSpan.FromMinutes(10));
            ApplyOnUi(() =>
            {
                RepositoryRebuildSummary = $"备份索引重建：{result.RebuiltGameCount} 个游戏成功，{result.FailedGameCount} 个失败，共索引 {result.IndexedVersionCount} 个版本";
                StatusMessage = result.Summary;
            });
        }

        private async Task RunPathRemapAsync()
        {
            var preview = await plugin.RequestAsync<PathRemapPreviewDto>(
                MessageTypes.PreviewPathRemap,
                new PathRemapRequestDto
                {
                    OldRoot = PathRemapOldRoot,
                    NewRoot = PathRemapNewRoot
                },
                TimeSpan.FromMinutes(2));
            ApplyOnUi(() => PathRemapSummary = preview.Summary);
            var message = $"预览到 {preview.AffectedRowCount} 条路径需要迁移。\n\n此操作会更新数据库与 Worker 设置，但不会移动任何文件。";
            if (preview.MissingTargetCount > 0)
                message += $"\n\n其中 {preview.MissingTargetCount} 条目标路径当前不存在；继续将仍应用迁移，取消则按默认策略跳过本次迁移。";
            var confirmed = await plugin.ConfirmAsync(
                "确认路径迁移",
                message,
                "继续迁移",
                "取消",
                true);
            if (!confirmed) return;
            var result = await plugin.RequestAsync<PathRemapResultDto>(MessageTypes.PathRemap, new PathRemapRequestDto
            {
                OldRoot = PathRemapOldRoot,
                NewRoot = PathRemapNewRoot,
                Confirmed = true,
                ApplyMissingTargets = preview.MissingTargetCount > 0
            }, TimeSpan.FromMinutes(5));
            ApplyOnUi(() =>
            {
                PathRemapSummary = $"路径迁移完成：更新 {result.AffectedRows} 条数据库路径" +
                                   (result.UpdatedSettings.Count > 0 ? "，同步 " + string.Join("、", result.UpdatedSettings) : "");
                StatusMessage = result.Summary;
            });
        }

        private async Task ReconcileTasksAsync()
        {
            var result = await plugin.RequestAsync<TaskReconcileResultDto>(MessageTypes.ReconcileTasks, new { }, TimeSpan.FromMinutes(3));
            ApplyOnUi(() =>
            {
                TaskReconcileSummary = $"任务协调：{result.InterruptedTaskCount} 个中断任务已标记为 WORKER_RESTARTED";
                StatusMessage = result.Summary;
            });
        }

        private async Task RefreshStorageAnalysisAsync()
        {
            var result = await plugin.RequestAsync<StorageAnalysisDto>(MessageTypes.StorageAnalysis, new { }, TimeSpan.FromMinutes(3));
            ApplyOnUi(() =>
            {
                StorageAnalysis = result;
                StatusMessage = result.Summary;
            });
        }

        private async Task RefreshRetentionSimulationAsync()
        {
            var result = await plugin.RequestAsync<RetentionSimulationPreviewDto>(MessageTypes.PreviewRetentionSimulation, new { }, TimeSpan.FromMinutes(3));
            ApplyOnUi(() =>
            {
                RetentionSimulation = result;
                StatusMessage = result.Summary;
                RaiseCommandStates();
            });
        }

        private async Task ApplyRetentionSimulationAsync()
        {
            var preview = RetentionSimulation;
            if (preview.DeleteCandidateCount <= 0)
            {
                StatusMessage = "当前没有可清理的候选版本。";
                return;
            }
            var confirmed = await plugin.ConfirmAsync(
                "应用保留策略清理",
                $"{preview.Summary}\n\n清理会删除候选 ZIP 归档并移除对应 SQLite 索引；用户锁定、PreRestore 与健康恢复点会被自动跳过。是否继续？",
                "清理候选版本",
                "取消",
                isDangerous: true);
            if (!confirmed) return;

            var result = await plugin.RequestAsync<RetentionSimulationResultDto>(
                MessageTypes.ApplyRetentionSimulation,
                new RetentionSimulationApplyRequestDto
                {
                    Confirmed = true,
                    PreviewId = preview.PreviewId,
                    PreviewGeneratedUtc = preview.GeneratedUtc,
                    ExpectedCandidateCount = preview.DeleteCandidateCount,
                    ExpectedReleaseBytes = preview.EstimatedReleaseBytes
                },
                TimeSpan.FromMinutes(10));
            ApplyOnUi(() =>
            {
                StatusMessage = result.Summary;
                plugin.ShowInfo(result.Summary);
            });
            await RefreshRetentionSimulationAsync();
            await RefreshDashboardAsync(false, false);
        }

        private async Task RefreshLocalMirrorStatusAsync()
        {
            var result = await plugin.RequestAsync<LocalMirrorStatusDto>(MessageTypes.MirrorLocalStatus, new { }, TimeSpan.FromMinutes(2));
            ApplyOnUi(() =>
            {
                LocalMirrorStatus = result;
                StatusMessage = result.Message;
                RaiseCommandStates();
            });
        }

        private async Task SyncLocalMirrorAsync()
        {
            var result = await plugin.RequestAsync<LocalMirrorSyncResultDto>(MessageTypes.MirrorLocalSync, new { }, TimeSpan.FromMinutes(30));
            ApplyOnUi(() =>
            {
                StatusMessage = result.Message;
                plugin.ShowInfo(result.Message);
            });
            await RefreshLocalMirrorStatusAsync();
        }

        private async Task CopyMaintenanceReportAsync()
        {
            var report = await plugin.RequestAsync<MaintenanceReportDto>(MessageTypes.GetMaintenanceReport, new { }, TimeSpan.FromMinutes(3));
            await CopyTextWithRetryAsync(report.ReportText, "健康报告已复制", "健康报告已复制到剪贴板。");
        }

        private async Task ExportMaintenanceReportAsync()
        {
            var report = await plugin.RequestAsync<MaintenanceReportDto>(MessageTypes.GetMaintenanceReport, new { }, TimeSpan.FromMinutes(3));
            var dialog = new SaveFileDialog
            {
                Title = "导出健康报告",
                Filter = "文本文件 (*.txt)|*.txt|Markdown (*.md)|*.md",
                FileName = $"GameSaveCenter-Health-{DateTime.Now:yyyyMMdd-HHmm}.txt",
                AddExtension = true,
                DefaultExt = ".txt"
            };
            if (dialog.ShowDialog() != true) return;
            File.WriteAllText(dialog.FileName, report.ReportText);
            StatusMessage = $"健康报告已导出：{dialog.FileName}";
            plugin.ShowInfo(StatusMessage);
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
            var executable = ProcessMappingExecutable;
            var target = ProcessMappingTargetGame ?? throw new InvalidOperationException("请选择映射目标游戏。");
            var saved=await plugin.RequestAsync<ProcessMappingDto>(MessageTypes.SaveProcessMapping,new ProcessMappingDto{ExecutableName=executable,PlayniteId=target.PlayniteId});
            await LoadDiagnosticsAsync();
            if (string.Equals(ProcessMappingExecutable, executable, StringComparison.Ordinal))
                ProcessMappingExecutable=string.Empty;
            StatusMessage=$"已将 {saved.ExecutableName} 绑定到 {saved.GameName}";
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
            await RefreshStorageAnalysisAsync();
            await RefreshRetentionSimulationAsync();
            await RefreshLocalMirrorStatusAsync();
            StatusMessage = "诊断信息已更新";
        }

        private async Task CreateDiagnosticsPackageAsync()
        {
            var result = await plugin.RequestAsync<DiagnosticsPackageResultDto>(
                MessageTypes.CreateDiagnosticsPackage,
                new CreateDiagnosticsPackageRequestDto
                {
                    PluginVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "dev",
                    PlayniteVersion = plugin.PlayniteApi.GetType().Assembly.GetName().Version?.ToString() ?? "unknown",
                    ThemeMode = plugin.Settings.ThemeMode.ToString(),
                    CurrentWorkspace = CurrentWorkspace.ToString(),
                    DpiScale = TryGetDpiScale(),
                    ScreenCount = TryGetScreenCount()
                },
                TimeSpan.FromMinutes(3));
            StatusMessage = result.Summary;
            plugin.ShowInfo($"诊断包已生成：{Path.GetFileName(result.PackagePath)}");
            OpenPath(result.PackagePath);
        }

        private const int MonitorCountMetric = 80;

        [DllImport("user32.dll", EntryPoint = "GetDpiForSystem")]
        private static extern uint GetDpiForSystemNative();

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);

        private static double TryGetDpiScale()
        {
            try
            {
                var dpi = GetDpiForSystemNative();
                return dpi > 0 ? Math.Round(dpi / 96d, 2) : 1;
            }
            catch
            {
                return 1;
            }
        }

        private static int TryGetScreenCount()
        {
            try
            {
                return Math.Max(1, GetSystemMetrics(MonitorCountMetric));
            }
            catch
            {
                return 1;
            }
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
                PlayniteId=selected.PlayniteId,RemoteDevice=selected.RemoteDeviceId,
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
                    PlayniteId=selected.PlayniteId,RemoteDevice=selected.RemoteDevice,
                    RemoteDeviceId=selected.RemoteDeviceId,BackupId=selected.RemoteBackupId
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
                        var selectedBackupId = SelectedBackup?.BackupId;
                        Replace(Backups, backupsTask.Result, SnapshotComparers.Backup);
                        Replace(SaveCandidates, candidatesTask.Result, SnapshotComparers.SaveCandidate);
                        SelectedBackup = Backups.FirstOrDefault(x => string.Equals(x.BackupId, selectedBackupId, StringComparison.OrdinalIgnoreCase))
                                         ?? Backups.FirstOrDefault();
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
                        var selectedMediaId = SelectedMedia?.MediaId;
                        SelectedMedia=Media.FirstOrDefault(x => string.Equals(x.MediaId, selectedMediaId, StringComparison.OrdinalIgnoreCase))
                                      ?? Media.FirstOrDefault();
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
            var game = SelectedGame ?? throw new InvalidOperationException("请先选择游戏。");
            var gameId = game.PlayniteId;
            var gameName = game.Name;
            var tasks = await plugin.RequestAsync<TaskStatusDto[]>(MessageTypes.BackupGame, new BackupRequestDto { PlayniteIds = { gameId }, Force = true, Reason = "Manual" }, TimeSpan.FromMinutes(15));
            NotifyTaskResults(tasks);
            await RefreshCoreAsync(false);
            if (CurrentWorkspace == WorkspaceKind.Saves && IsSelectedGame(gameId))
            {
                await LoadDetailsAsync();
                StatusMessage = Backups.Count > 0
                    ? $"备份完成，已读取 {Backups.Count} 个历史版本"
                    : "备份完成，但历史索引仍为空；请打开诊断页查看 Ludusavi 输出。";
            }
            else
            {
                StatusMessage = $"“{gameName}”的备份已完成，请返回存档中心查看历史版本。";
            }
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

        public void ImportDroppedGameTool(string? path)
        {
            if (SelectedGame == null)
            {
                StatusMessage = "请先选择游戏，再拖入工具文件。";
                return;
            }
            Run(() => ImportDroppedGameToolCoreAsync(path));
        }

        private async Task ImportDroppedGameToolCoreAsync(string? path)
        {
            var source = path ?? string.Empty;
            if (string.IsNullOrWhiteSpace(source))
            {
                StatusMessage = "拖入的文件路径为空。";
                return;
            }
            if (Directory.Exists(source))
            {
                await PrepareGameToolImportAsync(source, GameToolType.Trainer).ConfigureAwait(false);
                return;
            }
            var extension = Path.GetExtension(source).ToLowerInvariant();
            switch (extension)
            {
                case ".ct":
                    await PrepareGameToolImportAsync(source, GameToolType.CheatTable).ConfigureAwait(false);
                    break;
                case ".lnk":
                case ".bat":
                case ".cmd":
                case ".ps1":
                    await ExecuteGameToolImportAsync(source, GameToolType.CustomExecutable, Path.GetFileName(source), false).ConfigureAwait(false);
                    break;
                case ".exe":
                    var asTrainer = await plugin.ConfirmAsync(
                        "拖入的 EXE",
                        "这是一个修改器，还是普通启动项？\n\n修改器会在游戏启动时按策略运行；普通启动项属于自定义工具，需要额外分类。",
                        "修改器",
                        "普通启动项",
                        false).ConfigureAwait(false);
                    if (asTrainer)
                        await PrepareGameToolImportAsync(source, GameToolType.Trainer).ConfigureAwait(false);
                    else
                        await ExecuteGameToolImportAsync(source, GameToolType.CustomExecutable, Path.GetFileName(source), false).ConfigureAwait(false);
                    break;
                case ".zip":
                    await PrepareGameToolImportAsync(source, GameToolType.Trainer).ConfigureAwait(false);
                    break;
                default:
                    StatusMessage = "拖入的文件类型不支持；请使用 EXE、CT、LNK、BAT、CMD、PS1、ZIP 或目录。";
                    break;
            }
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
            ApplyOnUi(() =>
            {
                // RequestAsync completes on an IPC/Worker continuation. ImportEntryCandidates
                // is bound to a WPF CollectionView, so every collection, selection, and status
                // update must be applied through the Playnite dispatcher.
                pendingGameToolImportSource=inspection.SourcePath;
                pendingGameToolImportType=type;
                Replace(ImportEntryCandidates,inspection.Candidates);
                SelectedImportEntryCandidate=ImportEntryCandidates.FirstOrDefault();
                HasPendingGameToolEntrySelection=true;
                StatusMessage=$"检测到 {ImportEntryCandidates.Count} 个可执行文件，请选择主程序";
            });
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
            await LoadDetailsAsync();
            ApplyOnUi(() => SelectedGameTool=GameTools.FirstOrDefault(x=>x.ToolId==imported.ToolId)??GameTools.FirstOrDefault());
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
            ApplyOnUi(() =>
            {
                pendingGameToolImportSource=string.Empty;
                ImportEntryCandidates.Clear();
                SelectedImportEntryCandidate=null!;
                HasPendingGameToolEntrySelection=false;
                StatusMessage="已取消本次导入";
            });
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
                IfAlreadyRunning=tool.IfAlreadyRunning,RiskCategory=tool.RiskCategory,
                AllowUnknownToolWithAntiCheat=tool.AllowUnknownToolWithAntiCheat,
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
            var result=await plugin.RequestAsync<GameToolLaunchResultDto>(MessageTypes.LaunchGameTool,new GameToolCommandRequestDto{ToolId=SelectedGameTool.ToolId});
            ConfirmSuccess(result.Skipped
                ? "已有同一路径实例，已跳过启动"
                : "已启动 "+SelectedGameTool.DisplayName);
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
            ApplyOnUi(() => IsTrainerCatalogLoading = true);
            try
            {
                var result=await plugin.RequestAsync<TrainerCatalogSyncResultDto>(MessageTypes.SyncTrainerCatalog,new{},TimeSpan.FromMinutes(2));
                ConfirmSuccess(result.Message);
                if(!string.IsNullOrWhiteSpace(TrainerSearchText))await SearchTrainerCatalogAsync();
            }
            finally
            {
                ApplyOnUi(() => IsTrainerCatalogLoading = false);
            }
        }

        private async Task SearchTrainerCatalogAsync()
        {
            ApplyOnUi(() => IsTrainerCatalogLoading = true);
            try
            {
                var query=string.IsNullOrWhiteSpace(TrainerSearchText)?SelectedGame?.Name??string.Empty:TrainerSearchText.Trim();
                var results=await plugin.RequestAsync<TrainerCatalogItemDto[]>(MessageTypes.SearchTrainerCatalog,new TrainerCatalogQueryDto{Query=query,Limit=60},TimeSpan.FromMinutes(2));
                ApplyOnUi(() =>
                {
                    Replace(TrainerCatalogResults,results);
                    SelectedTrainerCatalogItem=TrainerCatalogResults.FirstOrDefault();
                    StatusMessage=results.Length==0?"没有找到匹配的 FLiNG 修改器":"找到 "+results.Length+" 个 FLiNG 结果";
                });
                // A search result is only useful when its downloadable releases are immediately visible.
                // Keep the explicit button for retrying a failed release lookup, but load the first result
                // automatically and load again whenever the user selects another catalogue entry in the view.
                if (results.Length > 0)
                {
                    // The selection event may have queued the same request while IsBusy was still true.
                    // This direct load is already part of the search operation, so consume that queue.
                    pendingTrainerReleaseCatalogId = null;
                    await LoadTrainerReleasesAsync();
                }
            }
            finally
            {
                ApplyOnUi(() => IsTrainerCatalogLoading = false);
            }
        }

        private async Task LoadTrainerReleasesAsync()
        {
            var selected = SelectedTrainerCatalogItem;
            if (selected == null || string.IsNullOrWhiteSpace(selected.CatalogId)) return;
            var catalogId = selected.CatalogId;
            var generation = Interlocked.Read(ref trainerReleaseLoadGeneration);
            trainerReleaseLoadCatalogId = catalogId;
            if (string.Equals(pendingTrainerReleaseCatalogId, catalogId, StringComparison.OrdinalIgnoreCase))
                pendingTrainerReleaseCatalogId = null;
            ApplyOnUi(() => IsTrainerReleasesLoading = true);
            try
            {
                var releases=await plugin.RequestAsync<TrainerReleaseDto[]>(MessageTypes.GetTrainerReleases,
                    new TrainerCatalogQueryDto{CatalogId=catalogId},TimeSpan.FromMinutes(2));
                ApplyOnUi(() =>
                {
                    if (generation != Interlocked.Read(ref trainerReleaseLoadGeneration)
                        || !string.Equals(SelectedTrainerCatalogItem?.CatalogId, catalogId, StringComparison.OrdinalIgnoreCase))
                        return;
                    Replace(TrainerReleases,releases);SelectedTrainerRelease=TrainerReleases.FirstOrDefault();
                    StatusMessage=releases.Length==0?"没有可下载版本":"已加载 "+releases.Length+" 个版本";
                });
            }
            finally
            {
                trainerReleaseLoadCatalogId = null;
                ApplyOnUi(() => IsTrainerReleasesLoading = false);
            }
        }

        private void RequestTrainerReleasesLoad(TrainerCatalogItemDto? requested = null)
        {
            if (requested != null
                && !string.IsNullOrWhiteSpace(requested.CatalogId)
                && !string.Equals(SelectedTrainerCatalogItem?.CatalogId, requested.CatalogId, StringComparison.OrdinalIgnoreCase))
                SelectedTrainerCatalogItem = requested;
            var selected = SelectedTrainerCatalogItem;
            if (selected == null || string.IsNullOrWhiteSpace(selected.CatalogId)) return;
            var catalogId = selected.CatalogId;
            if (IsTrainerReleasesLoading
                && string.Equals(trainerReleaseLoadCatalogId, catalogId, StringComparison.OrdinalIgnoreCase))
                return;
            pendingTrainerReleaseCatalogId = catalogId;
            IsTrainerReleasesLoading = true;
            StartQueuedTrainerReleaseLoad();
        }

        private void StartQueuedTrainerReleaseLoad()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(pendingTrainerReleaseCatalogId)) return;
            if (!string.Equals(SelectedTrainerCatalogItem?.CatalogId, pendingTrainerReleaseCatalogId, StringComparison.OrdinalIgnoreCase))
            {
                pendingTrainerReleaseCatalogId = null;
                IsTrainerReleasesLoading = false;
                return;
            }
            trainerReleaseLoadCatalogId = pendingTrainerReleaseCatalogId;
            pendingTrainerReleaseCatalogId = null;
            Run(LoadTrainerReleasesAsync);
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
            // BackupAll creates a durable Worker task and returns immediately. The full-library
            // scan continues after this pipe request completes and resumes from SQLite after a
            // Worker restart; the task center and terminal notification carry the real outcome.
            var submitted = await plugin.RequestAsync<TaskStatusDto[]>(MessageTypes.BackupAll, new BackupRequestDto { Force = true, Reason = "ManualAll" }, TimeSpan.FromSeconds(30));
            await RefreshCoreAsync(false);
            var job = submitted.FirstOrDefault(x => string.Equals(x.TaskType, "BackupAll", StringComparison.OrdinalIgnoreCase));
            ConfirmSuccess(job == null
                ? "整库备份请求已提交；请在任务中心查看状态"
                : $"已建立整库备份任务：{job.StateDisplay}，进度见任务中心");
        }

        private async Task DetectPathsAsync()
        {
            var candidates = await plugin.RequestAsync<SavePathCandidateDto[]>(MessageTypes.DetectSavePaths, new DetectionRequestDto { PlayniteId = SelectedGame.PlayniteId }, TimeSpan.FromMinutes(20));
            ConfirmSuccess(candidates.Length == 0 ? "未发现新的高可信存档路径候选" : $"发现 {candidates.Length} 个高可信存档路径候选");
            await LoadDetailsAsync();
        }

        private async Task ValidateAsync()
        {
            var game = SelectedGame ?? throw new InvalidOperationException("请先选择游戏。");
            var gameId = game.PlayniteId;
            var gameName = game.Name;
            await plugin.RequestAsync<object>(MessageTypes.ValidateGame, new ValidateGameRequestDto { PlayniteId = gameId });
            await RefreshCoreAsync(false);
            ConfirmSuccess($"{gameName} 的存档校验已完成");
        }

        private async Task ValidateRestoreReadinessAsync()
        {
            var game = SelectedGame ?? throw new InvalidOperationException("请先选择游戏。");
            var gameId = game.PlayniteId;
            var gameName = game.Name;
            var selectedId = SelectedBackup?.BackupId ?? throw new InvalidOperationException("请先选择备份版本。");
            var result = await plugin.RequestAsync<RestoreReadinessDto>(MessageTypes.ValidateRestoreReadiness,
                new RestoreReadinessRequestDto { PlayniteId = gameId, BackupId = selectedId },
                TimeSpan.FromMinutes(15));
            if (CurrentWorkspace == WorkspaceKind.Saves && IsSelectedGame(gameId))
                await LoadDetailsAsync(true);
            ConfirmSuccess($"{gameName} / {selectedId}：{result.StatusDisplay}。{result.Summary}");
        }

        private async Task SavePolicyAsync()
        {
            var selected = SelectedGame ?? throw new InvalidOperationException("请先选择游戏。");
            var playniteId = selected.PlayniteId;
            var gameName = selected.Name;
            var policy = GameSaveCenter.Core.Services.BackupPolicyTemplateCatalog.ClonePolicy(selected.Policy);
            await plugin.RequestAsync<object>(MessageTypes.UpdateGamePolicy, new GamePolicyUpdateDto { PlayniteId = playniteId, Policy = policy });
            if (IsSelectedGame(playniteId))
                UpdateSelectedGamePolicyBaseline(SelectedGame);
            ConfirmSuccess($"已保存 {gameName} 的游戏策略");
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
            var game = SelectedGame ?? throw new InvalidOperationException("请先选择游戏。");
            var template = SelectedPolicyTemplate ?? throw new InvalidOperationException("请先选择策略模板。");
            var gameId = game.PlayniteId;
            var gameName = game.Name;
            var templateId = template.TemplateId;
            var templateName = template.Name;
            await plugin.RequestAsync<object>(MessageTypes.ApplyPolicyTemplate,
                new ApplyPolicyTemplateDto { PlayniteId = gameId, TemplateId = templateId });
            await RefreshDashboardAsync(false, false);
            ConfirmSuccess($"已将策略模板“{templateName}”复制到 {gameName}；后续修改模板不会影响该游戏");
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
            var gameId = SelectedGame?.PlayniteId ?? throw new InvalidOperationException("请先选择游戏。");
            var backupId = SelectedBackup?.BackupId ?? throw new InvalidOperationException("请先选择备份版本。");
            var comment = BackupComment;
            var locked = LockSelectedBackup;
            await plugin.RequestAsync<object>(MessageTypes.UpdateBackupMetadata, new BackupMetadataUpdateDto { PlayniteId = gameId, BackupId = backupId, Comment = comment, Locked = locked });
            if (CurrentWorkspace == WorkspaceKind.Saves && IsSelectedGame(gameId))
            {
                if (string.Equals(SelectedBackup?.BackupId, backupId, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(BackupComment, comment, StringComparison.Ordinal)) backupCommentDirty = false;
                    if (LockSelectedBackup == locked) backupLockDirty = false;
                }
                await LoadDetailsAsync();
            }
            ConfirmSuccess("备份备注与锁定状态已保存");
        }

        private async Task CompareBackupAsync()
        {
            var index = Backups.IndexOf(SelectedBackup);
            if (index < 0 || index + 1 >= Backups.Count) { DiffSummary = "没有可比较的上一个版本。"; return; }
            var gameId = SelectedGame?.PlayniteId ?? throw new InvalidOperationException("请先选择游戏。");
            var leftBackupId = Backups[index + 1].BackupId;
            var rightBackupId = SelectedBackup.BackupId;
            var diff = await plugin.RequestAsync<BackupDiffDto>(MessageTypes.CompareBackups, new BackupCompareRequestDto { PlayniteId = gameId, LeftBackupId = leftBackupId, RightBackupId = rightBackupId });
            if (CurrentWorkspace != WorkspaceKind.Saves
                || !IsSelectedGame(gameId)
                || !string.Equals(SelectedBackup?.BackupId, rightBackupId, StringComparison.OrdinalIgnoreCase)) return;
            LastBackupDiff = diff;
            DiffSummary = diff.Summary;
        }

        private async Task PreviewRetentionAsync()
        {
            var gameId = SelectedGame?.PlayniteId ?? throw new InvalidOperationException("请先选择游戏。");
            var preview = await plugin.RequestAsync<RetentionPreviewDto>(MessageTypes.PreviewRetention, new GameQueryDto { PlayniteId = gameId });
            if (CurrentWorkspace != WorkspaceKind.Saves || !IsSelectedGame(gameId)) return;
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
            if (string.Equals(task.TaskType, "BackupAll", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.IsNullOrWhiteSpace(task.GameId)) return false;
            if (string.Equals(task.TaskType, "CloudUpload", StringComparison.OrdinalIgnoreCase)) return true;
            if (task.ErrorCode?.StartsWith("RCLONE_", StringComparison.OrdinalIgnoreCase) == true) return true;
            return string.Equals(task.TaskType, "Backup", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(task.TaskType, "MediaSync", StringComparison.OrdinalIgnoreCase);
        }

        private async Task RetryAllTasksAsync()
        {
            var candidates = Tasks
                .Where(CanRetryTask)
                .GroupBy(GetRetryGroupKey, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.OrderByDescending(x => x.CreatedUtc).First())
                .OrderByDescending(x => x.CreatedUtc)
                .ToList();
            if (candidates.Count == 0)
            {
                StatusMessage = "当前没有可安全重试的任务。";
                return;
            }

            var preview = string.Join("\n", candidates.Take(8).Select(x =>
                string.IsNullOrWhiteSpace(x.GameName) ? x.TaskTypeDisplay : $"{x.GameName} · {x.TaskTypeDisplay}"));
            if (candidates.Count > 8) preview += $"\n……以及另外 {candidates.Count - 8} 项";
            if (!await plugin.ConfirmAsync(
                    "批量安全重试",
                    $"将按游戏和任务类型各重试一次，共 {candidates.Count} 项。\n\n{preview}",
                    "全部重试",
                    "取消"))
            {
                StatusMessage = "已取消批量重试。";
                return;
            }

            await plugin.EnsureWorkerAsync();
            var succeeded = 0;
            var failed = 0;
            var failures = new List<string>();
            foreach (var candidate in candidates)
            {
                try
                {
                    var results = await RetryTaskCoreAsync(candidate);
                    foreach (var result in results) plugin.ShowTaskNotification(result);
                    var failedResult = results.FirstOrDefault(x => x.State == TaskState.Failed || x.State == TaskState.Cancelled);
                    if (failedResult == null && results.Count > 0)
                    {
                        succeeded++;
                        continue;
                    }

                    failed++;
                    var detail = failedResult?.ErrorMessage;
                    if (string.IsNullOrWhiteSpace(detail)) detail = failedResult?.DetailMessage;
                    failures.Add($"{candidate.GameName} · {candidate.TaskTypeDisplay}：{(string.IsNullOrWhiteSpace(detail) ? "未完成" : detail)}");
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    failed++;
                    failures.Add($"{candidate.GameName} · {candidate.TaskTypeDisplay}：{ex.Message}");
                }
            }

            await RefreshCoreAsync(false);
            StatusMessage = $"批量重试完成：成功 {succeeded} 项，失败 {failed} 项。";
            if (failed > 0)
            {
                var firstFailure = failures.FirstOrDefault();
                plugin.ShowError(StatusMessage + (string.IsNullOrWhiteSpace(firstFailure) ? string.Empty : $"\n{firstFailure}"));
            }
            else
            {
                plugin.ShowInfo(StatusMessage);
            }
        }

        private static string GetRetryGroupKey(TaskStatusDto task)
        {
            if (string.Equals(task.TaskType, "BackupAll", StringComparison.OrdinalIgnoreCase)) return "BackupAll";
            if (string.Equals(task.TaskType, "MediaInbox", StringComparison.OrdinalIgnoreCase)) return "MediaInbox";
            return $"{task.TaskType}:{task.GameId}";
        }

        private async Task RetrySelectedTaskAsync()
        {
            var task = SelectedTask ?? throw new InvalidOperationException("请先选择失败或已取消的任务。");
            await plugin.EnsureWorkerAsync();
            var results = await RetryTaskCoreAsync(task);
            NotifyTaskResults(results);
            await RefreshCoreAsync(false);
            StatusMessage = "重试任务已完成";
        }

        private async Task<List<TaskStatusDto>> RetryTaskCoreAsync(TaskStatusDto task)
        {
            if (string.Equals(task.TaskType, "CloudUpload", StringComparison.OrdinalIgnoreCase)
                || task.ErrorCode?.StartsWith("RCLONE_", StringComparison.OrdinalIgnoreCase) == true)
            {
                var result=await plugin.RequestAsync<TaskStatusDto>(
                    MessageTypes.RetryCloudUpload,
                    new GameQueryDto{PlayniteId=task.GameId},
                    TimeSpan.FromHours(2));
                return result == null ? new List<TaskStatusDto>() : new List<TaskStatusDto> { result };
            }
            else if (string.Equals(task.TaskType, "Backup", StringComparison.OrdinalIgnoreCase))
            {
                var result = await plugin.RequestAsync<TaskStatusDto[]>(
                    MessageTypes.BackupGame,
                    new BackupRequestDto { PlayniteIds = { task.GameId }, Force = true, Reason = "Retry" },
                    TimeSpan.FromMinutes(15));
                return result?.ToList() ?? new List<TaskStatusDto>();
            }
            else if (string.Equals(task.TaskType, "BackupAll", StringComparison.OrdinalIgnoreCase))
            {
                var result = await plugin.RequestAsync<TaskStatusDto[]>(
                    MessageTypes.BackupAll,
                    new BackupRequestDto { Force = true, Reason = "RetryAll" },
                    TimeSpan.FromSeconds(30));
                return result?.ToList() ?? new List<TaskStatusDto>();
            }
            else if (string.Equals(task.TaskType, "MediaSync", StringComparison.OrdinalIgnoreCase))
            {
                var request = new MediaSyncRequestDto { UploadAfterSync = plugin.Settings.EnableCloudUpload };
                request.PlayniteIds.Add(task.GameId);
                var result = await plugin.RequestAsync<TaskStatusDto[]>(MessageTypes.SyncMedia, request, TimeSpan.FromMinutes(60));
                return result?.ToList() ?? new List<TaskStatusDto>();
            }
            else if (string.Equals(task.TaskType, "MediaInbox", StringComparison.OrdinalIgnoreCase))
            {
                var result = await plugin.RequestAsync<TaskStatusDto[]>(MessageTypes.SyncMedia, new MediaSyncRequestDto
                {
                    IncludeUnassignedInbox = true,
                    SharedOnly = true,
                    UploadAfterSync = plugin.Settings.EnableCloudUpload
                }, TimeSpan.FromMinutes(60));
                return result?.ToList() ?? new List<TaskStatusDto>();
            }
            throw new NotSupportedException("该任务类型暂不支持安全重试。");
        }

        private async Task CopySelectedTaskErrorAsync()
        {
            if (SelectedTask == null) return;
            var text = $"{SelectedTask.GameName} · {SelectedTask.TaskType}\r\n{SelectedTask.DetailMessage}\r\n任务 ID：{SelectedTask.TaskId}";
            await CopyTextWithRetryAsync(text, "任务详情已复制", "任务详情已复制到剪贴板。");
        }

        private async Task CopyTextWithRetryAsync(string text, string statusMessage, string infoMessage)
        {
            for (var attempt = 0; attempt < 4; attempt++)
            {
                try
                {
                    Clipboard.SetText(text);
                    StatusMessage = statusMessage;
                    plugin.ShowInfo(infoMessage);
                    return;
                }
                catch (COMException) when (attempt < 3)
                {
                    // CLIPBRD_E_CANT_OPEN / COM exceptions mean another process owns the
                    // clipboard at this instant. A short asynchronous retry usually succeeds
                    // without blocking the Playnite dispatcher between attempts.
                    await Task.Delay(150 + attempt * 100).ConfigureAwait(true);
                }
                catch (Exception)
                {
                    break;
                }
            }
            StatusMessage = "复制失败：剪贴板暂时被其他程序占用，请稍后重试";
            plugin.ShowError("无法复制到剪贴板：剪贴板暂时被其他程序占用。请稍后重试。");
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

        private async Task CopyDiagnosticsAsync()
        {
            await CopyTextWithRetryAsync(DiagnosticSummary ?? string.Empty, "诊断信息已复制到剪贴板", "GameSaveCenter 诊断信息已复制");
        }

        private string BuildDiagnosticSummary(WorkerSettingsSnapshotDto settings)
        {
            var builder = new StringBuilder();
            builder.AppendLine("GameSaveCenter 诊断摘要");
            builder.AppendLine("生成时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss zzz"));
            builder.AppendLine("插件版本：" + (typeof(DashboardViewModel).Assembly.GetName().Version?.ToString() ?? "dev"));
            builder.AppendLine("Worker：" + (Snapshot.WorkerHealthy ? "正常" : "不可用") + " / " + Snapshot.WorkerVersion);
            builder.AppendLine("安全模式：" + (settings.SafeModeEnabled || Snapshot.SafeModeEnabled ? "已开启（自动操作已暂停）" : "未开启"));
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
                if (trainerReleaseLoadCatalogId != null && IsTrainerReleasesLoading)
                {
                    trainerReleaseLoadCatalogId = null;
                    IsTrainerReleasesLoading = false;
                }
                IsBusy = false;
                StartQueuedTrainerReleaseLoad();
                StartQueuedMediaInboxLoad();
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
            if (string.Equals(e.PropertyName, nameof(GamePickerViewModel.SelectedGame), StringComparison.Ordinal))
            {
                OnPropertyChanged(nameof(SelectedGame));
                return;
            }
            // SelectedItem raises both SelectedItem and SelectedGame notifications. Respond once
            // to the source notification so a rapid keyboard/mouse selection does not enqueue
            // duplicate IPC requests for the same game.
            if (!string.Equals(e.PropertyName, nameof(GamePickerViewModel.SelectedItem), StringComparison.Ordinal)) return;
            var selected = gamePicker.SelectedGame;
            UpdateSelectedGamePolicyBaseline(selected);
            OnPropertyChanged(nameof(SelectedGame));
            if (!suppressSelectionLoad)
            {
                RefreshSelectedGameIcon();
                RefreshSelectedGameBackground();
            }
            RaiseCommandStates();
            if (suppressSelectionLoad) return;
            ClearSelectedGameDetails();
            CancelDetailsLoad();
            if (selected != null && IsGameScopedWorkspace(CurrentWorkspace))
                Observe(LoadSelectionDetailsAsync(selected.PlayniteId));
        }

        private void OnPlayniteGameStarted(Guid playniteId)
        {
            if (!playniteGameStartedSubscription.IsSubscribed) return;
            ApplyOnUi(() =>
            {
                if (!playniteGameStartedSubscription.IsSubscribed) return;
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

        private void RefreshSelectedGameBackground()
        {
            selectedGameBackgroundPreferenceApplied = plugin.Settings.FollowSelectedGameBackground;
            if (!selectedGameBackgroundPreferenceApplied)
            {
                CancelSelectedGameBackgroundLoad();
                SelectedGameBackground = null;
                SelectedGameBackgroundAmbientBrush = null;
                HasSelectedGameBackgroundAmbientMaterial = false;
                return;
            }

            var generation = Interlocked.Increment(ref selectedGameBackgroundGeneration);
            var next = new CancellationTokenSource();
            var previous = Interlocked.Exchange(ref selectedGameBackgroundCancellation, next);
            previous?.Cancel();
            previous?.Dispose();
            SelectedGameBackground = null;
            SelectedGameBackgroundAmbientBrush = null;
            HasSelectedGameBackgroundAmbientMaterial = false;

            var selected = gamePicker.SelectedGame;
            if (selected == null || !Guid.TryParse(selected.PlayniteId, out var playniteId))
            {
                next.Dispose();
                Interlocked.CompareExchange(ref selectedGameBackgroundCancellation, null, next);
                return;
            }

            _ = LoadSelectedGameBackgroundAsync(playniteId, generation, next);
        }

        private async Task LoadSelectedGameBackgroundAsync(Guid playniteId, long generation, CancellationTokenSource cancellation)
        {
            try
            {
                var visual = await gameBackgroundProvider.LoadVisualAsync(playniteId, cancellation.Token).ConfigureAwait(false);
                if (cancellation.IsCancellationRequested || generation != Interlocked.Read(ref selectedGameBackgroundGeneration)) return;
                ApplyOnUi(() =>
                {
                    if (!cancellation.IsCancellationRequested && generation == Interlocked.Read(ref selectedGameBackgroundGeneration))
                    {
                        SelectedGameBackground = visual?.Image;
                        SelectedGameBackgroundAmbientBrush = visual?.AmbientBrush;
                        HasSelectedGameBackgroundAmbientMaterial = visual != null;
                    }
                });
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Could not load the selected Playnite game background; keeping the theme material fallback.");
            }
            finally
            {
                if (ReferenceEquals(Interlocked.CompareExchange(ref selectedGameBackgroundCancellation, null, cancellation), cancellation))
                    cancellation.Dispose();
            }
        }

        private void CancelSelectedGameBackgroundLoad()
        {
            Interlocked.Increment(ref selectedGameBackgroundGeneration);
            var cancellation = Interlocked.Exchange(ref selectedGameBackgroundCancellation, null);
            if (cancellation == null) return;
            try { cancellation.Cancel(); }
            finally { cancellation.Dispose(); }
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

        private void SaveUiStateSettings()
        {
            try
            {
                plugin.Settings.TaskStatusFilterState = TaskStatusFilter;
                plugin.Settings.TaskGameFilterState = TaskGameFilter;
                plugin.Settings.TaskTypeFilterState = TaskTypeFilter;
                plugin.Settings.TaskSearchTextState = TaskSearchText;
                plugin.Settings.MediaFilterState = MediaFilter;
                plugin.Settings.MediaSearchTextState = MediaSearchText;
                plugin.SavePluginSettings(plugin.Settings);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GameSaveCenter could not save UI state settings.");
            }
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
                AssignInboxMediaCommand, IgnoreInboxMediaCommand, AssignInboxMediaBatchCommand, IgnoreInboxMediaBatchCommand, RestoreIgnoredMediaBatchCommand,
                CancelTaskCommand, RetryTaskCommand, RetryAllTasksCommand, CopyTaskErrorCommand, RefreshDiagnosticsCommand, SyncDeviceStatesCommand, SaveDeviceDecisionCommand, ExitSafeModeCommand,
                StageRemoteBackupCommand,RestoreStagedRemoteBackupCommand,CopyDiagnosticsCommand,CreateDiagnosticsPackageCommand,RunIntegrityCheckCommand,CreateMetadataBackupCommand,RestoreMetadataBackupCommand,RebuildRepositoryCommand,RunPathRemapCommand,ReconcileTasksCommand,RefreshStorageAnalysisCommand,RefreshRetentionSimulationCommand,ApplyRetentionSimulationCommand,RefreshLocalMirrorStatusCommand,SyncLocalMirrorCommand,CopyMaintenanceReportCommand,ExportMaintenanceReportCommand,
                SaveProcessMappingCommand,DeleteProcessMappingCommand,RunEnvironmentCheckCommand,SkipOnboardingCommand,CompleteOnboardingCommand,OnboardingTestBackupCommand,
                OpenDataDirectoryCommand, OpenBackupDirectoryCommand, OpenMediaDirectoryCommand, OpenWorkerLogCommand
                ,ImportTrainerCommand,ImportCheatTableCommand,ImportCustomLaunchItemCommand,ImportToolFolderCommand,SaveGameToolCommand,LaunchGameToolCommand,
                ConfirmGameToolImportCommand,CancelGameToolImportCommand,
                OpenGameToolDirectoryCommand,DeleteGameToolCommand,RelocateGameToolCommand,SyncTrainerCatalogCommand,SearchTrainerCatalogCommand,ApplyRecommendedProtectionCommand,
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

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed class GameToolRunningOption
    {
        public GameToolIfAlreadyRunning Value { get; }
        public string Display { get; }
        public GameToolRunningOption(GameToolIfAlreadyRunning value, string display) { Value=value; Display=display; }
    }
    public sealed class GameToolRiskOption
    {
        public GameToolRiskCategory Value { get; }
        public string Display { get; }
        public GameToolRiskOption(GameToolRiskCategory value, string display) { Value=value; Display=display; }
    }
    public sealed class BackupAnomalyProtectionOption
    {
        public BackupAnomalyProtectionLevel Value { get; }
        public string Display { get; }
        public BackupAnomalyProtectionOption(BackupAnomalyProtectionLevel value, string display) { Value=value; Display=display; }
    }
}
