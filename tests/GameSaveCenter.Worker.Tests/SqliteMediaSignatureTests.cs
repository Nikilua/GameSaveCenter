using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class SqliteMediaSignatureTests : IDisposable
{
    private readonly string root=Path.Combine(Path.GetTempPath(),"GameSaveCenter.Tests",Guid.NewGuid().ToString("N"));
    private readonly SqliteStateStore store;

    public SqliteMediaSignatureTests()
    {
        store=new SqliteStateStore(new WorkerOptions
        {
            DataDirectory=root,
            LudusaviBackupDirectory=Path.Combine(root,"Saves"),
            MediaArchiveDirectory=Path.Combine(root,"Media")
        },NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task SignatureRoundTripsAndMatchesUnchangedFileTimes()
    {
        var path=Path.Combine(root,"capture.png");
        await File.WriteAllBytesAsync(path,new byte[]{1,2,3});

        var first=new FileInfo(path);
        await store.UpsertMediaFileSignatureAsync(path,first.Length,first.LastWriteTimeUtc,"hash-a","sample-a",CancellationToken.None);

        var loaded=await store.TryGetMediaFileSignatureAsync(path,CancellationToken.None);
        Assert.NotNull(loaded);
        Assert.Equal("hash-a",loaded.Sha256);
        Assert.Equal("sample-a",loaded.SampleHash);
        Assert.Equal(first.Length,loaded.Length);

        // ArchiveCandidateAsync reuses the hash only when size and write time still match.
        var fresh=new FileInfo(path);
        Assert.Equal(loaded.Length,fresh.Length);
        Assert.Equal(loaded.LastWriteTimeUtc,fresh.LastWriteTimeUtc);
    }

    [Fact]
    public async Task SignatureMissReturnsNullForUnknownPath()
    {
        Assert.Null(await store.TryGetMediaFileSignatureAsync(Path.Combine(root,"missing.png"),CancellationToken.None));
    }

    [Fact]
    public async Task ChangedFileProducesUpdatedSignatureOnConflict()
    {
        var path=Path.Combine(root,"clip.mp4");
        await File.WriteAllBytesAsync(path,new byte[]{1});
        var version1=new FileInfo(path);
        await store.UpsertMediaFileSignatureAsync(path,version1.Length,version1.LastWriteTimeUtc,"hash-1",CancellationToken.None);

        await File.WriteAllBytesAsync(path,new byte[]{1,2,3,4});
        var version2=new FileInfo(path);
        await store.UpsertMediaFileSignatureAsync(path,version2.Length,version2.LastWriteTimeUtc,"hash-2",CancellationToken.None);

        var loaded=await store.TryGetMediaFileSignatureAsync(path,CancellationToken.None);
        Assert.NotNull(loaded);
        Assert.Equal("hash-2",loaded.Sha256);
        Assert.Equal(version2.Length,loaded.Length);
    }

    [Fact]
    public async Task PruneRemovesOldRowsAndCapsTheRemainingCache()
    {
        for (var index = 0; index < 3; index++)
        {
            var path = Path.Combine(root, $"capture-{index}.png");
            await File.WriteAllBytesAsync(path, new byte[] { (byte)index });
            var file = new FileInfo(path);
            await store.UpsertMediaFileSignatureAsync(path, file.Length, file.LastWriteTimeUtc, $"hash-{index}", $"sample-{index}", CancellationToken.None);
        }

        await store.PruneMediaFileSignaturesAsync(DateTime.UtcNow.AddDays(-30), 2, CancellationToken.None);

        await using var connection = new SqliteConnection($"Data Source={Path.Combine(root, "gamesavecenter.db")}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM media_file_signatures;";
        Assert.Equal(2L, (long)(await command.ExecuteScalarAsync() ?? 0L));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if(Directory.Exists(root))Directory.Delete(root,true);
    }
}
