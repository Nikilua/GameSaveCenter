using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class DiagnosticsEvidenceSourceTests
{
    [Fact]
    public void IsolatedHostAuditFailsClosedAndNeverShutsDownTheUserHost()
    {
        var root = FindRepositoryRoot();
        var hostScript = File.ReadAllText(Path.Combine(root, "scripts", "real-host-audit.ps1"));
        var installScript = File.ReadAllText(Path.Combine(root, "scripts", "dev-install-run.ps1"));
        var isolationHelpers = File.ReadAllText(Path.Combine(root, "scripts", "PlayniteHostIsolation.ps1"));

        Assert.Contains("Real-host audits require an explicit user-data profile under repository .tmp.", hostScript);
        Assert.Contains("Real-host audits require an explicit Playnite.DesktopApp.exe path.", hostScript);
        Assert.Contains("Resolve-GscRepositoryScopedPath", hostScript);
        Assert.Contains("Initialize-GscIsolatedPlayniteProfile", hostScript);
        Assert.Contains("Assert-GscNoActivePlayniteProcesses -Processes $initialHostProcessSnapshot", hostScript);
        Assert.Contains("SkipPlayniteShutdownForIsolatedTarget = $true", hostScript);
        Assert.Contains("Get-GscPlayniteProcessStartEvidence", hostScript);
        Assert.DoesNotContain("Remove-Item -LiteralPath $Output -Recurse -Force", hostScript);

        Assert.Contains("function Resolve-GscRepositoryScopedPath", isolationHelpers);
        Assert.Contains("function Assert-GscNoReparsePointsUnderPath", isolationHelpers);
        Assert.Contains("function Assert-GscPlayniteProfileDatabaseIsolation", isolationHelpers);
        Assert.Contains("ReparsePoint", isolationHelpers);
        Assert.Contains("function Initialize-GscIsolatedPlayniteProfile", isolationHelpers);
        Assert.Contains("Refusing to overwrite existing Playnite host audit output", isolationHelpers);
        Assert.Contains("--userdatadir", isolationHelpers);
        Assert.Contains("function Get-GscPlayniteProcessStartEvidence", isolationHelpers);

        Assert.Contains("[switch]$SkipPlayniteShutdownForIsolatedTarget", installScript);
        Assert.Contains("SkipPlayniteShutdownForIsolatedTarget requires -NoStart.", installScript);
        Assert.Contains("SkipPlayniteShutdownForIsolatedTarget requires an explicit isolated PlayniteExtensionsPath under repository .tmp.", installScript);
        Assert.Contains("Assert-GscNoActivePlayniteProcesses -Processes $isolationConflicts", installScript);
        Assert.Contains("if ($SkipPlayniteShutdownForIsolatedTarget)", installScript);
        Assert.Contains("Stop-PlayniteAndOwnedWorkerReliably @stopArguments", installScript);
    }

    [Fact]
    public void RenderAndHostAuditEntriesDeclareEvidenceBoundariesAndTimingFields()
    {
        var root = FindRepositoryRoot();
        var harness = File.ReadAllText(Path.Combine(root, "tests", "GameSaveCenter.RenderHarness", "Program.cs"));
        var hostScript = File.ReadAllText(Path.Combine(root, "scripts", "real-host-audit.ps1"));
        var installScript = File.ReadAllText(Path.Combine(root, "scripts", "dev-install-run.ps1"));
        var hostSeeder = File.ReadAllText(Path.Combine(root, "tests", "GameSaveCenter.Playnite.HostAuditSeeder", "HostAuditSeederPlugin.cs"));
        var isolationHelpers = File.ReadAllText(Path.Combine(root, "scripts", "PlayniteHostIsolation.ps1"));

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
        Assert.Contains("New-IsolatedBootstrapFailureEvidence", hostScript);
        Assert.Contains("IsolatedProfileBootstrapFailure = $bootstrapFailureEvidence", hostScript);
        Assert.Contains("CefAccessDeniedMatches = $cefAccessDeniedMatches", hostScript);
        Assert.Contains("cef-startup-access-denied-before-main-window", hostScript);
        Assert.Contains("isolated-profile-bootstrap-failed-before-host-install", hostScript);
        Assert.Contains("CountsAsVisualPass = $false", hostScript);
        Assert.Contains("Backup\\config.json", hostScript);
        Assert.Contains("restored-from-isolated-backup", hostScript);
        Assert.Contains("host-startup-blocker.json", hostScript);
        Assert.Contains("cef-startup-access-denied-before-main-window", hostScript);
        Assert.Contains("CountsAsVisualPass = $false", hostScript);
        Assert.Contains("if ($commit) {", hostScript);
        Assert.DoesNotContain("if ($LASTEXITCODE -eq 0 -and $commit)", hostScript);
        Assert.Contains("SeedSyntheticLibrary", hostScript);
        Assert.Contains("Path must be a descendant of repository ${ScopeRootName}", isolationHelpers);
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
    public void IsolatedHostBootstrapClosesGracefullyAndFindsInstalledDesktopThemes()
    {
        var root = FindRepositoryRoot();
        var hostScript = File.ReadAllText(Path.Combine(root, "scripts", "real-host-audit.ps1"));
        var bootstrapStart = hostScript.IndexOf("function Initialize-IsolatedPlayniteConfig", StringComparison.Ordinal);
        var bootstrapEnd = hostScript.IndexOf("Initialize-IsolatedPlayniteConfig\n", bootstrapStart, StringComparison.Ordinal);

        Assert.True(bootstrapStart >= 0);
        Assert.True(bootstrapEnd > bootstrapStart);
        var bootstrap = hostScript.Substring(bootstrapStart, bootstrapEnd - bootstrapStart);
        Assert.Contains("WaitForExit(20000)", bootstrap);
        Assert.Contains("safestart.flag", bootstrap);
        Assert.DoesNotContain("Stop-Process -Id $bootstrapProcess.Id -Force", bootstrap);
        Assert.Contains("function Find-PlayniteDesktopThemeDirectory", hostScript);
        Assert.DoesNotContain("$env:APPDATA", hostScript);
        var themeLookupStart = hostScript.IndexOf("function Find-PlayniteDesktopThemeDirectory", StringComparison.Ordinal);
        var themeLookupEnd = hostScript.IndexOf("function New-IsolatedBootstrapFailureEvidence", themeLookupStart, StringComparison.Ordinal);
        Assert.True(themeLookupEnd > themeLookupStart);
        Assert.DoesNotContain("APPDATA", hostScript.Substring(themeLookupStart, themeLookupEnd - themeLookupStart));
        Assert.Contains("Themes\\Desktop", hostScript);
        Assert.Contains("theme.yaml", hostScript);

        var bootstrapFunction = hostScript.IndexOf("function Initialize-IsolatedPlayniteConfig", StringComparison.Ordinal);
        var bootstrapInvocation = hostScript.IndexOf("Initialize-IsolatedPlayniteConfig", bootstrapFunction + "function Initialize-IsolatedPlayniteConfig".Length, StringComparison.Ordinal);
        var failureEvidenceWrite = hostScript.IndexOf("$runnerMetadata.IsolatedProfileBootstrapFailure = $bootstrapFailureEvidence", StringComparison.Ordinal);
        var extensionInstall = hostScript.IndexOf("dev-install-run.ps1", bootstrapInvocation, StringComparison.Ordinal);
        Assert.True(bootstrapFunction >= 0);
        Assert.True(bootstrapInvocation >= 0);
        Assert.True(failureEvidenceWrite > bootstrapInvocation && failureEvidenceWrite < extensionInstall,
            "bootstrap failures must persist the runner and blocker evidence before the extension installer can run");
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
