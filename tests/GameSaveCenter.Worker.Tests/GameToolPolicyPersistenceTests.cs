using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class GameToolPolicyPersistenceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly SqliteStateStore store;

    public GameToolPolicyPersistenceTests()
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
    public async Task GameToolPoliciesSurviveStoreRoundTrip()
    {
        var entry = Path.Combine(root, "utility.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(entry)!);
        File.WriteAllText(entry, "not an executable");
        var now = DateTime.UtcNow;
        var tool = new GameToolDto
        {
            ToolId = "tool",
            PlayniteId = "game",
            ToolType = GameToolType.CustomExecutable,
            DisplayName = "Utility",
            IfAlreadyRunning = GameToolIfAlreadyRunning.Restart,
            RiskCategory = GameToolRiskCategory.GeneralUtility,
            ActiveVersionId = "version",
            CreatedUtc = now,
            UpdatedUtc = now
        };
        var version = new GameToolVersionDto
        {
            ToolId = tool.ToolId,
            VersionId = tool.ActiveVersionId,
            EntryPath = entry,
            CreatedUtc = now
        };

        await store.UpsertGameToolAsync(tool, version, CancellationToken.None);
        var saved = Assert.Single(await store.GetGameToolsAsync("game", CancellationToken.None));

        Assert.Equal(GameToolIfAlreadyRunning.Restart, saved.IfAlreadyRunning);
        Assert.Equal(GameToolRiskCategory.GeneralUtility, saved.RiskCategory);
    }

    public void Dispose()
    {
        try { Directory.Delete(root, true); }
        catch { }
    }
}
