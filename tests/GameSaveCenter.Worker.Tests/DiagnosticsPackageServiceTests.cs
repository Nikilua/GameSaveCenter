using System.IO.Compression;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class DiagnosticsPackageServiceTests
{
    [Fact]
    public async Task PackageIsBoundedAndExcludesSensitiveOrLargeData()
    {
        var root = Path.Combine(Path.GetTempPath(), "gsc-diagnostics-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var options = new WorkerOptions
            {
                DataDirectory = Path.Combine(root, "data"),
                LudusaviBackupDirectory = Path.Combine(root, "saves"),
                MediaArchiveDirectory = Path.Combine(root, "media"),
                LudusaviExecutable = Path.Combine(root, "ludusavi.exe"),
                RcloneExecutable = Path.Combine(root, "rclone.exe"),
                RcloneDestination = "remote:private"
            };
            Directory.CreateDirectory(options.DataDirectory);
            Directory.CreateDirectory(options.LogDirectory);
            await File.WriteAllTextAsync(
                Path.Combine(options.LogDirectory, "worker-launch.log"),
                "password=abc123\nAuthorization: Bearer token123\n--api-key secret123\nC:\\Users\\JohnDoe\\AppData\\Roaming\n?token=querysecret\nnormal diagnostic\n");
            var store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
            await store.InitializeAsync(CancellationToken.None);
            await store.AppendAuditAsync("Test", "password=secret", "{\"token\":\"secret\"}", CancellationToken.None);

            var result = await new DiagnosticsPackageService(options, store, NullLogger<DiagnosticsPackageService>.Instance)
                .CreateAsync(new CreateDiagnosticsPackageRequestDto
                {
                    PluginVersion = "0.6.70",
                    PlayniteVersion = "10.56",
                    ThemeMode = "Dark",
                    CurrentWorkspace = "Maintenance",
                    DpiScale = 1.25,
                    ScreenCount = 2
                }, CancellationToken.None);

            Assert.True(File.Exists(result.PackagePath));
            Assert.True(result.PackageBytes <= 2 * 1024 * 1024);
            using var archive = ZipFile.OpenRead(result.PackagePath);
            var names = archive.Entries.Select(x => x.FullName).ToArray();
            Assert.Contains("README.txt", names);
            Assert.Contains("system.json", names);
            Assert.Contains("worker.json", names);
            Assert.Contains("dependencies.json", names);
            Assert.Contains("database.json", names);
            Assert.Contains("recent-tasks.json", names);
            Assert.Contains("health.json", names);
            Assert.Contains("settings.json", names);
            Assert.Contains(names, x => x.EndsWith("worker-launch.log", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(names, x => x.EndsWith(".db", StringComparison.OrdinalIgnoreCase));
            var contents = string.Join("\n", archive.Entries.Select(ReadEntry));
            Assert.DoesNotContain("abc123", contents);
            Assert.DoesNotContain("token123", contents);
            Assert.DoesNotContain("secret123", contents);
            Assert.DoesNotContain("querysecret", contents);
            Assert.DoesNotContain("JohnDoe", contents);
            Assert.DoesNotContain("password=secret", contents);
            Assert.DoesNotContain("\"token\":\"secret\"", contents);
            Assert.Contains("[REDACTED]", contents);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            try { Directory.Delete(root, true); } catch { }
        }
    }

    private static string ReadEntry(ZipArchiveEntry entry)
    {
        using var reader = new StreamReader(entry.Open());
        return reader.ReadToEnd();
    }
}
