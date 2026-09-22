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
        => TestRepositoryContext.Root;
}
