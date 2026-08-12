using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class EnvironmentCheckServiceTests
{
    [Fact]
    public async Task MissingOptionalRcloneIsSkippedAndDatabaseProbeIsWritable()
    {
        var root = Path.Combine(Path.GetTempPath(), "gsc-environment-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var options = new WorkerOptions
            {
                DataDirectory = Path.Combine(root, "data"),
                LudusaviBackupDirectory = Path.Combine(root, "backups"),
                MediaArchiveDirectory = Path.Combine(root, "media"),
                LudusaviExecutable = string.Empty,
                RcloneExecutable = string.Empty,
                RcloneDestination = string.Empty
            };
            Directory.CreateDirectory(options.DataDirectory);
            var store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
            await store.InitializeAsync(CancellationToken.None);
            var runner = new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance);
            var ludusavi = new LudusaviClient(options, runner, NullLogger<LudusaviClient>.Instance);
            var catalog = new GameCatalogService(store, ludusavi, NullLogger<GameCatalogService>.Instance);
            var service = new EnvironmentCheckService(options, store, catalog, ludusavi,
                new RcloneClient(options, runner), NullLogger<EnvironmentCheckService>.Instance);

            var report = await service.RunAsync(new EnvironmentCheckRequestDto(), CancellationToken.None);

            Assert.Equal(EnvironmentCheckState.Passed, report.Items.Single(x => x.Key == "database").State);
            Assert.Equal(EnvironmentCheckState.Skipped, report.Items.Single(x => x.Key == "rclone").State);
            Assert.Equal(EnvironmentCheckState.Warning, report.Items.Single(x => x.Key == "library").State);
            Assert.Equal(EnvironmentCheckState.Failed, report.Items.Single(x => x.Key == "ludusavi").State);
            Assert.Equal(report.Items.Count(x => x.State == EnvironmentCheckState.Failed), report.FailedCount);
            Assert.False(report.IsReady);
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { }
        }
    }
}
