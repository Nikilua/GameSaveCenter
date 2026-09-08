using System.Security.Cryptography;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Ipc;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class MediaSyncServiceTests : IDisposable
{
    private readonly string root=Path.Combine(Path.GetTempPath(),"GameSaveCenter.Tests",Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public MediaSyncServiceTests()
    {
        options=new WorkerOptions
        {
            DataDirectory=root,
            LudusaviBackupDirectory=Path.Combine(root,"Saves"),
            MediaArchiveDirectory=Path.Combine(root,"Media")
        };
        store=new SqliteStateStore(options,NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task RestoreIgnoredBatchMovesArchiveCopyBackToPendingWithoutDeletingOriginal()
    {
        var originalPath=Path.Combine(root,"Captures","capture.png");
        var ignoredPath=Path.Combine(options.MediaArchiveDirectory,"_Inbox","Ignored","Screenshots","2026","08","capture.png");
        var content=new byte[] { 1, 2, 3, 4, 5 };
        Directory.CreateDirectory(Path.GetDirectoryName(originalPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(ignoredPath)!);
        await File.WriteAllBytesAsync(originalPath,content);
        await File.WriteAllBytesAsync(ignoredPath,content);

        var hash=Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
        await store.AddMediaAsync(new MediaItemDto
        {
            MediaId="ignored-media",
            Kind=MediaKind.Screenshot,
            Source=MediaSourceKind.Custom,
            ArchivePath=ignoredPath,
            OriginalPath=originalPath,
            CapturedUtc=new DateTime(2026,8,23,10,20,30,DateTimeKind.Utc),
            SizeBytes=content.Length,
            Sha256=hash,
            ClassificationState="Ignored",
            ClassificationReason="用户已忽略",
            CloudState="NotApplicable"
        },CancellationToken.None);

        var service=CreateService();
        var result=await service.RestoreIgnoredBatchAsync(new MediaInboxBatchRequestDto
        {
            MediaIds=new List<string>{"ignored-media"}
        },CancellationToken.None);

        Assert.Single(result.UpdatedItems);
        Assert.Empty(result.Failures);
        var restoredPath=result.UpdatedItems[0].ArchivePath;
        Assert.True(File.Exists(restoredPath));
        Assert.False(File.Exists(ignoredPath));
        Assert.True(File.Exists(originalPath));
        Assert.Equal(content,await File.ReadAllBytesAsync(restoredPath));
        var restored=await store.GetMediaByIdAsync("ignored-media",CancellationToken.None);
        Assert.Equal("Inbox",restored!.ClassificationState);
        Assert.Equal("用户撤销忽略，待重新归类",restored.ClassificationReason);
    }

    [Fact]
    public async Task UserMediaRetryReportsPolicyPauseWithoutStartingMediaSync()
    {
        await store.UpsertGamesAsync(new[]
        {
            new GameDescriptorDto { PlayniteId = "policy-game", Name = "Policy Game", Platform = GamePlatformKind.Steam }
        }, CancellationToken.None);

        var result = await CreateService().RetryCloudUploadForUserAsync("policy-game", CancellationToken.None);

        Assert.Equal(MediaCloudRetryOutcome.PausedByPolicy, result.Outcome);
        Assert.Null(result.Task);
        Assert.Contains("策略", result.Message);
    }

    [Fact]
    public async Task UserMediaRetryReportsUnavailableCloudWithoutSubmittingTask()
    {
        await store.UpsertGamesAsync(new[]
        {
            new GameDescriptorDto { PlayniteId = "cloud-disabled-game", Name = "Cloud Disabled Game", Platform = GamePlatformKind.Steam }
        }, CancellationToken.None);
        await store.SetPolicyAsync("cloud-disabled-game", new BackupPolicyDto { UploadAfterBackup = true }, CancellationToken.None);
        options.EnableCloudUpload = false;

        var result = await CreateService().RetryCloudUploadForUserAsync("cloud-disabled-game", CancellationToken.None);

        Assert.Equal(MediaCloudRetryOutcome.CannotSubmit, result.Outcome);
        Assert.Equal("RCLONE_NOT_CONFIGURED", result.ErrorCode);
        Assert.Null(result.Task);
        Assert.Contains("云端", result.Message);
    }

    [Fact]
    public async Task MissingConfiguredMediaSourceFailsTaskInsteadOfLookingLikeAnEmptyScan()
    {
        var game = new GameDescriptorDto
        {
            PlayniteId = "missing-media-source-game",
            Name = "Missing Media Source Game",
            Platform = GamePlatformKind.Unknown
        };
        await store.UpsertGamesAsync(new[] { game }, CancellationToken.None);
        options.EnableSteamMedia = false;
        options.EnablePlatformAdjacentMedia = false;
        options.EnableCustomMedia = true;
        var missingSource = Path.Combine(root, "消失的媒体来源", "Captures");
        await store.AddMediaSourceAsync(new MediaSourceRuleDto
        {
            SourceId = "missing-media-source-rule",
            PlayniteId = game.PlayniteId,
            RootPath = missingSource,
            IncludePattern = "*.png",
            SourceKind = MediaSourceKind.Custom,
            Enabled = true,
            SharedDirectory = false
        }, CancellationToken.None);

        var tasks = await CreateService().SyncAsync(new MediaSyncRequestDto
        {
            PlayniteIds = new List<string> { game.PlayniteId },
            IncludeUnassignedInbox = false
        }, CancellationToken.None);

        var task = Assert.Single(tasks);
        Assert.Equal(TaskState.Failed, task.State);
        Assert.Equal("MEDIA_SOURCE_UNAVAILABLE", task.ErrorCode);
        Assert.Contains(missingSource, task.ErrorMessage, StringComparison.Ordinal);
        Assert.Empty(await store.GetMediaAsync(game.PlayniteId, 50, CancellationToken.None));
    }

    [Fact]
    public async Task UnicodeAndLongMediaFileNameIsArchivedWithoutTreatingItAsMissing()
    {
        var game = new GameDescriptorDto
        {
            PlayniteId = "unicode-media-game",
            Name = "Unicode Media Game",
            Platform = GamePlatformKind.Unknown
        };
        await store.UpsertGamesAsync(new[] { game }, CancellationToken.None);
        options.EnableSteamMedia = false;
        options.EnablePlatformAdjacentMedia = false;
        options.EnableCustomMedia = true;
        var sourceRoot = Path.Combine(root, "媒体来源-中文");
        var sourceFile = Path.Combine(sourceRoot, new string('长', 80) + "-截图.png");
        Directory.CreateDirectory(sourceRoot);
        await File.WriteAllBytesAsync(sourceFile, new byte[] { 1, 2, 3, 5, 8, 13 });
        await AddCustomSourceAsync(game.PlayniteId, sourceRoot, "*.png");

        var tasks = await CreateService().SyncAsync(new MediaSyncRequestDto
        {
            PlayniteIds = new List<string> { game.PlayniteId },
            IncludeUnassignedInbox = false
        }, CancellationToken.None);

        var task = Assert.Single(tasks);
        Assert.Equal(TaskState.Succeeded, task.State);
        var media = Assert.Single(await store.GetMediaAsync(game.PlayniteId, 50, CancellationToken.None));
        Assert.Equal(sourceFile, media.OriginalPath);
        Assert.True(File.Exists(media.ArchivePath));
    }

    [Fact]
    public async Task LockedMediaFileFailsWithoutDeletingTheSource()
    {
        var game = new GameDescriptorDto
        {
            PlayniteId = "locked-media-game",
            Name = "Locked Media Game",
            Platform = GamePlatformKind.Unknown
        };
        await store.UpsertGamesAsync(new[] { game }, CancellationToken.None);
        options.EnableSteamMedia = false;
        options.EnablePlatformAdjacentMedia = false;
        options.EnableCustomMedia = true;
        var sourceRoot = Path.Combine(root, "locked-media-source");
        var sourceFile = Path.Combine(sourceRoot, "locked-capture.png");
        Directory.CreateDirectory(sourceRoot);
        await File.WriteAllBytesAsync(sourceFile, new byte[] { 21, 34, 55, 89 });
        await AddCustomSourceAsync(game.PlayniteId, sourceRoot, "*.png");
        using var fileLock = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.None);

        var tasks = await CreateService().SyncAsync(new MediaSyncRequestDto
        {
            PlayniteIds = new List<string> { game.PlayniteId },
            IncludeUnassignedInbox = false
        }, CancellationToken.None);

        var task = Assert.Single(tasks);
        Assert.Equal(TaskState.Failed, task.State);
        Assert.Equal("MEDIA_FILE_UNAVAILABLE", task.ErrorCode);
        Assert.True(File.Exists(sourceFile));
        Assert.Empty(await store.GetMediaAsync(game.PlayniteId, 50, CancellationToken.None));
    }

    [Fact]
    public async Task ClassificationPreviewUsesSourceRuleAndLeavesOverlappingSessionsAmbiguous()
    {
        var captured = DateTime.UtcNow.AddMinutes(-4);
        await store.UpsertGamesAsync(new[]
        {
            new GameDescriptorDto { PlayniteId = "game-1", Name = "Alpha Quest", Platform = GamePlatformKind.Steam },
            new GameDescriptorDto { PlayniteId = "game-2", Name = "Beta Quest", Platform = GamePlatformKind.Steam }
        }, CancellationToken.None);
        var sourceRoot = Path.Combine(root, "Captures");
        await store.AddMediaSourceAsync(new MediaSourceRuleDto
        {
            SourceId = "alpha-source", PlayniteId = "game-1", RootPath = sourceRoot,
            IncludePattern = "*.png", SourceKind = MediaSourceKind.Custom
        }, CancellationToken.None);
        var sourceMedia = await AddInboxMediaAsync("source-media", Path.Combine(sourceRoot, "alpha.png"), captured.AddMinutes(-20));
        var sharedMedia = await AddInboxMediaAsync("shared-media", Path.Combine(root, "Shared", "unknown.png"), captured);
        var mappedCapture = captured.AddHours(1);
        var mappedMedia = await AddInboxMediaAsync("mapped-media", Path.Combine(root, "Shared", "mapped.png"), mappedCapture);
        var unknownMedia = await AddInboxMediaAsync("unknown-media", Path.Combine(root, "Shared", "no-time.png"), default);
        await store.AddSessionAsync(new GameSessionEventDto
        {
            SessionId = "alpha-session", PlayniteId = "game-1", StartedUtc = captured.AddMinutes(-2),
            StoppedUtc = captured.AddMinutes(2), Source = SessionSourceKind.Playnite
        }, CancellationToken.None);
        await store.AddSessionAsync(new GameSessionEventDto
        {
            SessionId = "beta-session", PlayniteId = "game-2", StartedUtc = captured.AddMinutes(-2),
            StoppedUtc = captured.AddMinutes(2), Source = SessionSourceKind.Playnite
        }, CancellationToken.None);
        await store.AddSessionAsync(new GameSessionEventDto
        {
            SessionId = "mapped-session", PlayniteId = "game-2", StartedUtc = mappedCapture.AddMinutes(-2),
            StoppedUtc = mappedCapture.AddMinutes(2), ProcessName = "beta-game.exe", Source = SessionSourceKind.ProcessDetection
        }, CancellationToken.None);
        await store.UpsertProcessMappingAsync(new ProcessMappingDto
        {
            ExecutableName = "beta-game.exe", PlayniteId = "game-2", GameName = "Beta Quest"
        }, CancellationToken.None);

        var preview = await CreateService().CreateClassificationPreviewAsync(new MediaClassificationPreviewRequestDto
        {
            MediaIds = new List<string> { sourceMedia.MediaId, sharedMedia.MediaId, mappedMedia.MediaId, unknownMedia.MediaId }
        }, CancellationToken.None);

        var sourceSuggestion = Assert.Single(preview.Items, x => x.MediaId == sourceMedia.MediaId);
        Assert.Equal("game-1", sourceSuggestion.SuggestedPlayniteId);
        Assert.Equal("High", sourceSuggestion.Confidence);
        Assert.Contains("媒体来源规则", sourceSuggestion.Reason);
        var sharedSuggestion = Assert.Single(preview.Items, x => x.MediaId == sharedMedia.MediaId);
        Assert.Equal("Low", sharedSuggestion.Confidence);
        Assert.Empty(sharedSuggestion.SuggestedPlayniteId);
        Assert.Contains("多个候选", sharedSuggestion.Reason);
        var mappedSuggestion = Assert.Single(preview.Items, x => x.MediaId == mappedMedia.MediaId);
        Assert.Equal("game-2", mappedSuggestion.SuggestedPlayniteId);
        Assert.Equal("High", mappedSuggestion.Confidence);
        Assert.Contains("进程映射", mappedSuggestion.Reason);
        var unknownSuggestion = Assert.Single(preview.Items, x => x.MediaId == unknownMedia.MediaId);
        Assert.Equal("Low", unknownSuggestion.Confidence);
        Assert.Contains("时间未知", unknownSuggestion.Reason);
    }

    [Fact]
    public async Task ClassificationApplyAndUndoMovesOnlyArchiveCopyAndRestoresInboxState()
    {
        var captured = new DateTime(2026, 9, 5, 10, 20, 30, DateTimeKind.Utc);
        var game = new GameDescriptorDto { PlayniteId = "game-1", Name = "Alpha Quest", Platform = GamePlatformKind.Steam };
        await store.UpsertGamesAsync(new[] { game }, CancellationToken.None);
        var sourceRoot = Path.Combine(root, "Captures");
        var originalPath = Path.Combine(sourceRoot, "capture.png");
        var inboxPath = Path.Combine(options.MediaArchiveDirectory, "_Inbox", "Pending", "capture.png");
        var content = new byte[] { 8, 5, 3, 2, 1 };
        Directory.CreateDirectory(Path.GetDirectoryName(originalPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(inboxPath)!);
        await File.WriteAllBytesAsync(originalPath, content);
        await File.WriteAllBytesAsync(inboxPath, content);
        var hash = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
        var media = new MediaItemDto
        {
            MediaId = "apply-media", Kind = MediaKind.Screenshot, Source = MediaSourceKind.Custom,
            ArchivePath = inboxPath, OriginalPath = originalPath, CapturedUtc = captured, SizeBytes = content.Length,
            Sha256 = hash, ClassificationState = "Inbox", ClassificationReason = "待归类", CloudState = "NotApplicable"
        };
        await store.AddMediaAsync(media, CancellationToken.None);
        await store.AddMediaSourceAsync(new MediaSourceRuleDto
        {
            SourceId = "alpha-source", PlayniteId = game.PlayniteId, RootPath = sourceRoot,
            IncludePattern = "*.png", SourceKind = MediaSourceKind.Custom
        }, CancellationToken.None);
        var service = CreateService();
        var preview = await service.CreateClassificationPreviewAsync(new MediaClassificationPreviewRequestDto
        {
            MediaIds = new List<string> { media.MediaId }
        }, CancellationToken.None);
        Assert.True(preview.Items[0].CanApply);

        var applied = await service.ApplyClassificationPreviewAsync(new MediaClassificationApplyRequestDto
        {
            BatchId = preview.BatchId, HighConfidenceOnly = true
        }, CancellationToken.None);

        Assert.Equal("Applied", applied.State);
        Assert.Equal(1, applied.AppliedCount);
        var assigned = await store.GetMediaByIdAsync(media.MediaId, CancellationToken.None);
        Assert.Equal("Assigned", assigned!.ClassificationState);
        Assert.Equal(game.PlayniteId, assigned.PlayniteId);
        Assert.True(File.Exists(assigned.ArchivePath));
        Assert.True(File.Exists(originalPath));
        Assert.False(File.Exists(inboxPath));

        var restartedStore = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restartedStore.InitializeAsync(CancellationToken.None);
        var undone = await CreateService(restartedStore).UndoClassificationBatchAsync(new MediaClassificationUndoRequestDto
        {
            BatchId = preview.BatchId
        }, CancellationToken.None);

        Assert.Equal("Undone", undone.State);
        Assert.Equal(1, undone.UndoneCount);
        var restored = await store.GetMediaByIdAsync(media.MediaId, CancellationToken.None);
        Assert.Equal("Inbox", restored!.ClassificationState);
        Assert.Equal(string.Empty, restored.PlayniteId);
        Assert.Equal(inboxPath, restored.ArchivePath);
        Assert.True(File.Exists(inboxPath));
        Assert.False(File.Exists(assigned.ArchivePath));
        Assert.True(File.Exists(originalPath));
    }

    [Fact]
    public async Task ClassificationApplyLeavesChangedItemAndArchiveUntouched()
    {
        await store.UpsertGamesAsync(new[]
        {
            new GameDescriptorDto { PlayniteId = "game-1", Name = "Alpha Quest", Platform = GamePlatformKind.Steam }
        }, CancellationToken.None);
        var sourceRoot = Path.Combine(root, "Captures");
        var media = await AddInboxMediaAsync("changed-media", Path.Combine(sourceRoot, "capture.png"), DateTime.UtcNow.AddMinutes(-1));
        await store.AddMediaSourceAsync(new MediaSourceRuleDto
        {
            SourceId = "alpha-source", PlayniteId = "game-1", RootPath = sourceRoot,
            IncludePattern = "*.png", SourceKind = MediaSourceKind.Custom
        }, CancellationToken.None);
        var preview = await CreateService().CreateClassificationPreviewAsync(new MediaClassificationPreviewRequestDto
        {
            MediaIds = new List<string> { media.MediaId }
        }, CancellationToken.None);
        await store.UpdateMediaMetadataAsync(new MediaMetadataUpdateDto
        {
            MediaId = media.MediaId, IsFavorite = true, Comment = "用户刚刚补充的备注"
        }, CancellationToken.None);

        var result = await CreateService().ApplyClassificationPreviewAsync(new MediaClassificationApplyRequestDto
        {
            BatchId = preview.BatchId
        }, CancellationToken.None);

        Assert.Equal("Conflict", result.State);
        Assert.Equal(1, result.ConflictCount);
        var current = await store.GetMediaByIdAsync(media.MediaId, CancellationToken.None);
        Assert.Equal("Inbox", current!.ClassificationState);
        Assert.True(current.IsFavorite);
        Assert.Equal("用户刚刚补充的备注", current.Comment);
        Assert.True(File.Exists(media.ArchivePath));
    }

    [Fact]
    public async Task ClassificationCommitFailureLeavesRecoveryLedgerAndStartupRestoresArchiveCopy()
    {
        var prepared = await PrepareClassificationAsync("commit-failure-media");
        var service = CreateService();
        var preview = await service.CreateClassificationPreviewAsync(new MediaClassificationPreviewRequestDto
        {
            MediaIds = new List<string> { prepared.Media.MediaId }
        }, CancellationToken.None);
        await ExecuteSqlAsync(@"CREATE TRIGGER fail_classification_batch_item_update
BEFORE UPDATE OF item_state ON media_classification_batch_items
BEGIN SELECT RAISE(ABORT, 'injected batch item failure'); END;");

        var result = await service.ApplyClassificationPreviewAsync(new MediaClassificationApplyRequestDto
        {
            BatchId = preview.BatchId
        }, CancellationToken.None);

        Assert.Equal("Conflict", result.State);
        Assert.Equal(1, result.ConflictCount);
        Assert.True(File.Exists(prepared.AppliedPath));
        Assert.False(File.Exists(prepared.InboxPath));
        var beforeRecovery = await store.GetMediaByIdAsync(prepared.Media.MediaId, CancellationToken.None);
        Assert.Equal("Inbox", beforeRecovery!.ClassificationState);
        Assert.Equal(prepared.InboxPath, beforeRecovery.ArchivePath);
        Assert.NotEmpty(await store.GetPendingMediaClassificationOperationsAsync(CancellationToken.None));

        await ExecuteSqlAsync("DROP TRIGGER fail_classification_batch_item_update;");
        var recovery = await CreateService().RecoverPendingClassificationOperationsAsync(CancellationToken.None);

        Assert.Equal(1, recovery.RecoveredCount);
        Assert.Equal(0, recovery.RecoveryRequiredCount);
        Assert.True(File.Exists(prepared.InboxPath));
        Assert.False(File.Exists(prepared.AppliedPath));
        Assert.Empty(await store.GetPendingMediaClassificationOperationsAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ClassificationHistoryIsDurablePagedAndKeepsConflictCounts()
    {
        var now = DateTime.UtcNow;
        for (var index = 1; index <= 3; index++)
        {
            var batchId = $"history-batch-{index}";
            await store.CreateMediaClassificationBatchAsync(batchId, now.AddMinutes(index), now.AddHours(1),
                new[]
                {
                    new MediaClassificationBatchItemRecord
                    {
                        BatchId = batchId, MediaId = $"history-media-{index}", OriginalClassificationState = "Inbox",
                        OriginalClassificationReason = "待归类", OriginalArchivePath = $"archive-{index}",
                        OriginalPath = $"original-{index}", OriginalCapturedUtc = now, OriginalSha256 = $"hash-{index}",
                        TargetPlayniteId = "game-1", TargetReason = "来源规则", Confidence = "High"
                    }
                }, CancellationToken.None);
        }

        await store.UpdateMediaClassificationBatchItemAsync("history-batch-1", "history-media-1", "Applied", "applied-1", CancellationToken.None);
        await store.UpdateMediaClassificationBatchStateAsync("history-batch-1", "Applied", string.Empty, CancellationToken.None);
        await store.UpdateMediaClassificationBatchItemAsync("history-batch-2", "history-media-2", "Conflict", string.Empty, CancellationToken.None);
        await store.UpdateMediaClassificationBatchStateAsync("history-batch-2", "Conflict", "媒体状态已变化", CancellationToken.None);

        var restartedStore = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restartedStore.InitializeAsync(CancellationToken.None);
        var service = CreateService(restartedStore);
        var first = await service.GetClassificationHistoryAsync(new MediaClassificationHistoryRequestDto
        {
            Page = 0, PageSize = 2
        }, CancellationToken.None);
        var second = await service.GetClassificationHistoryAsync(new MediaClassificationHistoryRequestDto
        {
            Page = 1, PageSize = 2
        }, CancellationToken.None);
        var conflicts = await service.GetClassificationHistoryAsync(new MediaClassificationHistoryRequestDto
        {
            State = "Conflict", PageSize = 10
        }, CancellationToken.None);
        var appliedOnly = await service.GetClassificationHistoryAsync(new MediaClassificationHistoryRequestDto
        {
            State = "Applied", PageSize = 10
        }, CancellationToken.None);

        Assert.Equal(3, first.TotalCount);
        Assert.Equal(2, first.Items.Count);
        Assert.True(first.HasMore);
        Assert.Single(second.Items);
        Assert.False(second.HasMore);
        var applied = Assert.Single(appliedOnly.Items);
        Assert.Equal("history-batch-1", applied.BatchId);
        Assert.Equal(1, applied.AppliedCount);
        Assert.True(applied.IsUndoable);
        var conflict = Assert.Single(conflicts.Items);
        Assert.Equal(1, conflict.ConflictCount);
        Assert.Equal("媒体状态已变化", conflict.LastError);
    }

    [Fact]
    public async Task ClassificationHistoryRejectsStaleConsistencyTokenAfterNewBatch()
    {
        var now = DateTime.UtcNow;
        await store.CreateMediaClassificationBatchAsync("history-page-1", now, now.AddHours(1),
            new[]
            {
                new MediaClassificationBatchItemRecord
                {
                    BatchId = "history-page-1", MediaId = "history-page-media-1", OriginalClassificationState = "Inbox",
                    OriginalClassificationReason = "待归类", OriginalArchivePath = "archive-1",
                    OriginalPath = "original-1", OriginalCapturedUtc = now, OriginalSha256 = "hash-page-1",
                    TargetPlayniteId = "game-1", TargetReason = "来源规则", Confidence = "High"
                }
            }, CancellationToken.None);

        var service = CreateService();
        var firstPage = await service.GetClassificationHistoryAsync(new MediaClassificationHistoryRequestDto
        {
            Page = 0,
            PageSize = 1
        }, CancellationToken.None);

        await store.CreateMediaClassificationBatchAsync("history-page-2", now.AddMinutes(1), now.AddHours(1),
            new[]
            {
                new MediaClassificationBatchItemRecord
                {
                    BatchId = "history-page-2", MediaId = "history-page-media-2", OriginalClassificationState = "Inbox",
                    OriginalClassificationReason = "待归类", OriginalArchivePath = "archive-2",
                    OriginalPath = "original-2", OriginalCapturedUtc = now, OriginalSha256 = "hash-page-2",
                    TargetPlayniteId = "game-1", TargetReason = "来源规则", Confidence = "High"
                }
            }, CancellationToken.None);

        var stalePage = await service.GetClassificationHistoryAsync(new MediaClassificationHistoryRequestDto
        {
            Page = 1,
            PageSize = 1,
            ConsistencyToken = firstPage.ConsistencyToken
        }, CancellationToken.None);

        Assert.True(stalePage.PageResetRequired);
        Assert.Empty(stalePage.Items);
        Assert.False(stalePage.HasMore);
        Assert.NotEqual(firstPage.ConsistencyToken, stalePage.ConsistencyToken);
        Assert.Contains("刷新", stalePage.PageResetReason);
    }

    [Fact]
    public async Task ClassificationAuditFailureDoesNotRollbackCommittedBusinessState()
    {
        var prepared = await PrepareClassificationAsync("audit-failure-media");
        var service = CreateService();
        var preview = await service.CreateClassificationPreviewAsync(new MediaClassificationPreviewRequestDto
        {
            MediaIds = new List<string> { prepared.Media.MediaId }
        }, CancellationToken.None);
        await ExecuteSqlAsync(@"CREATE TRIGGER fail_classification_audit_insert
BEFORE INSERT ON audit_log
BEGIN SELECT RAISE(ABORT, 'injected audit failure'); END;");

        var result = await service.ApplyClassificationPreviewAsync(new MediaClassificationApplyRequestDto
        {
            BatchId = preview.BatchId
        }, CancellationToken.None);

        Assert.Equal("Applied", result.State);
        Assert.Equal(1, result.AppliedCount);
        Assert.Contains("审计记录写入失败", result.Items.Single().Message);
        var applied = await store.GetMediaByIdAsync(prepared.Media.MediaId, CancellationToken.None);
        Assert.Equal("Assigned", applied!.ClassificationState);
        Assert.True(File.Exists(prepared.AppliedPath));
        Assert.False(File.Exists(prepared.InboxPath));
        Assert.Empty(await store.GetPendingMediaClassificationOperationsAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ClassificationCancellationRestoresArchiveCopyWithIndependentRecoveryToken()
    {
        var prepared = await PrepareClassificationAsync("cancelled-media");
        var canceled = new CancellationTokenSource();
        var service = CreateService();
        service.ClassificationOperationStageHook = stage =>
        {
            if (stage == MediaClassificationOperationStage.AfterMove) canceled.Cancel();
            return Task.CompletedTask;
        };
        var preview = await service.CreateClassificationPreviewAsync(new MediaClassificationPreviewRequestDto
        {
            MediaIds = new List<string> { prepared.Media.MediaId }
        }, CancellationToken.None);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ApplyClassificationPreviewAsync(
            new MediaClassificationApplyRequestDto { BatchId = preview.BatchId }, canceled.Token));

        var current = await store.GetMediaByIdAsync(prepared.Media.MediaId, CancellationToken.None);
        Assert.Equal("Inbox", current!.ClassificationState);
        Assert.Equal(prepared.InboxPath, current.ArchivePath);
        Assert.True(File.Exists(prepared.InboxPath));
        Assert.False(File.Exists(prepared.AppliedPath));
        Assert.Empty(await store.GetPendingMediaClassificationOperationsAsync(CancellationToken.None));
    }

    private async Task<PreparedClassification> PrepareClassificationAsync(string mediaId)
    {
        var captured = new DateTime(2026, 9, 5, 10, 20, 30, DateTimeKind.Utc);
        var game = new GameDescriptorDto { PlayniteId = "game-1", Name = "Alpha Quest", Platform = GamePlatformKind.Steam };
        await store.UpsertGamesAsync(new[] { game }, CancellationToken.None);
        var sourceRoot = Path.Combine(root, "Captures", mediaId);
        var originalPath = Path.Combine(sourceRoot, "capture.png");
        var inboxPath = Path.Combine(options.MediaArchiveDirectory, "_Inbox", "Pending", mediaId + ".png");
        var appliedPath = Path.Combine(options.MediaArchiveDirectory, game.Name, "Screenshots", "2026", "09",
            $"2026-09-05_10-20-30_Custom_{mediaId[..8]}.png");
        var content = new byte[] { 8, 5, 3, 2, 1, (byte)mediaId.Length };
        Directory.CreateDirectory(Path.GetDirectoryName(originalPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(inboxPath)!);
        await File.WriteAllBytesAsync(originalPath, content);
        await File.WriteAllBytesAsync(inboxPath, content);
        var hash = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
        var media = new MediaItemDto
        {
            MediaId = mediaId, Kind = MediaKind.Screenshot, Source = MediaSourceKind.Custom,
            ArchivePath = inboxPath, OriginalPath = originalPath, CapturedUtc = captured, SizeBytes = content.Length,
            Sha256 = hash, ClassificationState = "Inbox", ClassificationReason = "待归类", CloudState = "NotApplicable"
        };
        await store.AddMediaAsync(media, CancellationToken.None);
        await store.AddMediaSourceAsync(new MediaSourceRuleDto
        {
            SourceId = mediaId + "-source", PlayniteId = game.PlayniteId, RootPath = sourceRoot,
            IncludePattern = "*.png", SourceKind = MediaSourceKind.Custom
        }, CancellationToken.None);
        // BuildArchivePath uses the first eight characters of the content hash, not the media ID.
        appliedPath = Path.Combine(options.MediaArchiveDirectory, game.Name, "Screenshots", "2026", "09",
            $"2026-09-05_10-20-30_Custom_{hash[..8]}.png");
        return new PreparedClassification(media, inboxPath, appliedPath);
    }

    private async Task ExecuteSqlAsync(string sql)
    {
        await using var connection = new SqliteConnection($"Data Source={options.DatabasePath};Cache=Shared;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private sealed record PreparedClassification(MediaItemDto Media, string InboxPath, string AppliedPath);

    private async Task<MediaItemDto> AddInboxMediaAsync(string mediaId, string originalPath, DateTime capturedUtc)
    {
        var content = new byte[] { 1, 4, 7, (byte)mediaId.Length, (byte)mediaId[0], (byte)mediaId[1] };
        var archivePath = Path.Combine(options.MediaArchiveDirectory, "_Inbox", "Pending", mediaId + ".png");
        Directory.CreateDirectory(Path.GetDirectoryName(originalPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(archivePath)!);
        await File.WriteAllBytesAsync(originalPath, content);
        await File.WriteAllBytesAsync(archivePath, content);
        var item = new MediaItemDto
        {
            MediaId = mediaId, Kind = MediaKind.Screenshot, Source = MediaSourceKind.Custom,
            ArchivePath = archivePath, OriginalPath = originalPath, CapturedUtc = capturedUtc,
            SizeBytes = content.Length, Sha256 = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant(),
            ClassificationState = "Inbox", ClassificationReason = "待归类", CloudState = "NotApplicable"
        };
        await store.AddMediaAsync(item, CancellationToken.None);
        return item;
    }

    private MediaSyncService CreateService(SqliteStateStore? stateStore = null)
    {
        stateStore ??= store;
        var runner=new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance);
        var ludusavi=new LudusaviClient(options,runner,NullLogger<LudusaviClient>.Instance);
        var catalog=new GameCatalogService(stateStore,ludusavi,NullLogger<GameCatalogService>.Instance);
        var tasks=new TaskCoordinator(stateStore,new TaskEventBroadcaster(),NullLogger<TaskCoordinator>.Instance);
        return new MediaSyncService(options,catalog,stateStore,new RcloneClient(options,runner),
            new CloudTransferCoordinator(NullLogger<CloudTransferCoordinator>.Instance),tasks,new GameOperationLock(),
            NullLogger<MediaSyncService>.Instance);
    }

    private Task AddCustomSourceAsync(string playniteId, string rootPath, string includePattern)
        => store.AddMediaSourceAsync(new MediaSourceRuleDto
        {
            SourceId = Guid.NewGuid().ToString("N"),
            PlayniteId = playniteId,
            RootPath = rootPath,
            IncludePattern = includePattern,
            SourceKind = MediaSourceKind.Custom,
            Enabled = true,
            SharedDirectory = false
        }, CancellationToken.None);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if(Directory.Exists(root))Directory.Delete(root,true);
    }
}
