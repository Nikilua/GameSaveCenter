using System.IO.Compression;
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
        var result = await service.CreateAsync(CancellationToken.None);

        Assert.True(File.Exists(result.PackagePath));
        Assert.True(result.PackageBytes > 0);
        using var archive = ZipFile.OpenRead(result.PackagePath);
        var names = archive.Entries.Select(x => x.FullName).ToArray();
        Assert.Contains("database/gamesavecenter.db", names);
        Assert.Contains("settings/worker-settings.json", names);
        Assert.Contains("manifest.json", names);
        Assert.Contains("README.txt", names);
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

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
