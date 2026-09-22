using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R17QuarantineLedgerSourceTests
{
    [Fact]
    public void MaintenanceLedgerShowsBothDurablePathsForQuarantineEntries()
    {
        var root = TestRepositoryContext.Root;
        var actions = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.MaintenanceActions.cs"));
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        Assert.Contains("OriginalPathDisplay", actions, StringComparison.Ordinal);
        Assert.Contains("QuarantinePathDisplay", actions, StringComparison.Ordinal);
        Assert.Contains("原路径：", maintenance, StringComparison.Ordinal);
        Assert.Contains("隔离路径：", maintenance, StringComparison.Ordinal);
        Assert.Contains("Value=\"RetentionQuarantine\"", maintenance, StringComparison.Ordinal);
    }

    [Fact]
    public void MaintenanceLedgerUsesTheExistingConfirmedTargetedRecoveryEntryPoint()
    {
        var root = TestRepositoryContext.Root;
        var actions = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.MaintenanceActions.cs"));
        var dispatcher = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "IpcRequestDispatcher.cs"));

        Assert.Contains("ActionText = \"受控恢复\"", actions, StringComparison.Ordinal);
        Assert.Contains("RetentionQuarantineRecoveryRequestDto { EntryId = item.EntryId, Confirmed = true }", actions, StringComparison.Ordinal);
        Assert.Contains("RecoverRetentionQuarantine", dispatcher, StringComparison.Ordinal);
    }
}
