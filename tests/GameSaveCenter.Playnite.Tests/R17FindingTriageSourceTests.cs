using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R17FindingTriageSourceTests
{
    [Fact]
    public void PersistenceCarriesEvidenceTimeAndExcludesResolvedRows()
    {
        var root = TestRepositoryContext.Root;
        var store = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Persistence", "SqliteStateStore.cs"));

        Assert.Contains("suggested_action,created_utc FROM findings WHERE resolved=0", store, StringComparison.Ordinal);
        Assert.Contains("CreatedUtc=reader.IsDBNull(6)?DateTime.MinValue", store, StringComparison.Ordinal);
        Assert.Contains("VALUES($id,$game,$severity,$code,$title,$detail,$action,$utc,0)", store, StringComparison.Ordinal);
    }

    [Fact]
    public void MaintenanceViewShowsThreeImpactGroupsAndEvidenceTimeWithoutReplacingTheExistingGrid()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var resolver = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "FindingTriageResolver.cs"));

        Assert.Contains("ItemsSource=\"{Binding FindingTriageGroups}\"", view, StringComparison.Ordinal);
        Assert.Contains("SelectedFinding.EvidenceTimeDisplay", view, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding Findings}\"", view, StringComparison.Ordinal);
        Assert.Contains("data.Findings = findingTriage.Items.ToList();", viewModel, StringComparison.Ordinal);
        Assert.Contains("GroupBy(BuildProblemKey", resolver, StringComparison.Ordinal);
    }
}
