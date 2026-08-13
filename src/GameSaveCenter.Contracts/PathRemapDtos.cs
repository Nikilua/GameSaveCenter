using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    /// <summary>Request to remap every stored path under one root to another root.</summary>
    public sealed class PathRemapRequestDto
    {
        public string OldRoot { get; set; } = string.Empty;
        public string NewRoot { get; set; } = string.Empty;
        public bool Confirmed { get; set; }
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
