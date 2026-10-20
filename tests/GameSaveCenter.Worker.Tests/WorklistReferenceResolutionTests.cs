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
            item =>
            {
                Assert.Equal(WorklistReferenceState.Indexed, item.State);
                Assert.Equal(gameId, item.PlayniteId);
            },
            item =>
            {
                Assert.Equal(WorklistReferenceState.Indexed, item.State);
                Assert.Equal(gameId, item.PlayniteId);
            },
            item => Assert.Equal(WorklistReferenceState.Indexed, item.State),
            item =>
            {
                Assert.Equal(WorklistReferenceState.Indexed, item.State);
                Assert.Equal(gameId, item.PlayniteId);
            },
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
        var exactFinding = await store.GetValidationFindingAsync(
            new ValidationFindingQueryDto { FindingId = findingId }, CancellationToken.None);
        Assert.NotNull(exactFinding);
        Assert.Equal(findingId, exactFinding!.FindingId);
        Assert.Equal(gameId, exactFinding.PlayniteId);
        Assert.Equal("Test finding", exactFinding.Title);
        Assert.Null(await store.GetValidationFindingAsync(
            new ValidationFindingQueryDto { FindingId = "different-id" }, CancellationToken.None));
    }

    [Fact]
    public async Task CurrentSummariesFollowRenamesAndOwnershipWithoutReturningMediaPaths()
    {
        const string gameId = "summary-game-1";
        const string ownerGameId = "summary-game-2";
        const string backupId = "summary-backup-1";
        const string mediaId = "summary-media-1";
        const string taskId = "summary-task-1";
        await store.UpsertGamesAsync(new[]
        {
            new GameDescriptorDto { PlayniteId = gameId, Name = "Old Game Name" },
            new GameDescriptorDto { PlayniteId = ownerGameId, Name = "Old Owner Name" }
        }, CancellationToken.None);
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            BackupId = backupId,
            PlayniteId = gameId,
            LudusaviName = "Old Indexed Name",
            CreatedUtc = DateTime.UtcNow
        }, "{}", CancellationToken.None);
        await store.AddMediaAsync(new MediaItemDto
        {
            MediaId = mediaId,
            PlayniteId = gameId,
            CapturedUtc = DateTime.UtcNow,
            Kind = MediaKind.Screenshot,
            Sha256 = Guid.NewGuid().ToString("N"),
            ArchivePath = Path.Combine(root, "private", "old-shot.png"),
            OriginalPath = Path.Combine(root, "source", "secret-shot.png")
        }, CancellationToken.None);
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = taskId,
            TaskType = "Backup",
            GameId = gameId,
            GameName = "Old Game Name",
            State = TaskState.Failed,
            CreatedUtc = DateTime.UtcNow
        }, CancellationToken.None);
        await store.AddFindingAsync(gameId, new ValidationFindingDto
        {
            Code = "SUMMARY_CHECK",
            Title = "Old finding title"
        }, CancellationToken.None);
        var findingId = Assert.Single(await store.GetOpenFindingsAsync(10, CancellationToken.None)).FindingId;
        var request = new WorklistReferenceQueryDto
        {
            References = new List<WorklistReferenceQueryItemDto>
            {
                Reference(1, BackupKey(gameId, backupId)),
                Reference(2, mediaId),
                Reference(3, taskId),
                Reference(4, findingId)
            }
        };

        var beforeRename = await store.ResolveWorklistReferencesAsync(request, CancellationToken.None);
        var oldBackup = beforeRename.References[0];
        var oldMedia = beforeRename.References[1];
        var oldTask = beforeRename.References[2];
        var oldFinding = beforeRename.References[3];
        Assert.Contains("Old Game Name", oldBackup.CurrentSummary);
        Assert.Equal("Old Game Name", oldBackup.ParentSummary);
        Assert.Contains("old-shot.png", oldMedia.CurrentSummary);
        Assert.DoesNotContain(root, oldMedia.CurrentSummary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private", oldMedia.CurrentSummary, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Old Game Name", oldMedia.ParentSummary);
        Assert.Contains("Backup", oldTask.CurrentSummary);
        Assert.Contains("失败", oldTask.CurrentSummary);
        Assert.Equal("Old Game Name", oldTask.ParentSummary);
        Assert.Contains("Old finding title", oldFinding.CurrentSummary);
        Assert.Equal("Old Game Name", oldFinding.ParentSummary);

        await store.UpsertGamesAsync(new[]
        {
            new GameDescriptorDto { PlayniteId = gameId, Name = "Renamed Game" },
            new GameDescriptorDto { PlayniteId = ownerGameId, Name = "New Owner Name" }
        }, CancellationToken.None);
        await using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={Path.Combine(root, "gamesavecenter.db")};Cache=Shared"))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE media SET playnite_id=$owner,archive_path=$path WHERE media_id=$media;
UPDATE tasks SET game_id=$owner WHERE task_id=$task;
UPDATE findings SET playnite_id=$owner WHERE finding_id=$finding;";
            command.Parameters.AddWithValue("$owner", ownerGameId);
            command.Parameters.AddWithValue("$path", Path.Combine(root, "private", "renamed-shot.png"));
            command.Parameters.AddWithValue("$media", mediaId);
            command.Parameters.AddWithValue("$task", taskId);
            command.Parameters.AddWithValue("$finding", findingId);
            await command.ExecuteNonQueryAsync();
        }

        var afterRename = await store.ResolveWorklistReferencesAsync(request, CancellationToken.None);
        Assert.Contains("Renamed Game", afterRename.References[0].CurrentSummary);
        Assert.Equal("Renamed Game", afterRename.References[0].ParentSummary);
        Assert.Contains("renamed-shot.png", afterRename.References[1].CurrentSummary);
        Assert.Equal(ownerGameId, afterRename.References[1].PlayniteId);
        Assert.Equal("New Owner Name", afterRename.References[1].ParentSummary);
        Assert.Equal(ownerGameId, afterRename.References[2].PlayniteId);
        Assert.Equal("New Owner Name", afterRename.References[2].ParentSummary);
        Assert.Equal(ownerGameId, afterRename.References[3].PlayniteId);
        Assert.Equal("New Owner Name", afterRename.References[3].ParentSummary);

        await store.DeleteBackupVersionAsync(gameId, backupId, CancellationToken.None);
        var afterDelete = await store.ResolveWorklistReferencesAsync(request, CancellationToken.None);
        Assert.Equal(WorklistReferenceState.NotFound, afterDelete.References[0].State);
        Assert.Empty(afterDelete.References[0].CurrentSummary);
        Assert.Equal(WorklistReferenceState.Indexed, afterDelete.References[1].State);
        Assert.Equal(WorklistReferenceState.Indexed, afterDelete.References[2].State);
        Assert.Equal(WorklistReferenceState.Indexed, afterDelete.References[3].State);
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
