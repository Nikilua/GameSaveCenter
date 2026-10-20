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
        /// <summary>Stable parent game id used to open game-scoped media/finding details.</summary>
        public string PlayniteId { get; set; } = string.Empty;
        /// <summary>Bounded, transient, path-free display summary read from the current indexed object.</summary>
        public string CurrentSummary { get; set; } = string.Empty;
        /// <summary>Bounded current owner game label; does not assert that source files are accessible.</summary>
        public string ParentSummary { get; set; } = string.Empty;
        public WorklistReferenceState State { get; set; } = WorklistReferenceState.NotChecked;
    }

    public sealed class ValidationFindingQueryDto
    {
        public string FindingId { get; set; } = string.Empty;
    }

    public sealed class ValidationFindingQueryResultDto
    {
        public ValidationFindingDto? Finding { get; set; }
    }
}
