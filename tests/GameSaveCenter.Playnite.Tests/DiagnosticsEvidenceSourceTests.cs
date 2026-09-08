using System;
using System.IO;
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
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
