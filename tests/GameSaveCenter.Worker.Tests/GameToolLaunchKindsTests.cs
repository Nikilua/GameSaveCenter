using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Services;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class GameToolLaunchKindsTests
{
    [Fact]
    public void NewToolDefaultsToConservativePolicies()
    {
        var tool = new GameToolDto();

        Assert.Equal(GameToolIfAlreadyRunning.Skip, tool.IfAlreadyRunning);
        Assert.Equal(GameToolRiskCategory.Unknown, tool.RiskCategory);
    }

    [Theory]
    [InlineData(@"C:\Tools\same.exe", @"c:\tools\same.exe", true)]
    [InlineData(@"C:\Tools\same.exe", @"C:\Other\same.exe", false)]
    public void ProcessGuardComparesExecutablePathRatherThanProcessName(string left, string right, bool expected)
    {
        Assert.Equal(expected, GameToolProcessGuard.PathsEqual(left, right));
    }

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
