using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    /// <summary>Raw SQLite facts gathered by the Worker during an integrity check.</summary>
    public sealed class DatabaseIntegrityProbeDto
    {
        public bool Opened { get; set; }
        public List<string> IntegrityRows { get; set; } = new List<string>();
        public List<string> ForeignKeyViolations { get; set; } = new List<string>();
        public List<string> MissingTables { get; set; } = new List<string>();
        public List<string> BackupArchivePaths { get; set; } = new List<string>();
        public List<string> GameToolEntryPaths { get; set; } = new List<string>();
        public List<string> MediaArchivePaths { get; set; } = new List<string>();
    }

    /// <summary>One actionable problem found by the global integrity check.</summary>
    public sealed class IntegrityFindingDto
    {
        public string Code { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning";
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string SuggestedAction { get; set; } = string.Empty;
    }

    /// <summary>Read-only report returned by the Worker after checking the whole installation.</summary>
    public sealed class IntegrityCheckResultDto
    {
        public DateTime CheckedUtc { get; set; } = DateTime.UtcNow;
        public string State { get; set; } = "Healthy";
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int SkippedCount { get; set; }
        public List<IntegrityFindingDto> Findings { get; set; } = new List<IntegrityFindingDto>();
        public string Summary { get; set; } = string.Empty;
        public string StateDisplay => State switch
        {
            "Error" => "错误",
            "Warning" => "需关注",
            "Skipped" => "已跳过",
            _ => "正常"
        };
    }
}
