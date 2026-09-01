using System.IO;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class GameToolLifecycleSourceTests
{
    [Fact]
    public void DelayedAutomaticLaunchAuditsUseTheLaunchCancellationToken()
    {
        var root=FindRepositoryRoot();
        var source=File.ReadAllText(Path.Combine(root,"src","GameSaveCenter.Worker","Services","GameToolService.cs"));
        var start=source.IndexOf("private async Task LaunchAfterDelayAsync",StringComparison.Ordinal);
        var end=source.IndexOf("private (Process Process,bool Trackable)",start,StringComparison.Ordinal);
        Assert.True(start>=0&&end>start,"Could not locate the delayed launch method.");
        var method=source.Substring(start,end-start);

        Assert.Contains("TryAppendAutoStartAuditAsync",method,StringComparison.Ordinal);
        Assert.DoesNotContain("CancellationToken.None",method,StringComparison.Ordinal);
        Assert.Contains("private async Task TryAppendAutoStartAuditAsync",source,StringComparison.Ordinal);
        Assert.Contains("if(token.IsCancellationRequested)",method,StringComparison.Ordinal);
        Assert.Contains("Could not persist automatic game tool audit",source,StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory=new DirectoryInfo(AppContext.BaseDirectory);
        while(directory!=null&&!File.Exists(Path.Combine(directory.FullName,"GameSaveCenter.sln")))
            directory=directory.Parent;
        return directory?.FullName??throw new DirectoryNotFoundException("Could not locate GameSaveCenter.sln.");
    }
}
