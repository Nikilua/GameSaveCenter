using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveCenter.Playnite.ViewModels;

public sealed partial class DashboardViewModel
{
    private IReadOnlyList<FindingTriageGroup> findingTriageGroups = Array.Empty<FindingTriageGroup>();

    public IReadOnlyList<FindingTriageGroup> FindingTriageGroups
    {
        get => findingTriageGroups;
        private set => SetValue(ref findingTriageGroups, value);
    }

    public string FindingTriageSummary
        => string.Join(" · ", FindingTriageGroups.Select(group => $"{group.Title} {group.Count}"));

    private void ApplyFindingTriage(FindingTriageResult result)
    {
        FindingTriageGroups = result.Groups;
        OnPropertyChanged(nameof(FindingTriageSummary));
    }
}
