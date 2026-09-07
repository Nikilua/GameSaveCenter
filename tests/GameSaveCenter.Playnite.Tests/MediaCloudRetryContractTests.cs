using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class MediaCloudRetryContractTests
{
    [Fact]
    public void MediaRetryUsesDedicatedIpcAndNeverFallsBackToMediaSync()
    {
        var root = FindRepositoryRoot();
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.CloudTransfers.cs"));
        var dispatcher = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "IpcRequestDispatcher.cs"));
        var semantics = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "IpcRequestSemantics.cs"));

        Assert.Contains("MessageTypes.RetryMediaCloudUpload", viewModel);
        Assert.DoesNotContain("MessageTypes.SyncMedia", viewModel);
        Assert.Contains("RetryCloudUploadForUserAsync", dispatcher);
        Assert.Contains("MessageTypes.RetryMediaCloudUpload", semantics);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
