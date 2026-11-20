using System.Diagnostics;
using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Worker.Tests;

public sealed class DeviceStateComparisonTests : IDisposable
{
    private static readonly DateTime Timestamp = new(2026, 11, 20, 1, 0, 0, DateTimeKind.Utc);
    private const string RemoteId = "ab123456789012345678901234567890";
    private readonly string root = Path.Combine(Path.GetTempPath(), "gsc-device-comparison-" + Guid.NewGuid().ToString("N"));
    private readonly List<string> calls = new();
    private readonly ITestOutputHelper output;

    public DeviceStateComparisonTests(ITestOutputHelper output) => this.output = output;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PublishedSummaryPicksOneBackupPerGameAtTiedTimestamp(bool reverseOrder)
    {
        var (store, service) = await CreateAsync(Sidecar(1));
        await AddBackup(store, "game-0", "older", Timestamp.AddDays(-1));
        await AddBackup(store, "game-0", reverseOrder ? "a" : "b", Timestamp);
        await AddBackup(store, "game-0", reverseOrder ? "b" : "a", Timestamp);
        await AddBackup(store, "another", "another-1", Timestamp);

        var result = await service.SyncAsync(CancellationToken.None);

        Assert.True(result.Uploaded);
        Assert.Equal(new[] { "copy", "lsf", "cat" }, calls);
        Assert.Equal("a", Assert.Single(result.Comparisons).LocalBackupId);
        var sidecarPath = Assert.Single(Directory.GetFiles(Path.Combine(root, "DeviceState"), "*.json"));
        var published = JsonSerializer.Deserialize<DeviceStateSidecarDto>(await File.ReadAllTextAsync(sidecarPath),
            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.Equal(new[] { "another-1", "a" }, published.Backups.Select(x => x.BackupId));
        Assert.Equal(new[] { "a", "b", "older" }, (await store.GetBackupVersionPageAsync(
            "game-0", 10, null, null, CancellationToken.None)).Items.Select(x => x.BackupId));
    }

    [Theory]
    [InlineData(499, false)]
    [InlineData(500, false)]
    [InlineData(501, true)]
    public async Task CoverageReportsEachDeviceAndScopesTheResult(int count, bool partial)
    {
        var first = Sidecar(count);
        first.Backups[^1].BackupId = "fork";
        first.Backups[^1].ParentBackupId = "common";
        var second = Sidecar(3, "Laptop", "cd123456789012345678901234567890");
        var (store, service) = await CreateAsync(first, second);
        await AddBackup(store, $"game-{count - 1}", "local", Timestamp, "common");

        var result = await service.SyncAsync(CancellationToken.None);

        Assert.Collection(result.Coverage,
            x => { Assert.Equal("Desktop", x.DeviceName); Assert.Equal(count, x.Total); Assert.Equal(Math.Min(count, 500), x.Compared); },
            x => { Assert.Equal("Laptop", x.DeviceName); Assert.Equal(3, x.Total); Assert.Equal(3, x.Compared); });
        Assert.Equal(partial, result.StatusMessage.Contains("仅完成部分比较"));
        Assert.Equal(!partial, result.Comparisons.Any(x => x.HasConflict));
        Assert.Contains(partial ? "在已比较条目中未发现" : "在已比较条目中发现", result.StatusMessage);
    }

    [Theory]
    [InlineData("local", "remote-0", false, true)]
    [InlineData("old-local", "remote-0", false, false)]
    [InlineData("local", "old-remote", false, false)]
    [InlineData("old-local", "old-remote", false, false)]
    [InlineData("local", "remote-0", true, true)]
    [InlineData(null, "remote-0", true, false)]
    [InlineData("local", null, true, false)]
    public async Task RecordedDecisionsApplyOnlyToTheSameBackupPair(
        string? recordedLocal, string? recordedRemote, bool legacyKey, bool applies)
    {
        var (store, service) = await CreateAsync(Sidecar(1));
        await AddBackup(store, "game-0", "local", Timestamp);
        var key = legacyKey ? "Desktop" : RemoteId;
        await store.SaveDeviceConflictDecisionAsync(new DeviceConflictDecisionDto
        {
            PlayniteId = "game-0", RemoteDevice = key, LocalBackupId = recordedLocal!, RemoteBackupId = recordedRemote!,
            Decision = "PreferLocal", Comment = "记下旧版本依据", DecidedUtc = Timestamp
        }, CancellationToken.None);

        var comparison = Assert.Single((await service.SyncAsync(CancellationToken.None)).Comparisons);

        Assert.Equal(applies ? "记录为优先本机" : "待复核", comparison.DecisionDisplay);
        if (applies)
        {
            Assert.Equal("记下旧版本依据", comparison.DecisionComment);
            Assert.Null(comparison.PreviousDecision);
        }
        else
        {
            Assert.Empty(comparison.Decision);
            Assert.Empty(comparison.DecisionComment);
            Assert.Null(comparison.DecidedUtc);
            Assert.NotNull(comparison.PreviousDecision);
            Assert.Equal("记下旧版本依据", comparison.PreviousDecision!.Comment);
            Assert.Equal(recordedLocal ?? string.Empty, comparison.PreviousDecision.LocalBackupId);
            Assert.Equal(recordedRemote ?? string.Empty, comparison.PreviousDecision.RemoteBackupId);
        }
        Assert.Equal(new[] { "copy", "lsf", "cat" }, calls);
        Assert.Single((await store.GetBackupVersionPageAsync("game-0", 10, null, null, CancellationToken.None)).Items);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(500)]
    public async Task BatchDecisionLookupMatchesPerRowLookup(int count)
    {
        var (store, service) = await CreateAsync(Sidecar(count));
        await AddBackup(store, "game-0", "local-0", Timestamp);
        await AddBackup(store, "game-1", "local-1", Timestamp);
        var token = CancellationToken.None;
        foreach (var (game, key, local, decision) in new[]
        {
            ("game-0", RemoteId, "old-local", "PreferRemote"),
            ("game-0", "Desktop", "local-0", "PreferLocal"),
            ("game-1", "Desktop", "local-1", "KeepBoth")
        })
        {
            await store.SaveDeviceConflictDecisionAsync(new DeviceConflictDecisionDto
            {
                PlayniteId = game, RemoteDevice = key, LocalBackupId = local,
                RemoteBackupId = game == "game-0" ? "remote-0" : "remote-1", Decision = decision, DecidedUtc = Timestamp
            }, token);
        }
        var games = Enumerable.Range(0, count).Select(i => $"game-{i}").ToArray();
        using var pool = new SqliteConnection($"Data Source={Path.Combine(root, "gamesavecenter.db")};Mode=ReadWriteCreate;Cache=Shared;Foreign Keys=True");
        await pool.OpenAsync(token);
        await pool.CloseAsync();

        SqliteConnection.ClearPool(pool);
        var timer = Stopwatch.StartNew();
        var (baseline, baselineQueries) = await ReadPerRowAsync();
        var baselineCold = timer.Elapsed.TotalMilliseconds;
        var baselineWarm = await MeasureWarmAsync(async () => { await ReadPerRowAsync(); });

        SqliteConnection.ClearPool(pool);
        timer.Restart();
        var batch = await store.GetDeviceConflictDecisionsAsync(RemoteId, "Desktop", games, token);
        var batchCold = timer.Elapsed.TotalMilliseconds;
        var batchWarm = await MeasureWarmAsync(async () => { await store.GetDeviceConflictDecisionsAsync(RemoteId, "Desktop", games, token); });

        foreach (var game in games)
            Assert.Equal(JsonSerializer.Serialize(baseline.GetValueOrDefault(game)), JsonSerializer.Serialize(batch.GetValueOrDefault(game)));
        Assert.Equal(count * 2 - 1, baselineQueries);
        var result = await service.SyncAsync(token);
        Assert.Equal("PreferRemote", result.Comparisons.Single(x => x.PlayniteId == "game-0").PreviousDecision?.Decision);
        Assert.Equal("KeepBoth", result.Comparisons.Single(x => x.PlayniteId == "game-1").Decision);

        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        timer.Restart();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            store.GetDeviceConflictDecisionsAsync(RemoteId, "Desktop", games, canceled.Token));
        output.WriteLine($"{count} rows | SQL per-row {baselineQueries}, batch 1 | cold {baselineCold:F3} -> {batchCold:F3} ms | " +
            $"warm median {baselineWarm:F3} -> {batchWarm:F3} ms | canceled before lookup {timer.Elapsed.TotalMilliseconds:F3} ms");

        async Task<(Dictionary<string, DeviceConflictDecisionDto> Decisions, int Queries)> ReadPerRowAsync()
        {
            var decisions = new Dictionary<string, DeviceConflictDecisionDto>();
            var queries = 0;
            foreach (var game in games)
            {
                queries++;
                var decision = await store.GetDeviceConflictDecisionAsync(game, RemoteId, token);
                if (decision == null)
                {
                    queries++;
                    decision = await store.GetDeviceConflictDecisionAsync(game, "Desktop", token);
                }
                if (decision != null)
                    decisions.Add(game, decision);
            }
            return (decisions, queries);
        }
    }

    [Fact]
    public async Task CancellationAfterReadingFirstSidecarStopsBeforeReadingTheNextDevice()
    {
        using var cancellation = new CancellationTokenSource();
        var first = Sidecar(500);
        var second = Sidecar(1, "Laptop", "cd123456789012345678901234567890");
        var (_, service) = await CreateAsync(new[] { first, second }, cancellation.Cancel);
        var timer = Stopwatch.StartNew();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.SyncAsync(cancellation.Token));

        Assert.Equal(new[] { "copy", "lsf", "cat" }, calls);
        output.WriteLine($"Canceled while consuming the first 500-row sidecar in {timer.Elapsed.TotalMilliseconds:F3} ms including publication and fake transport");
    }

    private static async Task<double> MeasureWarmAsync(Func<Task> action)
    {
        var elapsed = new List<double>();
        for (var i = 0; i < 5; i++)
        {
            var timer = Stopwatch.StartNew();
            await action();
            elapsed.Add(timer.Elapsed.TotalMilliseconds);
        }
        elapsed.Sort();
        return elapsed[2];
    }

    private async Task<(SqliteStateStore Store, DeviceStateService Service)> CreateAsync(params DeviceStateSidecarDto[] sidecars)
        => await CreateAsync(sidecars, null);

    private async Task<(SqliteStateStore Store, DeviceStateService Service)> CreateAsync(
        DeviceStateSidecarDto[] sidecars, Action? afterRead)
    {
        var options = new WorkerOptions
        {
            DataDirectory = root,
            LudusaviBackupDirectory = Path.Combine(root, "saves"),
            MediaArchiveDirectory = Path.Combine(root, "media"),
            EnableCloudUpload = true,
            RcloneExecutable = typeof(DeviceStateService).Assembly.Location,
            RcloneDestination = "fixture:"
        };
        var store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await store.InitializeAsync(CancellationToken.None);
        var byPath = sidecars.ToDictionary(x => $"{x.DeviceId}/DeviceState/{x.DeviceId}.json");
        var rclone = new RcloneClient(options, (_, args, _, _, token) =>
        {
            token.ThrowIfCancellationRequested();
            var command = args.ToArray();
            calls.Add(command[0]);
            var result = command[0] switch
            {
                "copy" => new ProcessResult(0, "", ""),
                "lsf" => new ProcessResult(0, string.Join("\n", byPath.Keys), ""),
                "cat" => new ProcessResult(0, JsonSerializer.Serialize(byPath[command[1]["fixture:".Length..]]), ""),
                _ => throw new InvalidOperationException($"Unexpected rclone command {command[0]}")
            };
            if (command[0] == "cat")
                afterRead?.Invoke();
            return Task.FromResult(result);
        });
        return (store, new DeviceStateService(options, store, rclone,
            new CloudTransferCoordinator(NullLogger<CloudTransferCoordinator>.Instance), NullLogger<DeviceStateService>.Instance));
    }

    private static DeviceStateSidecarDto Sidecar(int count, string name = "Desktop", string id = RemoteId) => new()
    {
        DeviceId = id,
        DeviceName = name,
        Backups = Enumerable.Range(0, count).Select(i => new DeviceBackupSummaryDto
        {
            PlayniteId = $"game-{i}", GameName = $"Game {i}", BackupId = $"remote-{i}", CreatedUtc = Timestamp
        }).ToList()
    };

    private static async Task AddBackup(SqliteStateStore store, string game, string backup, DateTime time, string parent = "")
    {
        await store.UpsertGamesAsync(new[] { new GameDescriptorDto { PlayniteId = game, Name = game } }, CancellationToken.None);
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            BackupId = backup, PlayniteId = game, CreatedUtc = time, ParentBackupId = parent
        }, "[]", CancellationToken.None);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }
}
