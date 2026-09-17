using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class SettingsAsyncValidationTests
{
    [Fact]
    public async Task SlowOlderRequestCannotOverwriteTheLatestRequest()
    {
        var olderStarted = new TaskCompletionSource<bool>();
        var olderCompletion = new TaskCompletionSource<string>();
        var latestApplied = new TaskCompletionSource<string>();
        var applied = new List<string>();
        using var coordinator = new LatestAsyncValidationCoordinator<string, string>();

        coordinator.Start(
            "A",
            async (_, _) =>
            {
                olderStarted.SetResult(true);
                return await olderCompletion.Task.ConfigureAwait(false);
            },
            (_, result) =>
            {
                applied.Add(result);
                if (result == "B") latestApplied.TrySetResult(result);
            },
            (_, exception) => throw exception);
        await olderStarted.Task;

        coordinator.Start(
            "B",
            (_, _) => Task.FromResult("B"),
            (_, result) =>
            {
                applied.Add(result);
                latestApplied.TrySetResult(result);
            },
            (_, exception) => throw exception);

        Assert.True(await CompletesWithin(latestApplied.Task));
        olderCompletion.SetResult("A");
        await Task.Delay(80);

        Assert.Equal(new[] { "B" }, applied);
    }

    [Fact]
    public async Task CancelSuppressesACompletedRequestAfterThePageLeaves()
    {
        var started = new TaskCompletionSource<bool>();
        var completion = new TaskCompletionSource<string>();
        var applied = false;
        using var coordinator = new LatestAsyncValidationCoordinator<string, string>();

        coordinator.Start(
            "page",
            async (_, _) =>
            {
                started.SetResult(true);
                return await completion.Task.ConfigureAwait(false);
            },
            (_, _) => applied = true,
            (_, exception) => throw exception);
        await started.Task;

        coordinator.Cancel();
        completion.SetResult("late");
        await Task.Delay(80);

        Assert.False(applied);
    }

    [Fact]
    public async Task BackgroundPathValidationUsesTheExistingDirectoryRules()
    {
        var root = Path.Combine(Path.GetTempPath(), "gsc-settings-async-path-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var worker = Path.Combine(root, "GameSaveCenter.Worker.exe");
            var marker = Path.Combine(root, "backup-marker.dat");
            File.WriteAllText(worker, "worker");
            File.WriteAllText(marker, "not a directory");
            var settings = new GameSaveCenterSettings
            {
                WorkerExecutable = worker,
                LudusaviBackupDirectory = marker,
                MediaArchiveDirectory = Path.Combine(root, "Media")
            };

            var errors = await SettingsPathValidationService.ValidateAsync(
                settings.CreatePathValidationSnapshot(),
                CancellationToken.None);

            Assert.Contains("存档目录路径指向文件", string.Join("；", errors));
            Assert.False(Directory.Exists(settings.MediaArchiveDirectory));
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { }
        }
    }

    private static async Task<bool> CompletesWithin(Task task)
    {
        var winner = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(2)));
        return ReferenceEquals(winner, task);
    }
}
