using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class LocalMirrorServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;

    public LocalMirrorServiceTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media"),
            LocalMirrorPath = Path.Combine(root, "Mirror")
        };
        Directory.CreateDirectory(options.DataDirectory);
        Directory.CreateDirectory(options.LudusaviBackupDirectory);
        Directory.CreateDirectory(options.MediaArchiveDirectory);
    }

    [Fact]
    public async Task DisabledMirrorReportsUnavailableAndSyncRequiresConfiguration()
    {
        var service = new LocalMirrorService(options, NullLogger<LocalMirrorService>.Instance);
        var status = await service.StatusAsync(CancellationToken.None);

        Assert.False(status.Enabled);
        Assert.False(status.Available);
        Assert.Contains("未启用", status.Message);

        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.SyncAsync(CancellationToken.None));
        Assert.Equal("LOCAL_MIRROR_NOT_CONFIGURED", ex.Code);
    }

    [Fact]
    public async Task MissingMirrorIsOfflineStatusNotSystemError()
    {
        options.EnableLocalMirror = true;
        var service = new LocalMirrorService(options, NullLogger<LocalMirrorService>.Instance);
        var status = await service.StatusAsync(CancellationToken.None);

        Assert.True(status.Enabled);
        Assert.False(status.Available);
        Assert.Contains("不可用", status.Message);

        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.SyncAsync(CancellationToken.None));
        Assert.Equal("LOCAL_MIRROR_UNAVAILABLE", ex.Code);
    }

    [Fact]
    public async Task SyncCopiesVerifiesAndNeverDeletesMirrorOnlyFiles()
    {
        options.EnableLocalMirror = true;
        var gameDir = Path.Combine(options.LudusaviBackupDirectory, "game");
        Directory.CreateDirectory(gameDir);
        var newFile = Path.Combine(gameDir, "new.zip");
        var existingFile = Path.Combine(gameDir, "same.zip");
        await File.WriteAllBytesAsync(newFile, new byte[2048]);
        await File.WriteAllBytesAsync(existingFile, new byte[1024]);

        Directory.CreateDirectory(options.LocalMirrorPath);
        var mirrorGameDir = Path.Combine(options.LocalMirrorPath, "game");
        Directory.CreateDirectory(mirrorGameDir);
        await File.WriteAllBytesAsync(Path.Combine(mirrorGameDir, "same.zip"), new byte[1024]);
        await File.WriteAllBytesAsync(Path.Combine(options.LocalMirrorPath, "mirror-only.zip"), new byte[128]);

        var service = new LocalMirrorService(options, NullLogger<LocalMirrorService>.Instance);
        var result = await service.SyncAsync(CancellationToken.None);

        Assert.Equal(1, result.CopiedCount);
        Assert.Equal(2, result.VerifiedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Equal(3072, result.TotalBytes);
        Assert.True(File.Exists(Path.Combine(mirrorGameDir, "new.zip")));
        Assert.True(File.Exists(Path.Combine(options.LocalMirrorPath, "mirror-only.zip")));
        Assert.True(File.Exists(Path.Combine(options.LocalMirrorPath, ".gsc-mirror-sync.json")));

        var status = await service.StatusAsync(CancellationToken.None);
        Assert.True(status.Available);
        Assert.Equal(2, status.VerifiedCount);
        Assert.Contains("镜像可用", status.Message);
    }

    [Fact]
    public async Task SyncReplacesSameSizeCorruptMirrorWithHashMismatch()
    {
        options.EnableLocalMirror = true;
        Directory.CreateDirectory(options.LocalMirrorPath);
        var source = Path.Combine(options.LudusaviBackupDirectory, "save.zip");
        var destination = Path.Combine(options.LocalMirrorPath, "save.zip");
        await File.WriteAllBytesAsync(source, new byte[] { 1, 2, 3, 4 });
        await File.WriteAllBytesAsync(destination, new byte[] { 9, 8, 7, 6 });
        var service = new LocalMirrorService(options, NullLogger<LocalMirrorService>.Instance);

        var result = await service.SyncAsync(CancellationToken.None);

        Assert.Equal(1, result.CopiedCount);
        Assert.Equal(1, result.VerifiedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, await File.ReadAllBytesAsync(destination));
    }

    [Fact]
    public async Task SyncHonorsCancellation()
    {
        options.EnableLocalMirror = true;
        Directory.CreateDirectory(options.LocalMirrorPath);
        await File.WriteAllBytesAsync(Path.Combine(options.LudusaviBackupDirectory, "a.zip"), new byte[100]);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var service = new LocalMirrorService(options, NullLogger<LocalMirrorService>.Instance);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.SyncAsync(cts.Token));
    }

    public void Dispose()
    {
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
