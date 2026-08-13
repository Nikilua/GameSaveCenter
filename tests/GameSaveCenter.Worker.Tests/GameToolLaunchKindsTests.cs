using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Services;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class GameToolLaunchKindsTests
{
    [Fact]
    public void RestartExactOnlyStopsTheTestOwnedExecutablePath()
    {
        var root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
        var first = Path.Combine(root, "one", "gsc-owned-process.exe");
        var second = Path.Combine(root, "two", "gsc-owned-process.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(first)!);
        Directory.CreateDirectory(Path.GetDirectoryName(second)!);
        var systemPing = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe");
        File.Copy(systemPing, first);
        File.Copy(systemPing, second);
        using var owned = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = first, Arguments = "127.0.0.1 -n 30 -w 1000", UseShellExecute = false, CreateNoWindow = true
        })!;
        try
        {
            Assert.True(SpinWait.SpinUntil(() => !owned.HasExited, TimeSpan.FromSeconds(2)));
            var samePath = GameToolProcessGuard.Scan(first);
            var differentPath = GameToolProcessGuard.Scan(second);

            Assert.Contains(owned.Id, samePath.MatchingProcessIds);
            Assert.DoesNotContain(owned.Id, differentPath.MatchingProcessIds);
            GameToolProcessGuard.RestartExact(first, samePath, TimeSpan.FromMilliseconds(250));
            Assert.True(owned.WaitForExit(3000));
        }
        finally
        {
            if (!owned.HasExited) owned.Kill(entireProcessTree: true);
            owned.WaitForExit(3000);
            Directory.Delete(root, recursive: true);
        }
    }

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
    [InlineData(GameToolIfAlreadyRunning.Skip, 1, false, GameToolProcessGuard.ExistingProcessAction.Skip)]
    [InlineData(GameToolIfAlreadyRunning.Restart, 1, false, GameToolProcessGuard.ExistingProcessAction.Restart)]
    [InlineData(GameToolIfAlreadyRunning.AllowAnotherInstance, 1, true, GameToolProcessGuard.ExistingProcessAction.Start)]
    [InlineData(GameToolIfAlreadyRunning.Skip, 0, false, GameToolProcessGuard.ExistingProcessAction.Start)]
    [InlineData(GameToolIfAlreadyRunning.Restart, 0, true, GameToolProcessGuard.ExistingProcessAction.BlockUnreadable)]
    public void ExistingProcessPolicyIsExplicitAndConservative(GameToolIfAlreadyRunning policy, int matches, bool unreadable, GameToolProcessGuard.ExistingProcessAction expected)
    {
        var scan = new GameToolProcessGuard.ScanResult(Enumerable.Range(1, matches).ToArray(), unreadable);
        Assert.Equal(expected, GameToolProcessGuard.Decide(policy, scan));
    }

    [Theory]
    [InlineData(GameToolType.CustomExecutable, GameToolRiskCategory.GeneralUtility, true, true)]
    [InlineData(GameToolType.CustomExecutable, GameToolRiskCategory.GameModification, true, false)]
    [InlineData(GameToolType.CustomExecutable, GameToolRiskCategory.Unknown, false, false)]
    [InlineData(GameToolType.Trainer, GameToolRiskCategory.Unknown, true, false)]
    [InlineData(GameToolType.CheatTable, GameToolRiskCategory.Unknown, true, false)]
    public void AntiCheatAutoStartUsesRiskCategory(GameToolType type, GameToolRiskCategory risk, bool antiCheat, bool expected)
    {
        var tool = new GameToolDto { ToolType = type, RiskCategory = risk };
        Assert.Equal(expected, GameToolAutoStartPolicy.IsAllowed(tool, antiCheat, out _));
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
