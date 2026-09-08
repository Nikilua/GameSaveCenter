using System;
using System.IO;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class SavePathDetectionServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "gsc-save-path-" + Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public SavePathDetectionServiceTests()
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
    public async Task MissingAdditionalRootFailsInsteadOfReturningAnEmptyCandidateList()
    {
        await store.UpsertGamesAsync(new[]
        {
            new GameDescriptorDto { PlayniteId = "save-path-game", Name = "Save Path Game" }
        }, CancellationToken.None);
        var missingRoot = Path.Combine(root, "消失的存档目录", "Unicode-路径");
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<WorkerOperationException>(() => service.DetectAsync(
            new DetectionRequestDto
            {
                PlayniteId = "save-path-game",
                AdditionalRoots = new List<string> { missingRoot }
            }, CancellationToken.None));

        Assert.Equal("SAVE_PATH_ROOT_UNAVAILABLE", exception.Code);
        Assert.Contains(missingRoot, exception.DiagnosticDetail, StringComparison.Ordinal);
    }

    private SavePathDetectionService CreateService()
    {
        var runner = new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance);
        var ludusavi = new LudusaviClient(options, runner, NullLogger<LudusaviClient>.Instance);
        var catalog = new GameCatalogService(store, ludusavi, NullLogger<GameCatalogService>.Instance);
        return new SavePathDetectionService(options, catalog, store, NullLogger<SavePathDetectionService>.Instance);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
