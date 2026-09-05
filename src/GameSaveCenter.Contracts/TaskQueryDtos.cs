using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    /// <summary>Server-side task history query. Date ranges are UTC half-open intervals.</summary>
    public sealed class TaskQueryDto
    {
        public int Limit { get; set; } = 50;
        /// <summary>Opaque cursor returned by the previous page; ordering is created time then task ID descending.</summary>
        public string Cursor { get; set; } = string.Empty;
        public TaskState? State { get; set; }
        public List<TaskState> States { get; set; } = new List<TaskState>();
        public string GameId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
        public string Search { get; set; } = string.Empty;
        public DateTime? StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }
    }

    /// <summary>Stable page of task history plus a query-independent summary for this filter.</summary>
    public sealed class TaskPageDto
    {
        public List<TaskStatusDto> Items { get; set; } = new List<TaskStatusDto>();
        public int TotalCount { get; set; }
        public bool HasMore { get; set; }
        public string NextCursor { get; set; } = string.Empty;
        public TaskSummaryDto Summary { get; set; } = new TaskSummaryDto();
    }

    /// <summary>SQL aggregate for a task query; it is independent of the page size/cursor.</summary>
    public sealed class TaskSummaryDto
    {
        public int TotalCount { get; set; }
        public int QueuedCount { get; set; }
        public int RunningCount { get; set; }
        public int WaitingForUserCount { get; set; }
        public int SucceededCount { get; set; }
        public int FailedCount { get; set; }
        public int CancelledCount { get; set; }
        public int PendingCloudCount { get; set; }
        public int ActiveCount => QueuedCount + RunningCount + WaitingForUserCount;
    }
}
