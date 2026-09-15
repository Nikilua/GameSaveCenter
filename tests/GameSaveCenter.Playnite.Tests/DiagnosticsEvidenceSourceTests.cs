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
        Assert.Contains("try { $_.Refresh() } catch { }", hostScript);
        Assert.Contains("Where-Object { $_.MainWindowHandle -ne 0 }", hostScript);
        Assert.Contains("if ($commit) {", hostScript);
        Assert.DoesNotContain("if ($LASTEXITCODE -eq 0 -and $commit)", hostScript);
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

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
