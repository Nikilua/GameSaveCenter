using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    public enum EnvironmentCheckState
    {
        Checking,
        Passed,
        Warning,
        Failed,
        Skipped
    }

    /// <summary>One non-destructive first-run readiness check.</summary>
    public sealed class EnvironmentCheckItemDto
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public EnvironmentCheckState State { get; set; } = EnvironmentCheckState.Checking;
        public string Summary { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public bool IsOptional { get; set; }

        public string StateDisplay => State switch
        {
            EnvironmentCheckState.Passed => "通过",
            EnvironmentCheckState.Warning => "注意",
            EnvironmentCheckState.Failed => "失败",
            EnvironmentCheckState.Skipped => "跳过",
            _ => "检查中"
        };

        public string StateGlyph => State switch
        {
            EnvironmentCheckState.Passed => "✓",
            EnvironmentCheckState.Warning => "!",
            EnvironmentCheckState.Failed => "×",
            EnvironmentCheckState.Skipped => "–",
            _ => "…"
        };
    }

    /// <summary>Worker-owned report used by the Playnite onboarding card.</summary>
    public sealed class EnvironmentCheckReportDto
    {
        public DateTime CheckedUtc { get; set; }
        public List<EnvironmentCheckItemDto> Items { get; set; } = new List<EnvironmentCheckItemDto>();
        public int PassedCount { get; set; }
        public int WarningCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public bool IsReady => FailedCount == 0;
        public string Summary { get; set; } = "尚未运行环境检查。";
        public string CheckedLocalDisplay => CheckedUtc == default(DateTime)
            ? "尚未检查"
            : CheckedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    }

    public sealed class EnvironmentCheckRequestDto
    {
        public bool IncludeRemoteProbe { get; set; } = true;
        /// <summary>Whether the check should enumerate the full Ludusavi backup list.</summary>
        public bool IncludeBackupProbe { get; set; } = true;
    }
}
