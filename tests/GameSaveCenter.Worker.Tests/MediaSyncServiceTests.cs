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

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if(Directory.Exists(root))Directory.Delete(root,true);
    }
}
