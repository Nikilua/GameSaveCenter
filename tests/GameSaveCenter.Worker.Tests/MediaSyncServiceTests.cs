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

    private MediaSyncService CreateService()
    {
        var runner=new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance);
        var ludusavi=new LudusaviClient(options,runner,NullLogger<LudusaviClient>.Instance);
        var catalog=new GameCatalogService(store,ludusavi,NullLogger<GameCatalogService>.Instance);
        var tasks=new TaskCoordinator(store,new TaskEventBroadcaster(),NullLogger<TaskCoordinator>.Instance);
        return new MediaSyncService(options,catalog,store,new RcloneClient(options,runner),
            new CloudTransferCoordinator(NullLogger<CloudTransferCoordinator>.Instance),tasks,new GameOperationLock(),
            NullLogger<MediaSyncService>.Instance);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if(Directory.Exists(root))Directory.Delete(root,true);
    }
}
