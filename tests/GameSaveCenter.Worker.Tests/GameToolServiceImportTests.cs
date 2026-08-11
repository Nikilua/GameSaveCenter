using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Ipc;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class GameToolServiceImportTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly SqliteStateStore store;
    private readonly WorkerOptions options;
    private readonly GameToolService service;

    public GameToolServiceImportTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        Directory.CreateDirectory(options.DataDirectory);
        Directory.CreateDirectory(options.LudusaviBackupDirectory);
        Directory.CreateDirectory(options.MediaArchiveDirectory);
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
        var broadcaster = new TaskEventBroadcaster();
        var tasks = new TaskCoordinator(store, broadcaster, NullLogger<TaskCoordinator>.Instance);
        service = new GameToolService(options, store, new FakeCatalog(), tasks, NullLogger<GameToolService>.Instance);
    }

    [Fact]
    public async Task CustomExecutable_KeepsExternalPathReferenceWithoutCopying()
    {
        var source = Path.Combine(root, "LosslessScaling.exe");
        File.WriteAllText(source, "fake exe");

        var imported = await service.ImportAsync(new ImportGameToolRequestDto
        {
            PlayniteId = "game",
            ToolType = GameToolType.CustomExecutable,
            SourcePath = source,
            EntryFileName = Path.GetFileName(source),
            DisplayName = "Lossless Scaling",
            CopyIntoLibrary = false
        }, CancellationToken.None);

        Assert.Equal(GameToolType.CustomExecutable, imported.ToolType);
        Assert.Equal(source, imported.ActiveVersion.EntryPath);
        Assert.Equal("Lossless Scaling", imported.DisplayName);
        Assert.True(imported.ActiveVersion.IsAvailable);
        var copiedFiles = Directory.EnumerateFiles(options.GameToolsDirectory, "*", SearchOption.AllDirectories).Select(Path.GetFileName).ToList();
        Assert.DoesNotContain(copiedFiles, name => name != null && name.EndsWith("LosslessScaling.exe", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("fix.bat")]
    [InlineData("launch.ps1")]
    public async Task CustomScript_KeepsExternalPathReference(string fileName)
    {
        var source = Path.Combine(root, fileName);
        File.WriteAllText(source, "test script");

        var imported = await service.ImportAsync(new ImportGameToolRequestDto
        {
            PlayniteId = "game",
            ToolType = GameToolType.CustomExecutable,
            SourcePath = source,
            EntryFileName = fileName,
            CopyIntoLibrary = false
        }, CancellationToken.None);

        Assert.Equal(source, imported.ActiveVersion.EntryPath);
        var copiedFiles = Directory.EnumerateFiles(options.GameToolsDirectory, "*", SearchOption.AllDirectories).Select(Path.GetFileName).ToList();
        Assert.DoesNotContain(copiedFiles, name => name != null && name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ImportMissingFile_ThrowsFileNotFoundException()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() => service.ImportAsync(new ImportGameToolRequestDto
        {
            PlayniteId = "game",
            ToolType = GameToolType.CustomExecutable,
            SourcePath = Path.Combine(root, "missing.exe"),
            CopyIntoLibrary = false
        }, CancellationToken.None));
    }

    public void Dispose()
    {
        try { Directory.Delete(root, true); }
        catch { }
    }

    private sealed class FakeCatalog : ITrainerCatalogSource
    {
        public Task<TrainerCatalogSyncResultDto> SyncCatalogAsync(CancellationToken token) => throw new NotSupportedException();
        public Task<List<TrainerCatalogItemDto>> SearchAsync(string query, int limit, CancellationToken token) => throw new NotSupportedException();
        public Task<List<TrainerReleaseDto>> GetReleasesAsync(string catalogId, CancellationToken token) => throw new NotSupportedException();
        public Task DownloadAsync(string releaseId, string targetPath, IProgress<(long Received, long? Total)>? progress, CancellationToken token) => throw new NotSupportedException();
    }
}
