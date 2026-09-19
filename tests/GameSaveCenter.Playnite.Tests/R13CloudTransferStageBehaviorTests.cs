using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R13CloudTransferStageBehaviorTests
{
    [Fact]
    public void MaintenanceQueueBindsStageAndKeepsGuaranteeSeparate()
    {
        var root = TestRepositoryContext.Root;
        var source = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        Assert.Contains("QueuePhaseDisplay", source);
        Assert.Contains("GuaranteeLevelDisplay", source);
        Assert.Contains("QueueControlDisplay", source);
    }
}
