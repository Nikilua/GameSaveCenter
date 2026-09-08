using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.RenderHarness;

public enum WorkspaceFixtureState
{
    Ready,
    Empty,
    Loading,
    Error,
    Stale,
    Offline
}

/// <summary>
/// Minimal view-model-shaped data for offscreen layout QA. It deliberately mirrors the
/// public binding surface of DashboardViewModel without starting Worker/IPC services.
/// </summary>
public sealed class FakeDashboardData
{
    private readonly WorkspaceFixtureState fixtureState;

    public string OnboardingTitle => "首次使用：准备环境";
    public string OnboardingDescription => "先确认 Worker、目录、SQLite 与备份工具可用。所有检查都是非破坏性的；你可以跳过，之后随时在维护中心重新运行。";
    public bool IsOnboardingPending => false;
    public ICommand OpenAttentionCenterCommand { get; } = new NoopCommand();
    public ICommand OpenMaintenanceCommand { get; } = new NoopCommand();
    public ICommand OpenCloudQueueCommand { get; } = new NoopCommand();
    public ICommand OpenActivityCommand { get; } = new NoopCommand();
    public ICommand OpenMediaWorkspaceCommand { get; } = new NoopCommand();
    public ICommand OpenSelectedFindingNavigationCommand { get; } = new NoopCommand();
    public ICommand RefreshCommand { get; } = new NoopCommand();
    public ICommand RefreshCloudTransfersCommand { get; } = new NoopCommand();
    public ICommand LoadMoreCloudTransfersCommand { get; } = new NoopCommand();
    public ICommand VerifyCloudTransferCommand { get; } = new NoopCommand();
    public ICommand RetryCloudUploadCommand { get; } = new NoopCommand();
    public ICommand RefreshMediaClassificationHistoryCommand { get; } = new NoopCommand();
    public ICommand LoadMoreMediaClassificationHistoryCommand { get; } = new NoopCommand();
    public ICommand LoadMoreMediaCommand { get; } = new NoopCommand();
    public ICommand ReloadMediaWindowCommand { get; } = new NoopCommand();
    public ICommand LoadMoreMediaInboxCommand { get; } = new NoopCommand();
    public ICommand ReloadMediaInboxCommand { get; } = new NoopCommand();
    public ICommand UndoMediaClassificationCommand { get; } = new NoopCommand();
    public ICommand LoadMoreTasksCommand { get; } = new NoopCommand();
    public ICommand RetryAllTasksCommand { get; } = new NoopCommand();
    public ICommand RetryTaskCommand { get; } = new NoopCommand();
    public ICommand CancelTaskCommand { get; } = new NoopCommand();
    public ICommand CopyTaskErrorCommand { get; } = new NoopCommand();
    public ICommand ClearTaskFiltersCommand { get; } = new NoopCommand();

    public FakeDashboardData(int rowCount = 8)
        : this(rowCount, WorkspaceFixtureState.Ready)
    {
    }

    public FakeDashboardData(int rowCount, WorkspaceFixtureState state)
    {
        fixtureState = state;
        rowCount = Math.Max(8, rowCount);
        Snapshot = new DashboardSnapshotDto
        {
            WorkerHealthy = state != WorkspaceFixtureState.Offline,
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

        var cloudNow = DateTime.UtcNow;
        CloudTransferItems.Add(new CloudTransferStatusDto { TransferKey = "Backup:game-1", Kind = CloudTransferKind.Backup, PlayniteId = "game-1", GameName = "Baldur's Gate 3", State = "Pending", UpdatedUtc = cloudNow.AddMinutes(-2) });
        CloudTransferItems.Add(new CloudTransferStatusDto { TransferKey = "Media:game-2", Kind = CloudTransferKind.Media, PlayniteId = "game-2", GameName = "演示游戏 2", State = "Verifying", OperationKind = CloudTransferOperationKind.Verify, UpdatedUtc = cloudNow.AddMinutes(-1) });
        CloudTransferItems.Add(new CloudTransferStatusDto { TransferKey = "Backup:game-3", Kind = CloudTransferKind.Backup, PlayniteId = "game-3", GameName = "演示游戏 3", State = "RetryScheduled", AttemptCount = 2, NextAttemptUtc = cloudNow.AddMinutes(18), LastErrorCode = "RCLONE_NETWORK_FAILED", LastError = "远端暂时不可用", UpdatedUtc = cloudNow.AddMinutes(-5) });
        CloudTransferItems.Add(new CloudTransferStatusDto { TransferKey = "Media:game-4", Kind = CloudTransferKind.Media, PlayniteId = "game-4", GameName = "演示游戏 4", State = "AuthenticationRequired", AttemptCount = 1, LastErrorCode = "RCLONE_AUTH_FAILED", LastError = "凭据已过期", UpdatedUtc = cloudNow.AddMinutes(-8) });
        CloudTransferItems.Add(new CloudTransferStatusDto { TransferKey = "Backup:game-5", Kind = CloudTransferKind.Backup, PlayniteId = "game-5", GameName = "演示游戏 5", State = "Uploaded", UpdatedUtc = cloudNow.AddHours(-1) });
        CloudTransferItems.Add(new CloudTransferStatusDto { TransferKey = "Media:game-6", Kind = CloudTransferKind.Media, PlayniteId = "game-6", GameName = "演示游戏 6", State = "RemoteVerified", UpdatedUtc = cloudNow.AddHours(-2) });
        CloudTransferItems.Add(new CloudTransferStatusDto { TransferKey = "Backup:game-7", Kind = CloudTransferKind.Backup, PlayniteId = "game-7", GameName = "演示游戏 7", State = "CheckFailed", LastErrorCode = "RCLONE_CHECK_FAILED", LastError = "远端内容未通过 check", UpdatedUtc = cloudNow.AddHours(-3) });
        CloudTransferItems.Add(new CloudTransferStatusDto { TransferKey = "Backup:game-8", Kind = CloudTransferKind.Backup, PlayniteId = "game-8", GameName = "演示游戏 8", State = "Failed", AttemptCount = 3, LastErrorCode = "RCLONE_NETWORK_FAILED", LastError = "自动重试已达上限", UpdatedUtc = cloudNow.AddHours(-4) });
        CloudTransferViewSummary = new CloudTransferSummaryDto
        {
            TotalCount = CloudTransferItems.Count,
            PendingCount = 1,
            VerifyingCount = 1,
            RetryScheduledCount = 1,
            AuthenticationRequiredCount = 1,
            UploadedCount = 1,
            VerifiedCount = 1,
            CheckFailedCount = 1,
            FailedCount = 1,
            Page = 0,
            PageSize = 100,
            LoadedCount = CloudTransferItems.Count,
            HasMore = false,
            Items = new System.Collections.Generic.List<CloudTransferStatusDto>(CloudTransferItems)
        };
        Snapshot.CloudTransfers = CloudTransferViewSummary;

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

        var fixtureNowUtc = DateTime.UtcNow;
        for (var i = 1; i <= rowCount; i++)
        {
            var protectedFixture = i % 4 == 0;
            Games.Add(new GameStatusDto
            {
                PlayniteId = "game-" + i,
                Name = i == 1 ? SelectedGame.Name : $"演示游戏 {i}",
                Platform = GamePlatformKind.Steam,
                IsInstalled = true,
                LastPlayedUtc = fixtureNowUtc.AddDays(-i),
                LudusaviMatched = protectedFixture || i % 2 == 0,
                LastBackupUtc = protectedFixture
                    ? fixtureNowUtc.AddDays(-i).AddMinutes(10)
                    : i % 3 == 0 ? fixtureNowUtc.AddDays(-(i + 1)) : null,
                BackupVersionCount = i * 2,
                LatestRestoreReadinessStatus = protectedFixture || i % 3 == 0 ? RestoreReadinessStatus.Ready : null,
                Policy = protectedFixture
                    ? new BackupPolicyDto { Enabled = true, BackupOnGameStop = true, BackupDuringPlay = true }
                    : new BackupPolicyDto(),
                MediaCount = i * 3,
                CloudState = i % 3 == 0 ? "Pending" : "Uploaded",
                HealthState = i % 5 == 0 ? "Warning" : "Ready"
            });
        }

        RecentProtection = new RecentProtectionAssessmentService().Assess(Games, 30, fixtureNowUtc);

        for (var i = 1; i <= rowCount; i++)
        {
            var taskState = i % 4 == 0 ? TaskState.Failed
                : i % 4 == 1 ? TaskState.Running
                : i % 4 == 2 ? TaskState.Succeeded
                : TaskState.Cancelled;
            Tasks.Add(new TaskStatusDto
            {
                TaskId = "T-" + i.ToString("D4"),
                TaskType = i % 3 == 0 ? "MediaSync" : "Backup",
                GameId = "game-" + i,
                GameName = Games[i - 1].Name,
                State = taskState,
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

        Activities.Add(new ActivityEntryDto
        {
            Kind = "Backup",
            Result = "Succeeded",
            GameName = Games[0].Name,
            Summary = "Ludusavi 备份结果：新增 1 个文件",
            CreatedUtc = DateTime.UtcNow.AddMinutes(-8)
        });
        Activities.Add(new ActivityEntryDto
        {
            Kind = "Cloud",
            Result = "Failed",
            GameName = Games[1].Name,
            Summary = "云端复制失败，本地版本已保留",
            CreatedUtc = DateTime.UtcNow.AddMinutes(-20)
        });
        Activities.Add(new ActivityEntryDto
        {
            Kind = "Integrity",
            Result = "Succeeded",
            GameName = "全局",
            Summary = "完整性自检通过：数据库、目录和已索引文件正常",
            CreatedUtc = DateTime.UtcNow.AddMinutes(-35)
        });

        for (var i = 1; i <= 4; i++)
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

        var classificationNow = DateTime.UtcNow;
        MediaClassificationPreview = new MediaClassificationPreviewDto
        {
            BatchId = "preview-demo",
            CreatedUtc = classificationNow.AddMinutes(-4),
            ExpiresUtc = classificationNow.AddMinutes(6),
            Items = new System.Collections.Generic.List<MediaClassificationSuggestionDto>
            {
                new MediaClassificationSuggestionDto { MediaId = "IN-1", FileName = "shared-1.png", SuggestedGameName = "Baldur's Gate 3", SuggestedPlayniteId = "game-1", Confidence = "High", Reason = "媒体来源规则" },
                new MediaClassificationSuggestionDto { MediaId = "IN-2", FileName = "shared-2.png", Confidence = "Low", Reason = "多个候选游戏，保持未归类" }
            },
            HighConfidenceCount = 1,
            LowConfidenceCount = 1
        };
        MediaClassificationHistoryItems.Add(new MediaClassificationBatchSummaryDto
        {
            BatchId = "classification-1", State = "AppliedWithConflicts", CreatedUtc = classificationNow.AddHours(-1),
            UpdatedUtc = classificationNow.AddMinutes(-7), ExpiresUtc = classificationNow.AddHours(-1), ItemCount = 12,
            AppliedCount = 8, ConflictCount = 2, UndoneCount = 2
        });
        MediaClassificationHistoryItems.Add(new MediaClassificationBatchSummaryDto
        {
            BatchId = "classification-2", State = "UndoneWithConflicts", CreatedUtc = classificationNow.AddHours(-3),
            UpdatedUtc = classificationNow.AddHours(-2), ExpiresUtc = classificationNow.AddHours(-3), ItemCount = 6,
            UndoneCount = 5, ConflictCount = 1
        });
        MediaClassificationHistoryItems.Add(new MediaClassificationBatchSummaryDto
        {
            BatchId = "classification-3", State = "Preview", CreatedUtc = classificationNow.AddMinutes(-4),
            UpdatedUtc = classificationNow.AddMinutes(-4), ExpiresUtc = classificationNow.AddMinutes(6), ItemCount = 2
        });
        SelectedMediaClassificationBatch = MediaClassificationHistoryItems[0];

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

        MaintenanceActionItems.Add(new MaintenanceActionItem
        {
            ItemId = "inspection-1",
            CategoryDisplay = "恢复巡检",
            Title = "检查最近可恢复版本",
            StatusDisplay = "待确认",
            Detail = "最近一次验证发现一个可继续检查的版本，建议查看详情。",
            LastVerifiedDisplay = "今天 09:18",
            NextAttemptDisplay = "手动触发",
            ActionText = "查看巡检",
            ActionToolTip = "查看恢复巡检记录",
            ActionKind = MaintenanceActionKind.HealthInspection
        });
        MaintenanceActionItems.Add(new MaintenanceActionItem
        {
            ItemId = "transfer-1",
            CategoryDisplay = "云端队列",
            Title = "Baldur's Gate 3 云端上传",
            StatusDisplay = "等待重试",
            Detail = "远端暂时不可用，本地版本已保留，不会自动覆盖本地文件。",
            LastAttemptDisplay = "今天 09:12",
            NextAttemptDisplay = "18 分钟后",
            ActionText = "打开任务",
            ActionToolTip = "查看这条云端队列记录",
            ActionKind = MaintenanceActionKind.CloudTransfer,
            TransferKey = "Backup:game-1",
            TransferState = "RetryScheduled"
        });
        MaintenanceActionItems.Add(new MaintenanceActionItem
        {
            ItemId = "quarantine-1",
            CategoryDisplay = "隔离账本",
            Title = "发现待人工确认的隔离文件",
            StatusDisplay = "需确认",
            Detail = "文件身份与当前索引不一致，继续协调前需要明确确认。",
            LedgerUpdatedDisplay = "昨天 22:41",
            NextAttemptDisplay = "确认后执行",
            ActionText = "查看账本",
            ActionToolTip = "查看隔离账本记录",
            ActionKind = MaintenanceActionKind.RetentionQuarantine,
            EntryId = "entry-1"
        });

        RebuildMaintenanceActionSections();

        ApplyWorkspaceFixtureState();

        SelectedTask = Tasks[0];
        SelectedMedia = Media.Count > 0 ? Media[0] : null;
        SelectedInboxMedia = UnassignedMedia.Count > 0 ? UnassignedMedia[0] : null;
        SelectedFinding = Findings.Count > 0 ? Findings[0] : null;
        SelectedDeviceComparison = DeviceComparisons.Count > 0 ? DeviceComparisons[0] : null;
        SelectedProcessMapping = ProcessMappings.Count > 0 ? ProcessMappings[0] : null;
        SelectedCloudTransfer = CloudTransferItems.Count > 0 ? CloudTransferItems[0] : null;
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

    private void ApplyWorkspaceFixtureState()
    {
        if (fixtureState == WorkspaceFixtureState.Ready || fixtureState == WorkspaceFixtureState.Stale)
            return;

        Media.Clear();
        UnassignedMedia.Clear();
        Findings.Clear();
        Audit.Clear();
        MaintenanceActionItems.Clear();
        MaintenanceActionSections.Clear();
        Snapshot.UnassignedMediaCount = 0;
    }

    private void RebuildMaintenanceActionSections()
    {
        MaintenanceActionSections.Clear();
        var ordered = MaintenanceActionItems
            .OrderBy(item => item.Group)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var sections = new[]
        {
            new MaintenanceActionSection(
            MaintenanceActionGroup.NeedsManualHandling,
            "需要人工处理",
            "冲突、失败或隔离账本不会被自动覆盖；逐条确认后再继续。",
            ordered.Where(item => item.Group == MaintenanceActionGroup.NeedsManualHandling)),
            new MaintenanceActionSection(
            MaintenanceActionGroup.WaitingForRetry,
            "等待自动重试",
            "这些记录已有下一次尝试时间，不需要重复点击上传。",
            ordered.Where(item => item.Group == MaintenanceActionGroup.WaitingForRetry)),
            new MaintenanceActionSection(
            MaintenanceActionGroup.Routine,
            "例行巡检",
            "按需运行非破坏性检查，结果会回到同一维护上下文。",
            ordered.Where(item => item.Group == MaintenanceActionGroup.Routine))
        };
        foreach (var section in sections.Where(section => section.Items.Count > 0))
            MaintenanceActionSections.Add(section);
    }

    private string FixtureStateText => fixtureState.ToString();
    private bool HasFixtureData => fixtureState is WorkspaceFixtureState.Ready or WorkspaceFixtureState.Stale;
    private bool IsFixtureOffline => fixtureState == WorkspaceFixtureState.Offline;
    private bool IsFixtureOverlay => fixtureState is WorkspaceFixtureState.Loading or WorkspaceFixtureState.Error or WorkspaceFixtureState.Offline;
    private string FixtureStateDetail => fixtureState switch
    {
        WorkspaceFixtureState.Error => "错误详情：Worker 暂时没有返回完整列表。请稍后重试；这段长错误用于验证详情区域不会裁切。",
        WorkspaceFixtureState.Stale => "上次成功读取：今天 09:18；本次刷新失败，仍保留旧数据。",
        WorkspaceFixtureState.Offline => "Worker 当前离线；恢复连接后才能确认最新数量。",
        WorkspaceFixtureState.Loading => "正在等待 Worker 返回结果。",
        _ => string.Empty
    };

    private string FixtureStateTitle(string subject) => fixtureState switch
    {
        WorkspaceFixtureState.Loading => $"正在读取{subject}",
        WorkspaceFixtureState.Empty => $"当前没有{subject}",
        WorkspaceFixtureState.Stale => $"{subject}显示已过期",
        WorkspaceFixtureState.Error => $"{subject}读取失败",
        WorkspaceFixtureState.Offline => "Worker 当前离线",
        _ => string.Empty
    };

    private string FixtureStateMessage(string subject) => fixtureState switch
    {
        WorkspaceFixtureState.Loading => $"正在读取{subject}；已有内容会保留到新结果确认后。",
        WorkspaceFixtureState.Empty => $"当前没有{subject}，新的内容会在 Worker 返回后显示。",
        WorkspaceFixtureState.Stale => $"仍保留上次成功读取的{subject}；本次刷新没有覆盖它。",
        WorkspaceFixtureState.Error => $"当前{subject}暂时无法读取，请稍后重试。",
        WorkspaceFixtureState.Offline => $"{subject}暂时不可用；Worker 恢复后可重新读取。",
        _ => string.Empty
    };

    public DashboardSnapshotDto Snapshot { get; }
    private OverviewPriorityState OverviewPriority => OverviewPriorityResolver.Resolve(Snapshot, IsOnboardingPending);
    public string OverviewPriorityKind => OverviewPriority.Kind;
    public string OverviewPriorityTitle => OverviewPriority.Title;
    public string OverviewPriorityDescription => OverviewPriority.Description;
    public string OverviewPriorityActionText => OverviewPriority.ActionText;
    public string OverviewPriorityActionToolTip => OverviewPriority.ActionToolTip;
    public ICommand OverviewPriorityActionCommand => OverviewPriority.ActionKind switch
    {
        "Maintenance" => OpenMaintenanceCommand,
        "CloudQueue" => OpenCloudQueueCommand,
        "Media" => OpenMediaWorkspaceCommand,
        "Attention" => OpenAttentionCenterCommand,
        _ => RefreshCommand
    };
    public EnvironmentCheckReportDto EnvironmentCheck { get; }
    public GameStatusDto SelectedGame { get; }
    public RecentProtectionSummary RecentProtection { get; }
    public ObservableCollection<GameStatusDto> Games { get; } = new ObservableCollection<GameStatusDto>();
    public ObservableCollection<TaskStatusDto> Tasks { get; } = new ObservableCollection<TaskStatusDto>();
    public ICollectionView TasksView { get; }
    public ObservableCollection<TaskStatusDto> OverviewTasks { get; } = new ObservableCollection<TaskStatusDto>();
    public ObservableCollection<ActivityEntryDto> Activities { get; } = new ObservableCollection<ActivityEntryDto>();
    public ObservableCollection<ValidationFindingDto> AttentionFindings { get; } = new ObservableCollection<ValidationFindingDto>();
    public ObservableCollection<MediaItemDto> Media { get; } = new ObservableCollection<MediaItemDto>();
    public ObservableCollection<MaintenanceActionItem> MaintenanceActionItems { get; } = new ObservableCollection<MaintenanceActionItem>();
    public ObservableCollection<MaintenanceActionSection> MaintenanceActionSections { get; } = new ObservableCollection<MaintenanceActionSection>();
    public ICollectionView MediaView { get; }
    public bool MediaPageHasMore => true;
    public string MediaLoadedSummary => $"当前保留 {Media.Count} 条（窗口上限 2000）";
    public ObservableCollection<MediaItemDto> UnassignedMedia { get; } = new ObservableCollection<MediaItemDto>();
    public ObservableCollection<MediaItemDto> MediaInboxItems => UnassignedMedia;
    public bool MediaInboxPageHasMore => false;
    public string MediaInboxLoadedSummary => $"当前保留 {MediaInboxItems.Count} 条（窗口上限 2000）";
    public bool IsWorkerOffline => IsFixtureOffline;
    public string MediaDetailsState => FixtureStateText;
    public string MediaDetailsPresenterState => IsFixtureOffline ? "Offline" : fixtureState == WorkspaceFixtureState.Stale ? "Degraded" : FixtureStateText;
    public string MediaDetailsStateTitle => FixtureStateTitle("当前游戏媒体");
    public string MediaDetailsStateMessage => FixtureStateMessage("当前游戏媒体");
    public string MediaDetailsStateDetail => FixtureStateDetail;
    public bool MediaDetailsStateOverlayVisible => IsFixtureOverlay;
    public bool MediaDetailsStaleVisible => !IsFixtureOffline && fixtureState == WorkspaceFixtureState.Stale;
    public string MediaInboxState => FixtureStateText;
    public string MediaInboxPresenterState => IsFixtureOffline ? "Offline" : fixtureState == WorkspaceFixtureState.Stale ? "Degraded" : FixtureStateText;
    public string MediaInboxStateTitle => FixtureStateTitle("媒体收件箱");
    public string MediaInboxStateMessage => FixtureStateMessage("媒体收件箱");
    public string MediaInboxStateDetail => FixtureStateDetail;
    public bool MediaInboxStateOverlayVisible => IsFixtureOverlay;
    public bool MediaInboxStaleVisible => !IsFixtureOffline && fixtureState == WorkspaceFixtureState.Stale;
    public string MediaInboxCountDisplay => HasFixtureData ? MediaInboxItems.Count.ToString() : "—";
    public string MediaInboxCountCaption => fixtureState switch
    {
        WorkspaceFixtureState.Loading => "正在读取 · 来源文件始终保留",
        WorkspaceFixtureState.Error => "无法读取 · 尚未确认数量",
        WorkspaceFixtureState.Stale => "缓存 · 上次成功 今天 09:18",
        WorkspaceFixtureState.Offline => "离线 · 无法读取",
        _ => "待归类 · 来源文件始终保留"
    };
    public string MaintenanceState => FixtureStateText;
    public string MaintenancePresenterState => IsFixtureOffline ? "Offline" : fixtureState == WorkspaceFixtureState.Stale ? "Degraded" : FixtureStateText;
    public string MaintenanceStateTitle => FixtureStateTitle("维护信息");
    public string MaintenanceStateMessage => FixtureStateMessage("维护信息");
    public string MaintenanceStateDetail => FixtureStateDetail;
    public bool MaintenanceStateOverlayVisible => IsFixtureOverlay;
    public bool MaintenanceStaleVisible => !IsFixtureOffline && fixtureState == WorkspaceFixtureState.Stale;
    public string MaintenanceActionSummary => fixtureState == WorkspaceFixtureState.Ready
        ? "恢复巡检：待确认 · 云端待处理：2 · 隔离账本：1 项"
        : FixtureStateMessage("维护信息");
    public ObservableCollection<string> MediaInboxModeOptions { get; } = new ObservableCollection<string> { "待归类", "已忽略" };
    public string MediaInboxMode { get; set; } = "待归类";
    public string MediaInboxTitle => MediaInboxMode == "已忽略" ? "已忽略媒体" : "待归类媒体";
    public string MediaInboxEmptyText => MediaInboxMode == "已忽略" ? "当前没有已忽略的媒体。" : "当前没有等待归类的媒体。";
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
    public ObservableCollection<CloudTransferStatusDto> CloudTransferItems { get; } = new ObservableCollection<CloudTransferStatusDto>();
    public ObservableCollection<MediaClassificationBatchSummaryDto> MediaClassificationHistoryItems { get; } = new ObservableCollection<MediaClassificationBatchSummaryDto>();
    public MediaClassificationBatchSummaryDto? SelectedMediaClassificationBatch { get; set; }
    public MediaClassificationPreviewDto MediaClassificationPreview { get; }
    public string MediaClassificationPreviewSummary => $"{MediaClassificationPreview.SummaryDisplay} 预览有效期至 {MediaClassificationPreview.ExpiresUtc.ToLocalTime():MM-dd HH:mm}。";
    public ObservableCollection<MediaClassificationHistoryStateOption> MediaClassificationHistoryStateOptions { get; } = new ObservableCollection<MediaClassificationHistoryStateOption>
    {
        new MediaClassificationHistoryStateOption(string.Empty, "全部批次"), new MediaClassificationHistoryStateOption("Applied", "已应用"), new MediaClassificationHistoryStateOption("Conflict", "应用冲突")
    };
    public string MediaClassificationHistoryStateFilter { get; set; } = string.Empty;
    public bool MediaClassificationHistoryHasMore => false;
    public string MediaClassificationHistoryLoadedSummary => $"已加载全部 {MediaClassificationHistoryItems.Count} 个批次";
    public CloudTransferSummaryDto CloudTransferViewSummary { get; private set; } = new CloudTransferSummaryDto();
    public CloudTransferStatusDto? SelectedCloudTransfer { get; set; }
    public ObservableCollection<CloudTransferFilterOption> CloudTransferStateOptions { get; } = new ObservableCollection<CloudTransferFilterOption>
    {
        new CloudTransferFilterOption(string.Empty, "全部状态"), new CloudTransferFilterOption("Pending", "待上传"), new CloudTransferFilterOption("Uploaded", "已上传")
    };
    public ObservableCollection<CloudTransferFilterOption> CloudTransferKindOptions { get; } = new ObservableCollection<CloudTransferFilterOption>
    {
        new CloudTransferFilterOption(string.Empty, "全部类型"), new CloudTransferFilterOption("Backup", "备份"), new CloudTransferFilterOption("Media", "媒体")
    };
    public string CloudTransferStateFilter { get; set; } = string.Empty;
    public string CloudTransferKindFilter { get; set; } = string.Empty;
    public bool CloudTransferHasMore => CloudTransferViewSummary.HasMore;
    public string CloudTransferLoadedSummary => $"已加载全部 {CloudTransferItems.Count} 项";
    public int MediaTabIndex { get; set; }
    public int MaintenanceTabIndex { get; set; }
    public ObservableCollection<string> TaskStatusFilterOptions { get; } = new ObservableCollection<string> { "全部", "运行中", "等待中", "失败", "已完成" };
    public ObservableCollection<string> TaskGameFilterOptions { get; } = new ObservableCollection<string> { "全部" };
    public ObservableCollection<string> TaskTypeFilterOptions { get; } = new ObservableCollection<string> { "全部", "存档备份", "媒体同步", "云端上传" };
    public ObservableCollection<string> TaskHistoryScopeOptions { get; } = new ObservableCollection<string> { "最近任务", "全部历史" };
    public ObservableCollection<string> TaskHistoryRangeOptions { get; } = new ObservableCollection<string> { "全部时间", "今天", "昨天", "近7天", "近30天" };
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
        ,
        EnableLocalMirror = true,
        LocalMirrorPath = @"H:\GameSaveCenter-Mirror"
    };
    public LocalMirrorStatusDto LocalMirrorStatus { get; } = new LocalMirrorStatusDto
    {
        Enabled = true,
        Available = true,
        MirrorPath = @"H:\GameSaveCenter-Mirror",
        LastSyncUtc = DateTime.UtcNow.AddHours(-2),
        CopiedCount = 184,
        VerifiedCount = 184,
        TotalBytes = 128L * 1024 * 1024 * 1024,
        Message = "镜像可用：184 个文件，共 128 GiB；最近同步 2026-08-14 00:00。"
    };
    public int RunningTaskCount { get; }
    public int WaitingTaskCount => Tasks.Count(task => task.State == TaskState.Queued || task.State == TaskState.WaitingForUser);
    public string TaskWaitingSummary => $"排队/等待确认：{WaitingTaskCount}";
    public int RetryableTaskCount { get; }
    public string TaskRetrySummary => $"失败 {Tasks.Count(task => task.State == TaskState.Failed)} · 已取消 {Tasks.Count(task => task.State == TaskState.Cancelled)}";
    public int CompletedTaskCount { get; }
    public int TaskTotalCount => Tasks.Count;
    public string TaskTotalCountLabel => "任务总数（全部历史）";
    public string TaskHistoryScope { get; set; } = "最近任务";
    public string TaskHistoryRange { get; set; } = "全部时间";
    public bool TaskHistoryHasMore => false;
    public string TaskLoadedSummary => $"已加载 {Tasks.Count} / {Tasks.Count} 条 · {TaskHistoryScope}";
    public string TaskActiveFiltersSummary => "当前未设置额外条件";
    public bool TaskPageHasLoaded => true;
    public bool IsTaskPageLoading => fixtureState == WorkspaceFixtureState.Loading;
    public bool TaskPageLoadFailed => fixtureState == WorkspaceFixtureState.Error;
    public bool TaskPageHasItems => (fixtureState is WorkspaceFixtureState.Ready or WorkspaceFixtureState.Stale) && Tasks.Count > 0;
    public string TaskPageState => fixtureState switch
    {
        WorkspaceFixtureState.Loading => "Loading",
        WorkspaceFixtureState.Error => "Error",
        WorkspaceFixtureState.Empty => "Empty",
        _ => Tasks.Count > 0 ? "Ready" : "Empty"
    };
    public string TaskPageStatusSummary => "最近更新：2026-09-05 00:00:00";
    public string TaskPageErrorMessage => fixtureState == WorkspaceFixtureState.Error ? FixtureStateDetail : string.Empty;
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
    public string SelectedFindingNavigationText => FindingNavigationResolver.Resolve(SelectedFinding).Text;
    public string SelectedFindingNavigationToolTip => FindingNavigationResolver.Resolve(SelectedFinding).ToolTip;
    public bool HasSelectedFindingNavigation => FindingNavigationResolver.Resolve(SelectedFinding).IsAvailable;
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
    public bool TaskHasActiveFilters => false;
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
    public StorageAnalysisDto StorageAnalysis { get; } = new StorageAnalysisDto
    {
        BackupDirectoryAvailable = true,
        VolumeRoot = "D:",
        VolumeTotalBytes = 1024L * 1024 * 1024 * 1024,
        VolumeFreeBytes = 512L * 1024 * 1024 * 1024,
        RepositoryBytes = 128L * 1024 * 1024 * 1024,
        IndexedBackupBytes = 96L * 1024 * 1024 * 1024,
        BackupVersionCount = 184,
        Summary = "卷 D: 剩余 512 GiB / 共 1 TiB；索引 184 个版本，索引体积 96 GiB，目录实测 128 GiB。近 30 天新增 12 GiB（估算）",
        PredictionSummary = "按最近 30 天新增速度估算，约 14 个月后达到磁盘 90% 用量。",
        Trends =
        {
            new StorageTrendDto { Days = 7, AddedBytes = 2L * 1024 * 1024 * 1024, AddedVersionCount = 9 },
            new StorageTrendDto { Days = 30, AddedBytes = 12L * 1024 * 1024 * 1024, AddedVersionCount = 31 },
            new StorageTrendDto { Days = 90, AddedBytes = 30L * 1024 * 1024 * 1024, AddedVersionCount = 74 }
        },
        TopGames =
        {
            new StorageGameRankDto { GameName = "Cyberpunk 2077", BackupCount = 12, BackupBytes = 20L * 1024 * 1024 * 1024, LatestBackupUtc = DateTime.UtcNow.AddHours(-3) },
            new StorageGameRankDto { GameName = "Baldur's Gate 3", BackupCount = 18, BackupBytes = 16L * 1024 * 1024 * 1024, LatestBackupUtc = DateTime.UtcNow.AddDays(-1) },
            new StorageGameRankDto { GameName = "Elden Ring", BackupCount = 8, BackupBytes = 9L * 1024 * 1024 * 1024, LatestBackupUtc = DateTime.UtcNow.AddDays(-2) }
        }
    };
    public RetentionSimulationPreviewDto RetentionSimulation { get; } = new RetentionSimulationPreviewDto
    {
        ExistingVersionCount = 520,
        KeepVersionCount = 320,
        DeleteCandidateCount = 200,
        UserLockedCount = 18,
        HealthProtectedCount = 12,
        PreRestoreCount = 4,
        EstimatedReleaseBytes = 43L * 1024 * 1024 * 1024,
        Summary = "现有 520 个版本，建议保留 320 个，候选清理 200 个（预计释放 43 GiB）；用户锁定 18，健康恢复点保护 12，PreRestore 4。预览只读，清理不会自动执行。",
        Items =
        {
            new RetentionSimulationItemDto
            {
                PlayniteId = "g1", GameName = "Cyberpunk 2077", BackupId = "old-1",
                CreatedUtc = DateTime.UtcNow.AddDays(-300), TotalBytes = 4L * 1024 * 1024 * 1024,
                ArchivePath = @"D:\GameSaveCenterData\Saves\Cyberpunk 2077\old-1.zip", Reason = "超出保留窗口或桶位"
            },
            new RetentionSimulationItemDto
            {
                PlayniteId = "g2", GameName = "Baldur's Gate 3", BackupId = "old-2",
                CreatedUtc = DateTime.UtcNow.AddDays(-260), TotalBytes = 3L * 1024 * 1024 * 1024,
                ArchivePath = @"D:\GameSaveCenterData\Saves\Baldur's Gate 3\old-2.zip", Reason = "超出保留窗口或桶位"
            }
        }
    };
    public string DeviceStateMessage { get; } = "最近一次设备摘要对比完成：2 个游戏需要人工决定，其余一致。";
    public string StagedRemoteBackupStatus { get; } = "尚未下载远端存档。下载只会写入本机隔离区，不会覆盖当前存档。";
}

internal sealed class NoopCommand : ICommand
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
    }
}
