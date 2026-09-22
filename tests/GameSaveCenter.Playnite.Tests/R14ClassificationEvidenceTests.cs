using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R14ClassificationEvidenceTests
{
    [Fact]
    public void ClassificationPreviewShowsStructuredEvidenceAndPendingNegativeState()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var contracts = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "MediaClassificationDtos.cs"));
        var worker = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Services", "MediaSyncService.cs"));

        Assert.Contains("MediaClassificationPreview.Items", view, StringComparison.Ordinal);
        Assert.Contains("EvidenceSummaryDisplay", view, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding Evidence}\"", view, StringComparison.Ordinal);
        Assert.Contains("待判断：没有可核实的来源规则、游戏会话、进程映射或文件名依据。", view, StringComparison.Ordinal);
        Assert.Contains("MediaClassificationEvidenceDto", contracts, StringComparison.Ordinal);
        Assert.Contains("\"SourceRule\"", worker, StringComparison.Ordinal);
        Assert.Contains("\"GameSession\"", worker, StringComparison.Ordinal);
        Assert.Contains("\"ProcessMapping\"", worker, StringComparison.Ordinal);
    }
}
