using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    /// <summary>Request to remap every stored path under one root to another root.</summary>
    public sealed class PathRemapRequestDto
    {
        public string OldRoot { get; set; } = string.Empty;
        public string NewRoot { get; set; } = string.Empty;
        public bool Confirmed { get; set; }
        public bool ApplyMissingTargets { get; set; }
    }

    public sealed class PathRemapPreviewItemDto
    {
        public string Category { get; set; } = string.Empty;
        public string OldPath { get; set; } = string.Empty;
        public string NewPath { get; set; } = string.Empty;
        public bool TargetExists { get; set; }

        /// <summary>Full path retained for copyable read-only preview cells.</summary>
        public string OldPathDisplay => OldPath;

        /// <summary>Full remapped path retained for copyable read-only preview cells.</summary>
        public string NewPathDisplay => NewPath;

        public string TargetStateDisplay => TargetExists ? "目标已存在" : "目标不存在（需明确确认）";
    }

    public sealed class PathRemapPreviewDto
    {
        public List<PathRemapPreviewItemDto> Items { get; set; } = new List<PathRemapPreviewItemDto>();
        public int AffectedRowCount { get; set; }
        public int MissingTargetCount { get; set; }
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>Result of a batch path migration.</summary>
    public sealed class PathRemapResultDto
    {
        public string OldRoot { get; set; } = string.Empty;
        public string NewRoot { get; set; } = string.Empty;
        public int AffectedRows { get; set; }
        public List<string> UpdatedSettings { get; set; } = new List<string>();
        public string Summary { get; set; } = string.Empty;
    }
}
