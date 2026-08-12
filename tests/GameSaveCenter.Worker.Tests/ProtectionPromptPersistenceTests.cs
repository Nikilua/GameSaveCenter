using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class ProtectionPromptPersistenceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly SqliteStateStore store;

    public ProtectionPromptPersistenceTests()
    {
        var options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task DeferredDecisionKeepsObservationAndPromptTimestamp()
    {
        await store.RecordProtectionPromptObservationAsync("game", true, true, CancellationToken.None);
        var observed = await store.GetProtectionPromptRecordAsync("game", CancellationToken.None);

        await store.SetProtectionPromptStateAsync("game", ProtectionPromptState.Deferred, CancellationToken.None);
        var deferred = await store.GetProtectionPromptRecordAsync("game", CancellationToken.None);

        Assert.True(observed.LastSaveRecognized);
        Assert.NotNull(observed.LastPromptUtc);
        Assert.Equal(ProtectionPromptState.Deferred, deferred.State);
        Assert.True(deferred.LastSaveRecognized);
        Assert.Equal(observed.LastPromptUtc, deferred.LastPromptUtc);
    }

    [Fact]
    public async Task NewGameStartsAtNeverShownWithoutObservation()
    {
        var record = await store.GetProtectionPromptRecordAsync("new-game", CancellationToken.None);

        Assert.Equal(ProtectionPromptState.NeverShown, record.State);
        Assert.False(record.LastSaveRecognized);
        Assert.Null(record.LastPromptUtc);
    }

    public void Dispose()
    {
        try { Directory.Delete(root, true); }
        catch { }
    }
}
