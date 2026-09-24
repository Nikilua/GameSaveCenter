using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class DiagnosticsEvidenceSourceTests
{
    [Fact]
    public void RenderAndHostAuditEntriesDeclareEvidenceBoundariesAndTimingFields()
    {
        var root = FindRepositoryRoot();
        var harness = File.ReadAllText(Path.Combine(root, "tests", "GameSaveCenter.RenderHarness", "Program.cs"));
        var hostScript = File.ReadAllText(Path.Combine(root, "scripts", "real-host-audit.ps1"));
        var installScript = File.ReadAllText(Path.Combine(root, "scripts", "dev-install-run.ps1"));
        var hostSeeder = File.ReadAllText(Path.Combine(root, "tests", "GameSaveCenter.Playnite.HostAuditSeeder", "HostAuditSeederPlugin.cs"));

        Assert.Contains("EvidenceSource: {sourceKind}", harness);
        Assert.Contains("ResolveGitValue(\"rev-parse HEAD\")", harness);
        Assert.Contains("layout_ms=", harness);
        Assert.Contains("render_ms=", harness);
        Assert.Contains("request_ms=not-applicable", harness);
        Assert.Contains("runner-metadata.json", hostScript);
        Assert.Contains("EvidenceSource = 'RealPlaynite'", hostScript);
        Assert.Contains("DpiScale = 'captured by WPF VisualTreeHelper.GetDpi'", hostScript);
        Assert.Contains("Timing = 'capture manifest includes", hostScript);
        Assert.Contains("System.Windows.Forms.Screen]::AllScreens", hostScript);
        Assert.Contains("DisplayTopology", hostScript);
        Assert.Contains("Q24_03PhysicalCrossScreen", hostScript);
        Assert.Contains("blocked-single-display", hostScript);
        Assert.Contains("GSC_UI_AUDIT_PIPE_NAME", hostScript);
        Assert.Contains("GSC_UI_AUDIT_EVENT_PIPE_NAME", hostScript);
        Assert.Contains("GameSaveCenter.Worker.Audit.", hostScript);
        Assert.Contains("IpcIsolation", hostScript);
        Assert.Contains("Get-PlayniteProcessSnapshot", hostScript);
        Assert.Contains("$process.Refresh()", hostScript);
        Assert.Contains("Where-Object { $_.MainWindowHandle -ne 0 }", hostScript);
        Assert.Contains("GscTopLevelWindowProbe", hostScript);
        Assert.Contains("Get-PlayniteUiAutomationCandidates", hostScript);
        Assert.Contains("win32-top-level-enumeration", hostScript);
        Assert.Contains("AutomationRootControlType", hostScript);
        Assert.Contains("CandidateWindows", hostScript);
        Assert.Contains("MatchedWindows", hostScript);
        Assert.Contains("UiAutomation = $UiAutomationProbe", hostScript);
        Assert.Contains("host-window-exposure.json", hostScript);
        Assert.Contains("playnite-process-without-top-level-window", hostScript);
        Assert.Contains("top-level-window-observed-ui-automation-not-confirmed", hostScript);
        Assert.Contains("SidebarAutomationFound", hostScript);
        Assert.Contains("SkipInstallTests", hostScript);
        Assert.Contains("skipped-by-explicit-audit-switch", hostScript);
        Assert.Contains("Initialize-IsolatedPlayniteConfig", hostScript);
        Assert.Contains("Backup\\config.json", hostScript);
        Assert.Contains("restored-from-isolated-backup", hostScript);
        Assert.Contains("host-startup-blocker.json", hostScript);
        Assert.Contains("cef-startup-access-denied-before-main-window", hostScript);
        Assert.Contains("CountsAsVisualPass = $false", hostScript);
        Assert.Contains("if ($commit) {", hostScript);
        Assert.DoesNotContain("if ($LASTEXITCODE -eq 0 -and $commit)", hostScript);
        Assert.Contains("SeedSyntheticLibrary", hostScript);
        Assert.Contains("Synthetic library seeding is restricted to an isolated profile below repository .tmp", hostScript);
        Assert.Contains("GameSaveCenter_AuditSeeder_$seederId", hostScript);
        Assert.Contains("GSC_UI_AUDIT_SEED_RUN_ID", hostScript);
        Assert.Contains("RuntimeManifestObserved", hostScript);
        Assert.Contains("not-observed", hostScript);
        Assert.Contains("SkipPackageArchives", hostScript);
        Assert.Contains("RunLogPath = Join-Path $Output 'dev-install.log'", hostScript);
        Assert.Contains("InstallReportPath = Join-Path $Output 'dev-install-report.txt'", hostScript);
        Assert.Contains("$packageArguments = @{", installScript);
        Assert.Contains("Configuration = $Configuration", installScript);
        Assert.Contains("SkipBuild = $true", installScript);
        Assert.Contains("BuildOutputRoot = $buildOutputRoot", installScript);
        Assert.Contains("$packageArguments.SkipPackageArchives = $true", installScript);
        Assert.Contains("@packageArguments", installScript);
        Assert.DoesNotContain("@('-Configuration', $Configuration", installScript);
        Assert.Contains("GSC_UI_AUDIT_SEED_DIRECTORY", hostSeeder);
        Assert.Contains("Guid.TryParse(runId, out _)", hostSeeder);
        Assert.Contains("PlayniteApi.Database.ImportGame(metadata)", hostSeeder);
        Assert.Contains("synthetic-library-manifest.json", hostSeeder);
    }

    [Fact]
    public void Legacy52ReviewTableKeepsEveryOriginalStateReasonAndNewMapping()
    {
        var root = FindRepositoryRoot();
        var review = File.ReadAllLines(Path.Combine(root, "docs", "design", "UI_FINESSE_REVIEW_2026-09-13.md"));
        var rows = review
            .Where(line => line.StartsWith("| P", StringComparison.Ordinal))
            .Select(line => line.Split('|').Select(cell => cell.Trim()).ToArray())
            .ToArray();

        Assert.Equal(52, rows.Length);
        Assert.Equal(52, rows.Select(row => row[1]).Distinct(StringComparer.Ordinal).Count());
        Assert.All(rows, row =>
        {
            Assert.True(row.Length >= 6);
            Assert.StartsWith("P", row[1], StringComparison.Ordinal);
            Assert.NotEqual(string.Empty, row[2]);
            Assert.NotEqual(string.Empty, row[3]);
            Assert.NotEqual(string.Empty, row[4]);
            Assert.StartsWith("Q", row[5], StringComparison.Ordinal);
        });

        var states = rows.GroupBy(row => row[2]).ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        Assert.Equal(14, states["已验收"]);
        Assert.Equal(1, states["已满足无需修改"]);
        Assert.Equal(31, states["代码完成待验收"]);
        Assert.Equal(6, states["外部阻塞"]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(64)]
    [InlineData(512)]
    public void SyntheticHostAuditCatalogIsBoundedStableAndHasNoInstallTargets(int count)
    {
        var games = GameSaveCenter.Playnite.HostAuditSeeder.SyntheticGameCatalog.Create(count);

        Assert.Equal(count, games.Count);
        Assert.Equal(count, games.Select(game => game.GameId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(games, game =>
        {
            Assert.StartsWith(GameSaveCenter.Playnite.HostAuditSeeder.SyntheticGameCatalog.GameIdPrefix, game.GameId, StringComparison.Ordinal);
            Assert.StartsWith(GameSaveCenter.Playnite.HostAuditSeeder.SyntheticGameCatalog.NamePrefix, game.Name, StringComparison.Ordinal);
            Assert.False(game.IsInstalled);
            Assert.Null(game.InstallDirectory);
        });
        Assert.Equal(games.Select(game => game.GameId),
            GameSaveCenter.Playnite.HostAuditSeeder.SyntheticGameCatalog.Create(count).Select(game => game.GameId));
    }

    [Fact]
    public void SyntheticHostAuditCatalogRejectsUnboundedRequests()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameSaveCenter.Playnite.HostAuditSeeder.SyntheticGameCatalog.Create(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => GameSaveCenter.Playnite.HostAuditSeeder.SyntheticGameCatalog.Create(513));
    }

    private static string FindRepositoryRoot()
        => TestRepositoryContext.Root;
}
