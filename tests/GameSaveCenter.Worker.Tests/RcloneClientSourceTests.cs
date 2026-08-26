using System.IO;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class RcloneClientSourceTests
{
    [Fact]
    public void SafeRunnerDoesNotExposeAWorkingDirectoryThatIsPassedAsStandardInput()
    {
        var root = FindRepositoryRoot();
        var source = File.ReadAllText(Path.Combine(
            root,
            "src",
            "GameSaveCenter.Worker",
            "Infrastructure",
            "RcloneClient.cs"));

        Assert.DoesNotContain("workingDirectory", source, StringComparison.Ordinal);
        Assert.Contains("RunSafeAsync(IReadOnlyList<string> arguments, TimeSpan timeout, CancellationToken token)", source, StringComparison.Ordinal);
        Assert.Contains("_runner.RunAsync(_options.RcloneExecutable, arguments, null, timeout, token)", source, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
