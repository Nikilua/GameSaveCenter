using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class GameCatalogPersistenceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;
    private readonly GameCatalogService catalog;

    public GameCatalogPersistenceTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = root,
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media"),
            LudusaviExecutable = Path.Combine(root, "missing-ludusavi.exe")
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
        var runner = new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance);
        var ludusavi = new LudusaviClient(options, runner, NullLogger<LudusaviClient>.Instance);
        catalog = new GameCatalogService(store, ludusavi, NullLogger<GameCatalogService>.Instance);
    }

    [Fact]
    public async Task FullRefreshSkipsUnchangedRowsAndOnlyPersistsChangedDescriptors()
    {
        var original = new GameDescriptorDto
        {
            PlayniteId = "game-1",
            Name = "Example Game",
            Platform = GamePlatformKind.Steam,
            PlatformGameId = "123",
            IsInstalled = true
        };

        await catalog.UpsertAndMatchAsync(new[] { original }, CancellationToken.None);
        var firstTimestamp = await ReadUpdatedUtcAsync("game-1");

        await store.SetGameMatchAsync("game-1", "Ludusavi Example", 0.95,
            GameMatchInput.CreateHash(original), CancellationToken.None);
        firstTimestamp = await ReadUpdatedUtcAsync("game-1");

        await Task.Delay(25);
        await catalog.UpsertAndMatchAsync(new[] { original }, CancellationToken.None);
        var unchangedTimestamp = await ReadUpdatedUtcAsync("game-1");
        Assert.Equal(firstTimestamp, unchangedTimestamp);

        await Task.Delay(25);
        var changed = new GameDescriptorDto
        {
            PlayniteId = original.PlayniteId,
            Name = "Example Game (Updated)",
            Platform = original.Platform,
            PlatformGameId = original.PlatformGameId,
            IsInstalled = original.IsInstalled
        };
        await catalog.UpsertAndMatchAsync(new[] { changed }, CancellationToken.None);
        var changedTimestamp = await ReadUpdatedUtcAsync("game-1");
        Assert.NotEqual(unchangedTimestamp, changedTimestamp);

        var changedCache = await store.GetGameMatchCacheAsync(CancellationToken.None);
        Assert.Empty(changedCache["game-1"].LudusaviName);
        Assert.Null(changedCache["game-1"].LastMatchAttemptUtc);
    }

    [Fact]
    public async Task LargeRefreshPersistsEveryDescriptorBeforeAnyBackgroundMatch()
    {
        var games = Enumerable.Range(0, 520)
            .Select(index => new GameDescriptorDto
            {
                PlayniteId = "large-game-" + index,
                Name = "Large Game " + index,
                Platform = GamePlatformKind.Steam,
                PlatformGameId = (10000 + index).ToString(),
                IsInstalled = true
            })
            .ToArray();

        // Ludusavi is intentionally unavailable in this fixture. Descriptor persistence must
        // still complete for the whole library; matching availability is a separate concern.
        await catalog.UpsertAndMatchAsync(games, CancellationToken.None);

        var persisted = await store.GetGamesAsync(CancellationToken.None);
        Assert.Equal(games.Length, persisted.Count);
    }

    [Fact]
    public async Task InstallStateChangePersistsWithoutInvalidatingExistingMatch()
    {
        var notInstalled = new GameDescriptorDto
        {
            PlayniteId = "dead-space",
            Name = "死亡空间",
            Platform = GamePlatformKind.Steam,
            PlatformGameId = "17470",
            IsInstalled = false
        };
        await catalog.UpsertAndMatchAsync(new[] { notInstalled }, CancellationToken.None);
        await store.SetGameMatchAsync(
            notInstalled.PlayniteId,
            "Dead Space",
            1.0,
            GameMatchInput.CreateHash(notInstalled),
            CancellationToken.None);

        var installed = new GameDescriptorDto
        {
            PlayniteId = notInstalled.PlayniteId,
            Name = notInstalled.Name,
            Platform = notInstalled.Platform,
            PlatformGameId = notInstalled.PlatformGameId,
            IsInstalled = true
        };
        await catalog.UpsertAndMatchAsync(new[] { installed }, CancellationToken.None);

        var persisted = await store.GetGameMatchCacheAsync(CancellationToken.None);
        Assert.True(persisted[installed.PlayniteId].Descriptor.IsInstalled);
        Assert.Equal("Dead Space", persisted[installed.PlayniteId].LudusaviName);
        Assert.Equal(1.0, persisted[installed.PlayniteId].Confidence);
    }

    [Fact]
    public async Task DescriptorOnlySyncUpdatesFreshnessWithoutMatchingOtherGames()
    {
        var first = new GameDescriptorDto
        {
            PlayniteId = "sync-one",
            Name = "Sync One",
            Platform = GamePlatformKind.Steam,
            PlatformGameId = "100",
            IsInstalled = false,
            PlayniteIsInstalled = false,
            InstallStateSource = GameInstallStateSources.None
        };
        var other = new GameDescriptorDto
        {
            PlayniteId = "sync-other",
            Name = "Sync Other",
            Platform = GamePlatformKind.Steam,
            PlatformGameId = "200",
            IsInstalled = true,
            PlayniteIsInstalled = true,
            InstallStateSource = GameInstallStateSources.PlayniteFlag
        };
        await catalog.UpsertAndMatchAsync(new[] { first, other }, CancellationToken.None);
        await store.SetGameMatchAsync(first.PlayniteId, "Sync One", 0.9, GameMatchInput.CreateHash(first), CancellationToken.None);
        await store.SetGameMatchAsync(other.PlayniteId, "Sync Other", 0.8, GameMatchInput.CreateHash(other), CancellationToken.None);
        var before = await store.GetGameDiscoveryDiagnosticAsync(first.PlayniteId, CancellationToken.None);

        first.IsInstalled = true;
        first.PlayniteIsInstalled = true;
        first.InstallStateSource = GameInstallStateSources.PlayniteFlag;
        await catalog.UpsertDescriptorOnlyAsync(first, CancellationToken.None);

        var current = await store.GetGameDiscoveryDiagnosticAsync(first.PlayniteId, CancellationToken.None);
        var otherCurrent = await store.GetGameDiscoveryDiagnosticAsync(other.PlayniteId, CancellationToken.None);
        Assert.NotNull(current);
        Assert.NotNull(current!.DescriptorSyncedUtc);
        Assert.True(current.DescriptorSyncedUtc >= before!.DescriptorSyncedUtc);
        Assert.True(current.Descriptor.IsInstalled);
        Assert.Equal("Sync One", current.LudusaviName);
        Assert.Equal("Sync Other", otherCurrent!.LudusaviName);
    }

    [Fact]
    public async Task DiscoveryDiagnosticReportsBackupAndMatchAttemptWithoutPrivatePaths()
    {
        var game = new GameDescriptorDto
        {
            PlayniteId = "diagnostic-game",
            Name = "Diagnostic Game",
            Platform = GamePlatformKind.Gog,
            PlatformGameId = "gog-1",
            InstallDirectory = Path.Combine(root, "missing-install"),
            IsInstalled = false,
            InstallStateSource = GameInstallStateSources.None
        };
        await catalog.UpsertAndMatchAsync(new[] { game }, CancellationToken.None);
        await store.SetGameMatchAsync(game.PlayniteId, string.Empty, 0, GameMatchInput.CreateHash(game), CancellationToken.None);
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            BackupId = "diagnostic-backup",
            PlayniteId = game.PlayniteId,
            LudusaviName = game.Name,
            CreatedUtc = DateTime.UtcNow,
            TotalBytes = 10,
            FileCount = 1
        }, "[]", CancellationToken.None);

        var diagnostic = await catalog.GetDiscoveryDiagnosticAsync(new GameDiscoveryDiagnosticRequestDto
        {
            PlayniteId = game.PlayniteId,
            CurrentStatusFilter = "有备份",
            CurrentPlatformFilter = "GOG",
            CurrentSearchText = "diagnostic"
        }, CancellationToken.None);

        Assert.True(diagnostic.WorkerRecordExists);
        Assert.False(diagnostic.IsInstalled);
        Assert.True(diagnostic.HasInstallDirectoryConfigured);
        Assert.False(diagnostic.InstallDirectoryPresent);
        Assert.Equal(1, diagnostic.BackupVersionCount);
        Assert.Equal("UnmatchedAfterAttempt", diagnostic.MatchState);
        Assert.Equal("有备份", diagnostic.CurrentStatusFilter);
        Assert.DoesNotContain(root, System.Text.Json.JsonSerializer.Serialize(diagnostic));
    }

    private async Task<string?> ReadUpdatedUtcAsync(string playniteId)
    {
        await using var connection = new SqliteConnection($"Data Source={options.DatabasePath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT updated_utc FROM games WHERE playnite_id=$id;";
        command.Parameters.AddWithValue("$id", playniteId);
        return await command.ExecuteScalarAsync() as string;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }
}
