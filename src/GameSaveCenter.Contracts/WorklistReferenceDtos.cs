using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    public enum WorklistReferenceState
    {
        NotChecked = 0,
        Indexed = 1,
        Resolved = 2,
        NotFound = 3,
        InvalidReference = 4,
        Unavailable = 5
    }

    /// <summary>Bounded exact-ID lookup for references shown in a user worklist.</summary>
    public sealed class WorklistReferenceQueryDto
    {
        public const int MaximumReferences = 200;
        public List<WorklistReferenceQueryItemDto> References { get; set; } = new List<WorklistReferenceQueryItemDto>();
    }

    public sealed class WorklistReferenceQueryItemDto
    {
        /// <summary>Stable numeric value of the plugin WorklistObjectKind enum.</summary>
        public int Kind { get; set; }
        public string StableObjectId { get; set; } = string.Empty;
    }

    public sealed class WorklistReferenceQueryResultDto
    {
        public List<WorklistReferenceQueryResultItemDto> References { get; set; } = new List<WorklistReferenceQueryResultItemDto>();
    }

    public sealed class WorklistReferenceQueryResultItemDto
    {
        public int Kind { get; set; }
        public string StableObjectId { get; set; } = string.Empty;
        public WorklistReferenceState State { get; set; } = WorklistReferenceState.NotChecked;
    }
}
