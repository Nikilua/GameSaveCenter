using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class GameToolLaunchKindsTests
{
    [Theory]
    [InlineData(@"C:\Tools\app.exe", GameToolLaunchKind.Executable, true)]
    [InlineData(@"C:\Tools\app.lnk", GameToolLaunchKind.Shortcut, false)]
    [InlineData(@"C:\Tools\fix.bat", GameToolLaunchKind.BatchScript, false)]
    [InlineData(@"C:\Tools\fix.cmd", GameToolLaunchKind.BatchScript, false)]
    [InlineData(@"C:\Tools\launch.ps1", GameToolLaunchKind.PowerShellScript, false)]
    [InlineData(@"C:\Tools\notes.txt", GameToolLaunchKind.ShellDocument, false)]
    [InlineData(@"", GameToolLaunchKind.ShellDocument, false)]
    public void FromPath_ClassifiesExtensionAndTracking(string path, GameToolLaunchKind expectedKind, bool expectedTrackable)
    {
        Assert.Equal(expectedKind, GameToolLaunchKinds.FromPath(path));
        Assert.Equal(expectedTrackable, GameToolLaunchKinds.CanTrackProcess(path));
    }
}
