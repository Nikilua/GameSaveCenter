using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class WorklistReferenceResolutionTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly SqliteStateStore store;

    public WorklistReferenceResolutionTests()
    {
        store = new SqliteStateStore(new WorkerOptions
        {
            DataDirectory = root,
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        }, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task LookupUsesExactStableKeysAndReportsResolvedFindingsSeparately()
    {
        const string gameId = "game-1";
        const string backupId = "backup-1";
        const string mediaId = "media-1";
        const string taskId = "task-1";
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            BackupId = backupId,
            PlayniteId = gameId,
            LudusaviName = "Test Game",
            CreatedUtc = DateTime.UtcNow
        }, "{}", CancellationToken.None);
        await store.AddMediaAsync(new MediaItemDto
        {
            MediaId = mediaId,
            PlayniteId = gameId,
            CapturedUtc = DateTime.UtcNow,
            Sha256 = Guid.NewGuid().ToString("N"),
            ArchivePath = "ignored-by-lookup",
            OriginalPath = "ignored-by-lookup"
        }, CancellationToken.None);
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = taskId,
            TaskType = "Backup",
            GameId = gameId,
            GameName = "Test Game",
            CreatedUtc = DateTime.UtcNow
        }, CancellationToken.None);
        await store.AddFindingAsync(gameId, new ValidationFindingDto
        {
            Code = "TEST",
            Title = "Test finding"
        }, CancellationToken.None);
        var findingId = Assert.Single(await store.GetOpenFindingsAsync(10, CancellationToken.None)).FindingId;

        var result = await store.ResolveWorklistReferencesAsync(new WorklistReferenceQueryDto
        {
            References = new List<WorklistReferenceQueryItemDto>
            {
                Reference(1, BackupKey(gameId, backupId)),
                Reference(2, mediaId),
                Reference(3, taskId),
                Reference(4, findingId),
                Reference(1, BackupKey(gameId, "missing")),
                Reference(2, "missing"),
                Reference(99, "unsupported"),
                Reference(1, "v1|2:ab|2:cd")
            }
        }, CancellationToken.None);

        Assert.Collection(result.References,
            item => Assert.Equal(WorklistReferenceState.Indexed, item.State),
            item => Assert.Equal(WorklistReferenceState.Indexed, item.State),
            item => Assert.Equal(WorklistReferenceState.Indexed, item.State),
            item => Assert.Equal(WorklistReferenceState.Indexed, item.State),
            item => Assert.Equal(WorklistReferenceState.NotFound, item.State),
            item => Assert.Equal(WorklistReferenceState.NotFound, item.State),
            item => Assert.Equal(WorklistReferenceState.InvalidReference, item.State),
            item => Assert.Equal(WorklistReferenceState.NotFound, item.State));

        await using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={Path.Combine(root, "gamesavecenter.db")};Cache=Shared"))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "UPDATE findings SET resolved=1 WHERE finding_id=$id;";
            command.Parameters.AddWithValue("$id", findingId);
            Assert.Equal(1, await command.ExecuteNonQueryAsync());
        }

        var resolved = await store.ResolveWorklistReferencesAsync(new WorklistReferenceQueryDto
        {
            References = new List<WorklistReferenceQueryItemDto> { Reference(4, findingId) }
        }, CancellationToken.None);

        Assert.Equal(WorklistReferenceState.Resolved, Assert.Single(resolved.References).State);
    }

    [Fact]
    public async Task LookupRejectsMoreThanTheBoundedNumberOfReferences()
    {
        var request = new WorklistReferenceQueryDto();
        request.References.AddRange(Enumerable.Range(0, WorklistReferenceQueryDto.MaximumReferences + 1)
            .Select(index => Reference(2, "media-" + index)));

        await Assert.ThrowsAsync<InvalidOperationException>(() => store.ResolveWorklistReferencesAsync(request, CancellationToken.None));
    }

    private static WorklistReferenceQueryItemDto Reference(int kind, string id)
        => new WorklistReferenceQueryItemDto { Kind = kind, StableObjectId = id };

    private static string BackupKey(string gameId, string backupId)
        => $"v1|{gameId.Length}:{gameId}|{backupId.Length}:{backupId}";

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
