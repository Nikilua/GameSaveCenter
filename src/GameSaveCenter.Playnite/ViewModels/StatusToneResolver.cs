using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels;

internal static class StatusToneResolver
{
    public static string CloudTransfers(CloudTransferSummaryDto? summary)
    {
        if (summary == null) return "Neutral";
        if (summary.AuthenticationRequiredCount > 0
            || summary.CheckFailedCount > 0
            || summary.FailedCount > 0)
            return "Error";
        if (summary.RetryScheduledCount > 0) return "Warning";
        if (summary.TransferringCount > 0 || summary.VerifyingCount > 0) return "Info";
        if (summary.VerifiedCount > 0 || summary.UploadedCount > 0) return "Success";
        return "Neutral";
    }

    public static string EnvironmentCheck(EnvironmentCheckReportDto? report)
    {
        if (report == null) return "Neutral";
        if (report.FailedCount > 0) return "Error";
        if (report.WarningCount > 0) return "Warning";
        if (report.PassedCount > 0) return "Success";
        if (report.Items?.Exists(item => item.State == EnvironmentCheckState.Checking) == true) return "Info";
        return "Neutral";
    }
}
