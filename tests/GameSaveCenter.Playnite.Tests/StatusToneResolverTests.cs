using System.Collections.Generic;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class StatusToneResolverTests
{
    [Fact]
    public void CloudQueueToneTracksActualQueueStateWithFailurePrecedence()
    {
        Assert.Equal("Neutral", StatusToneResolver.CloudTransfers(null));
        Assert.Equal("Neutral", StatusToneResolver.CloudTransfers(new CloudTransferSummaryDto()));
        Assert.Equal("Neutral", StatusToneResolver.CloudTransfers(new CloudTransferSummaryDto { PendingCount = 2 }));
        Assert.Equal("Info", StatusToneResolver.CloudTransfers(new CloudTransferSummaryDto { TransferringCount = 1 }));
        Assert.Equal("Info", StatusToneResolver.CloudTransfers(new CloudTransferSummaryDto { VerifyingCount = 1 }));
        Assert.Equal("Success", StatusToneResolver.CloudTransfers(new CloudTransferSummaryDto { VerifiedCount = 1 }));
        Assert.Equal("Success", StatusToneResolver.CloudTransfers(new CloudTransferSummaryDto { UploadedCount = 1 }));
        Assert.Equal("Warning", StatusToneResolver.CloudTransfers(new CloudTransferSummaryDto { RetryScheduledCount = 1 }));
        Assert.Equal("Error", StatusToneResolver.CloudTransfers(new CloudTransferSummaryDto { AuthenticationRequiredCount = 1 }));
        Assert.Equal("Error", StatusToneResolver.CloudTransfers(new CloudTransferSummaryDto { CheckFailedCount = 1 }));
        Assert.Equal("Error", StatusToneResolver.CloudTransfers(new CloudTransferSummaryDto { FailedCount = 1 }));
        Assert.Equal("Error", StatusToneResolver.CloudTransfers(new CloudTransferSummaryDto
        {
            RetryScheduledCount = 2,
            AuthenticationRequiredCount = 1,
            TransferringCount = 1
        }));
    }

    [Fact]
    public void EnvironmentCheckToneTracksFailuresWarningsPassesAndInProgressChecks()
    {
        Assert.Equal("Neutral", StatusToneResolver.EnvironmentCheck(null));
        Assert.Equal("Neutral", StatusToneResolver.EnvironmentCheck(new EnvironmentCheckReportDto()));
        Assert.Equal("Info", StatusToneResolver.EnvironmentCheck(new EnvironmentCheckReportDto
        {
            Items = new List<EnvironmentCheckItemDto>
            {
                new() { State = EnvironmentCheckState.Checking }
            }
        }));
        Assert.Equal("Success", StatusToneResolver.EnvironmentCheck(new EnvironmentCheckReportDto { PassedCount = 3 }));
        Assert.Equal("Warning", StatusToneResolver.EnvironmentCheck(new EnvironmentCheckReportDto { PassedCount = 2, WarningCount = 1 }));
        Assert.Equal("Error", StatusToneResolver.EnvironmentCheck(new EnvironmentCheckReportDto { WarningCount = 1, FailedCount = 1 }));
    }
}
