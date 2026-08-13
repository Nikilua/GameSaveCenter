using System;

namespace GameSaveCenter.Contracts
{
    /// <summary>User-readable maintenance health report. This is not the diagnostics ZIP.</summary>
    public sealed class MaintenanceReportDto
    {
        public DateTime GeneratedUtc { get; set; } = DateTime.UtcNow;
        public string Summary { get; set; } = string.Empty;
        public string ReportText { get; set; } = string.Empty;
    }
}
