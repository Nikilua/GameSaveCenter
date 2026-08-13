using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class RepositoryRebuildServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public RepositoryRebuildServiceTests()
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
    public async Task RebuildContinuesAfterGameFailureAndReportsCounts()
    {
        var catalog = new FakeCatalog();
        catalog.Matches["g1"] = ("Demo One", 1.0);
        catalog.Matches["g2"] = ("Demo Two", 0.9);
        var rebuilder = new FakeRebuilder(store);
        rebuilder.FailGames.Add("g2");

        var service = new RepositoryRebuildService(catalog, rebuilder, store, NullLogger<RepositoryRebuildService>.Instance);
        var result = await service.RebuildAsync(CancellationToken.None);

        Assert.Equal(1, result.RebuiltGameCount);
        Assert.Equal(1, result.FailedGameCount);
        Assert.Equal(1, result.IndexedVersionCount);
        Assert.Contains("1 个游戏成功", result.Summary);
        Assert.Contains("1 个失败", result.Summary);
        Assert.Equal(2, rebuilder.Calls);
        var audit = await store.GetAuditAsync(20, CancellationToken.None);
        Assert.Contains(audit, x => x.Category == "RepositoryRebuild");
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }

    private sealed class FakeCatalog : IRestoreCatalog
    {
        public Dictionary<string, (string Name, double Confidence)> Matches { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Task<GameDescriptorDto?> GetGameAsync(string playniteId, CancellationToken token)
            => Task.FromResult<GameDescriptorDto?>(null);

        public Task<Dictionary<string, (string Name, double Confidence)>> GetMatchesAsync(CancellationToken token)
            => Task.FromResult(Matches);
    }

    private sealed class FakeRebuilder : IBackupHistoryRebuilder
    {
        private readonly SqliteStateStore store;

        public FakeRebuilder(SqliteStateStore store)
        {
            this.store = store;
        }

        public int Calls { get; private set; }
        public HashSet<string> FailGames { get; } = new(StringComparer.OrdinalIgnoreCase);

        public async Task RefreshBackupHistoryAsync(string playniteId, string ludusaviName, CancellationToken token)
        {
            Calls++;
            if (FailGames.Contains(playniteId))
                throw new InvalidOperationException("rebuild failed");
            await store.AddBackupVersionAsync(new BackupVersionDto
            {
                PlayniteId = playniteId,
                BackupId = "b-" + playniteId,
                LudusaviName = ludusaviName,
                CreatedUtc = DateTime.UtcNow,
                TotalBytes = 1,
                FileCount = 1
            }, "{}", token).ConfigureAwait(false);
        }
    }
}
