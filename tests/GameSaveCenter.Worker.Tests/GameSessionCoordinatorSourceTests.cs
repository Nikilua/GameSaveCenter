using System.IO;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class GameSessionCoordinatorSourceTests
{
    [Fact]
    public void DetachedSessionOperationsFollowWorkerShutdown()
    {
        var root=FindRepositoryRoot();
        var source=File.ReadAllText(Path.Combine(root,"src","GameSaveCenter.Worker","Services","GameSessionCoordinator.cs"));

        Assert.Contains("IHostApplicationLifetime? _lifetime",source,StringComparison.Ordinal);
        Assert.Contains("IHostApplicationLifetime? lifetime=null",source,StringComparison.Ordinal);
        Assert.Contains("private CancellationToken ApplicationStopping => _lifetime?.ApplicationStopping ?? CancellationToken.None;",source,StringComparison.Ordinal);
        Assert.DoesNotContain("StartAutomaticAsync(incoming,CancellationToken.None)",source,StringComparison.Ordinal);
        Assert.DoesNotContain("CancellationToken.None),\"exit backup\"",source,StringComparison.Ordinal);
        Assert.DoesNotContain("CancellationToken.None),\"exit media sync\"",source,StringComparison.Ordinal);
        Assert.DoesNotContain("CancellationToken.None),\"timed media sync\"",source,StringComparison.Ordinal);
        Assert.DoesNotContain("},CancellationToken.None),\"timed backup\"",source,StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory=new DirectoryInfo(AppContext.BaseDirectory);
        while(directory!=null&&!File.Exists(Path.Combine(directory.FullName,"GameSaveCenter.sln")))
            directory=directory.Parent;
        return directory?.FullName??throw new DirectoryNotFoundException("Could not locate GameSaveCenter.sln.");
    }
}
