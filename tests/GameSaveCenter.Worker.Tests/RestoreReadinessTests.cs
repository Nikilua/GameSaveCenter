using System.IO.Compression;
using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class RestoreReadinessTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly RestoreReadinessService service;

    public RestoreReadinessTests()
    {
        Directory.CreateDirectory(root);
        service = new RestoreReadinessService(NullLogger<RestoreReadinessService>.Instance);
    }

    [Fact]
    public async Task ValidZip_IsExtractedToIsolation_AndReportsReady()
    {
        var archive = CreateArchive(("drive-C/saves/profile.dat", "save-data"));
        var version = Version(archive, 1, 9);

        var result = await service.ValidateAsync(version, Manifest("drive-C/saves/profile.dat", 9), Path.Combine(root, "staging"), CancellationToken.None);

        Assert.Equal(RestoreReadinessStatus.Ready, result.Status);
        Assert.True(result.ArchiveReadable);
        Assert.True(result.ExtractSucceeded);
        Assert.Equal(1, result.ActualFileCount);
        Assert.Equal(9, result.ActualTotalSize);
        Assert.Equal("Cleaned", result.StagingCleanupStatus);
        Assert.True(!Directory.Exists(Path.Combine(root, "staging")) || !Directory.EnumerateDirectories(Path.Combine(root, "staging")).Any());
    }

    [Fact]
    public async Task CorruptZip_IsReportedWithoutTouchingARealSavePath()
    {
        var archive = Path.Combine(root, "corrupt.zip");
        await File.WriteAllTextAsync(archive, "not a zip");

        var result = await service.ValidateAsync(Version(archive, 1, 9), "[]", Path.Combine(root, "staging"), CancellationToken.None);

        Assert.Equal(RestoreReadinessStatus.Corrupted, result.Status);
        Assert.False(result.ArchiveReadable);
        Assert.False(Directory.Exists(Path.Combine(root, "real-save-data")));
    }

    [Fact]
    public async Task MissingArchive_IsReportedAsCorrupted()
    {
        var result = await service.ValidateAsync(Version(Path.Combine(root, "missing.zip"), 1, 1), "[]", Path.Combine(root, "staging"), CancellationToken.None);

        Assert.Equal(RestoreReadinessStatus.Corrupted, result.Status);
        Assert.Equal(1, result.ErrorCount);
    }

    [Fact]
    public async Task UnsafeEntry_IsRejectedAndDoesNotEscapeStaging()
    {
        var archive = CreateArchive(("../outside.txt", "escape"));

        var result = await service.ValidateAsync(Version(archive, 1, 6), "[]", Path.Combine(root, "staging"), CancellationToken.None);

        Assert.Equal(RestoreReadinessStatus.Corrupted, result.Status);
        Assert.False(File.Exists(Path.Combine(root, "outside.txt")));
    }

    [Fact]
    public async Task IndexMismatch_IsWarning_AndZeroLengthEntryIsVisible()
    {
        var archive = CreateArchive(("profile.dat", "save"), ("empty.dat", string.Empty));

        var result = await service.ValidateAsync(Version(archive, 1, 999), Manifest("profile.dat", 4), Path.Combine(root, "staging"), CancellationToken.None);

        Assert.Equal(RestoreReadinessStatus.Warning, result.Status);
        Assert.True(result.WarningCount >= 2);
        Assert.Equal(2, result.ActualFileCount);
    }

    [Fact]
    public async Task MissingManifestEntry_IsCorrupted_EvenWhenCountsMatch()
    {
        var archive = CreateArchive(("other.dat", "data"));

        var result = await service.ValidateAsync(Version(archive, 1, 4), Manifest("profile.dat", 4), Path.Combine(root, "staging"), CancellationToken.None);

        Assert.Equal(RestoreReadinessStatus.Corrupted, result.Status);
        Assert.False(result.ExtractSucceeded);
        Assert.Contains("缺少", result.Summary);
    }

    [Fact]
    public async Task HashMismatch_IsCorrupted()
    {
        var archive = CreateArchive(("profile.dat", "save"));
        var manifest = JsonSerializer.Serialize(new[]
        {
            new FileManifestEntry { RelativePath = "profile.dat", SizeBytes = 4, Sha256 = new string('0', 64) }
        });

        var result = await service.ValidateAsync(Version(archive, 1, 4), manifest, Path.Combine(root, "staging"), CancellationToken.None);

        Assert.Equal(RestoreReadinessStatus.Corrupted, result.Status);
        Assert.Equal("Failed", result.HashValidation);
        Assert.True(result.ErrorCount > 0);
    }

    [Fact]
    public async Task InvalidManifest_IsFailed_AndDoesNotExtract()
    {
        var archive = CreateArchive(("profile.dat", "save"));

        var result = await service.ValidateAsync(Version(archive, 1, 4), "{not-json", Path.Combine(root, "staging"), CancellationToken.None);

        Assert.Equal(RestoreReadinessStatus.Failed, result.Status);
        Assert.False(result.ArchiveReadable);
        Assert.True(!Directory.Exists(Path.Combine(root, "staging")) || !Directory.EnumerateFileSystemEntries(Path.Combine(root, "staging")).Any());
    }

    [Fact]
    public async Task Cancellation_IsObserved_AndIsolationIsCleaned()
    {
        var archive = CreateArchive(("profile.dat", "save"));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ValidateAsync(Version(archive, 1, 4), Manifest("profile.dat", 4), Path.Combine(root, "staging"), cancellation.Token));

        Assert.True(!Directory.Exists(Path.Combine(root, "staging")) || !Directory.EnumerateFileSystemEntries(Path.Combine(root, "staging")).Any());
    }

    [Fact]
    public async Task UnreadableStagingRoot_IsFailedWithoutTouchingLivePath()
    {
        var archive = CreateArchive(("profile.dat", "save"));
        var stagingFile = Path.Combine(root, "staging-file");
        await File.WriteAllTextAsync(stagingFile, "not a directory");

        var result = await service.ValidateAsync(Version(archive, 1, 4), Manifest("profile.dat", 4), stagingFile, CancellationToken.None);

        Assert.Equal(RestoreReadinessStatus.Failed, result.Status);
        Assert.False(File.Exists(Path.Combine(root, "real-save-data", "profile.dat")));
    }

    [Fact]
    public async Task SimpleOrMissingArchivePath_IsUnsupported()
    {
        var result = await service.ValidateAsync(new BackupVersionDto { BackupId = "simple", ArchivePath = "" }, "[]", Path.Combine(root, "staging"), CancellationToken.None);

        Assert.Equal(RestoreReadinessStatus.Unsupported, result.Status);
    }

    [Fact]
    public async Task Readiness_IsPersistedAcrossStoreRecreation()
    {
        var options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "state"),
            LudusaviBackupDirectory = Path.Combine(root, "saves"),
            MediaArchiveDirectory = Path.Combine(root, "media")
        };
        var store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await store.InitializeAsync(CancellationToken.None);
        var archive = CreateArchive(("profile.dat", "save"));
        var version = new BackupVersionDto
        {
            BackupId = "backup-1.zip", PlayniteId = "game-1", LudusaviName = "Game",
            CreatedUtc = DateTime.UtcNow, FileCount = 1, TotalBytes = 4, ArchivePath = archive
        };
        await store.AddBackupVersionAsync(version, Manifest("profile.dat", 4), CancellationToken.None);
        var readiness = await service.ValidateAsync(version, Manifest("profile.dat", 4), Path.Combine(root, "readiness"), CancellationToken.None);
        await store.SaveRestoreReadinessAsync("game-1", version.BackupId, readiness, CancellationToken.None);

        var restarted = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restarted.InitializeAsync(CancellationToken.None);
        var loaded = (await restarted.GetBackupVersionsAsync("game-1", CancellationToken.None)).Single();

        Assert.Equal(archive, loaded.ArchivePath);
        Assert.Equal(RestoreReadinessStatus.Ready, loaded.RestoreReadiness?.Status);
    }

    private BackupVersionDto Version(string archive, int fileCount, long bytes) => new()
    {
        BackupId = Path.GetFileNameWithoutExtension(archive),
        ArchivePath = archive,
        FileCount = fileCount,
        TotalBytes = bytes
    };

    private string CreateArchive(params (string Name, string Content)[] entries)
    {
        var path = Path.Combine(root, Guid.NewGuid().ToString("N") + ".zip");
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        foreach (var (name, content) in entries)
        {
            using var writer = new StreamWriter(archive.CreateEntry(name).Open());
            writer.Write(content);
        }
        return path;
    }

    private static string Manifest(string path, long bytes)
        => JsonSerializer.Serialize(new[] { new FileManifestEntry { RelativePath = path, SizeBytes = bytes } });

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
