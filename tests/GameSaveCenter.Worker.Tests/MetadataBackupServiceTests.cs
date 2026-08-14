using System.IO.Compression;
using System.Text;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class MetadataBackupServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public MetadataBackupServiceTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task MetadataBackupContainsConsistentDatabaseSettingsAndManifest()
    {
        await store.AppendAuditAsync("Test", "metadata row", "{}", CancellationToken.None);
        Directory.CreateDirectory(Path.GetDirectoryName(options.RuntimeSettingsPath)!);
        await File.WriteAllTextAsync(options.RuntimeSettingsPath, "token=super-secret\nLudusaviExecutable=C:\\tools\\ludusavi.exe\n");

        var service = new MetadataBackupService(options, store, NullLogger<MetadataBackupService>.Instance);
        var result = await service.CreateAsync(new MetadataBackupCreateRequestDto(), CancellationToken.None);

        Assert.True(File.Exists(result.PackagePath));
        Assert.True(result.PackageBytes > 0);
        using var archive = ZipFile.OpenRead(result.PackagePath);
        var names = archive.Entries.Select(x => x.FullName).ToArray();
        Assert.Contains("database/gamesavecenter.db", names);
        Assert.Contains("settings/worker-settings.json", names);
        Assert.Contains("manifest.json", names);
        Assert.Contains("README.txt", names);
        Assert.DoesNotContain("settings/plugin-settings.json", names);
        Assert.DoesNotContain(names, x => x.Contains("Saves", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, x => x.Contains("Media", StringComparison.OrdinalIgnoreCase));

        var settingsEntry = archive.GetEntry("settings/worker-settings.json")!;
        using (var reader = new StreamReader(settingsEntry.Open()))
        {
            var settingsText = await reader.ReadToEndAsync();
            Assert.DoesNotContain("super-secret", settingsText);
            Assert.Contains("[REDACTED]", settingsText);
        }

        var databaseEntry = archive.GetEntry("database/gamesavecenter.db")!;
        var restored = Path.Combine(root, "restored.db");
        databaseEntry.ExtractToFile(restored, true);
        await using var connection = new SqliteConnection($"Data Source={restored};Mode=ReadOnly;Cache=Shared");
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM audit_log;";
        Assert.Equal(1L, Convert.ToInt64(await command.ExecuteScalarAsync()));
    }

    [Fact]
    public async Task MetadataRestoreReplacesDatabaseAndKeepsPreRestore()
    {
        await store.AppendAuditAsync("Test", "before package", "{}", CancellationToken.None);
        Directory.CreateDirectory(Path.GetDirectoryName(options.RuntimeSettingsPath)!);
        await File.WriteAllTextAsync(options.RuntimeSettingsPath, "LudusaviExecutable=C:\\tools\\ludusavi.exe\n");
        const string pluginSettings = "{\"Settings\":{\"WorkerExecutable\":\"C:\\\\tools\\\\worker.exe\",\"ThemeMode\":2}}";
        var service = new MetadataBackupService(options, store, NullLogger<MetadataBackupService>.Instance);
        var backup = await service.CreateAsync(new MetadataBackupCreateRequestDto { PluginSettingsJson = pluginSettings }, CancellationToken.None);
        await store.AppendAuditAsync("Test", "after package", "{}", CancellationToken.None);

        var preview = await service.PreviewAsync(backup.PackagePath, CancellationToken.None);
        Assert.True(preview.Valid);
        Assert.Contains("plugin-settings.json", preview.Summary);
        Assert.Contains("worker.exe", preview.PluginSettingsJson);
        var restored = await service.RestoreAsync(new MetadataRestoreRequestDto
        {
            PackagePath = backup.PackagePath,
            Confirmed = true
        }, CancellationToken.None);

        Assert.True(restored.Restored);
        Assert.Contains("worker.exe", restored.PluginSettingsJson);
        Assert.True(Directory.Exists(restored.PreRestorePath));
        Assert.True(File.Exists(Path.Combine(restored.PreRestorePath, "gamesavecenter.db")));
        await using var connection = new SqliteConnection($"Data Source={options.DatabasePath};Mode=ReadOnly;Cache=Shared");
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM audit_log;";
        Assert.Equal(2L, Convert.ToInt64(await command.ExecuteScalarAsync()));
    }

    [Fact]
    public async Task MetadataRestoreRequiresConfirmation()
    {
        var service = new MetadataBackupService(options, store, NullLogger<MetadataBackupService>.Instance);
        var backup = await service.CreateAsync(new MetadataBackupCreateRequestDto(), CancellationToken.None);

        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.RestoreAsync(
            new MetadataRestoreRequestDto { PackagePath = backup.PackagePath, Confirmed = false },
            CancellationToken.None));
        Assert.Equal("METADATA_RESTORE_NOT_CONFIRMED", ex.Code);
    }

    [Fact]
    public async Task MetadataRestoreRejectsHashMismatchAndTraversal()
    {
        var service = new MetadataBackupService(options, store, NullLogger<MetadataBackupService>.Instance);
        var hashBad = Path.Combine(root, "bad-hash.zip");
        CreatePackage(hashBad, new Dictionary<string, byte[]>
        {
            ["manifest.json"] = Encoding.UTF8.GetBytes("{\"SchemaVersion\":1,\"DatabaseSha256\":\"00\"}"),
            ["database/gamesavecenter.db"] = new byte[] { 1, 2, 3 }
        });
        var preview = await service.PreviewAsync(hashBad, CancellationToken.None);
        Assert.False(preview.Valid);
        Assert.Contains("哈希", preview.Summary, StringComparison.OrdinalIgnoreCase);

        var traversal = Path.Combine(root, "traversal.zip");
        CreatePackage(traversal, new Dictionary<string, byte[]>
        {
            ["manifest.json"] = Encoding.UTF8.GetBytes("{\"SchemaVersion\":1,\"DatabaseSha256\":\"00\"}"),
            ["database/gamesavecenter.db"] = new byte[] { 1, 2, 3 },
            ["../evil"] = Encoding.UTF8.GetBytes("x")
        });
        var traversalPreview = await service.PreviewAsync(traversal, CancellationToken.None);
        Assert.False(traversalPreview.Valid);
        Assert.Contains("越界", traversalPreview.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MetadataBackupRejectsInvalidPluginSettings()
    {
        var service = new MetadataBackupService(options, store, NullLogger<MetadataBackupService>.Instance);
        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.CreateAsync(
            new MetadataBackupCreateRequestDto { PluginSettingsJson = "{not-json" },
            CancellationToken.None));
        Assert.Equal("METADATA_PLUGIN_SETTINGS_INVALID", ex.Code);
    }

    private static void CreatePackage(string path, IReadOnlyDictionary<string, byte[]> entries)
    {
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        foreach (var pair in entries)
        {
            var entry = archive.CreateEntry(pair.Key);
            using var stream = entry.Open();
            stream.Write(pair.Value, 0, pair.Value.Length);
        }
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
