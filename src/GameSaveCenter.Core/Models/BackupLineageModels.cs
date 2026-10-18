using System;
using System.Collections.Generic;

namespace GameSaveCenter.Core.Models
{
    public enum BackupLineageRelationKind
    {
        Parent,
        EquivalentContent
    }

    public enum BackupLineageIssueKind
    {
        MissingBackupId,
        DuplicateBackupId,
        ParentNotFound,
        AmbiguousParentReference,
        ParentCycle,
        UnknownRelationship
    }

    /// <summary>A compact, read-only projection of a backup version for lineage views.</summary>
    public sealed class BackupLineageNode
    {
        public int Index { get; set; }
        public string BackupId { get; set; } = string.Empty;
        public string ParentBackupId { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
        public string SourceDevice { get; set; } = string.Empty;
        public long TotalBytes { get; set; }
        public int FileCount { get; set; }
        public string ContentFingerprint { get; set; } = string.Empty;
    }

    /// <summary>A relationship supported by an explicit parent ID or matching content fingerprint.</summary>
    public sealed class BackupLineageRelation
    {
        public BackupLineageRelationKind Kind { get; set; }
        public int FromNodeIndex { get; set; }
        public int ToNodeIndex { get; set; }
    }

    /// <summary>A structural problem or uncertainty found while building a lineage model.</summary>
    public sealed class BackupLineageIssue
    {
        public BackupLineageIssueKind Kind { get; set; }
        public string BackupId { get; set; } = string.Empty;
        public string RelatedBackupId { get; set; } = string.Empty;
        public List<int> NodeIndices { get; set; } = new List<int>();
    }

    public sealed class BackupLineageModel
    {
        public int InputItemLimit { get; set; }
        public bool InputTruncated { get; set; }
        public List<BackupLineageNode> Nodes { get; set; } = new List<BackupLineageNode>();
        public List<BackupLineageRelation> Relations { get; set; } = new List<BackupLineageRelation>();
        public List<BackupLineageIssue> Issues { get; set; } = new List<BackupLineageIssue>();
    }
}
