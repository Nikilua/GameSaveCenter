using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R18WorkspaceRevisitSourceTests
{
    [Fact]
    public void ProductionShellMeasuresFirstActivationAndKeepsCachedPageIdentity()
    {
        var shell = Read("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml.cs");

        Assert.Contains("private readonly Dictionary<WorkspaceKind, UserControl> pages", shell);
        Assert.Contains("private readonly HashSet<WorkspaceKind> activatedWorkspaces", shell);
        Assert.Contains("var firstActivation = activatedWorkspaces.Add(workspace)", shell);
        Assert.Contains("var contentChanged = !ReferenceEquals(PageHost.Content, page)", shell);
        Assert.Contains("page={(contentChanged ? \"attach\" : \"reuse\")}", shell);
        Assert.Contains("WorkspacePages created={pages.Count} binding=", shell);
    }

    [Fact]
    public void WorkspaceRevisitReadsAreScopedAndDoNotCallFullLibraryRefresh()
    {
        var root = TestRepositoryContext.Root;
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var start = viewModel.IndexOf("public void RequestWorkspaceLoad()", StringComparison.Ordinal);
        var end = viewModel.IndexOf("private bool IsSelectedGame", start, StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start);
        var request = viewModel.Substring(start, end - start);

        Assert.Contains("RunWorkspaceLoad", request);
        Assert.DoesNotContain("RefreshDashboard", request);
        Assert.DoesNotContain("Synchronize", request);
        Assert.Contains("GetWorkspaceLoadContextKey", viewModel);
        Assert.Contains("workspaceRevisitLoadGate.InvalidateAll();", viewModel);
    }

    private static string Read(params string[] segments)
        => File.ReadAllText(Path.Combine(new[] { TestRepositoryContext.Root }.Concat(segments).ToArray()));
}
