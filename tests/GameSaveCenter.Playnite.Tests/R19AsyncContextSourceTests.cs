using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R19AsyncContextSourceTests
{
    [Fact]
    public void DetailsReadsRequireTheStartingGameWorkspaceAndGenerationAtCommitBoundaries()
    {
        var root = TestRepositoryContext.Root;
        var implementation = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var detailsStart = implementation.IndexOf("private async Task LoadDetailsAsync", StringComparison.Ordinal);
        var detailsEnd = implementation.IndexOf("public void RequestWorkspaceLoad()", detailsStart, StringComparison.Ordinal);
        var details = implementation.Substring(detailsStart, detailsEnd - detailsStart);
        var workspaceStart = implementation.IndexOf("public WorkspaceKind CurrentWorkspace", StringComparison.Ordinal);
        var workspaceEnd = implementation.IndexOf("public LayoutMode LayoutMode", workspaceStart, StringComparison.Ordinal);
        var workspace = implementation.Substring(workspaceStart, workspaceEnd - workspaceStart);
        var cancelStart = implementation.IndexOf("private void CancelDetailsLoad()", StringComparison.Ordinal);
        var cancelEnd = implementation.IndexOf("private CancellationTokenSource BeginMediaPageRequest()", cancelStart, StringComparison.Ordinal);
        var cancel = implementation.Substring(cancelStart, cancelEnd - cancelStart);

        Assert.Contains("var requestWorkspace = CurrentWorkspace", details);
        Assert.Contains("var requestGeneration = expectedGeneration != 0", details);
        Assert.Contains("IsCurrentDetailsLoad(id, cancellationToken, requestGeneration, requestWorkspace)", details);
        Assert.Contains("CurrentWorkspace == expectedWorkspace", details);
        Assert.Contains("if (currentWorkspace != value)", workspace);
        Assert.Contains("CancelDetailsLoad();", workspace);
        Assert.Contains("Interlocked.Increment(ref mediaPageGeneration);", cancel);
        Assert.Contains("FailMediaDetailsLoad(ex, mediaRequestGeneration)", details);
    }
}
