using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    public enum NotificationSeverity
    {
        Information = 0,
        Success = 1,
        Warning = 2,
        Error = 3
    }

    /// <summary>
    /// Durable, local notification metadata. Summary text is a short user-facing snapshot;
    /// task state remains authoritative in the task store.
    /// </summary>
    public sealed class NotificationRecordDto
    {
        public string NotificationId { get; set; } = Guid.NewGuid().ToString("N");
        public string DeduplicationKey { get; set; } = string.Empty;
        /// <summary>Stable identity for one explicit retry lineage; empty for standalone notices.</summary>
        public string IssueKey { get; set; } = string.Empty;
        /// <summary>Unique terminal event identity used to make retry aggregation idempotent.</summary>
        public string OccurrenceKey { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string FirstTaskId { get; set; } = string.Empty;
        public string GameId { get; set; } = string.Empty;
        /// <summary>Explicit origin label captured from the event that created this notice.</summary>
        public string Source { get; set; } = string.Empty;
        public NotificationSeverity Severity { get; set; } = NotificationSeverity.Information;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public int OccurrenceCount { get; set; } = 1;
        public DateTime FirstOccurredUtc { get; set; }
        public DateTime LastOccurredUtc { get; set; }
        public string LastError { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public bool IsRead { get; set; }
    }

    /// <summary>Bounded read request for the local notification inbox.</summary>
    public sealed class NotificationRecordsQueryDto
    {
        public int Limit { get; set; } = 500;
    }

    /// <summary>Bounded IDs for marking visible notification records as read.</summary>
    public sealed class NotificationRecordIdsDto
    {
        public List<string> NotificationIds { get; set; } = new List<string>();
    }

    /// <summary>Explicit request for deleting read notification records only.</summary>
    public sealed class NotificationRecordsClearReadDto
    {
    }
}
