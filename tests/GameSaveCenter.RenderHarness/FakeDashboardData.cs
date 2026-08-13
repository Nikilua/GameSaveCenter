using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.RenderHarness;

/// <summary>
/// Minimal view-model-shaped data for offscreen layout QA. It deliberately mirrors the
/// public binding surface of DashboardViewModel without starting Worker/IPC services.
/// </summary>
public sealed class FakeDashboardData
{
    public string OnboardingTitle => "首次使用：准备环境";
    public string OnboardingDescription => "先确认 Worker、目录、SQLite 与备份工具可用。所有检查都是非破坏性的；你可以跳过，之后随时在维护中心重新运行。";

    public FakeDashboardData(int rowCount = 8)
    {
        rowCount = Math.Max(8, rowCount);
        Snapshot = new DashboardSnapshotDto
        {
            WorkerHealthy = true,
            WorkerVersion = "0.6.70",
            LudusaviAvailable = true,
            LudusaviVersion = "0.31.0",
            RcloneAvailable = true,
            ManagedGames = 300,
            MatchedGames = 256,
            RunningGames = 1,
            WarningGames = 3,
            PendingCloudTasks = 2,
            UnassignedMediaCount = 7
        };

        EnvironmentCheck = new EnvironmentCheckReportDto
        {
            CheckedUtc = DateTime.UtcNow,
            PassedCount = 8,
            SkippedCount = 1,
            Summary = "检查完成：环境已准备好，可以手动执行一次测试备份。"
        };
        EnvironmentCheck.Items.Add(new EnvironmentCheckItemDto
        {
            Key = "worker", Title = "Worker 服务", State = EnvironmentCheckState.Passed,
            Summary = "IPC 请求已成功到达 Worker。", Detail = "0.6.70.0"
        });
        EnvironmentCheck.Items.Add(new EnvironmentCheckItemDto
        {
            Key = "data", Title = "数据目录", State = EnvironmentCheckState.Passed,
            Summary = "目录可创建、写入和删除临时探针。", Detail = @"C:\GameSaveCenterData"
        });
        EnvironmentCheck.Items.Add(new EnvironmentCheckItemDto
        {
            Key = "backup", Title = "存档目录", State = EnvironmentCheckState.Passed,
            Summary = "目录可创建、写入和删除临时探针。", Detail = @"D:\GameSaveCenterData\Saves"
        });
        EnvironmentCheck.Items.Add(new EnvironmentCheckItemDto
        {
            Key = "media", Title = "媒体目录", State = EnvironmentCheckState.Passed,
            Summary = "目录可创建、写入和删除临时探针。", Detail = @"D:\GameSaveCenterData\Media"
        });
        EnvironmentCheck.Items.Add(new EnvironmentCheckItemDto
        {
            Key = "database", Title = "SQLite 数据库", State = EnvironmentCheckState.Passed,
            Summary = "数据库可读取和写入临时探针。", Detail = @"C:\GameSaveCenterData\gamesavecenter.db"
        });
        EnvironmentCheck.Items.Add(new EnvironmentCheckItemDto
        {
            Key = "library", Title = "Playnite 游戏库", State = EnvironmentCheckState.Passed,
            Summary = "已读取 300 个游戏。", Detail = string.Empty
        });
        EnvironmentCheck.Items.Add(new EnvironmentCheckItemDto
        {
            Key = "ludusavi", Title = "Ludusavi", State = EnvironmentCheckState.Passed,
            Summary = "版本检查和只读备份列表调用均成功。", Detail = @"D:\Tools\ludusavi.exe"
        });
        EnvironmentCheck.Items.Add(new EnvironmentCheckItemDto
        {
            Key = "rclone", Title = "Rclone 与云端", State = EnvironmentCheckState.Skipped,
            Summary = "未配置可选的 Rclone 远端。", Detail = "可在设置中配置，之后重新运行检查。", IsOptional = true
        });
        EnvironmentCheck.Items.Add(new EnvironmentCheckItemDto
        {
            Key = "disk", Title = "磁盘空间", State = EnvironmentCheckState.Passed,
            Summary = "可用空间约 128 GiB。", Detail = "C:"
        });

        SelectedGame = new GameStatusDto
        {
            PlayniteId = "game-1",
            Name = "Baldur's Gate 3",
            Platform = GamePlatformKind.Steam,
            IsInstalled = true,
            LudusaviMatched = true,
            BackupVersionCount = 12,
            MediaCount = 32,
            LastBackupUtc = DateTime.UtcNow.AddHours(-2),
            CloudState = "Uploaded",
            HealthState = "Ready",
            LudusaviName = "BG3"
        };

        for (var i = 1; i <= rowCount; i++)
        {
            Games.Add(new GameStatusDto
            {
                PlayniteId = "game-" + i,
                Name = i == 1 ? SelectedGame.Name : $"演示游戏 {i}",
                Platform = GamePlatformKind.Steam,
                IsInstalled = true,
                LudusaviMatched = i % 2 == 0,
                BackupVersionCount = i * 2,
                MediaCount = i * 3,
                CloudState = i % 3 == 0 ? "Pending" : "Uploaded",
                HealthState = i % 5 == 0 ? "Warning" : "Ready"
            });
        }

        for (var i = 1; i <= rowCount; i++)
        {
            var state = i % 4 == 0 ? TaskState.Failed
                : i % 4 == 1 ? TaskState.Running
                : i % 4 == 2 ? TaskState.Succeeded
                : TaskState.Cancelled;
            Tasks.Add(new TaskStatusDto
            {
                TaskId = "T-" + i.ToString("D4"),
                TaskType = i % 3 == 0 ? "MediaSync" : "Backup",
                GameId = "game-" + i,
                GameName = Games[i - 1].Name,
                State = state,
                ProgressPercent = i % 4 == 0 ? 0 : Math.Min(100, i * 12),
                Message = i % 4 == 0 ? "远端暂时不可用，本地版本已保留。" : $"任务 {i} 完成",
                ErrorCode = i % 4 == 0 ? "E_CLOUD" : string.Empty,
                ErrorMessage = i % 4 == 0 ? "Rclone remote unavailable; local source retained." : string.Empty,
                CreatedUtc = DateTime.UtcNow.AddMinutes(-30 * i),
                StartedUtc = DateTime.UtcNow.AddMinutes(-30 * i + 1),
                FinishedUtc = i % 4 == 1 ? null : DateTime.UtcNow.AddMinutes(-30 * i + 25)
            });
        }

        foreach (var task in Tasks)
        {
            OverviewTasks.Add(task);
            if (task.State == TaskState.Running) RunningTaskCount++;
            if (task.State == TaskState.Failed) RetryableTaskCount++;
            if (task.State == TaskState.Succeeded) CompletedTaskCount++;
        }

        for (var i = 1; i <= 6; i++)
        {
            AttentionFindings.Add(new ValidationFindingDto
            {
                PlayniteId = "game-" + i,
                GameName = Games[i - 1].Name,
                Severity = i % 3 == 0 ? FindingSeverity.Error : FindingSeverity.Warning,
                Code = "C" + i,
                Title = i % 3 == 0 ? "云端等待重试" : "存档路径未确认",
                Detail = i % 3 == 0 ? "Rclone 远端暂时不可用，本地版本已保留。" : "发现一个评分较高的候选目录，建议进入存档中心确认规则。",
                SuggestedAction = i % 3 == 0 ? "在任务中心查看详情并重试。" : "进入存档中心确认路径规则。"
            });
        }

        for (var i = 1; i <= 6; i++)
        {
            Media.Add(new MediaItemDto
            {
                MediaId = "M-" + i,
                PlayniteId = SelectedGame.PlayniteId,
                Kind = i % 3 == 0 ? MediaKind.VideoClip : MediaKind.Screenshot,
                Source = i % 3 == 0 ? MediaSourceKind.XboxGameBar : MediaSourceKind.Steam,
                ArchivePath = $@"D:\Media\{SelectedGame.Name}\capture-{i}.{(i % 3 == 0 ? "mp4" : "png")}",
                OriginalPath = $@"D:\Captures\capture-{i}.{(i % 3 == 0 ? "mp4" : "png")}",
                CapturedUtc = DateTime.UtcNow.AddDays(-i),
                SizeBytes = 6_500_000L + i * 1_100_000L,
                IsFavorite = i % 2 == 0,
                Comment = i % 2 == 0 ? "营地夜景" : string.Empty,
                CloudState = i % 4 == 0 ? "Failed" : "Uploaded",
                ClassificationState = "Assigned",
                ClassificationReason = "与当前游戏匹配"
            });
            UnassignedMedia.Add(new MediaItemDto
            {
                MediaId = "IN-" + i,
                Kind = i % 3 == 0 ? MediaKind.VideoClip : MediaKind.Screenshot,
                Source = i % 3 == 0 ? MediaSourceKind.XboxGameBar : MediaSourceKind.WindowsScreenshot,
                ArchivePath = $@"D:\Media\Inbox\shared-{i}.{(i % 3 == 0 ? "mp4" : "png")}",
                OriginalPath = $@"D:\Captures\shared-{i}.{(i % 3 == 0 ? "mp4" : "png")}",
                CapturedUtc = DateTime.UtcNow.AddHours(-i * 3),
                SizeBytes = 4_200_000L + i * 900_000L,
                ClassificationState = "Inbox",
                ClassificationReason = "无法唯一判断所属游戏"
            });
        }

        MediaSources.Add(new MediaSourceRuleDto
        {
            SourceId = "steam",
            SourceKind = MediaSourceKind.Steam,
            RootPath = @"%PROGRAMFILES(X86)%\Steam\userdata\*\760\remote\*\screenshots",
            Enabled = true,
            SharedDirectory = true
        });
        MediaSources.Add(new MediaSourceRuleDto
        {
            SourceId = "xbox",
            SourceKind = MediaSourceKind.XboxGameBar,
            RootPath = @"%USERPROFILE%\Videos\Captures",
            Enabled = true,
            SharedDirectory = true
        });
        MediaSources.Add(new MediaSourceRuleDto
        {
            SourceId = "custom",
            SourceKind = MediaSourceKind.Custom,
            RootPath = @"D:\Pictures\Games\{GameName}",
            IncludePattern = "*",
            Enabled = true,
            SharedDirectory = true
        });

        for (var i = 1; i <= rowCount; i++)
        {
            Findings.Add(new ValidationFindingDto
            {
                PlayniteId = "game-" + i,
                GameName = Games[i - 1].Name,
                Severity = i % 3 == 0 ? FindingSeverity.Error : FindingSeverity.Warning,
                Code = "F" + i,
                Title = i % 3 == 0 ? "云端上传失败" : "存档路径未确认",
                Detail = i % 3 == 0 ? "远端暂时不可用，本地版本已保留；不会撤销本地备份。" : "发现候选目录，建议人工确认规则。",
                SuggestedAction = i % 3 == 0 ? "在任务中心诊断并重试。" : "进入存档中心确认候选路径。"
            });
            Audit.Add(new AuditLogEntryDto
            {
                Category = i % 3 == 0 ? "Cloud" : "Backup",
                Message = i % 3 == 0 ? "Rclone remote unavailable; local source retained." : $"Backup version {i} created and validated.",
                CreatedUtc = DateTime.UtcNow.AddMinutes(-i * 9)
            });
            DeviceComparisons.Add(new DeviceConflictStatusDto
            {
                PlayniteId = "game-" + i,
                GameName = Games[i - 1].Name,
                RemoteDevice = "LAPTOP-02",
                LocalBackupId = "local-" + i,
                RemoteBackupId = "remote-" + i,
                HasConflict = i % 2 == 0,
                Reason = "DifferentDevicesChangedWithinTenMinutes",
                SuggestedBackupId = "remote-" + i,
                Confidence = 0.82,
                LocalCreatedUtc = DateTime.UtcNow.AddDays(-i),
                RemoteCreatedUtc = DateTime.UtcNow.AddDays(-i).AddMinutes(3),
                Decision = i % 3 == 0 ? "Defer" : string.Empty
            });
            ProcessMappings.Add(new ProcessMappingDto
            {
                ExecutableName = i == 1 ? "skse64.exe" : $"modlauncher-{i}.exe",
                PlayniteId = "game-" + i,
                GameName = Games[i - 1].Name,
                Enabled = true,
                CreatedUtc = DateTime.UtcNow.AddDays(-i)
            });
        }

        for (var i = 1; i <= 12; i++)
        {
            LastRetentionPreview.KeepBackupIds.Add("keep-" + i);
            if (i % 4 == 0) LastRetentionPreview.DeleteCandidateIds.Add("candidate-" + i);
        }

        for (var i = 1; i <= rowCount; i++)
        {
            Backups.Add(new BackupVersionDto
            {
                BackupId = "B-" + i.ToString("D4"),
                PlayniteId = SelectedGame.PlayniteId,
                LudusaviName = SelectedGame.Name,
                CreatedUtc = DateTime.UtcNow.AddDays(-i),
                TotalBytes = 24_800_000L + i * 1_000_000L,
                FileCount = 120 + i,
                IsLocked = i % 4 == 0,
                Comment = i % 3 == 0 ? "周末手动备份" : string.Empty,
                SourceDevice = i % 2 == 0 ? "LAPTOP-02" : "DESKTOP-01",
                OperatingSystem = "Windows 11",
                IsPreRestore = i == 1
            });
            SaveCandidates.Add(new SavePathCandidateDto
            {
                PlayniteId = SelectedGame.PlayniteId,
                Path = $@"D:\Games\{SelectedGame.Name}\Save\{i}\Slot{i}",
                Score = 0.82 + i * 0.01,
                Reasons = new System.Collections.Generic.List<string> { "包含存档扩展名", "最近写入时间匹配" },
                Status = "Pending"
            });
            GameTools.Add(new GameToolDto
            {
                ToolId = "T-" + i,
                PlayniteId = SelectedGame.PlayniteId,
                ToolType = i % 2 == 0 ? GameToolType.CheatTable : GameToolType.Trainer,
                SourceType = i % 2 == 0 ? GameToolSourceType.Manual : GameToolSourceType.Fling,
                DisplayName = i == 1 ? "风灵月影修改器" : $"演示工具 {i}",
                Enabled = true,
                AutoStart = i % 2 == 0,
                LaunchDelaySeconds = 8,
                CloseOnGameExit = i % 3 == 0,
                RequiresAdmin = i % 4 == 0,
                ActiveVersionId = "v1",
                CreatedUtc = DateTime.UtcNow.AddDays(-i),
                UpdatedUtc = DateTime.UtcNow.AddDays(-i)
            });
            var tool = GameTools[i - 1];
            tool.Versions.Add(new GameToolVersionDto
            {
                VersionId = "v1",
                ToolId = tool.ToolId,
                VersionName = "v1.0",
                EntryPath = $@"D:\Tools\{tool.DisplayName}\trainer.exe",
                WorkingDirectory = $@"D:\Tools\{tool.DisplayName}",
                IsAvailable = true,
                CreatedUtc = DateTime.UtcNow.AddDays(-i)
            });
            if (i % 2 == 0)
            {
                tool.Versions.Add(new GameToolVersionDto
                {
                    VersionId = "v2",
                    ToolId = tool.ToolId,
                    VersionName = "v1.1",
                    EntryPath = $@"D:\Tools\{tool.DisplayName}\trainer_v11.exe",
                    IsAvailable = true,
                    CreatedUtc = DateTime.UtcNow.AddDays(-i + 1)
                });
            }
            TrainerCatalogResults.Add(new TrainerCatalogItemDto
            {
                CatalogId = "C-" + i,
                Title = i == 1 ? "Baldur's Gate 3 Trainer" : $"演示 Trainer {i}",
                NormalizedTitle = i == 1 ? "baldurs gate 3 trainer" : $"demo trainer {i}",
                PageUrl = "https://example.com/trainer",
                GameVersion = "1.0",
                OptionCount = 12 + i,
                LastUpdatedUtc = DateTime.UtcNow.AddDays(-i),
                LastSyncedUtc = DateTime.UtcNow.AddDays(-i)
            });
            TrainerReleases.Add(new TrainerReleaseDto
            {
                ReleaseId = "R-" + i,
                CatalogId = "C-" + i,
                DisplayName = i == 1 ? "Baldur's Gate 3 v1.0 Plus 20 Trainer" : $"Trainer v1.{i} Plus {i + 10}",
                DownloadUrl = "https://example.com/download",
                SizeBytes = 1_800_000L + i * 100_000L,
                PublishedUtc = DateTime.UtcNow.AddDays(-i)
            });
        }

        for (var i = 1; i <= 4; i++)
        {
            ImportEntryCandidates.Add(new GameToolEntryCandidateDto
            {
                RelativePath = $@"trainer_win64\{i}\trainer.exe",
                SizeBytes = 800_000L + i * 10_000L
            });
        }

        SelectedTask = Tasks[0];
        SelectedMedia = Media.Count > 0 ? Media[0] : null;
        SelectedInboxMedia = UnassignedMedia.Count > 0 ? UnassignedMedia[0] : null;
        SelectedFinding = Findings.Count > 0 ? Findings[0] : null;
        SelectedDeviceComparison = DeviceComparisons.Count > 0 ? DeviceComparisons[0] : null;
        SelectedProcessMapping = ProcessMappings.Count > 0 ? ProcessMappings[0] : null;
        SelectedBackup = Backups[0];
        SelectedCandidate = SaveCandidates[0];
        SelectedGameTool = GameTools[0];
        SelectedGameToolVersion = SelectedGameTool.ActiveVersion;
        SelectedTrainerCatalogItem = TrainerCatalogResults[0];
        SelectedTrainerRelease = TrainerReleases[0];
        SelectedImportEntryCandidate = ImportEntryCandidates[0];
        MediaTargetGame = Games[0];
        InboxTargetGame = Games[0];
        ProcessMappingTargetGame = Games[0];
        LastBackupDiff = new BackupDiffDto
        {
            LeftBackupId = Backups[0].BackupId,
            RightBackupId = Backups[1].BackupId,
            Added = new System.Collections.Generic.List<string> { "Data/Save.bin" },
            Removed = new System.Collections.Generic.List<string> { "Data/OldSave.bin" },
            Modified = new System.Collections.Generic.List<string> { "Settings.ini" },
            UnchangedCount = 180,
            Summary = "新备份新增 1 个文件、修改 1 个文件、删除 1 个文件。"
        };
        DiffSummary = "差异摘要：新备份较旧备份有 1 个新增、1 个修改、1 个删除。";

        MediaView = CollectionViewSource.GetDefaultView(Media);
        TasksView = CollectionViewSource.GetDefaultView(Tasks);
    }

    public DashboardSnapshotDto Snapshot { get; }
    public EnvironmentCheckReportDto EnvironmentCheck { get; }
    public GameStatusDto SelectedGame { get; }
    public ObservableCollection<GameStatusDto> Games { get; } = new ObservableCollection<GameStatusDto>();
    public ObservableCollection<TaskStatusDto> Tasks { get; } = new ObservableCollection<TaskStatusDto>();
    public ICollectionView TasksView { get; }
    public ObservableCollection<TaskStatusDto> OverviewTasks { get; } = new ObservableCollection<TaskStatusDto>();
    public ObservableCollection<ValidationFindingDto> AttentionFindings { get; } = new ObservableCollection<ValidationFindingDto>();
    public ObservableCollection<MediaItemDto> Media { get; } = new ObservableCollection<MediaItemDto>();
    public ICollectionView MediaView { get; }
    public ObservableCollection<MediaItemDto> UnassignedMedia { get; } = new ObservableCollection<MediaItemDto>();
    public ObservableCollection<MediaSourceRuleDto> MediaSources { get; } = new ObservableCollection<MediaSourceRuleDto>();
    public ObservableCollection<ValidationFindingDto> Findings { get; } = new ObservableCollection<ValidationFindingDto>();
    public ObservableCollection<AuditLogEntryDto> Audit { get; } = new ObservableCollection<AuditLogEntryDto>();
    public ObservableCollection<DeviceConflictStatusDto> DeviceComparisons { get; } = new ObservableCollection<DeviceConflictStatusDto>();
    public ObservableCollection<ProcessMappingDto> ProcessMappings { get; } = new ObservableCollection<ProcessMappingDto>();
    public ObservableCollection<BackupVersionDto> Backups { get; } = new ObservableCollection<BackupVersionDto>();
    public ObservableCollection<SavePathCandidateDto> SaveCandidates { get; } = new ObservableCollection<SavePathCandidateDto>();
    public ObservableCollection<GameToolDto> GameTools { get; } = new ObservableCollection<GameToolDto>();
    public ObservableCollection<TrainerCatalogItemDto> TrainerCatalogResults { get; } = new ObservableCollection<TrainerCatalogItemDto>();
    public ObservableCollection<TrainerReleaseDto> TrainerReleases { get; } = new ObservableCollection<TrainerReleaseDto>();
    public ObservableCollection<GameToolEntryCandidateDto> ImportEntryCandidates { get; } = new ObservableCollection<GameToolEntryCandidateDto>();
    public ObservableCollection<string> TaskStatusFilterOptions { get; } = new ObservableCollection<string> { "全部", "等待中", "执行中", "成功", "失败", "已取消" };
    public ObservableCollection<string> TaskGameFilterOptions { get; } = new ObservableCollection<string> { "全部" };
    public ObservableCollection<string> TaskTypeFilterOptions { get; } = new ObservableCollection<string> { "全部", "存档备份", "媒体同步", "云端上传" };
    public ObservableCollection<string> MediaFilterOptions { get; } = new ObservableCollection<string> { "全部", "截图", "录像", "收藏" };
    public ObservableCollection<string> DeviceDecisionOptions { get; } = new ObservableCollection<string> { "稍后处理", "记录为优先本机", "记录为优先远端" };
    public RetentionPreviewDto LastRetentionPreview { get; } = new RetentionPreviewDto();
    public WorkerSettingsSnapshotDto EffectiveSettings { get; } = new WorkerSettingsSnapshotDto
    {
        DataDirectory = @"D:\GameSaveCenter\data",
        LudusaviExecutable = @"D:\Tools\ludusavi.exe",
        LudusaviBackupDirectory = @"D:\GameSaveCenter\backups",
        RcloneExecutable = @"D:\Tools\rclone.exe",
        RcloneDestinationConfigured = true,
        MediaArchiveDirectory = @"D:\GameSaveCenter\media"
    };
    public int RunningTaskCount { get; }
    public int RetryableTaskCount { get; }
    public int CompletedTaskCount { get; }
    public TaskStatusDto SelectedTask { get; set; } = null!;
    public BackupVersionDto SelectedBackup { get; set; } = null!;
    public SavePathCandidateDto SelectedCandidate { get; set; } = null!;
    public GameToolDto SelectedGameTool { get; set; } = null!;
    public GameToolVersionDto SelectedGameToolVersion { get; set; } = null!;
    public TrainerCatalogItemDto SelectedTrainerCatalogItem { get; set; } = null!;
    public TrainerReleaseDto SelectedTrainerRelease { get; set; } = null!;
    public GameToolEntryCandidateDto SelectedImportEntryCandidate { get; set; } = null!;
    public BackupDiffDto LastBackupDiff { get; set; } = new BackupDiffDto();
    public MediaItemDto? SelectedMedia { get; set; }
    public MediaItemDto? SelectedInboxMedia { get; set; }
    public ValidationFindingDto? SelectedFinding { get; set; }
    public DeviceConflictStatusDto? SelectedDeviceComparison { get; set; }
    public ProcessMappingDto? SelectedProcessMapping { get; set; }
    public GameStatusDto MediaTargetGame { get; set; } = null!;
    public GameStatusDto InboxTargetGame { get; set; } = null!;
    public GameStatusDto ProcessMappingTargetGame { get; set; } = null!;
    public MediaStorageSummaryDto MediaSummary { get; set; } = new MediaStorageSummaryDto
    {
        TotalCount = 32,
        ScreenshotCount = 28,
        VideoCount = 4,
        FavoriteCount = 4,
        TotalBytes = 1_840_000_000L
    };
    public string TaskStatusFilter { get; set; } = "全部";
    public string TaskGameFilter { get; set; } = "全部";
    public string TaskTypeFilter { get; set; } = "全部";
    public string TaskSearchText { get; set; } = string.Empty;
    public string MediaFilter { get; set; } = "全部";
    public string MediaSearchText { get; set; } = string.Empty;
    public string MediaComment { get; set; } = string.Empty;
    public bool MediaFavorite { get; set; } = true;
    public string DeviceDecision { get; set; } = "稍后处理";
    public string DeviceDecisionComment { get; set; } = string.Empty;
    public string ProcessMappingExecutable { get; set; } = "skse64.exe";
    public string DiffSummary { get; set; } = string.Empty;
    public bool LockSelectedBackup { get; set; }
    public string BackupComment { get; set; } = string.Empty;
    public string TrainerSearchText { get; set; } = string.Empty;
    public bool HasPendingGameToolEntrySelection { get; set; } = true;
    public string DiagnosticSummary { get; } = "09:31:12 SUCCESS Worker IPC health check passed.\n09:30:58 INFO Media scan started.\n09:18:06 ERROR Rclone remote unavailable; local source retained.";
    public string RetentionSummary { get; } = "全局保留策略只读预览：当前建议保留 12 个版本，候选清理 3 个版本。";
    public string DeviceStateMessage { get; } = "最近一次设备摘要对比完成：2 个游戏需要人工决定，其余一致。";
    public string StagedRemoteBackupStatus { get; } = "尚未下载远端存档。下载只会写入本机隔离区，不会覆盖当前存档。";
}
