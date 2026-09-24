using System.IO.Compression;
using System.Security.Cryptography;
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
    public async Task ValidZip_WithoutManifestHashes_IsWarning_AndDoesNotClaimHashSuccess()
    {
        var archive = CreateArchive(("drive-C/saves/profile.dat", "save-data"));
        var version = Version(archive, 1, 9);

        var result = await service.ValidateAsync(version, Manifest("drive-C/saves/profile.dat", 9), Path.Combine(root, "staging"), CancellationToken.None);

        Assert.Equal(RestoreReadinessStatus.Warning, result.Status);
        Assert.True(result.ArchiveReadable);
        Assert.True(result.ExtractSucceeded);
        Assert.Equal(1, result.ActualFileCount);
        Assert.Equal(9, result.ActualTotalSize);
        Assert.Equal("NotAvailable", result.HashValidation);
        Assert.Equal(0, result.HashCoveredFileCount);
        Assert.Equal(1, result.HashEligibleFileCount);
        Assert.Contains("不等于校验成功", result.Summary);
        Assert.Equal("Cleaned", result.StagingCleanupStatus);
        Assert.True(!Directory.Exists(Path.Combine(root, "staging")) || !Directory.EnumerateDirectories(Path.Combine(root, "staging")).Any());
    }

    [Fact]
    public async Task PartialManifestHashes_AreWarning_AndReportCoverage()
    {
        var archive = CreateArchive(("profile.dat", "save"), ("settings.dat", "data2"));
        var manifest = JsonSerializer.Serialize(new[]
        {
            new FileManifestEntry { RelativePath = "profile.dat", SizeBytes = 4, Sha256 = Sha256("save") },
            new FileManifestEntry { RelativePath = "settings.dat", SizeBytes = 5 }
        });

        var result = await service.ValidateAsync(Version(archive, 2, 9), manifest, Path.Combine(root, "staging"), CancellationToken.None);

        Assert.Equal(RestoreReadinessStatus.Warning, result.Status);
        Assert.Equal("Partial", result.HashValidation);
        Assert.Equal(1, result.HashCoveredFileCount);
        Assert.Equal(2, result.HashEligibleFileCount);
        Assert.True(result.ExtractSucceeded);
        Assert.Contains("部分文件哈希", result.Summary);
        Assert.Contains("未覆盖文件不能视为已校验", result.Summary);
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
    public async Task LockedArchive_IsFailedWithBackupIdentityAndCleanedStaging()
    {
        var archive = CreateArchive(("profile.dat", "save"));
        var version = Version(archive, 1, 4);
        version.BackupId = "backup-locked-during-review";
        var staging = Path.Combine(root, "locked-staging");
        RestoreReadinessDto result;

        using (new FileStream(archive, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            result = await service.ValidateAsync(
                version,
                Manifest("profile.dat", 4),
                staging,
                CancellationToken.None);
        }

        Assert.Equal(RestoreReadinessStatus.Failed, result.Status);
        Assert.False(result.ArchiveReadable);
        Assert.Equal(1, result.ErrorCount);
        Assert.Equal("backup-locked-during-review", result.BackupVersionId);
        Assert.Contains("文件系统错误", result.Summary);
        Assert.Equal("Cleaned", result.StagingCleanupStatus);
        Assert.Equal(archive, version.ArchivePath);
        Assert.True(File.Exists(archive));
        Assert.True(!Directory.Exists(staging) || !Directory.EnumerateFileSystemEntries(staging).Any());
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
    public async Task SameSizeArchiveMutation_IsRejectedWhenReadinessIsRevalidated()
    {
        var archive = CreateArchive(("profile.dat", "save"));
        var manifest = JsonSerializer.Serialize(new[]
        {
            new FileManifestEntry { RelativePath = "profile.dat", SizeBytes = 4, Sha256 = Sha256("save") }
        });

        var first = await service.ValidateAsync(Version(archive, 1, 4), manifest, Path.Combine(root, "staging-first"), CancellationToken.None);
        Assert.Equal(RestoreReadinessStatus.Ready, first.Status);
        Assert.Equal(4, first.ActualTotalSize);

        CreateArchiveAt(archive, ("profile.dat", "data"));
        var second = await service.ValidateAsync(Version(archive, 1, 4), manifest, Path.Combine(root, "staging-second"), CancellationToken.None);

        Assert.Equal(RestoreReadinessStatus.Corrupted, second.Status);
        Assert.Equal(4, second.ActualTotalSize);
        Assert.Equal("Failed", second.HashValidation);
        Assert.Contains("校验失败", second.Summary);
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
        Assert.Equal(RestoreReadinessStatus.Warning, loaded.RestoreReadiness?.Status);
        Assert.Equal("NotAvailable", loaded.RestoreReadiness?.HashValidation);
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
        return CreateArchiveAt(path, entries);
    }

    private static string CreateArchiveAt(string path, params (string Name, string Content)[] entries)
    {
        if (File.Exists(path)) File.Delete(path);
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

    private static string Sha256(string content)
    {
        using var sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content))).Replace("-", string.Empty);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
